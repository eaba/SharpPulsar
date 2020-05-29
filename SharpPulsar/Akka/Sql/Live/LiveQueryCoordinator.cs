﻿using System.Text.RegularExpressions;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;

namespace SharpPulsar.Akka.Sql.Live
{
    public class LiveQueryCoordinator : ReceiveActor
    {
        private IActorRef _pulsarManager;
        public LiveQueryCoordinator(IActorRef pulsar)
        {
            _pulsarManager = pulsar;
            Receive<LiveSql>(l =>
            {
                var topic = Regex.Replace(l.Topic, @"[^\w\d]", "");
                var child = Context.Child(topic);
                if (child.IsNobody())
                    Context.ActorOf(LiveQuery.Prop(pulsar, l), topic);
                else
                    child.Tell(l);
            });
        }

        public static Props Prop(IActorRef pulsar)
        {
            return Props.Create(()=> new LiveQueryCoordinator(pulsar));
        }
    }
}