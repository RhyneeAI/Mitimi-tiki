using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    [Header("Adaptive State")]
    [Range(0f, 1f)]
    public float performanceIndex = 0.10f;

    [Range(1, 11)]
    public int currentLevel = 1;
    private int level11LockQuestions = 5;
    private int level11QuestionsPlayed = 0;
    private bool isLevel11Locked = false;

    [Range(1, 11)]
    public int targetLevel = 1;

    [Header("Tuning")]
    public int evaluationWindow = 3;
    public float correctBaseGain = 0.015f;
    public float correctSpeedBonus = 0.025f;
    public float wrongPenalty = 0.045f;
    public float timeoutPenalty = 0.055f;

    private Queue<int> recentTargetLevels = new Queue<int>();

    public bool IsLevel11Locked => isLevel11Locked;
    public int Level11QuestionsPlayed => level11QuestionsPlayed;
    public int Level11LockQuestions => level11LockQuestions;

    public void ResetAI()
    {
        performanceIndex = 0.11f;
        currentLevel = 1;
        targetLevel = 1;
        recentTargetLevels.Clear();
    }

    public void RegisterResult(bool isCorrect, bool isTimeout, float timeUsed, float totalTime)
    {
        float safeTotalTime = Mathf.Max(0.1f, totalTime);
        float clampedTimeUsed = Mathf.Clamp(timeUsed, 0f, safeTotalTime);
        float speedRatio = Mathf.Clamp01(1f - (clampedTimeUsed / safeTotalTime));

        if (isCorrect)
        {
            float gain = correctBaseGain + (speedRatio * correctSpeedBonus);
            performanceIndex = Mathf.Clamp01(performanceIndex + gain);
        }
        else
        {
            if (!isTimeout)
            {
                performanceIndex = Mathf.Clamp01(performanceIndex - wrongPenalty);
            }
            // timeout penalty di-handle di ApplyTimeoutPenalty(level)
        }

        targetLevel = GetLevelFromPerformanceIndex(performanceIndex);
        PushTargetLevel(targetLevel);

        // Hitung berapa soal yang sudah dimainkan di level 11
        if (currentLevel == 11)
        {
            level11QuestionsPlayed++;

            // aktifkan lock jika baru saja masuk level 11
            if (!isLevel11Locked)
            {
                isLevel11Locked = true;
                level11QuestionsPlayed = 1; // soal pertama di level 11
            }
        }

        UpdateDisplayedLevel();
    }

    public void ApplyTimePenalty(float penaltyPercent)
    {
        // penaltyPercent = 0.05, 0.10, atau 0.15
        // mengurangi sebesar X% dari perolehan correctBaseGain + maxSpeedBonus saat itu
        float baseGain = correctBaseGain + correctSpeedBonus;
        float reduction = baseGain * penaltyPercent;
        performanceIndex = Mathf.Clamp01(performanceIndex - reduction);
    }

    public void ApplyTimeoutPenalty(int level)
    {
        float penalty = 0.02f * level;
        performanceIndex = Mathf.Clamp01(performanceIndex - penalty);

        targetLevel = GetLevelFromPerformanceIndex(performanceIndex);
        PushTargetLevel(targetLevel);
        UpdateDisplayedLevel();
    }

    int GetLevelFromPerformanceIndex(float pi)
    {
        if (pi < 0.16f) return 1;
        if (pi < 0.22f) return 2;
        if (pi < 0.29f) return 3;
        if (pi < 0.36f) return 4;
        if (pi < 0.43f) return 5;
        if (pi < 0.50f) return 6;
        if (pi < 0.58f) return 7;
        if (pi < 0.66f) return 8;
        if (pi < 0.76f) return 9;
        if (pi < 0.835f) return 10;
        return 11;
    }

    void PushTargetLevel(int level)
    {
        recentTargetLevels.Enqueue(level);

        while (recentTargetLevels.Count > evaluationWindow)
        {
            recentTargetLevels.Dequeue();
        }
    }

    private void UpdateDisplayedLevel()
    {
        if (recentTargetLevels.Count < evaluationWindow)
            return;

        int minLevel = int.MaxValue;
        int maxLevel = int.MinValue;
        int sum = 0;

        foreach (int lvl in recentTargetLevels)
        {
            if (lvl < minLevel) minLevel = lvl;
            if (lvl > maxLevel) maxLevel = lvl;
            sum += lvl;
        }

        int averageLevel = Mathf.RoundToInt(sum / (float)recentTargetLevels.Count);

        // Aturan naik/turun normal
        int newLevel = currentLevel;

        if (minLevel == maxLevel)
        {
            if (averageLevel > currentLevel)
                newLevel += 1;
            else if (averageLevel < currentLevel)
                newLevel -= 1;
        }
        else
        {
            if (averageLevel >= currentLevel + 2)
                newLevel += 1;
            else if (averageLevel <= currentLevel - 2)
                newLevel -= 1;
        }

        newLevel = Mathf.Clamp(newLevel, 1, 11);

        // LOGIKA LOCK LEVEL 11
        if (currentLevel == 11)
        {
            if (isLevel11Locked && level11QuestionsPlayed < level11LockQuestions)
            {
                // Selama lock aktif dan jumlah soal belum mencapai limit,
                // paksa tetap level 11, abaikan penurunan.
                newLevel = 11;
            }
            else
            {
                // Setelah lewat 5 soal, boleh turun lagi
                isLevel11Locked = false;
            }
        }

        currentLevel = newLevel;
    }

    public float GetSpeedRatio(float timeUsed, float totalTime)
    {
        float safeTotalTime = Mathf.Max(0.1f, totalTime);
        return Mathf.Clamp01(1f - (timeUsed / safeTotalTime));
    }
}