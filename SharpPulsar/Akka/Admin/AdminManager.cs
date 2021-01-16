﻿using System.Text.RegularExpressions;
using Akka.Actor;
using SharpPulsar.Messages;

namespace SharpPulsar.Akka.Admin
{
    public class AdminManager:ReceiveActor
    {
        public AdminManager(AdminConfiguration configuration, IActorRef pulsarManager)
        {
            foreach (var s in configuration.BrokerWebServiceUrl)
            {
                var an = Regex.Replace(s, @"[^\w\d]", "");
                Context.ActorOf(AdminCoordinator.Prop(s, pulsarManager), an);
            }

            Receive((InternalCommands.Admin q) =>
            {
                var an = Regex.Replace(q.BrokerDestinationUrl, @"[^\w\d]", "");
                var actor = Context.Child(an);
                if (actor.IsNobody())
                    q.Log($"{q.BrokerDestinationUrl} not found");
                else
                    actor.Tell(q);
            });
        }
        public static Props Prop(AdminConfiguration configuration, IActorRef pulsarManager)
        {
            return Props.Create(() => new AdminManager(configuration, pulsarManager));
        }
    }
}
