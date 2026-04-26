using System.Collections;
using System.Collections.Generic;
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
        DocumentReference sessionRef = db.Collection("sessions").Document();

        Dictionary<string, object> sessionDoc = new Dictionary<string, object>
        {
            { "playerName", data.playerName },
            { "pi", data.pi },
            { "finalScore", data.finalScore },
            { "finalLevel", data.finalLevel },
            { "totalQuestion", data.totalQuestion },
            { "totalCorrect", data.totalCorrect },
            { "playedAt", data.playedAt }
        };

        var sessionTask = sessionRef.SetAsync(sessionDoc);
        yield return new WaitUntil(() => sessionTask.IsCompleted);

        if (sessionTask.Exception != null)
        {
            Debug.LogError("Gagal simpan session: " + sessionTask.Exception);
            yield break;
        }

        foreach (QuestionResult q in data.questions)
        {
            Dictionary<string, object> questionDoc = new Dictionary<string, object>
            {
                { "questionNumber", q.questionNumber },
                { "question", q.question },
                { "answerA", q.answerA },
                { "answerB", q.answerB },
                { "answerC", q.answerC },
                { "answerD", q.answerD },
                { "correctAnswer", q.correctAnswer },
                { "playerAnswer", q.playerAnswer },
                { "pi", q.pi },
                { "score", q.score },
                { "level", q.level },
                { "life", q.life },
                { "time", q.time },
                { "timeUsed", q.timeUsed },
                { "isCorrect", q.isCorrect },
                { "isTimeout", q.isTimeout },
                { "created", q.created }
            };

            var questionTask = sessionRef.Collection("questions").Document().SetAsync(questionDoc);
            yield return new WaitUntil(() => questionTask.IsCompleted);

            if (questionTask.Exception != null)
            {
                Debug.LogError("Gagal simpan question: " + questionTask.Exception);
            }
        }

        Debug.Log("Semua data sesi berhasil disimpan");
    }
}