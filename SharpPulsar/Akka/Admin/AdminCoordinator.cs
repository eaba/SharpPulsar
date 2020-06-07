﻿using Akka.Actor;
using Akka.Routing;
using SharpPulsar.Akka.InternalCommands;

namespace SharpPulsar.Akka.Admin
{
    public class AdminCoordinator:ReceiveActor
    {
        public AdminCoordinator(string server, IActorRef pulsarManager)
        {
            var coordinator = Context.ActorOf(AdminWorker.Prop(server, pulsarManager).WithRouter(new RoundRobinPool(10, new DefaultResizer(2, 10))),
                "AdminCoordinatorWorkerPool");
            Receive((InternalCommands.Admin q) => coordinator.Tell(q));
        }
        public static Props Prop(string server, IActorRef pulsarManager)
        {
            return Props.Create(() => new AdminCoordinator(server, pulsarManager));
        }
    }
}
