using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM Sources")]
    [SerializeField] private AudioSource bgmSource;        // untuk musik berlanjut
    [SerializeField] private AudioSource bgmSourceLevel11; // untuk lv11

    [Header("SFX Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip[] bgmMainList; // daftar lagu Home+Loading+Game
    [SerializeField] private AudioClip   bgmLevel11;  // khusus lv11
    private int lastMainBgmIndex = -1;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip sfxButtonClick;
    [SerializeField] private AudioClip sfxModalClose;
    [SerializeField] private AudioClip sfxModalOpen;
    [SerializeField] private AudioClip sfxGameOver;
    [SerializeField] private AudioClip sfxAnswerCorrect;
    [SerializeField] private AudioClip sfxAnswerWrong;
    [SerializeField] private AudioClip sfxGameStart;

    [Header("Tags")]
    [SerializeField] private string tagButtonClick  = "ButtonClick";
    [SerializeField] private string tagButtonClose  = "ButtonClose";
    [SerializeField] private string tagModalOpen    = "ModalOpen";

    private bool isLevel11Playing = false;

    // ─────────────────────────────────────────────
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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ─────────────────────────────────────────────
    void Start()
    {
        PlayMainBGM();
        StartCoroutine(RegisterButtonsRoutine());
    }

    // Tiap ganti scene, register ulang tombol dari tag
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RegisterButtonsRoutine());
    }

    // ─────────────────────────────────────────────
    #region BGM

    public void PlayMainBGM()
    {
        AudioClip next = GetRandomMainBgm();
        if (next == null)
        {
            Debug.LogWarning("[AudioManager] bgmMainList kosong.");
            return;
        }

        // kalau lagu sekarang sudah sama & lagi main, tidak usah restart
        if (bgmSource.clip == next && bgmSource.isPlaying) return;

        isLevel11Playing = false;
        bgmSourceLevel11.Stop();

        bgmSource.clip = next;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayLevel11BGM()
    {
        if (isLevel11Playing) return;
        isLevel11Playing = true;

        // fade out bgm utama
        StartCoroutine(FadeOut(bgmSource, 0.5f, () =>
        {
            bgmSourceLevel11.clip  = bgmLevel11;
            bgmSourceLevel11.loop  = true;
            bgmSourceLevel11.Play();
        }));
    }

    public void StopLevel11BGM()
    {
        if (!isLevel11Playing) return;
        isLevel11Playing = false;

        StartCoroutine(FadeOut(bgmSourceLevel11, 0.5f, () =>
        {
            PlayMainBGM();
        }));
    }

    private IEnumerator FadeOut(AudioSource source, float duration, System.Action onDone = null)
    {
        float startVolume = source.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume; // reset untuk next play
        onDone?.Invoke();
    }

    #endregion

    // ─────────────────────────────────────────────
    #region SFX

    public void PlayButtonClick()  => PlaySFX(sfxButtonClick);
    public void PlayButtonClose()  => PlaySFX(sfxModalClose);
    public void PlayModalOpen()    => PlaySFX(sfxModalOpen);
    public void PlayGameOver()     => PlaySFX(sfxGameOver);
    public void PlayAnswerCorrect() => PlaySFX(sfxAnswerCorrect);
    public void PlayAnswerWrong()   => PlaySFX(sfxAnswerWrong);
    public void PlayGameStart()     => PlaySFX(sfxGameStart);

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    private AudioClip GetRandomMainBgm()
    {
        if (bgmMainList == null || bgmMainList.Length == 0)
            return null;

        if (bgmMainList.Length == 1)
            return bgmMainList[0];

        int index;

        // pilih index random yang beda dari sebelumnya
        do
        {
            index = Random.Range(0, bgmMainList.Length);
        }
        while (index == lastMainBgmIndex);

        lastMainBgmIndex = index;
        return bgmMainList[index];
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Auto Register Buttons by Tag

    // Cari semua button berdasarkan tag, attach listener audio
    private IEnumerator RegisterButtonsRoutine()
    {
        // Tunggu 1 frame supaya scene selesai load
        yield return null;

        RegisterTag(tagButtonClick, PlayButtonClick);
        RegisterTag(tagButtonClose, PlayButtonClose);
        RegisterTag(tagModalOpen,   PlayModalOpen);
    }

    private void RegisterTag(string tag, System.Action sfxAction)
    {
        if (string.IsNullOrEmpty(tag)) return;

        GameObject[] objects;

        try
        {
            objects = GameObject.FindGameObjectsWithTag(tag);
        }
        catch
        {
            Debug.LogWarning($"[AudioManager] Tag '{tag}' tidak ditemukan di Tag Manager.");
            return;
        }

        foreach (GameObject obj in objects)
        {
            UnityEngine.UI.Button btn = obj.GetComponent<UnityEngine.UI.Button>();

            if (btn == null) continue;

            // Hindari register ganda
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => sfxAction?.Invoke());
        }
    }

    #endregion
}