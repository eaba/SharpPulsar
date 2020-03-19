﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Dispatch;
using Akka.Event;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Akka.Network
{
    public sealed class ClientConnection: ReceiveActor, IWithUnboundedStash
    {
        internal readonly IAuthentication Authentication;
        private PulsarStream _stream;
        private IActorRef _self;
        private IActorRef _parent;
        private ReadOnlySequence<byte> _pong = new ReadOnlySequence<byte>(Commands.NewPong());
        private State _state;
        internal EndPoint RemoteAddress;
        internal int _remoteEndpointProtocolVersion = (int)ProtocolVersion.V15;
        public IActorRef Connection;
        private Dictionary<long, KeyValuePair<IActorRef, Payload>> _requests = new Dictionary<long, KeyValuePair<IActorRef, Payload>>();

        internal string ProxyToTargetBrokerAddress;
        private string _remoteHostName;

        private ILoggingAdapter Log;
        private ClientConfigurationData _conf;
        private IActorRef _manager;
        // Added for mutual authentication.
        internal IAuthenticationDataProvider AuthenticationDataProvider;

        public enum State
        {
            None,
            SentConnectFrame,
            Ready,
            Failed,
            Connecting
        }
        public ClientConnection(EndPoint endPoint, ClientConfigurationData conf, IActorRef manager)
        {
            _self = Self;
            RemoteHostName = "kubernetes";//Dns.GetHostEntry(((IPEndPoint) endPoint).Address).HostName;
            _conf = conf;
            _manager = manager;
            Connection = Self;
            RemoteAddress = endPoint;
            Log = Context.System.Log;
            if (conf.MaxLookupRequest < conf.ConcurrentLookupRequest)
                throw new Exception("ConcurrentLookupRequest must be less than MaxLookupRequest");
            Authentication = conf.Authentication;
            
            _parent = Context.Parent;
            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(5), Self, new OpenConnection(), ActorRefs.NoSender);
            var connector = new Connector(_conf);
            Receive<OpenConnection>(e =>
            {
                try
                {
                   
                    Context.System.Log.Info($"Opening Connection to: {RemoteAddress}");
                    _stream = new PulsarStream(connector.Connect((IPEndPoint)RemoteAddress));
                    _ = ProcessIncommingFrames();
                    var c = new ConnectionCommand(NewConnectCommand());
                    //if we got here, lets assume connection was successful
                    Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(30), Self, new ConnectionCommand(Commands.NewPing()), ActorRefs.NoSender);
                    _ = _stream.Send(new ReadOnlySequence<byte>(c.Command));
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(5), Self, new OpenConnection(), ActorRefs.NoSender);
                }
            });
            Receive<Payload>(p =>
            {
                var t = _requests.TryAdd(p.RequestId, new KeyValuePair<IActorRef, Payload>(Sender, p));
                _ = _stream.Send(new ReadOnlySequence<byte>(p.Bytes));
            });
            Receive<ConnectionCommand>(p =>
            {
                _ = _stream.Send(new ReadOnlySequence<byte>(p.Command));
            });
            ReceiveAny(_ => { Stash.Stash(); });
        }

        private void Open()
        {
           
        }

        public static Props Prop(EndPoint endPoint, ClientConfigurationData conf, IActorRef manager)
        {
            return Props.Create(() => new ClientConnection(endPoint, conf, manager));
        }

        
        protected override void PostStop()
        {
            try
            {
                _stream.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                
            }
        }

        protected override void Unhandled(object message)
        {
            Console.WriteLine($"Unhandled {message.GetType()} in {Self.Path}");
        }

		public byte[] NewConnectCommand()
		{
			// mutual authentication is to auth between `remoteHostName` and this client for this channel.
			// each channel will have a mutual client/server pair, mutual client evaluateChallenge with init data,
			// and return authData to server.
			AuthenticationDataProvider = Authentication.GetAuthData(RemoteHostName);
			var authData = AuthenticationDataProvider.Authenticate(new Shared.Auth.AuthDataShared(Shared.Auth.AuthDataShared.InitAuthData));
            var assemblyName = Assembly.GetCallingAssembly().GetName();
            var auth = new AuthData {auth_data = ((byte[]) (object) authData.Bytes)};
            var clientVersion = assemblyName.Name + " " + assemblyName.Version.ToString(3);

            return Commands.NewConnect(Authentication.AuthMethodName, auth, 15, clientVersion, ProxyToTargetBrokerAddress, string.Empty, null, string.Empty);
		}
        private sealed class ConnectionCommand
        {
            public ConnectionCommand(byte[] command)
            {
                Command = command;
            }

            public byte[] Command { get; }
        }
		public void HandlePing(CommandPing ping)
		{
			// Immediately reply success to ping requests
			if (Log.IsEnabled(LogLevel.DebugLevel))
			{
				//Log.Debug($"[{RemoteAddress}] Replying back to ping message");
			}
            _ = _stream.Send(_pong);
		}
        
        public IPEndPoint TargetBroker
		{
			set => ProxyToTargetBrokerAddress = $"{value.Address}:{value.Port:D}";
		}
        public async Task ProcessIncommingFrames()
        {
            await Task.Yield();

            try
            {
                await foreach (var frame in _stream.Frames())
                {
                    var commandSize = frame.ReadUInt32(0, true);
                    var cmd = Serializer.Deserialize(frame.Slice(4, commandSize));
                    var t = cmd.type;
                    
                    switch (cmd.type)
                    {
                        case BaseCommand.Type.GetLastMessageIdResponse:
                            var mid = cmd.getLastMessageIdResponse.LastMessageId;
                            var rquestid = (long)cmd.getLastMessageIdResponse.RequestId;
                            _requests[rquestid].Key.Tell(new LastMessageIdResponse((long)mid.ledgerId, (long)mid.entryId, mid.Partition, mid.BatchIndex));
                            _requests.Remove(rquestid);
                            break;
                        case BaseCommand.Type.Connected:
                            var c = cmd.Connected;
                            _parent.Tell(new ConnectedServerInfo(c.MaxMessageSize, c.ProtocolVersion, c.ServerVersion, RemoteHostName), _self);
                            Log.Info($"Now connected: Host = {RemoteHostName}, ProtocolVersion = {c.ProtocolVersion}");
                            break;
                        case BaseCommand.Type.GetTopicsOfNamespaceResponse:
                            var ns = cmd.getTopicsOfNamespaceResponse;
                            var requestid = (long)ns.RequestId;
                            _requests[requestid].Key.Tell(new NamespaceTopics(requestid, ns.Topics.ToList()));
                            _requests.Remove(requestid);
                            break;
                        case BaseCommand.Type.Message:
                            var msg = cmd.Message;
                            _manager.Tell(new MessageReceived((long)msg.ConsumerId, new MessageIdReceived((long)msg.MessageId.ledgerId, (long)msg.MessageId.entryId, msg.MessageId.BatchIndex, msg.MessageId.Partition), frame.Slice(commandSize + 4), (int)msg.RedeliveryCount));
                            break;
                        case BaseCommand.Type.Success:
                            var s = cmd.Success;
                            _requests[(long)s.RequestId].Key.Tell(new SubscribeSuccess(s?.Schema, (long)s.RequestId, s.Schema != null));
                            _requests.Remove((long)s.RequestId);
                            break;
                        case BaseCommand.Type.SendReceipt:
                            var send = cmd.SendReceipt;
                            _requests[(long)send.SequenceId].Key.Tell(new SentReceipt((long)send.ProducerId, (long)send.SequenceId, (long)send.MessageId.entryId, (long)send.MessageId.ledgerId, send.MessageId.BatchIndex, send.MessageId.Partition));
                            _requests.Remove((long)send.SequenceId);
                            break;
                        case BaseCommand.Type.GetOrCreateSchemaResponse:
                            var res = cmd.getOrCreateSchemaResponse;
                            _requests[(long)res.RequestId].Key.Tell(new GetOrCreateSchemaServerResponse((long)res.RequestId, res.ErrorMessage, res.ErrorCode, res.SchemaVersion));
                            _requests.Remove((long)res.RequestId);
                            break;
                        case BaseCommand.Type.ProducerSuccess:
                            var p = cmd.ProducerSuccess;
                            _requests[(long)p.RequestId].Key.Tell(new ProducerCreated(p.ProducerName, (long)p.RequestId, p.LastSequenceId, p.SchemaVersion));
                            _requests.Remove((long)p.RequestId);
                            break;
                        case BaseCommand.Type.Error:
                            var er = cmd.Error;
                            _requests[(long) er.RequestId].Key.Tell(new PulsarError(er.Message));
                            _requests.Remove((long)er.RequestId);
                            break;
                        case BaseCommand.Type.GetSchemaResponse:
                            var schema = cmd.getSchemaResponse.Schema;
                            var a = _requests[(long) cmd.getSchemaResponse.RequestId].Key;
                            if(schema == null)
                                a.Tell(new NullSchema());
                            else
                                a.Tell(new SchemaResponse(schema.SchemaData, schema.Name, schema.Properties.ToImmutableDictionary(x => x.Key, x => x.Value), schema.type, (long)cmd.getSchemaResponse.RequestId));
                            _requests.Remove((long)cmd.getSchemaResponse.RequestId);
                            break;
                        case BaseCommand.Type.LookupResponse:
                            var m = cmd.lookupTopicResponse;
                            _requests[(long)m.RequestId].Key.Tell(new BrokerLookUp(m.Message, m.Authoritative, m.Response, m.brokerServiceUrl, m.brokerServiceUrlTls, (long)m.RequestId));
                            _requests.Remove((long)m.RequestId);
                            break;
                        case BaseCommand.Type.PartitionedMetadataResponse:
                            var part = cmd.partitionMetadataResponse;
                            var rPay = _requests[(long)part.RequestId];
                            rPay.Key.Tell(new Partitions((int)part.Partitions, (long)part.RequestId, rPay.Value.Topic));
                            _requests.Remove((long)part.RequestId);
                            break;
                        case BaseCommand.Type.SendError:
                            var e = cmd.SendError;
                            break;
                        case BaseCommand.Type.Ping:
                            HandlePing(cmd.Ping);
                            break;
                        case BaseCommand.Type.CloseProducer:
                            _manager.Tell(new ProducerClosed((long)cmd.CloseProducer.ProducerId));
                            break;
                        default:
                            _manager.Tell(new ConsumerClosed((long)cmd.CloseConsumer.ConsumerId));
                            break;
                    }
                }
            }
            catch { }
        }
        
        public string RemoteHostName
		{
			get => _remoteHostName;
			set => _remoteHostName = value;
		}

        
        public IStash Stash { get; set; }
    }
    public sealed class OpenConnection
    {

    }

}
