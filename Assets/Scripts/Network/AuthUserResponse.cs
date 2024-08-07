using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthUserResponse
{
    [JsonProperty("token")]
    public string token;
    public int id;
    public string name;
}
