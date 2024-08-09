using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager instance;
    public static NetworkManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gameObj = new GameObject("NetworkManager");
                instance = gameObj.AddComponent<NetworkManager>();
                DontDestroyOnLoad(gameObj);
            }
            return instance;
        }
    }


    const string API_URL = "https://api-slidepuzzle.japaneast.cloudapp.azure.com/api/";

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

    public void LoadUser(Action<bool> result)
    {
        if (!File.Exists(Application.persistentDataPath + "/saveData.json"))
        {
            //登録
            StartCoroutine(RegistUser(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), result));
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

            StartCoroutine(LoginUser(userName, password, result));
        }
    }

    public IEnumerator LoadMasterData(Action<bool> response)
    {
        UnityWebRequest request = UnityWebRequest.Get(API_URL + "stages");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            stages = JsonConvert.DeserializeObject<Stage[]>(json);
        }
        response?.Invoke(request.result == UnityWebRequest.Result.Success);
    }

    private IEnumerator RegistUser(string name, string password, Action<bool> response)
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
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //帰ってきたIDを保存
            string result = request.downloadHandler.text;
            Debug.Log(result);
            AuthUserResponse authResp = JsonConvert.DeserializeObject<AuthUserResponse>(result);
            userID = authResp.id;
            userName = authResp.name;
            authToken = authResp.token;

            //次のログイン用に保存
            SaveUserData(userID, userName, password);
        }

        response?.Invoke(request.result == UnityWebRequest.Result.Success);
    }

    private IEnumerator LoginUser(string name, string password, Action<bool> response)
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
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //帰ってきたIDを保存
            string result = request.downloadHandler.text;
            AuthUserResponse authResp = JsonConvert.DeserializeObject<AuthUserResponse>(result);
            userID = authResp.id;
            authToken = authResp.token;
        }
        response?.Invoke(request.result == UnityWebRequest.Result.Success);
    }

    public IEnumerator GetUserStage(Action<UserStage[]> response)
    {
        UnityWebRequest request = UnityWebRequest.Get(API_URL + "stages/" + userID);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            response?.Invoke(JsonConvert.DeserializeObject<UserStage[]>(json));
        }
        else
        {
            response?.Invoke(null);
        }
    }

    public IEnumerator SendUserStage(int stageId, float clearTime, Action<bool> response)
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
        yield return request.SendWebRequest();

        response?.Invoke(request.result == UnityWebRequest.Result.Success);
    }

    public IEnumerator GetStageRecord(int stageId, Action<StageRecordResponse> response)
    {
        UnityWebRequest request = UnityWebRequest.Get(API_URL + "stages/record?user_id=" + userID + "&stage_id=" + stageId);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            response?.Invoke(JsonConvert.DeserializeObject<StageRecordResponse>(json));
        }
        else
        {
            response?.Invoke(null);
        }
    }
}

