// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace PulsarAdmin.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class ResourceQuota
    {
        /// <summary>
        /// Initializes a new instance of the ResourceQuota class.
        /// </summary>
        public ResourceQuota()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ResourceQuota class.
        /// </summary>
        public ResourceQuota(double? msgRateIn = default(double?), double? msgRateOut = default(double?), double? bandwidthIn = default(double?), double? bandwidthOut = default(double?), double? memory = default(double?), bool? dynamicProperty = default(bool?))
        {
            MsgRateIn = msgRateIn;
            MsgRateOut = msgRateOut;
            BandwidthIn = bandwidthIn;
            BandwidthOut = bandwidthOut;
            Memory = memory;
            DynamicProperty = dynamicProperty;
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
        [JsonProperty(PropertyName = "msgRateOut")]
        public double? MsgRateOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "bandwidthIn")]
        public double? BandwidthIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "bandwidthOut")]
        public double? BandwidthOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "memory")]
        public double? Memory { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "dynamic")]
        public bool? DynamicProperty { get; set; }

    }
}