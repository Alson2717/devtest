using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMaster : Singleton<UIMaster>
{
    public const string KEY_STARS_COUNT = "starsCount";
    public const string KEY_DARK_MODE = "darkMode";

    public const int DARK_MODE_ON_VALUE = 0;

    #region Inspector
    [Header("Score UI")]
    [SerializeField]
    private TextMeshProUGUI mainScoreText;
    [SerializeField]
    private TextMeshProUGUI bestScoreText;
    [SerializeField]
    private CanvasGroup bestScoreAlpha;
    [SerializeField]
    private CanvasGroup topLeftButtonsAlpha;

    [Header("Star UI")]
    [SerializeField]
    private Transform starCountUI;
    [SerializeField]
    private TextMeshProUGUI starCountText;

    [Header("Main Menu Canvas")]
    [SerializeField]
    private Canvas mainMenuCanvas;
    [SerializeField]
    private CanvasGroup mainMenuCanvasAlpha;

    [Header("Game Canvas")]
    [SerializeField]
    private Canvas gameCanvas;
    [SerializeField]
    private CanvasGroup gameCanvasAlpha;

    [Header("Pause Canvas")]
    [SerializeField]
    private Canvas pauseCanvas;

    [Header("Settings Canvas")]
    [SerializeField]
    private Canvas settingsCanvas;

    [Header("Buttons")]
    [SerializeField]
    private PopupButton unstuckButton;
    [SerializeField]
    private PopupButton resetButton;

    [Header("Other UI")]
    [SerializeField]
    private Image fadeImage;

    [Header("Audio")]
    [SerializeField]
    private AudioSettings[] progressiveScoreSounds;
    [SerializeField]
    private AudioSettings countBounceSound;

    [Header("Prefabs")]
    [SerializeField]
    private GameplayText prefabBounceText;
    [SerializeField]
    private GameplayText prefabPerfectText;
    [SerializeField]
    private ScoreText prefabScoreText;

    [Header("Popup Settings")]
    [SerializeField]
    private float popupShowTime = 0.15f;
    [SerializeField]
    private Vector3 popupTargetScale = Vector3.one;

    [Header("Settings")]
    [SerializeField]
    private float initialTextDelay = 0.1f;
    [SerializeField]
    private float textDelay = 0.33f;
    [SerializeField]
    private float textFadeawayTime = 0.5f;
    [SerializeField]
    private float textYMovement = 1.0f;
    #endregion

    private int delayedScore = 0;
    public int CurrentScore
    {
        get;
        private set;
    }

    public int StarCount
    {
        get;
        private set;
    }

    public bool DarkMode
    {
        get;
        private set;
    }
    [HideInInspector]
    public List<DarkModeBase> darkModeThings = new List<DarkModeBase>();

    private Pool<GameplayText> bounceTextPool = new Pool<GameplayText>();
    private Pool<GameplayText> perfectTextPool = new Pool<GameplayText>();
    private Pool<ScoreText> scoreTextPool = new Pool<ScoreText>();

    private float prevTimeScale;
    protected override void SingletonAwake()
    {
        StarCount = GetStarsCount();
        starCountText.text = StarCount.ToString();
    }
    protected override void SingletonDestroy()
    {
       
    }
    private void Start()
    {
        bestScoreAlpha.alpha = 0.0f;
        mainScoreText.text = CurrentScore.ToString();

        mainMenuCanvas.gameObject.SetActive(true);
        gameCanvas.gameObject.SetActive(false);       

        unstuckButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);

        pauseCanvas.gameObject.SetActive(false);
        settingsCanvas.gameObject.SetActive(false);

        // doing this to ensure that all darkmodebase stuff is subscribed
        StartCoroutine(NextFrameSetDarkMode());
    }

    public bool IgnoreGameInput()
    {
        return NonGameplayUIIsOpen();
    }

    public bool NonGameplayUIIsOpen()
    {
        return settingsCanvas.gameObject.activeSelf || pauseCanvas.gameObject.activeSelf;
    }

    public void OpenSettings()
    {
        settingsCanvas.gameObject.SetActive(true);
    }
    public void CloseSettings()
    {
        settingsCanvas.gameObject.SetActive(false);
    }

    public void Pause()
    {
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0.0f;

        pauseCanvas.gameObject.SetActive(true);
    }
    public void Unpause()
    {
        Time.timeScale = prevTimeScale;

        pauseCanvas.gameObject.SetActive(false);
    }

    public void ToggleDarkMode()
    {
        SetDarkMode(!DarkMode);
    }
    public void SetDarkMode(bool darkMode)
    {
        this.DarkMode = darkMode;

        int darkModeValue = DARK_MODE_ON_VALUE;
        if (!darkMode)
            darkModeValue = 1;
        PlayerPrefs.SetInt(KEY_DARK_MODE, darkModeValue);

        foreach (DarkModeBase thing in darkModeThings)
        {
            thing.OnDarkModeChanged(darkMode);
        }
    }
    public static bool IsDarkMode()
    {
        return PlayerPrefs.GetInt(KEY_DARK_MODE, DARK_MODE_ON_VALUE) == DARK_MODE_ON_VALUE;
    }

    public void ShowUnstuck()
    {
        unstuckButton.Popup(popupShowTime, popupTargetScale);
    }
    public void HideUnstuck()
    {
        unstuckButton.Hide(popupShowTime);
    }
    public void ShowGameOver(int bestScore)
    {
        bestScoreText.text = bestScore.ToString();
        StartCoroutine(FadeInBestScore(0.25f));

        DoThingWithDelay(Callback_ShowResetButton, 0.2f);
    }

    public void FadeInGameCanvas(float time)
    {
        StartCoroutine(FadeInGameCanvasCoroutine(time));
    }

    private void Callback_ShowResetButton()
    {
        resetButton.Popup(popupShowTime, popupTargetScale);
    }

    public void AddScore(int amount, int perfects, int bounces, float leftMax, float rightMin)
    {
        int soundIndex = perfects;
        if (soundIndex >= progressiveScoreSounds.Length)
            soundIndex = progressiveScoreSounds.Length - 1;

        CurrentScore += amount;
        float delay = initialTextDelay;

        Vector3 textPosition = GameMaster.Instance.Ball.transform.position;

        if (perfects > 0)
        {
            GameplayText perfect = perfectTextPool.ExtractFirst(prefabPerfectText);
            perfect.gameObject.SetActive(false);
            perfect.SetValue(perfects);

            perfect.transform.position = textPosition;
            AdjustTextPosition(perfect.Text, leftMax, rightMin);

            AudioSettings perfectSound = progressiveScoreSounds[soundIndex];

            DoCoroutineWithDelay(GameplayTextCoroutine(perfect, perfectTextPool, textFadeawayTime, textYMovement, perfectSound), delay);

            delay += textDelay;
        }
        if (bounces > 0)
        {
            GameplayText bounce = bounceTextPool.ExtractFirst(prefabBounceText);
            bounce.gameObject.SetActive(false);
            bounce.SetValue(bounces);

            bounce.transform.position = textPosition;
            AdjustTextPosition(bounce.Text, leftMax, rightMin);

            DoCoroutineWithDelay(GameplayTextCoroutine(bounce, bounceTextPool, textFadeawayTime, textYMovement, countBounceSound), delay);

            delay += textDelay;
        }

        StartCoroutine(ScoreDelayCoroutine(delay, amount));

        ScoreText scoreText = scoreTextPool.ExtractFirst(prefabScoreText);
        scoreText.gameObject.SetActive(false);
        scoreText.SetScore(amount);

        scoreText.transform.position = textPosition;
        AdjustTextPosition(scoreText.Text, leftMax, rightMin);

        AudioSettings sound = default;
        if (soundIndex == 0)
            sound = progressiveScoreSounds[soundIndex];
        DoCoroutineWithDelay(ScoreTextCoroutine(scoreText, textFadeawayTime, textYMovement, sound), delay);
    }
    public void AddStar(BasketPatternStar star)
    {
        starCountUI.localScale += new Vector3(0.2f, 0.2f, 0.2f);

        StarCount++;
        starCountText.text = StarCount.ToString();

        SetStarsCount(StarCount);
    }

    public static void SetStarsCount(int value)
    {
        PlayerPrefs.SetInt(KEY_STARS_COUNT, value);
    }
    public static int GetStarsCount()
    {
        return PlayerPrefs.GetInt(KEY_STARS_COUNT, 0);
    }

    private void AdjustTextPosition(TextMeshPro text, float leftMax, float rightMin)
    {
        Vector3 position = text.transform.position;

        text.ForceMeshUpdate(true);

        Bounds aabb = text.textBounds;

        // center is always zero for some reason
        Vector3 max = position + aabb.extents;
        Vector3 min = position - aabb.extents;

        Debug.DrawLine(min, max, Color.yellow, 30.0f);

        if (min.x < leftMax)
            position.x += leftMax - min.x;
        if (max.x > rightMin)
            position.x += rightMin - max.x;

        text.transform.position = position;
    }

    private void DoCoroutineWithDelay(IEnumerator coroutine, float delay)
    {
        StartCoroutine(DoCoroutineWithDelayCoroutine(coroutine, delay));
    }
    private void DoThingWithDelay(System.Action action, float delay)
    {
        StartCoroutine(DoThingWithDelayCoroutine(action, delay));
    }

    #region Coroutines
    private IEnumerator NextFrameSetDarkMode()
    {
        yield return null;
        SetDarkMode(PlayerPrefs.GetInt(KEY_DARK_MODE, DARK_MODE_ON_VALUE) == DARK_MODE_ON_VALUE);
    }

    private IEnumerator DoCoroutineWithDelayCoroutine(IEnumerator coroutine, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(coroutine);
    }

    private IEnumerator ScoreDelayCoroutine(float delay, int toAdd)
    {
        yield return new WaitForSeconds(delay);
        delayedScore += toAdd;
        mainScoreText.text = delayedScore.ToString();
    }
    private IEnumerator GameplayTextCoroutine(GameplayText text, Pool<GameplayText> pool, float time, float yMovement, AudioSettings sound)
    {
        AudioMaster.Instance.PlaySound(sound);

        text.gameObject.SetActive(true);
        yield return TextFadeawayCoroutine(text.Text, text.transform, time, yMovement);
        text.gameObject.SetActive(false);
        pool.Add(text);
    }
    private IEnumerator ScoreTextCoroutine(ScoreText text, float time, float yMovement, AudioSettings sound)
    {
        AudioMaster.Instance.PlaySound(sound);

        text.gameObject.SetActive(true);
        yield return TextFadeawayCoroutine(text.Text, text.transform, time, yMovement);
        text.gameObject.SetActive(false);
        scoreTextPool.Add(text);
    }
    private IEnumerator TextFadeawayCoroutine(TextMeshPro text, Transform toMove, float time, float yMovement)
    {
        LerpInstruction<float> alphaInstruction = new LerpInstruction<float>(1.0f, 0.0f, Mathf.Lerp, text.SetAlpha);
        LerpInstruction<float> yMovementInstruction = new LerpInstruction<float>();
        yMovementInstruction.firstValue = toMove.transform.position.y;
        yMovementInstruction.lastValue = toMove.transform.position.y + yMovement;
        yMovementInstruction.lerp = Mathf.Lerp;
        yMovementInstruction.set = toMove.transform.SetWorldPositionY;

        yield return this.Lerp(time, alphaInstruction, yMovementInstruction);
    }
    public IEnumerator FadeScreenCoroutine(float time, float fromAlpha, float toAlpha)
    {
        yield return this.LerpUnscaled(time, fromAlpha, toAlpha, Mathf.Lerp, fadeImage.SetAlpha);
    }
    private IEnumerator DoThingWithDelayCoroutine(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    private IEnumerator FadeInBestScore(float time)
    {
        topLeftButtonsAlpha.interactable = false;

        LerpInstruction<float> bestScore = new LerpInstruction<float>(0.0f, 1.0f, Mathf.Lerp, bestScoreAlpha.SetAlpha);
        LerpInstruction<float> topLeft = new LerpInstruction<float>(topLeftButtonsAlpha.alpha, 0.0f, Mathf.Lerp, topLeftButtonsAlpha.SetAlpha);

        yield return this.Lerp(time, bestScore, topLeft);

        topLeftButtonsAlpha.gameObject.SetActive(false);
    }
    private IEnumerator FadeInGameCanvasCoroutine(float time)
    {
        mainMenuCanvasAlpha.interactable = false;
        gameCanvas.gameObject.SetActive(true);

        LerpInstruction<float> game = new LerpInstruction<float>(0.0f, 1.0f, Mathf.Lerp, gameCanvasAlpha.SetAlpha);
        LerpInstruction<float> menu = new LerpInstruction<float>(mainMenuCanvasAlpha.alpha, 0.0f, Mathf.Lerp, mainMenuCanvasAlpha.SetAlpha);

        yield return this.Lerp(time, game, menu);

        mainMenuCanvas.gameObject.SetActive(false);
    }
    #endregion
}

