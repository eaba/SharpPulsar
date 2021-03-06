﻿using System.Text.Json.Serialization;

namespace SharpPulsar.Presto
{
    public class Warning
    {
        [JsonPropertyName("warningCode")]
        public WarningCode WarningCode { get; set; }
        [JsonPropertyName("message")]
        public  string Message { get; set; }
    }
}
