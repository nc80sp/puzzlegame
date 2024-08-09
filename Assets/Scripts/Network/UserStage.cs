using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserStage
{
    public int Id { get; set; }
    [JsonProperty("user_id")]
    public int UserID { get; set; }
    [JsonProperty("stage_id")]
    public int StageID { get; set; }
    [JsonProperty("clear_count")]
    public int ClearCount { get; set; }
    [JsonProperty("best_time")]
    public float BestTime { get; set; }

}
