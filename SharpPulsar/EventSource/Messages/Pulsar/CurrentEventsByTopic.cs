﻿
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Configuration;

namespace SharpPulsar.EventSource.Messages.Pulsar
{
    /// <summary>
    /// Same type of query as <see cref="EventsByTopic"/> but the event query
    /// is completed immediately when it reaches the end of the "result set". Events that are
    /// stored after the query is completed are not included in the event stream.
    /// </summary>
    public sealed class CurrentEventsByTopic<T> : IPulsarEventSourceMessage<T>
    {
        public CurrentEventsByTopic(string tenant, string ns, string topic, long fromSequenceId, long toSequenceId, string adminUrl, ReaderConfigurationData<T> configuration, ClientConfigurationData clientConfiguration)
        {
            Tenant = tenant;
            Namespace = ns;
            Topic = topic;
            FromSequenceId = fromSequenceId;
            ToSequenceId = toSequenceId;
            Source = SourceType.Pulsar;
            AdminUrl = adminUrl;
            Configuration = configuration;
            ClientConfiguration = clientConfiguration;
        }

        public string Tenant { get; }
        public string Namespace { get; }
        public string Topic { get; }
        public string AdminUrl { get; }
        public long FromSequenceId { get; } //Compute ledgerId and entryId for this 
        public long ToSequenceId { get; } //Compute ledgerId and entryId for this 
        public SourceType Source { get; }
        public ReaderConfigurationData<T> Configuration { get; }
        public ClientConfigurationData ClientConfiguration { get; }
    }
}
