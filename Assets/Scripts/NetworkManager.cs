using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms.Impl;

public class NetworkManager
{
    const string API_URL = "https://api-slidepuzzle.japaneast.cloudapp.azure.com/api/";

    private static NetworkManager instance;
    public static NetworkManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new NetworkManager();
            }
            return instance;
        }
    }

    public Stage[] stages;
    private int userID;
    private string userName;
    private string authToken;

    private void SaveUserData(int userID, string userName, string password)
    {
        //データの保存
        UserSaveData userSaveData = new UserSaveData();
        userSaveData.UserID = userID;
        userSaveData.UserName = userName;
        userSaveData.password = password;
        string json = JsonConvert.SerializeObject(userSaveData);
        var Writer = new StreamWriter(Application.persistentDataPath + "/saveData.json");
        Writer.Write(json);
        Writer.Flush();
        Writer.Close();
    }

    public async void LoadUser()
    {
        if (!File.Exists(Application.persistentDataPath + "/saveData.json"))
        {
            //登録
            await RegistUser(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }
        else
        {
            //ログイン
            var reader = new StreamReader(Application.persistentDataPath + "/saveData.json");
            string json = reader.ReadToEnd();
            reader.Close();
            UserSaveData userSaveData = JsonConvert.DeserializeObject<UserSaveData>(json);
            userID = userSaveData.UserID;
            userName = userSaveData.UserName;
            string password = userSaveData.password;

            await LoginUser(userName, password);
        }
    }

    public async void LoadMasterData()
    {
        UnityWebRequest request = UnityWebRequest.Get(API_URL + "stages");
        await request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            stages = JsonConvert.DeserializeObject<Stage[]>(json);
        }
    }

    private async Task RegistUser(string name, string password)
    {
        var requestData = new
        {
            name = name,
            level = 1,
            exp = 0,
            life = 0,
            password = password
        };
        string json = JsonConvert.SerializeObject(requestData);
        UnityWebRequest request = UnityWebRequest.Post(
            API_URL + "register",
            json,
            "application/json");
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //帰ってきたIDを保存
            string result = request.downloadHandler.text;
            Debug.Log(result);
            AuthUserResponse response = JsonConvert.DeserializeObject<AuthUserResponse>(result);
            userID = response.id;
            userName = response.name;
            authToken = response.token;

            //次のログイン用に保存
            SaveUserData(userID, userName, password);
        }
    }

    private async Task LoginUser(string name, string password)
    {
        var requestData = new
        {
            name = name,
            password = password
        };
        string json = JsonConvert.SerializeObject(requestData);
        UnityWebRequest request = UnityWebRequest.Post(
            API_URL + "login",
            json,
            "application/json");
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //帰ってきたIDを保存
            string result = request.downloadHandler.text;
            AuthUserResponse response = JsonConvert.DeserializeObject<AuthUserResponse>(result);
            userID = response.id;
            authToken = response.token;
        }
    }

    public async Task<UserStage[]> GetUserStage()
    {
        UnityWebRequest request = UnityWebRequest.Get(API_URL + "stages/" + userID);
        await request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            return JsonConvert.DeserializeObject<UserStage[]>(json);
        }
        return null;
    }

    public async Task SendUserStage(int stageId, float clearTime)
    {
        var requestData = new
        {
            user_id = userID,
            stage_id = stageId,
            time = clearTime
        };
        string json = JsonConvert.SerializeObject(requestData);
        UnityWebRequest request = UnityWebRequest.Post(
            API_URL + "stages/store",
            json,
            "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + authToken);
        await request.SendWebRequest();
    }

    public async Task<StageRecordResponse> GetStageRecord(int stageId)
    {
        UnityWebRequest request = UnityWebRequest.Get(API_URL + "stages/record?user_id=" + userID + "&stage_id=" + stageId);
        await request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            return JsonConvert.DeserializeObject<StageRecordResponse>(json);
        }
        return null;
    }
}