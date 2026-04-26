using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private List<QuestionResult> questionHistory = new List<QuestionResult>();

    [Header("UI")]
    public PlayerNameInput playerNameInput; 
    public TMP_Text question;
    public TMP_Text score;
    public TMP_Text[] level;
    public GameObject[] levelIcon;
    public TMP_Text health;
    public TMP_Text timer;
    public TMP_Text operand1;
    public TMP_Text _operator;
    public TMP_Text operand2;
    public TMP_Text answerA;
    public GameObject[] resultA; // [0] Correct, [1] Wrong
    public TMP_Text answerB;
    public GameObject[] resultB; // [0] Correct, [1] Wrong
    public TMP_Text answerC;
    public GameObject[] resultC; // [0] Correct, [1] Wrong
    public TMP_Text answerD;
    public GameObject[] resultD; // [0] Correct, [1] Wrong

    [Header("Manager")]
    public AIManager aiManager;
    public UIManager uiManager;
    public FirebaseManager firebaseManager;

    [Header("Panel")]
    public string homeSceneName = "Home";
    public GameObject confirmPanel;
    public GameObject gameOverPanel;
    public GameObject startGamePanel;

    [Header("Game State")]
    public int currentScore = 0;
    public int currentLevel = 1;
    public int currentHealth = 5;

    private float currentTimer;
    private float totalTimerForQuestion;
    private bool questionActive = false;
    private bool canAnswer = false;

    private int correctAnswer;
    public int totalQuestionCount;
    public int totalCorrectAnswer;
    private int correctIndex;
    private int currentOperand1;
    private int currentOperand2;
    private string currentOperatorSymbol;

    private int currentAnswerA;
    private int currentAnswerB;
    private int currentAnswerC;
    private int currentAnswerD;

    [Header("Game State for Limitations")]
    private int level10QuestionCount = 0;
    private int level11QuestionCount = 0;
    private bool isGamePaused = false;
    private bool isSessionFinished = false;

    private List<TMP_Text> answerTexts;
    private List<GameObject[]> resultObjects;

    private readonly Color colorNormal  = new Color(56f/255f, 53f/255f, 56f/255f);
    private readonly Color colorWarning = new Color(1f, 0.75f, 0f);   // kuning/orange
    private readonly Color colorDanger  = new Color(1f, 0.2f, 0.2f);  // merah

    public int GetCorrectIndex() => correctIndex;
    public bool IsQuestionActive()
    {
        return questionActive;
    }

    public bool IsGameOver()
    {
        return currentHealth <= 0;
    }

    private void SaveQuestionResult(int playerAnswer, bool isCorrect, bool isTimeout, float timeUsed)
    {
        QuestionResult qr = new QuestionResult
        {
            questionNumber = totalQuestionCount,
            question = currentOperand1 + " " + currentOperatorSymbol + " " + currentOperand2,

            answerA = currentAnswerA,
            answerB = currentAnswerB,
            answerC = currentAnswerC,
            answerD = currentAnswerD,

            correctAnswer = correctAnswer,
            playerAnswer = playerAnswer,

            pi = aiManager.performanceIndex,
            score = currentScore,
            level = currentLevel,
            life = currentHealth,

            time = totalTimerForQuestion,
            timeUsed = timeUsed,

            isCorrect = isCorrect,
            isTimeout = isTimeout,

            created = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        questionHistory.Add(qr);
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (!questionActive) return;

        currentTimer -= Time.deltaTime;
        currentTimer = Mathf.Max(0f, currentTimer);
        timer.text = currentTimer.ToString("F1");

        float timerRatio = currentTimer / totalTimerForQuestion;

        if (currentLevel == 11)
        {
            timer.color = colorDanger;
        }
        else
        {
            if (timerRatio <= 0.10f)
                timer.color = colorDanger;
            else if (timerRatio <= 0.3f)
                timer.color = colorWarning;
            else
                timer.color = colorNormal;
        }

        if (currentTimer <= 0f && canAnswer)
        {
            HandleTimeout();
        }
    }

    public void OnClickStartGame()
    {
        if (!playerNameInput.TrySubmit())
            return; // validasi gagal, pesan sudah tampil di InformationMessage

        uiManager.HidePanel(startGamePanel);

        // Nama valid, lanjut mulai game
        answerTexts = new List<TMP_Text> { answerA, answerB, answerC, answerD };
        resultObjects = new List<GameObject[]> { resultA, resultB, resultC, resultD };

        aiManager.ResetAI();
        currentLevel = aiManager.currentLevel;

        UpdateHUD();
        GenerateNewQuestion();

        Debug.Log("Playername : " + PlayerManager.GetPlayerName());
    }

    public void OnClickHome()
    {
        PauseGame();

        if (uiManager != null)
        {
            uiManager.ShowPanel(confirmPanel);
        }
    }

    public void PauseGame()
    {
        if (isGamePaused) return;

        isGamePaused = true;
        canAnswer = false;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (!isGamePaused) return;

        isGamePaused = false;
        canAnswer = true;
        Time.timeScale = 1f;
    }

    public void SelectAnswer(int answerIndex)
    {
        if (!canAnswer) return;
        if (answerIndex < 0 || answerIndex >= 4) return;

        canAnswer = false;
        questionActive = false;

        int chosenValue = int.Parse(answerTexts[answerIndex].text);
        bool isCorrect = chosenValue == correctAnswer;

        float timeUsed = totalTimerForQuestion - currentTimer;
        float timeRatio = timeUsed / totalTimerForQuestion; // rasio waktu terpakai

        ResetResultIcons();

        if (isCorrect)
        {
            resultObjects[answerIndex][0].SetActive(true);

            int gainedScore = CalculateScore(currentTimer, currentLevel);
            currentScore += gainedScore;
            totalCorrectAnswer += 1;

            // Kirim dulu hasil benar ke AIManager
            aiManager.RegisterResult(true, false, timeUsed, totalTimerForQuestion);

            // Baru terapkan penalti waktu jika terlalu lama
            // timeRatio = waktu terpakai / total, jadi sisa waktu = 1 - timeRatio
            float remainingRatio = 1f - timeRatio;

            if (remainingRatio <= 0.10f)        // sisa <= 10% dari total
                aiManager.ApplyTimePenalty(0.1f);
            else if (remainingRatio <= 0.3f)   // sisa <= 25           %
                aiManager.ApplyTimePenalty(0.05f);
        }
        else
        {
            resultObjects[answerIndex][1].SetActive(true);
            resultObjects[correctIndex][0].SetActive(true);

            currentHealth -= 1;
            currentHealth = Mathf.Max(0, currentHealth);

            aiManager.RegisterResult(false, false, timeUsed, totalTimerForQuestion);
        }

        SaveQuestionResult(chosenValue, isCorrect, false, timeUsed);

        currentLevel = aiManager.currentLevel;
        UpdateHUD();

        if (currentHealth <= 0)
        {
            StartCoroutine(GameOverRoutine());
            return;
        }

        Debug.Log($"Corr answer : {totalCorrectAnswer}, PI : {aiManager.performanceIndex}");

        StartCoroutine(NextQuestionRoutine());
    }

    void HandleTimeout()
    {
        canAnswer = false;
        questionActive = false;

        ResetResultIcons();
        resultObjects[correctIndex][0].SetActive(true);

        currentHealth -= 1;
        currentHealth = Mathf.Max(0, currentHealth);

        float timeUsed = totalTimerForQuestion;

        aiManager.RegisterResult(false, true, timeUsed, totalTimerForQuestion);
        aiManager.ApplyTimeoutPenalty(currentLevel);

        SaveQuestionResult(-1, false, true, timeUsed);

        currentLevel = aiManager.currentLevel;
        UpdateHUD();

        if (currentHealth <= 0)
        {
            StartCoroutine(GameOverRoutine());
            return;
        }

        StartCoroutine(NextQuestionRoutine());
    }

    IEnumerator NextQuestionRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        GenerateNewQuestion();
    }

    IEnumerator GameOverRoutine()
    {
        canAnswer = false;
        questionActive = false;
        Time.timeScale = 0f;

        if (uiManager != null)
            uiManager.ShowPanel(gameOverPanel);

        // Siapkan data session
        GameSessionData sessionData = new GameSessionData
        {
            playerName    = PlayerManager.GetPlayerName(),
            pi            = aiManager.performanceIndex,
            finalScore    = currentScore,
            finalLevel    = currentLevel,
            totalQuestion = totalQuestionCount,
            totalCorrect  = totalCorrectAnswer,
            playedAt      = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            questions     = new List<QuestionResult>(questionHistory)
        };

        // Simpan untuk Result panel di Home
        SessionResultData.SetResult(
            currentScore,
            currentLevel,
            totalQuestionCount,
            totalCorrectAnswer,
            aiManager.performanceIndex,
            questionHistory
        );

        // Beritahu LoadingContext bahwa kita perlu tunggu Firebase
        LoadingContext.PrepareLoad("Home", true);

        // Simpan ke Firebase (async, akan panggil NotifyFirebaseDone() setelah selesai)
        if (firebaseManager != null)
            firebaseManager.SaveGameSession(sessionData);

        // Tampilkan game over panel sebentar
        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = 1f;

        // Pindah ke Loading Scene
        if (uiManager != null)
            uiManager.LoadScene("Loading");
    }

    void GenerateNewQuestion()
    {
        if (isSessionFinished) return;

        if (currentLevel == 10 && level10QuestionCount >= 10)
        {
            Debug.Log("Level 10 limit reached.");
            isSessionFinished = true;

            StartCoroutine(GameOverRoutine());
            return;
        }

        if (currentLevel == 11 && level11QuestionCount >= 5)
        {
            Debug.Log("Level 11 limit reached.");
            isSessionFinished = true;

            StartCoroutine(GameOverRoutine());
            return;
        }

        if (currentLevel == 10)
        {
            level10QuestionCount++;
        }

        if (currentLevel == 11)
        {
            level11QuestionCount++;
        }
        
        ResetResultIcons();
        OperationType opType = GetOperationForLevel(currentLevel);
        GenerateOperands(opType, currentLevel, out currentOperand1, out currentOperand2, out correctAnswer, out currentOperatorSymbol);

        operand1.text = currentOperand1.ToString();
        operand2.text = currentOperand2.ToString();
        _operator.text = currentOperatorSymbol;

        List<int> choices = GenerateChoices(correctAnswer);
        correctIndex = choices.IndexOf(correctAnswer);

        for (int i = 0; i < 4; i++)
        {
            answerTexts[i].text = choices[i].ToString();
        }

        totalTimerForQuestion = GetTimerByLevel(currentLevel);
        currentTimer = totalTimerForQuestion;
        timer.color = colorNormal; 
        questionActive = true;
        canAnswer = true;
        totalQuestionCount += 1;

        currentAnswerA = choices[0];
        currentAnswerB = choices[1];
        currentAnswerC = choices[2];
        currentAnswerD = choices[3];

        UpdateHUD();
    }

    void UpdateHUD()
    {
        question.text = "Pertanyaan ke-" + totalQuestionCount;
        score.text = "Score : " + currentScore.ToString();
        level[0].text = "Lv. " + currentLevel;
        level[1].text = "'11'";
        health.text = currentHealth.ToString();
        timer.text = currentTimer.ToString("F1");

        bool isLevel11 = (currentLevel == 11);

        if (isLevel11)
        {
            // Paksa semua merah di level 11
            question.color = colorDanger;
            score.color = colorDanger;
            health.color = colorDanger;
            timer.color = colorDanger;

            // Icon: 11 aktif, biasa nonaktif
            if (levelIcon != null && levelIcon.Length >= 2)
            {
                levelIcon[0].SetActive(false); // icon biasa
                levelIcon[1].SetActive(true);  // icon khusus level 11
            }

            level[0].color = colorDanger;
            level[1].color = colorDanger;
        }
        else
        {
            // Warna normal di luar level 11
            question.color = colorNormal;
            score.color = colorNormal;
            health.color = colorNormal;
            // timer diatur dinamis di Update(), jadi di sini cukup default:
            timer.color = colorNormal;

            if (levelIcon != null && levelIcon.Length >= 2)
            {
                levelIcon[0].SetActive(true);  // icon biasa
                levelIcon[1].SetActive(false); // icon 11 mati
            }

            level[0].color = colorNormal;
            level[1].color = colorNormal;
        }
    }

    void ResetResultIcons()
    {
        foreach (GameObject[] pair in resultObjects)
        {
            if (pair == null || pair.Length < 2) continue;
            pair[0].SetActive(false);
            pair[1].SetActive(false);
        }
    }

    int CalculateScore(float remainingTime, int levelValue)
    {
        // Base score dari level: makin tinggi level, makin besar base
        int baseScore = levelValue * 100;

        // Speed bonus: proporsional, tidak pakai Random
        // semakin cepat menjawab, semakin besar bonus
        float timeRatio = remainingTime / GetTimerByLevel(levelValue); // 0.0 - 1.0
        int speedBonus = Mathf.RoundToInt(timeRatio * levelValue * 50);

        return baseScore + speedBonus;
    }

    float GetTimerByLevel(int levelValue)
    {
        float value = 15f - ((levelValue - 1) * 0.5f);
        return Mathf.Clamp(value, 10f, 15f);
    }

    OperationType GetOperationForLevel(int levelValue)
    {
        List<OperationType> allowed = new List<OperationType>();

        allowed.Add(OperationType.Add);
        allowed.Add(OperationType.Subtract);

        if (levelValue >= 3) allowed.Add(OperationType.Multiply);
        if (levelValue >= 4) allowed.Add(OperationType.Divide);

        int index = Random.Range(0, allowed.Count);
        return allowed[index];
    }

    void GenerateOperands(OperationType opType, int levelValue, out int a, out int b, out int result, out string opSymbol)
    {
        a = 0;
        b = 0;
        result = 0;
        opSymbol = "+";

        switch (opType)
        {
            case OperationType.Add:
            {
                int lMin, lMax, rMin, rMax;
                GetAddDigitsByLevel(levelValue, out lMin, out lMax, out rMin, out rMax);

                a = RandomNumberWithDigitRange(lMin, lMax);
                b = RandomNumberWithDigitRange(rMin, rMax);

                // kadang tukar posisi agar variasi 3d+2d dan 2d+3d muncul
                if (Random.Range(0, 2) == 0)
                {
                    int temp = a;
                    a = b;
                    b = temp;
                }

                result = a + b;
                opSymbol = "+";
                break;
            }

            case OperationType.Subtract:
            {
                int lMin, lMax, rMin, rMax;
                GetSubDigitsByLevel(levelValue, out lMin, out lMax, out rMin, out rMax);

                a = RandomNumberWithDigitRange(lMin, lMax);
                b = RandomNumberWithDigitRange(rMin, rMax);

                if (Random.Range(0, 2) == 0)
                {
                    int temp = a;
                    a = b;
                    b = temp;
                }

                if (b > a)
                {
                    int temp = a;
                    a = b;
                    b = temp;
                }

                result = a - b;
                opSymbol = "-";
                break;
            }

            case OperationType.Multiply:
            {
                int lMin, lMax, rMin, rMax, maxResultDigits;
                GetMultiplyDigitsByLevel(levelValue, out lMin, out lMax, out rMin, out rMax, out maxResultDigits);

                if (lMax == 0 || rMax == 0)
                {
                    goto case OperationType.Add;
                }

                int safety = 0;
                do
                {
                    safety++;
                    if (safety > 100)
                    {
                        a = 2;
                        b = 2;
                        result = 4;
                        opSymbol = "x";
                        return;
                    }

                    a = RandomNumberWithDigitRange(lMin, lMax);
                    b = RandomNumberWithDigitRange(rMin, rMax);
                    result = a * b;

                } while (GetDigitCount(result) > maxResultDigits);

                opSymbol = "x";
                break;
            }

            case OperationType.Divide:
            {
                GenerateDivisionOperands(levelValue, out a, out b, out result, out opSymbol);
                break;
            }
        }
    }

    int RandomNumberWithDigitRange(int minDigits, int maxDigits)
    {
        minDigits = Mathf.Clamp(minDigits, 1, 9);
        maxDigits = Mathf.Clamp(maxDigits, minDigits, 9);

        int chosenDigits = Random.Range(minDigits, maxDigits + 1);

        int minValue = (chosenDigits == 1) ? 1 : (int)Mathf.Pow(10, chosenDigits - 1);
        int maxValue = (int)Mathf.Pow(10, chosenDigits) - 1;

        return Random.Range(minValue, maxValue + 1);
    }

    void GenerateDivisionOperands(int levelValue, out int a, out int b, out int result, out string opSymbol)
    {
        int dMin, dMax, vMin, vMax;
        GetDivisionDigitsByLevel(levelValue, out dMin, out dMax, out vMin, out vMax);

        a = 1; b = 1; result = 1;
        opSymbol = "÷";

        if (dMax == 0 || vMax == 0)
        {
            // level ini tidak mengizinkan division, fallback
            a = 1; b = 1; result = 1;
            return;
        }

        int safety = 0;
        do
        {
            safety++;
            if (safety > 100)
            {
                a = 4; b = 2; result = 2;
                return;
            }

            b = RandomNumberWithDigitRange(vMin, vMax);
            result = RandomNumberWithDigitRange(vMin, vMax);
            a = b * result;

        } while (GetDigitCount(a) < dMin || GetDigitCount(a) > dMax);
    }

    int GetDigitCount(int value)
    {
        value = Mathf.Abs(value);
        if (value == 0) return 1;
        return value.ToString().Length;
    }

    void GetAddDigitsByLevel(int levelValue,
                         out int leftMin, out int leftMax,
                         out int rightMin, out int rightMax)
    {
        switch (levelValue)
        {
            case 1:
                leftMin = 1; leftMax = 1; 
                rightMin = 1; rightMax = 1;
                break;
            case 2:
                leftMin = 1; leftMax = 2; // 1–2 digit
                rightMin = 1; rightMax = 1;
                break;
            case 3:
                leftMin = 2; leftMax = 2;
                rightMin = 1; rightMax = 1; // 2d + 1–2d
                break;
            case 4:
                leftMin = 2; leftMax = 2;
                rightMin = 1; rightMax = 2; // 2d + 1–2d
                break;
            case 5:
                leftMin = 2; leftMax = 2;
                rightMin = 2; rightMax = 2; // 2d + 1–2d
                break;
            case 6:
                leftMin = 2; leftMax = 3; // 2–3d
                rightMin = 2; rightMax = 2; // 2d pasti
                break;
            case 7:
                leftMin = 3; leftMax = 3;
                rightMin = 2; rightMax = 2;
                break;
            case 8:
                leftMin = 3; leftMax = 3;
                rightMin = 2; rightMax = 3;
                break;
            case 9:
                leftMin = 3; leftMax = 3;
                rightMin = 3; rightMax = 3;
                break;
            case 10:
                leftMin = 3; leftMax = 4;
                rightMin = 3; rightMax = 3;
                break;
            default: // 11+
                leftMin = 4; leftMax = 4;
                rightMin = 3; rightMax = 4;
                break;
        }
    }

    void GetSubDigitsByLevel(int levelValue,
                         out int leftMin, out int leftMax,
                         out int rightMin, out int rightMax)
    {
        // Bisa sama dengan Add; kalau mau lebih jahat sedikit, tingkatkan rightMin
        GetAddDigitsByLevel(levelValue, out leftMin, out leftMax, out rightMin, out rightMax);
    }

    void GetMultiplyDigitsByLevel(int levelValue,
                              out int leftMin, out int leftMax,
                              out int rightMin, out int rightMax,
                              out int maxResultDigits)
    {
        switch (levelValue)
        {
            case 1:
                leftMin = leftMax = rightMin = rightMax = 0;
                maxResultDigits = 0;
                break;
            case 2:
                leftMin = 1; leftMax = 1;
                rightMin = 1; rightMax = 1;
                maxResultDigits = 2;
                break;
            case 3:
                leftMin = 1; leftMax = 2;
                rightMin = 1; rightMax = 1;
                maxResultDigits = 2;
                break;
            case 4:
                leftMin = 2; leftMax = 2;
                rightMin = 1; rightMax = 1;
                maxResultDigits = 3;
                break;
            case 5:
                leftMin = 2; leftMax = 2;
                rightMin = 1; rightMax = 2;
                maxResultDigits = 3;
                break;
            case 6:
                leftMin = 2; leftMax = 2;
                rightMin = 2; rightMax = 2;
                maxResultDigits = 3;
                break;
            case 7:
                leftMin = 2; leftMax = 3;
                rightMin = 2; rightMax = 2;
                maxResultDigits = 3;
                break;
            case 8:
                leftMin = 3; leftMax = 3;
                rightMin = 2; rightMax = 2;
                maxResultDigits = 4;
                break;
            case 9:
                leftMin = 3; leftMax = 3;
                rightMin = 2; rightMax = 3;
                maxResultDigits = 4;
                break;
            case 10:
                leftMin = 3; leftMax = 3;
                rightMin = 3; rightMax = 3;
                maxResultDigits = 4;
                break;
            default:
                leftMin = 3; leftMax = 4;
                rightMin = 3; rightMax = 3;
                maxResultDigits = 5;
                break;
        }
    }

    void GetDivisionDigitsByLevel(int levelValue,
                              out int dividendMin, out int dividendMax,
                              out int divisorMin, out int divisorMax)
    {
        switch (levelValue)
        {
            case 1:
            case 2:
                dividendMin = dividendMax = 0;
                divisorMin = divisorMax = 0;
                break;
            case 3:
            case 4:
                dividendMin = 1; dividendMax = 1;
                divisorMin = 1; divisorMax = 1; // 1d / 1d
                break;
            case 5:
                dividendMin = 1; dividendMax = 2;
                divisorMin = 1; divisorMax = 1; // 1d / 1d
                break;
            case 6:
                dividendMin = 2; dividendMax = 2;
                divisorMin = 1; divisorMax = 1; // 2d / 1d
                break;
            case 7:
                dividendMin = 2; dividendMax = 3;
                divisorMin = 1; divisorMax = 2; // 2d / 1–2d
                break;
            case 8:
                dividendMin = 3; dividendMax = 3;
                divisorMin = 1; divisorMax = 2; // 2d / 1–2d
                break;
            case 9:
                dividendMin = 3; dividendMax = 3;
                divisorMin = 2; divisorMax = 2; // 3d / 1d
                break;
            case 10:
                dividendMin = 3; dividendMax = 4;
                divisorMin = 2; divisorMax = 2; // 3d / 1–2d
                break;
            default: // 11+
                dividendMin = 4; dividendMax = 4;
                divisorMin = 2; divisorMax = 2; // 4d / 1–2d
                break;
        }
    }

    List<int> GenerateChoices(int correct)
    {
        HashSet<int> values = new HashSet<int>();
        values.Add(correct);

        while (values.Count < 4)
        {
            int offset = Random.Range(-15, 16);
            if (offset == 0) offset = 1;

            int fake = correct + offset;

            if (correct > 20)
            {
                int variance = Mathf.Max(2, Mathf.RoundToInt(correct * 0.15f));
                fake = correct + Random.Range(-variance, variance + 1);
            }

            if (fake < 0) continue;
            values.Add(fake);
        }

        List<int> shuffled = new List<int>(values);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int rnd = Random.Range(i, shuffled.Count);
            int temp = shuffled[i];
            shuffled[i] = shuffled[rnd];
            shuffled[rnd] = temp;
        }

        return shuffled;
    }

    public void OnClickAnswerA() => SelectAnswer(0);
    public void OnClickAnswerB() => SelectAnswer(1);
    public void OnClickAnswerC() => SelectAnswer(2);
    public void OnClickAnswerD() => SelectAnswer(3);
}

public enum OperationType
{
    Add,
    Subtract,
    Multiply,
    Divide
}

