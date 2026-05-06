using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// #if !UNITY_WEBGL || UNITY_EDITOR
// using Firebase;
// using Firebase.Extensions;
// using Firebase.Firestore;
// #endif

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    [SerializeField] private string apiBaseUrl = "https://mitimi-tiki2d-api.vercel.app";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void SaveGameSession(GameSessionData data)
    {
        StartCoroutine(SaveGameSessionWebRoutine(data));
    }

    public void LoadLeaderboard(Action<List<RankingEntryData>> onComplete)
    {
        StartCoroutine(LoadLeaderboardWebRoutine(onComplete));
    }

    private IEnumerator SaveGameSessionWebRoutine(GameSessionData data)
    {
        string url = apiBaseUrl + "/api/play";
        string json = JsonUtility.ToJson(data);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 15;

        yield return request.SendWebRequest();

        #if UNITY_2020_1_OR_NEWER
            bool hasError = request.result == UnityWebRequest.Result.ConnectionError ||
                            request.result == UnityWebRequest.Result.ProtocolError;
        #else
            bool hasError = request.isNetworkError || request.isHttpError;
        #endif

        if (hasError)
        {
            Debug.LogError("[WebAPI] SaveGameSession failed: " + request.error);
            Debug.LogError("[WebAPI] Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("[WebAPI] SaveGameSession success: " + request.downloadHandler.text);
        }

        LoadingContext.NotifyFirebaseDone();
    }

    private IEnumerator LoadLeaderboardWebRoutine(Action<List<RankingEntryData>> onComplete)
    {
        string url = apiBaseUrl + "/api/leaderboard";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = 15;

        yield return request.SendWebRequest();

        #if UNITY_2020_1_OR_NEWER
                bool hasError = request.result == UnityWebRequest.Result.ConnectionError ||
                                request.result == UnityWebRequest.Result.ProtocolError;
        #else
                bool hasError = request.isNetworkError || request.isHttpError;
        #endif

        if (hasError)
        {
            Debug.LogError("[WebAPI] LoadLeaderboard failed: " + request.error);
            Debug.LogError("[WebAPI] Response: " + request.downloadHandler.text);
            onComplete?.Invoke(new List<RankingEntryData>());
            yield break;
        }

        string json = request.downloadHandler.text;
        LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(json);

        if (response != null && response.entries != null)
            onComplete?.Invoke(response.entries);
        else
            onComplete?.Invoke(new List<RankingEntryData>());
    }
}