﻿using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Precondition;
using SharpPulsar.Queues;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPulsar.User
{
    public class Producer<T> : IProducer<T>
    {
        private readonly IActorRef _producerActor;
        private readonly ProducerQueueCollection<T> _queue;
        private readonly ISchema<T> _schema;
        private readonly ProducerConfigurationData _conf;

        public Producer(IActorRef producer, ProducerQueueCollection<T> queue, ISchema<T> schema, ProducerConfigurationData conf)
        {
            _producerActor = producer;
            _queue = queue;
            _schema = schema;
            _conf = conf;
        }
        public string Topic 
            => TopicAsync().GetAwaiter().GetResult();
        public async ValueTask<string> TopicAsync() 
            => await _producerActor.Ask<string>(GetTopic.Instance);

        public string ProducerName 
            => ProducerNameAsync().GetAwaiter().GetResult();
        public async ValueTask<string> ProducerNameAsync()
            => await _producerActor.Ask<string>(GetProducerName.Instance);

        public long LastSequenceId 
            => LastSequenceIdAsync().GetAwaiter().GetResult();
        public async ValueTask<long> LastSequenceIdAsync()
            => await _producerActor.Ask<long>(GetLastSequenceId.Instance);
        
        public IProducerStats Stats 
            => StatsAsync().GetAwaiter().GetResult();
        public async ValueTask<IProducerStats> StatsAsync()
            => await _producerActor.Ask<IProducerStats>(GetStats.Instance);

        public bool Connected 
            => ConnectedAsync().GetAwaiter().GetResult();
        public async ValueTask<bool> ConnectedAsync() 
            => await _producerActor.Ask<bool>(IsConnected.Instance);

        public long LastDisconnectedTimestamp 
            => LastDisconnectedTimestampAsync().GetAwaiter().GetResult();
        public async ValueTask<long> LastDisconnectedTimestampAsync()
            => await _producerActor.Ask<long>(GetLastDisconnectedTimestamp.Instance);

        public void Close()
        {
            _producerActor.GracefulStop(TimeSpan.FromSeconds(5));
        }

        public void Flush()
        {
            _producerActor.Tell(Messages.Producer.Flush.Instance);
        }

        public ITypedMessageBuilder<T> NewMessage()
        {
            return new TypedMessageBuilder<T>(_producerActor, _schema);
        }

        public ITypedMessageBuilder<V> NewMessage<V>(ISchema<V> schema)
        {
            Condition.CheckArgument(schema != null);
            return new TypedMessageBuilder<V>(_producerActor, schema);
        }

        public TypedMessageBuilder<T> NewMessage(Transaction txn)
        {
            // check the producer has proper settings to send transactional messages
            if (_conf.SendTimeoutMs > 0)
            {
                throw new ArgumentException("Only producers disabled sendTimeout are allowed to" + " produce transactional messages");
            }

            return new TypedMessageBuilder<T>(_producerActor, _schema, txn);
        }
        internal IActorRef GetProducer => _producerActor;

        public AckReceived Send(T message)
        {
            return SendAsync(message).GetAwaiter().GetResult();
        }
        public async ValueTask<AckReceived> SendAsync(T message)
        {
            await NewMessage().Value(message).SendAsync().ConfigureAwait(false);
            return _queue.Receipt.Take();
        }
        public AckReceived SendReceipt(int timeoutMilliseconds = 30000, CancellationToken token = default)
        {
            if (_queue.Receipt.TryTake(out var sent, timeoutMilliseconds, token))
            {
                return sent;
            }
            return sent;
        }
    }
}
