// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace PulsarAdmin.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class TopicStats
    {
        /// <summary>
        /// Initializes a new instance of the TopicStats class.
        /// </summary>
        public TopicStats()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the TopicStats class.
        /// </summary>
        public TopicStats(double? msgRateIn = default(double?), double? msgThroughputIn = default(double?), double? msgRateOut = default(double?), double? msgThroughputOut = default(double?), double? averageMsgSize = default(double?), long? storageSize = default(long?), long? backlogSize = default(long?), IList<PublisherStats> publishers = default(IList<PublisherStats>), IDictionary<string, SubscriptionStats> subscriptions = default(IDictionary<string, SubscriptionStats>), IDictionary<string, ReplicatorStats> replication = default(IDictionary<string, ReplicatorStats>), string deduplicationStatus = default(string), long? bytesInCounter = default(long?), long? msgInCounter = default(long?))
        {
            MsgRateIn = msgRateIn;
            MsgThroughputIn = msgThroughputIn;
            MsgRateOut = msgRateOut;
            MsgThroughputOut = msgThroughputOut;
            AverageMsgSize = averageMsgSize;
            StorageSize = storageSize;
            BacklogSize = backlogSize;
            Publishers = publishers;
            Subscriptions = subscriptions;
            Replication = replication;
            DeduplicationStatus = deduplicationStatus;
            BytesInCounter = bytesInCounter;
            MsgInCounter = msgInCounter;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "msgRateIn")]
        public double? MsgRateIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "msgThroughputIn")]
        public double? MsgThroughputIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "msgRateOut")]
        public double? MsgRateOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "msgThroughputOut")]
        public double? MsgThroughputOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "averageMsgSize")]
        public double? AverageMsgSize { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "storageSize")]
        public long? StorageSize { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "backlogSize")]
        public long? BacklogSize { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "publishers")]
        public IList<PublisherStats> Publishers { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "subscriptions")]
        public IDictionary<string, SubscriptionStats> Subscriptions { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "replication")]
        public IDictionary<string, ReplicatorStats> Replication { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "deduplicationStatus")]
        public string DeduplicationStatus { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "bytesInCounter")]
        public long? BytesInCounter { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "msgInCounter")]
        public long? MsgInCounter { get; set; }

    }
}