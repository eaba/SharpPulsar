﻿using Akka.Actor;
using Akka.Configuration;
using NLog;
using SharpPulsar.Configuration;
using SharpPulsar.User;

namespace SharpPulsar
{
    public sealed class PulsarSystem
    {
        private static PulsarSystem _instance;
        private static readonly object Lock = new object();
        private readonly ActorSystem _actorSystem;
        private readonly ClientConfigurationData _conf;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        public static PulsarSystem GetInstance(ActorSystem actorSystem, ClientConfigurationData conf)
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new PulsarSystem(actorSystem, conf);
                    }
                }
            }
            return _instance;
        }
        public static PulsarSystem GetInstance(ClientConfigurationData conf, NLog.Config.LoggingConfiguration loggingConfiguration = null)
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new PulsarSystem(conf, loggingConfiguration);
                    }
                }
            }
            return _instance;
        }
        private PulsarSystem(ClientConfigurationData conf, NLog.Config.LoggingConfiguration loggingConfiguration)
        {
            var nlog = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets
                .FileTarget("logFile")
            {
                FileName = "logs.log",
                Layout = "[${longdate}] [${logger}] ${level:uppercase=true}] : ${event-properties:actorPath} ${message} ${exception:format=tostring}",
                ArchiveEvery = NLog.Targets.FileArchivePeriod.Hour,
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.DateAndSequence
            };
            nlog.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            LogManager.Configuration = loggingConfiguration ?? nlog;
            _conf = conf;
            var config = ConfigurationFactory.ParseString(@"
            akka
            {
                loglevel = DEBUG
			    log-config-on-start = on 
                loggers=[""Akka.Logger.NLog.NLogLogger, Akka.Logger.NLog""]
			    actor 
                {              
				      debug 
				      {
					      receive = on
					      autoreceive = on
					      lifecycle = on
					      event-stream = on
					      unhandled = on
				      }  
			    }
                coordinated-shutdown
                {
                    exit-clr = on
                }
            }"
            );
            _actorSystem = ActorSystem.Create("Pulsar", config);
            _cnxPool = _actorSystem.ActorOf(ConnectionPool.Prop(conf), "ConnectionPool");
            _client = _actorSystem.ActorOf(PulsarClientActor.Prop(conf, _actorSystem.Scheduler.Advanced, _cnxPool), "PulsarClient");
        }
        private PulsarSystem(ActorSystem actorSystem, ClientConfigurationData conf)
        {
            _actorSystem = actorSystem;
            _conf = conf;
            _cnxPool = _actorSystem.ActorOf(ConnectionPool.Prop(conf), "ConnectionPool");
            _client = _actorSystem.ActorOf(PulsarClientActor.Prop(conf, _actorSystem.Scheduler.Advanced, _cnxPool), "PulsarClient");
        }
        public PulsarClient NewClient() 
        {
            return new PulsarClient(_client, _conf, _actorSystem);
        }
        public Admin NewAdmin() 
        {
            return null;
        }
        public EventSource NewEventSource() 
        {
            return null;
        }
    }
}
