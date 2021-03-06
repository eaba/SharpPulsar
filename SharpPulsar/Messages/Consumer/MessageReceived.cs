﻿using System.Buffers;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages.Consumer
{
    public class MessageReceived
    {
        public MessageReceived(MessageMetadata metadata, ReadOnlySequence<byte> payload, MessageIdData messageId, int redeliveryCount, bool chueckSum, short magicNumber)
        {
            MessageId = messageId;
            Payload = payload;
            RedeliveryCount = redeliveryCount;
            Metadata = metadata;
            CheckSum = chueckSum;
            MagicNumber = magicNumber;
        }
        public MessageMetadata Metadata { get; }
        public MessageIdData MessageId { get; }
        public ReadOnlySequence<byte> Payload { get; }
        public int RedeliveryCount { get; }
        public bool CheckSum { get; }
        public short MagicNumber { get; }
    }
}
