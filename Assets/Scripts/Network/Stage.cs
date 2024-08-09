using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage
{
    public int Id { get; set; }
    public string Name { get; set; }

    [JsonProperty("limit_time")]
    public float LimitTime { get; set; }

    [JsonProperty("shuffle_count")]
    public int ShuffleCount { get; set; }
}
