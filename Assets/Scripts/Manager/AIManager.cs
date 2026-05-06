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
    public float correctBaseGain   = 0.012f;
    public float correctSpeedBonus = 0.022f;
    public float wrongPenalty      = 0.055f;
    public float timeoutPenalty    = 0.065f;
    public float piCap             = 0.97f;

    private Queue<int> recentTargetLevels = new Queue<int>();

    public bool IsLevel11Locked       => isLevel11Locked;
    public int  Level11QuestionsPlayed => level11QuestionsPlayed;
    public int  Level11LockQuestions   => level11LockQuestions;

    // ─────────────────────────────────────────────
    // RESET
    // ─────────────────────────────────────────────
    public void ResetAI()
    {
        performanceIndex = 0.11f;
        currentLevel     = 1;
        targetLevel      = 1;
        level11QuestionsPlayed = 0;
        isLevel11Locked  = false;
        recentTargetLevels.Clear();
    }

    // ─────────────────────────────────────────────
    // REGISTER HASIL SOAL
    // ─────────────────────────────────────────────
    public void RegisterResult(bool isCorrect, bool isTimeout, float timeUsed, float totalTime)
    {
        float safeTotalTime   = Mathf.Max(0.1f, totalTime);
        float clampedTimeUsed = Mathf.Clamp(timeUsed, 0f, safeTotalTime);
        float speedRatio      = Mathf.Clamp01(1f - (clampedTimeUsed / safeTotalTime));

        if (isCorrect)
        {
            float gain = correctBaseGain + (speedRatio * correctSpeedBonus);
            // Pakai piCap agar PI tidak melewati batas maksimum
            performanceIndex = Mathf.Clamp(performanceIndex + gain, 0f, piCap);
        }
        else
        {
            if (!isTimeout)
            {
                // Penalti jawaban salah
                performanceIndex = Mathf.Clamp01(performanceIndex - wrongPenalty);
            }
            // Penalti timeout di-handle di ApplyTimeoutPenalty()
        }

        targetLevel = GetLevelFromPerformanceIndex(performanceIndex);
        PushTargetLevel(targetLevel);

        // Hitung soal di level 11
        if (currentLevel == 11)
        {
            level11QuestionsPlayed++;

            if (!isLevel11Locked)
            {
                isLevel11Locked        = true;
                level11QuestionsPlayed = 1; // soal pertama di level 11
            }
        }

        UpdateDisplayedLevel();
    }

    // ─────────────────────────────────────────────
    // PENALTI WAKTU (jawab benar tapi terlalu lambat)
    // ─────────────────────────────────────────────
    public void ApplyTimePenalty(float penaltyPercent)
    {
        // penaltyPercent: 0.05 atau 0.10
        // Penalty = correctBaseGain * (penaltyPercent * 10)
        // Contoh: 0.10 → 0.012 * 1.0 = 0.012 (menghapus hampir seluruh base gain)
        float penalty = correctBaseGain * (penaltyPercent * 10f);
        performanceIndex = Mathf.Clamp01(performanceIndex - penalty);
    }

    // ─────────────────────────────────────────────
    // PENALTI TIMEOUT (waktu habis)
    // ─────────────────────────────────────────────
    public void ApplyTimeoutPenalty(int level)
    {
        // Semakin tinggi level, semakin besar penalti
        // Level 1  → 0.065 * 1.05 = 0.068
        // Level 5  → 0.065 * 1.25 = 0.081
        // Level 11 → 0.065 * 1.55 = 0.101
        float penalty = timeoutPenalty * (1f + (level * 0.05f));
        performanceIndex = Mathf.Clamp01(performanceIndex - penalty);

        targetLevel = GetLevelFromPerformanceIndex(performanceIndex);
        PushTargetLevel(targetLevel);
        UpdateDisplayedLevel();
    }

    // ─────────────────────────────────────────────
    // LEVEL DARI PI
    // ─────────────────────────────────────────────
    int GetLevelFromPerformanceIndex(float pi)
    {
        if (pi < 0.16f) return 1;
        if (pi < 0.22f) return 2;
        if (pi < 0.29f) return 3;
        if (pi < 0.36f) return 4;
        if (pi < 0.44f) return 5;
        if (pi < 0.52f) return 6;
        if (pi < 0.59f) return 7;
        if (pi < 0.67f) return 8;
        if (pi < 0.76f) return 9;
        if (pi < 0.83f) return 10;
        return 11;
    }

    // ─────────────────────────────────────────────
    // QUEUE TARGET LEVEL (untuk smoothing)
    // ─────────────────────────────────────────────
    void PushTargetLevel(int level)
    {
        recentTargetLevels.Enqueue(level);

        while (recentTargetLevels.Count > evaluationWindow)
            recentTargetLevels.Dequeue();
    }

    // ─────────────────────────────────────────────
    // UPDATE LEVEL YANG DITAMPILKAN (gradual)
    // ─────────────────────────────────────────────
    private void UpdateDisplayedLevel()
    {
        if (recentTargetLevels.Count < evaluationWindow)
            return;

        int minLevel = int.MaxValue;
        int maxLevel = int.MinValue;
        int sum      = 0;

        foreach (int lvl in recentTargetLevels)
        {
            if (lvl < minLevel) minLevel = lvl;
            if (lvl > maxLevel) maxLevel = lvl;
            sum += lvl;
        }

        int averageLevel = Mathf.RoundToInt(sum / (float)recentTargetLevels.Count);
        int newLevel     = currentLevel;

        if (minLevel == maxLevel)
        {
            // Semua target di queue sama → naikkan/turunkan 1 langkah
            if (averageLevel > currentLevel)       newLevel += 1;
            else if (averageLevel < currentLevel)  newLevel -= 1;
        }
        else
        {
            // Ada variasi → butuh selisih 2 sebelum berubah
            if (averageLevel >= currentLevel + 2)      newLevel += 1;
            else if (averageLevel <= currentLevel - 2) newLevel -= 1;
        }

        newLevel = Mathf.Clamp(newLevel, 1, 11);

        // LOGIKA LOCK LEVEL 11
        if (currentLevel == 11)
        {
            if (isLevel11Locked && level11QuestionsPlayed < level11LockQuestions)
            {
                // Paksa tetap level 11 selama lock aktif
                newLevel = 11;
            }
            else
            {
                // Lewati batas soal → boleh turun
                isLevel11Locked = false;
            }
        }

        currentLevel = newLevel;
    }

    // ─────────────────────────────────────────────
    // HELPER
    // ─────────────────────────────────────────────
    public float GetSpeedRatio(float timeUsed, float totalTime)
    {
        float safeTotalTime = Mathf.Max(0.1f, totalTime);
        return Mathf.Clamp01(1f - (timeUsed / safeTotalTime));
    }
}