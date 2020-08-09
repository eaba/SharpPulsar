﻿using Newtonsoft.Json;

namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    public class Provider
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = "azure-dns";

        [JsonProperty(PropertyName = "azuredns")]
        public AzureDns AzureDns { get; set; }
    }
}