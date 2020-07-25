﻿
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.EventSource.Messages.Pulsar
{
    /// <summary>
    /// Same type of query as <see cref="EventsByTopic"/> but the event query
    /// is completed immediately when it reaches the end of the "result set". Events that are
    /// stored after the query is completed are not included in the event stream.
    /// </summary>
    public sealed class CurrentEventsByTopic : IPulsarEventSourceMessage
    {
        public CurrentEventsByTopic(string tenant, string ns, string topic, long fromSequenceId, long toSequenceId, SourceType source, string adminUrl, ReaderConfigurationData configuration)
        {
            Tenant = tenant;
            Namespace = ns;
            Topic = topic;
            FromSequenceId = fromSequenceId;
            ToSequenceId = toSequenceId;
            Source = source;
            AdminUrl = adminUrl;
            Configuration = configuration;
        }

        public string Tenant { get; }
        public string Namespace { get; }
        public string Topic { get; }
        public string AdminUrl { get; }
        public long FromSequenceId { get; } //Compute ledgerId and entryId for this 
        public long ToSequenceId { get; } //Compute ledgerId and entryId for this 
        public SourceType Source { get; }
        public ReaderConfigurationData Configuration { get; }
    }
}
