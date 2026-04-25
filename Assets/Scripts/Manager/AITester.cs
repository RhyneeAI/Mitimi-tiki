using System.Collections;
using UnityEngine;

public class AITester : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public AIManager aiManager;

    [Header("Test Config")]
    public bool autoRunOnStart = false;
    public int maxQuestions = 200;

    [Header("Skill / Accuracy")]
    [Range(0f, 1f)]
    public float lowLevelAccuracy = 0.85f;   // level rendah
    [Range(0f, 1f)]
    public float highLevelAccuracy = 0.70f;  // level tinggi
    public int highLevelThreshold = 8;       // mulai level berapa dianggap tinggi

    [Header("Speed (Time Ratio)")]
    [Tooltip("Min ratio waktu dipakai di level rendah")]
    [Range(0f, 1f)]
    public float lowLevelMinTimeRatio = 0.3f;   // 30% waktu

    [Tooltip("Max ratio waktu dipakai di level rendah")]
    [Range(0f, 1f)]
    public float lowLevelMaxTimeRatio = 0.6f;   // 60% waktu

    [Tooltip("Min ratio waktu dipakai di level tinggi")]
    [Range(0f, 1f)]
    public float highLevelMinTimeRatio = 0.5f;  // 50% waktu

    [Tooltip("Max ratio waktu dipakai di level tinggi")]
    [Range(0f, 1f)]
    public float highLevelMaxTimeRatio = 0.9f;  // 90% waktu

    void Start()
    {
        if (autoRunOnStart)
        {
            StartCoroutine(RunSimulationPlay());
        }
    }

    public void StartTest()
    {
        StartCoroutine(RunSimulationPlay());
    }

    IEnumerator RunSimulationPlay()
    {
        if (aiManager != null)
        {
            aiManager.ResetAI();
        }

        int questionCount = 0;

        // tunggu soal pertama aktif
        while (!gameManager.IsQuestionActive())
            yield return null;

        while (questionCount < maxQuestions && !gameManager.IsGameOver())
        {
            int level = aiManager != null ? aiManager.currentLevel : 1;
            float totalTime = GetTimerByLevel(level);

            // pilih accuracy & time range berdasar level
            float acc;
            float minR, maxR;

            if (level < highLevelThreshold)
            {
                acc = lowLevelAccuracy;
                minR = lowLevelMinTimeRatio;
                maxR = lowLevelMaxTimeRatio;
            }
            else
            {
                acc = highLevelAccuracy;
                minR = highLevelMinTimeRatio;
                maxR = highLevelMaxTimeRatio;
            }

            acc = Mathf.Clamp01(acc);
            minR = Mathf.Clamp01(minR);
            maxR = Mathf.Clamp(maxR, minR + 0.01f, 1f); // pastikan max > min

            // tentukan kapan AI menjawab (dalam detik)
            float timeRatio = Random.Range(minR, maxR);
            float timeToWait = totalTime * timeRatio;

            float t = 0f;
            while (t < timeToWait && gameManager.IsQuestionActive() && !gameManager.IsGameOver())
            {
                t += Time.deltaTime;
                yield return null;
            }

            if (gameManager.IsGameOver())
                break;

            if (!gameManager.IsQuestionActive())
            {
                // soal sudah berakhir (timeout atau next question), tunggu soal berikutnya
                while (!gameManager.IsQuestionActive() && !gameManager.IsGameOver())
                    yield return null;
                continue;
            }

            // tentukan apakah akan menjawab benar
            bool willAnswerCorrect = Random.value < acc;

            int correctIndex = gameManager.GetCorrectIndex();
            int chosenIndex;

            if (willAnswerCorrect)
            {
                chosenIndex = correctIndex;
            }
            else
            {
                // pilih index lain
                int idx;
                do
                {
                    idx = Random.Range(0, 4);
                } while (idx == correctIndex);
                chosenIndex = idx;
            }

            // klik jawaban
            gameManager.SelectAnswer(chosenIndex);
            questionCount++;

            Debug.Log($"[AI Tester] Q{questionCount} | Level: {level} | " +
                      $"Chosen: {chosenIndex}, Correct: {correctIndex}, " +
                      $"AccUsed: {acc:F2}, TimeRatio: {timeRatio:F2}");

            // tunggu soal berikutnya
            while (!gameManager.IsQuestionActive() && !gameManager.IsGameOver())
                yield return null;
        }

        Debug.Log($"[AI Tester] Selesai. Total soal: {questionCount}, Final Level: {(aiManager != null ? aiManager.currentLevel : -1)}");
    }

    float GetTimerByLevel(int levelValue)
    {
        if (levelValue <= 0) levelValue = 1;
        float value = 15f - ((levelValue - 1) * 0.5f);
        return Mathf.Clamp(value, 10f, 15f);
    }
}