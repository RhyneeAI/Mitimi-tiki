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
    [SerializeField] private float tipChangeInterval = 2.5f;

    [Header("Manager")]
    private UIManager uiManager;

    private int currentTipIndex = -1;

    private string[] tips = {
        "Jawab secepat mungkin untuk mencapai level tertinggi!",
        "Matematika itu kayak lari sprint. Cepat + tepat = juara!",
        "Salah sekali masih aman. Salah lima kali? Ya… bye bye.",
        "Semakin cepat jawab, semakin keren. Otakmu lagi nge-boost mode turbo!",
        "Kesulitan naik otomatis. Anggap aja boss level makin kuat.",
        "Terkadang jawaban yang benar itu lebih penting daripada kecepatan",
        "Skor tinggi bikin puas. Tapi bertahan lama bikin panas.",
        "Matematika = olahraga otak. Main terus, otak makin lentur.",
        "HP habis = Game Over. Jadi jangan boros salah, hemat nyawa!",
        "Semakin lama bertahan, semakin epic. Kayak marathon, bukan sprint doang.",
        "Refleks + logika = kombinasi maut. Latih terus biar makin GG.",
        "Main boleh, tapi jangan lupa injak rumput ya",
        "Anggap soal kayak monster. Jawaban benar = critical hit!",
        "Main santai boleh, tapi ingat ada batas waktu jawab. Jangan sampai kebablasan.",
        "Kamu jago gak?, coba sampai level paling tinggi dong!",
        "Ya siapa tau aja kan, setelah kamu main game ini IQ mu naik gitu...",
        "Tantangan didepan mata, tinggal kamu milih untuk jadi PEMENANG atau PECUNDANG ?",
        "Siapa cepat dia dapat...",
        "Jangan lupa cek leaderboard untuk lihat ranking kamu ya",
        "Ingat, ada waktu dan ada nyawa, jangan sampai lengah!",
        "Lalalalalalala...",
        "Kalau jago pasti bisa !!!"
    };

    void Start()
    {
        if(tipsText)
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

            float realProgress    = Mathf.Clamp01(asyncLoad.progress / 0.919f);
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
                asyncLoad.allowSceneActivation = true;

            yield return null;
        }
    }
}