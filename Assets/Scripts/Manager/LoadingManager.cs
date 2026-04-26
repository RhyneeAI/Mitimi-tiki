using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private Image loadingBarImage;
    [SerializeField] private Sprite[] loadingFrames;
    [SerializeField] private TMP_Text tipsText;
    [SerializeField] private float minimumLoadingTime = 5f;
    [SerializeField] private float tipChangeInterval = 2f;

    private int currentTipIndex = -1;

    private string[] tips = {
        "Jawab secepat mungkin untuk mencapai level tertinggi!",
        "Matematika itu kayak lari sprint. Cepat + tepat = juara!",
        "Salah sekali masih aman. Salah lima kali? Ya… bye bye.",
        "Semakin cepat jawab, semakin keren. Otakmu lagi nge-boost mode turbo!",
    };

    void Start()
    {
        ShowRandomTip();
        StartCoroutine(LoadSceneAsync(LoadingContext.targetScene)); // ← baca dari context
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShowRandomTip();
        }
    }

    void ShowRandomTip()
    {
        if (tips == null || tips.Length == 0 || tipsText == null)
            return;

        int randomIndex = Random.Range(0, tips.Length);

        if (tips.Length > 1)
        {
            while (randomIndex == currentTipIndex)
                randomIndex = Random.Range(0, tips.Length);
        }

        currentTipIndex = randomIndex;
        tipsText.text = tips[currentTipIndex];
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        float loadTimer = 0f;
        float tipTimer  = 0f;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            loadTimer += Time.deltaTime;
            tipTimer  += Time.deltaTime;

            float realProgress    = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            float fakeProgress    = Mathf.Clamp01(loadTimer / minimumLoadingTime);
            float displayProgress = Mathf.Min(realProgress, fakeProgress);

            int frameIndex = Mathf.FloorToInt(displayProgress * loadingFrames.Length);
            frameIndex = Mathf.Clamp(frameIndex, 0, loadingFrames.Length - 1);

            if (loadingBarImage != null && loadingFrames.Length > 0)
                loadingBarImage.sprite = loadingFrames[frameIndex];

            if (tipTimer >= tipChangeInterval)
            {
                ShowRandomTip();
                tipTimer = 0f;
            }

            bool sceneReady    = asyncLoad.progress >= 0.9f;
            bool timeReady     = loadTimer >= minimumLoadingTime;

            // ← Satu-satunya tambahan: cek Firebase hanya kalau memang diperlukan
            bool firebaseReady = !LoadingContext.waitForFirebase || LoadingContext.firebaseDone;

            if (sceneReady && timeReady && firebaseReady)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}