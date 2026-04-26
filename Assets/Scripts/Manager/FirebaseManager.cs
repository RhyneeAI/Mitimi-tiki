using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    private FirebaseFirestore db;

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

        db = FirebaseFirestore.DefaultInstance;
    }

    public void SaveGameSession(GameSessionData data)
    {
        StartCoroutine(SaveGameSessionRoutine(data));
    }

    private IEnumerator SaveGameSessionRoutine(GameSessionData data)
    {
        Debug.Log($"[FirebaseManager] SaveGameSessionRoutine start. Questions: {data.questions?.Count ?? 0}");
        float timeoutSeconds = 10f;
        float timer = 0f;

        DocumentReference sessionRef = db.Collection("sessions").Document();

        Dictionary<string, object> sessionDoc = new Dictionary<string, object>
        {
            { "playerName",    data.playerName },
            { "pi",            data.pi },
            { "finalScore",    data.finalScore },
            { "finalLevel",    data.finalLevel },
            { "totalQuestion", data.totalQuestion },
            { "totalCorrect",  data.totalCorrect },
            { "playedAt",      data.playedAt }
        };

        var sessionTask = sessionRef.SetAsync(sessionDoc);

        // Tunggu sampai selesai ATAU timeout
        while (!sessionTask.IsCompleted && timer < timeoutSeconds)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!sessionTask.IsCompleted)
        {
            // Timeout pada simpan session
            Debug.LogError($"Gagal simpan session: TIMEOUT setelah {timeoutSeconds} detik");
            LoadingContext.NotifyFirebaseDone(); // lanjutkan loading, walau gagal simpan
            yield break;
        }

        if (sessionTask.Exception != null)
        {
            Debug.LogError("Gagal simpan session: " + sessionTask.Exception);
            LoadingContext.NotifyFirebaseDone();
            yield break;
        }

        // Reset timer untuk loop pertanyaan
        LoadingContext.NotifyFirebaseDone();
        timer = 0f;

        foreach (QuestionResult q in data.questions)
        {
            Dictionary<string, object> questionDoc = new Dictionary<string, object>
            {
                { "questionNumber", q.questionNumber },
                { "question",       q.question },
                { "answerA",        q.answerA },
                { "answerB",        q.answerB },
                { "answerC",        q.answerC },
                { "answerD",        q.answerD },
                { "correctAnswer",  q.correctAnswer },
                { "playerAnswer",   q.playerAnswer },
                { "pi",             q.pi },
                { "score",          q.score },
                { "level",          q.level },
                { "life",           q.life },
                { "time",           q.time },
                { "timeUsed",       q.timeUsed },
                { "isCorrect",      q.isCorrect },
                { "isTimeout",      q.isTimeout },
                { "created",        q.created }
            };

            var questionTask = sessionRef.Collection("questions").Document().SetAsync(questionDoc);

            timer = 0f;
            while (!questionTask.IsCompleted && timer < timeoutSeconds)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (!questionTask.IsCompleted)
            {
                Debug.LogError($"Gagal simpan question (TIMEOUT setelah {timeoutSeconds} detik)");
                // lanjut loop berikut (atau bisa juga break; kalau mau stop di sini)
                continue;
            }

            if (questionTask.Exception != null)
            {
                Debug.LogError("Gagal simpan question: " + questionTask.Exception);
            }
        }

        Debug.Log("Semua data sesi selesai diproses (berhasil/gagal/timeout).");

        // Beritahu LoadingManager bahwa Firebase sudah selesai (berapapun hasilnya)
        LoadingContext.NotifyFirebaseDone();
    }

    public void LoadLeaderboard(Action<List<RankingEntryData>> onComplete)
    {
        if (db == null)
        {
            Debug.LogError("FirebaseManager: Firestore db is NULL. Pastikan sudah diinisialisasi di Awake().");
            onComplete?.Invoke(new List<RankingEntryData>());
            return;
        }
        
        db.Collection("sessions")
          .OrderByDescending("pi")
          .OrderByDescending("playedAt")
          .Limit(50)
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              List<RankingEntryData> result = new List<RankingEntryData>();

              if (task.IsFaulted || task.IsCanceled)
              {
                  Debug.LogError("Failed to load leaderboard: " + task.Exception);
                  onComplete?.Invoke(result);
                  return;
              }

              QuerySnapshot snapshot = task.Result;

              foreach (DocumentSnapshot doc in snapshot.Documents)
              {
                  Dictionary<string, object> data = doc.ToDictionary();

                  RankingEntryData entry = new RankingEntryData
                  {
                      playerName = data.ContainsKey("playerName") ? data["playerName"].ToString() : "-",
                      playedAt = data.ContainsKey("playedAt") ? data["playedAt"].ToString() : "-",
                      finalLevel = data.ContainsKey("finalLevel") ? Convert.ToInt32(data["finalLevel"]) : 0,
                      totalQuestion = data.ContainsKey("totalQuestion") ? Convert.ToInt32(data["totalQuestion"]) : 0,
                      totalCorrect = data.ContainsKey("totalCorrect") ? Convert.ToInt32(data["totalCorrect"]) : 0,
                      finalScore = data.ContainsKey("finalScore") ? Convert.ToInt32(data["finalScore"]) : 0
                  };

                  result.Add(entry);
              }

              onComplete?.Invoke(result);
          });
    }
}