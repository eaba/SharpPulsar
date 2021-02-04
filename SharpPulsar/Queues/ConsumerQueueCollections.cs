﻿
using SharpPulsar.Interfaces;
using SharpPulsar.Messages;
using SharpPulsar.Stats.Consumer.Api;
using System.Collections.Concurrent;

namespace SharpPulsar.Queues
{
    public class ConsumerQueueCollections<T>
    {
        public BlockingCollection<IMessage<T>> Receive { get; } = new BlockingCollection<IMessage<T>>();
        public BlockingCollection<IMessages<T>> BatchReceive { get; } = new BlockingCollection<IMessages<T>>();
        public BlockingCollection<ClientExceptions> AcknowledgeException { get; } = new BlockingCollection<ClientExceptions>();
        public BlockingCollection<ClientExceptions> NegativeAcknowledgeException { get; } = new BlockingCollection<ClientExceptions>();
        public BlockingCollection<ClientExceptions> ReconsumeLaterException { get; } = new BlockingCollection<ClientExceptions>();
        public BlockingCollection<ClientExceptions> RedeliverUnacknowledgedException { get; } = new BlockingCollection<ClientExceptions>();
        public BlockingCollection<ClientExceptions> UnsubscribeException { get; } = new BlockingCollection<ClientExceptions>();
        public BlockingCollection<ClientExceptions> SeekException { get; } = new BlockingCollection<ClientExceptions>();
        public BlockingCollection<ClientExceptions> AcknowledgeCumulativeException { get; } = new BlockingCollection<ClientExceptions>();
        public BlockingCollection<string> ConsumerName { get; } = new BlockingCollection<string>();
        public BlockingCollection<string> Subscription { get; } = new BlockingCollection<string>();
        public BlockingCollection<string> Topic { get; } = new BlockingCollection<string>();
        public BlockingCollection<long> LastDisconnectedTimestamp { get; } = new BlockingCollection<long>();
        public BlockingCollection<bool> HasReachedEndOfTopic { get; } = new BlockingCollection<bool>();
        public BlockingCollection<bool> Connected { get; } = new BlockingCollection<bool>();
        public BlockingCollection<IMessageId> LastMessageId = new BlockingCollection<IMessageId>();
        public BlockingCollection<IConsumerStats> Stats = new BlockingCollection<IConsumerStats>();
    }
}