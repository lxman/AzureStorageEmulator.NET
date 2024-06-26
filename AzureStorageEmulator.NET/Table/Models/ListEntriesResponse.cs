﻿using System.Text.Json.Serialization;

namespace AzureStorageEmulator.NET.Table.Models
{
    public class ListEntriesResponse
    {
        [JsonPropertyName("odata.metadata")]
        public string Metadata { get; set; }

        [JsonPropertyName("value")]
        public List<Dictionary<string, object>> Objects { get; set; }
    }
}