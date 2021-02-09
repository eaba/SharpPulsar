// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace PulsarAdmin.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using SharpPulsar.Common;

    /// <summary>
    /// The configuration data for a cluster
    /// </summary>
    public partial class ClusterData
    {
        /// <summary>
        /// Initializes a new instance of the ClusterData class.
        /// </summary>
        public ClusterData()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ClusterData class.
        /// </summary>
        /// <param name="serviceUrl">The HTTP rest service URL (for admin
        /// operations)
        /// </param>
        public ClusterData(string serviceUrl)
        {
            ServiceUrl = serviceUrl;
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ClusterData class.
        /// </summary>
        /// <param name="serviceUrl">The HTTP rest service URL (for admin
        /// operations)</param>
        /// <param name="serviceUrlTls">The HTTPS rest service URL (for admin
        /// operations)</param>
        public ClusterData(string serviceUrl, string serviceUrlTls)
        {
            ServiceUrl = serviceUrl;
            ServiceUrlTls = serviceUrlTls;
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ClusterData class.
        /// </summary>
        /// <param name="serviceUrl">The HTTP rest service URL (for admin
        /// operations)</param>
        /// <param name="serviceUrlTls">The HTTPS rest service URL (for admin
        /// operations)</param>
        /// <param name="brokerServiceUrl">The broker service url (for produce
        /// and consume operations)</param>
        /// <param name="brokerServiceUrlTls">The secured broker service url
        /// (for produce and consume operations)</param>
        public ClusterData(string serviceUrl, string serviceUrlTls, string brokerServiceUrl, string brokerServiceUrlTls)
        {
            ServiceUrl = serviceUrl;
            ServiceUrlTls = serviceUrlTls;
            BrokerServiceUrl = brokerServiceUrl;
            BrokerServiceUrlTls = brokerServiceUrlTls;
            CustomInit();
        }
        /// <summary>
        /// Initializes a new instance of the ClusterData class.
        /// </summary>
        /// <param name="serviceUrl">The HTTP rest service URL (for admin
        /// operations)</param>
        /// <param name="serviceUrlTls">The HTTPS rest service URL (for admin
        /// operations)</param>
        /// <param name="brokerServiceUrl">The broker service url (for produce
        /// and consume operations)</param>
        /// <param name="brokerServiceUrlTls">The secured broker service url
        /// (for produce and consume operations)</param>
        /// <param name="peerClusterNames">A set of peer cluster names</param>
        /// <param name="proxyServiceUrl">Proxy-service url when client would like to connect to broker via proxy.", example = "pulsar+ssl://ats-proxy.example.com:4443 or " + "pulsar://ats-proxy.example.com:4080")</param>
        /// <param name="proxyProtocol">protocol to decide type of proxy routing eg: SNI-routing", example = "SNI")</param>
        public ClusterData(string serviceUrl, string serviceUrlTls, string brokerServiceUrl, string brokerServiceUrlTls, IList<string> peerClusterNames, string proxyServiceUrl, ProxyProtocol proxyProtocol)
        {
            ServiceUrl = serviceUrl;
            ServiceUrlTls = serviceUrlTls;
            BrokerServiceUrl = brokerServiceUrl;
            BrokerServiceUrlTls = brokerServiceUrlTls;
            PeerClusterNames = peerClusterNames;
            ProxyServiceUrl = proxyServiceUrl;
            ProxyProtocol = proxyProtocol;
            CustomInit();
        }
        

        /// <summary>
        /// Initializes a new instance of the ClusterData class.
        /// </summary>
        /// <param name="serviceUrl">The HTTP rest service URL (for admin
        /// operations)</param>
        /// <param name="serviceUrlTls">The HTTPS rest service URL (for admin
        /// operations)</param>
        /// <param name="brokerServiceUrl">The broker service url (for produce
        /// and consume operations)</param>
        /// <param name="brokerServiceUrlTls">The secured broker service url
        /// (for produce and consume operations)</param>
        /// <param name="peerClusterNames">A set of peer cluster names</param>
        public ClusterData(string serviceUrl, string serviceUrlTls, string brokerServiceUrl, string brokerServiceUrlTls, IList<string> peerClusterNames)
        {
            ServiceUrl = serviceUrl;
            ServiceUrlTls = serviceUrlTls;
            BrokerServiceUrl = brokerServiceUrl;
            BrokerServiceUrlTls = brokerServiceUrlTls;
            PeerClusterNames = peerClusterNames;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the HTTP rest service URL (for admin operations)
        /// </summary>
        [JsonProperty(PropertyName = "serviceUrl")]
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the HTTPS rest service URL (for admin operations)
        /// </summary>
        [JsonProperty(PropertyName = "serviceUrlTls")]
        public string ServiceUrlTls { get; set; }

        /// <summary>
        /// Gets or sets the broker service url (for produce and consume
        /// operations)
        /// </summary>
        [JsonProperty(PropertyName = "brokerServiceUrl")]
        public string BrokerServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the secured broker service url (for produce and
        /// consume operations)
        /// </summary>
        [JsonProperty(PropertyName = "brokerServiceUrlTls")]
        public string BrokerServiceUrlTls { get; set; }

        /// <summary>
        /// Gets or sets a set of peer cluster names
        /// </summary>
        [JsonProperty(PropertyName = "peerClusterNames")]
        public IList<string> PeerClusterNames { get; set; }

        /// <summary>
        /// Gets or sets proxy service url
        /// </summary>
        [JsonProperty(PropertyName = "proxyServiceUrl")]
        public string ProxyServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the proxyProtocol type
        /// </summary>
        [JsonProperty(PropertyName = "proxyProtocol")]
        public ProxyProtocol ProxyProtocol { get; set; }

    }
}
