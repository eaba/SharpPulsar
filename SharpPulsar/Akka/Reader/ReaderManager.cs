﻿using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Reader
{
    public class ReaderManager:ReceiveActor, IWithUnboundedStash
    {
        private IActorRef _network;
        private ClientConfigurationData _config;
        public ReaderManager(ClientConfigurationData configuration, IActorRef network)
        {
            _network = network;
            _config = configuration;
            Receive<NewReader>(NewReader);
            ReceiveAny(x =>
            {
                Context.System.Log.Info($"{x.GetType().Name} not supported");
            });
        }
        public static Props Prop(ClientConfigurationData clientConfiguration, IActorRef network)
        {
            return Props.Create(() => new ReaderManager(clientConfiguration, network));
        }
        
        private void NewReader(NewReader reader)
        {
            var schema = reader.ReaderConfiguration.Schema;
            var clientConfig = reader.Configuration;
            var readerConfig = reader.ReaderConfiguration;
            Context.ActorOf(Reader.Prop(clientConfig, readerConfig, _network));
        }
        
        public IStash Stash { get; set; }
    }
}
