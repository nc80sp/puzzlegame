using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageRecordResponse
{
    [JsonProperty("my_record")]
    public float MyRecord;
    [JsonProperty("world_record")]
    public float WorldRecord;
}
