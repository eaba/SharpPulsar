﻿using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public sealed class CreateConsumer
    {
        public CreateConsumer(ISchema schema, ConsumerConfigurationData consumerConfiguration, ConsumerType consumerType, Seek seek = null)
        {
            Schema = schema;
            ConsumerConfiguration = consumerConfiguration;
            ConsumerType = consumerType;
            Seek = seek;
        }
        public Seek Seek { get; }
        public ConsumerType ConsumerType { get; }
        public ISchema Schema { get; }
        public ConsumerConfigurationData ConsumerConfiguration { get; }
    }
    internal sealed class NewConsumer
    {
        public NewConsumer(ISchema schema, ClientConfigurationData configuration, ConsumerConfigurationData consumerConfiguration, ConsumerType consumerType, Seek seek = null)
        {
            Schema = schema;
            Configuration = configuration;
            ConsumerConfiguration = consumerConfiguration; 
            ConsumerType = consumerType;
            Seek = seek;
        }
        public Seek Seek { get; }
        public ConsumerType ConsumerType { get; }
        public ISchema Schema { get; }
        public ClientConfigurationData Configuration { get; }
        public ConsumerConfigurationData ConsumerConfiguration { get; }
    }

    public sealed class Seek
    {
        public Seek(SeekType type, object input)
        {
            Type = type;
            Input = input;
        }

        public SeekType? Type { get; }
        public object Input { get; }
    }
    public enum SeekType
    {
        Timestamp,
        MessageId
    }
    public enum ConsumerType
    {
        Single,
        Multi,
        Pattern
    }
}
