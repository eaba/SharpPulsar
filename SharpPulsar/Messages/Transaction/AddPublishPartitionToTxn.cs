﻿using BAMCIS.Util.Concurrent;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Transaction;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace SharpPulsar.Messages.Transaction
{
    public sealed class AddPublishPartitionToTxn
    {
        public TxnID TxnID { get; }
        public ImmutableList<string> Topics { get; }
        public AddPublishPartitionToTxn(TxnID txnID, IList<string> topics)
        {
            TxnID = txnID;
            Topics = topics.ToImmutableList();
        }
    }
    public sealed class SubscriptionToTxn
    {
        public TxnID TxnID { get; }
        public string Topic { get; }
        public string Subscription { get; }
        public SubscriptionToTxn(TxnID txnID, string topic, string subscription)
        {
            TxnID = txnID;
            Topic = topic;
            Subscription = subscription;
        }
    }
    public sealed class AddSubscriptionToTxn
    {
        public TxnID TxnID { get; }
        public string Topic { get; }
        public ImmutableList<Subscription> Subscriptions { get; }
        public AddSubscriptionToTxn(TxnID txnID, IList<Subscription> subscriptions)
        {
            TxnID = txnID;
            Subscriptions = subscriptions.ToImmutableList();
        }
    }
    public sealed class NewTransaction
    {
        public long TxnRequestTimeoutMs { get; }
        public TimeUnit TimeUnit { get; }
        public NewTransaction(long txnRequestTimeoutMs, TimeUnit unit)
        {
            TxnRequestTimeoutMs = txnRequestTimeoutMs;
            TimeUnit = unit;
        }
    }
}
