﻿//-----------------------------------------------------------------------
// <copyright file="TcpManager.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.IO;

namespace SharpPulsar.PulsarSocket
{
    using static PulsarTcp;
    using ByteBuffer = ArraySegment<byte>;

    /// <summary>
    /// INTERNAL API
    /// 
    /// TcpManager is a facade for accepting commands (<see cref="Command"/>) to open client or server TCP connections.
    /// 
    /// TcpManager is obtainable by calling {{{ IO(Tcp) }}} (see [[akka.io.IO]] and [[akka.io.Tcp]])
    /// 
    /// == Bind ==
    /// 
    /// To bind and listen to a local address, a <see cref="Bind"/> command must be sent to this actor. If the binding
    /// was successful, the sender of the <see cref="Bind"/> will be notified with a <see cref="Bound"/>
    /// message. The sender() of the <see cref="Bound"/> message is the Listener actor (an internal actor responsible for
    /// listening to server events). To unbind the port an <see cref="Unbind"/> message must be sent to the Listener actor.
    /// 
    /// If the bind request is rejected because the Tcp system is not able to register more channels (see the nr-of-selectors
    /// and max-channels configuration options in the akka.io.tcp section of the configuration) the sender will be notified
    /// with a <see cref="CommandFailed"/> message. This message contains the original command for reference.
    /// 
    /// When an inbound TCP connection is established, the handler will be notified by a <see cref="Connected"/> message.
    /// The sender of this message is the Connection actor (an internal actor representing the TCP connection). At this point
    /// the procedure is the same as for outbound connections (see section below).
    /// 
    /// == Connect ==
    /// 
    /// To initiate a connection to a remote server, a <see cref="Connect"/> message must be sent to this actor. If the
    /// connection succeeds, the sender() will be notified with a <see cref="Connected"/> message. The sender of the
    /// <see cref="Connected"/> message is the Connection actor (an internal actor representing the TCP connection). Before
    /// starting to use the connection, a handler must be registered to the Connection actor by sending a <see cref="Register"/>
    /// command message. After a handler has been registered, all incoming data will be sent to the handler in the form of
    /// <see cref="Received"/> messages. To write data to the connection, a <see cref="Write"/> message must be sent
    /// to the Connection actor.
    /// 
    /// If the connect request is rejected because the Tcp system is not able to register more channels (see the nr-of-selectors
    /// and max-channels configuration options in the akka.io.tcp section of the configuration) the sender will be notified
    /// with a <see cref="CommandFailed"/> message. This message contains the original command for reference.
    /// </summary>
    internal sealed class PulsarTcpManager : ActorBase
    {
        private readonly TcpExt _tcp;

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="tcp">TBD</param>
        public PulsarTcpManager(TcpExt tcp)
        {
            _tcp = tcp;
            Context.System.EventStream.Subscribe(Self, typeof(DeadLetter));
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="message">TBD</param>
        /// <exception cref="ArgumentException">TBD</exception>
        /// <returns>TBD</returns>
        protected override bool Receive(object message)
        {
            var c = message as Connect;
            if (c != null)
            {
                var commander = Sender;
                if (c.Options.Any(r => r is PulsarTlsConnectionOption))
                {
                    Context.ActorOf(Props.Create(() =>
                        new PulsarTlsOutgoingConnection(_tcp, commander, c)).WithDispatcherIfNeeded(c.Options));
                }
                else
                {
                    Context.ActorOf(Props.Create<PulsarTcpOutgoingConnection>(_tcp, commander, c).WithDispatcherIfNeeded(c.Options));
                }
                return true;
            }
            var b = message as Bind;
            if (b != null)
            {
                var commander = Sender;
                if (b.Options.Any(r => r is PulsarTlsConnectionOption))
                {
                    Context.ActorOf(Props.Create<PulsarTlsListener>(() =>
                        new PulsarTlsListener(_tcp, commander, b)));
                }
                else
                {
                    Context.ActorOf(Props.Create<PulsarTcpListener>(_tcp, commander, b));
                }

                return true;
            }
            var dl = message as DeadLetter;
            if (dl != null)
            {
                var completed = dl.Message as SocketCompleted;
                if (completed != null)
                {
                    //TODO: release resources?
                }
                return true;
            }
            throw new ArgumentException($"The supplied message of type {message.GetType().Name} is invalid. Only Connect and Bind messages are supported. " +
                                        $"If you are going to manage your connection state, you need to communicate with Tcp.Connected sender actor. " +
                                        $"See more here: https://getakka.net/articles/networking/io.html", nameof(message));
        }
    }
    static class TcpActorPropsExtensions
    {
        public static Props WithDispatcherIfNeeded(this Props props,
            IEnumerable<Inet.SocketOption> options)
        {
            if (options.FirstOrDefault(o => o is PulsarWorkerDispatcher) is
                PulsarWorkerDispatcher opt)
            {
                return props.WithDispatcher(opt.Dispatcher);
            }

            return props;
        }
    }
}