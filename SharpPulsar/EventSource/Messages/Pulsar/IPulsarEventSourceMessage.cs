﻿using SharpPulsar.Configuration;

namespace SharpPulsar.Akka.EventSource.Messages.Pulsar
{
    public interface IPulsarEventSourceMessage<T>: IEventSourceMessage
    {
        public string AdminUrl { get; }
        public ReaderConfigurationData<T> Configuration { get; }
        public ClientConfigurationData ClientConfiguration { get; }
    }
}
