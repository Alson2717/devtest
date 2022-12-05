using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameMaster : Singleton<GameMaster>
{
    public const string KEY_BEST_SCORE = "DonkeyBallBestScore";

    public static bool FirstPlay
    {
        get;
        private set;
    } = true;

    #region Inspector
    [Header("Scene Stuff")]
    [SerializeField]
    private Ball ball;
    [SerializeField]
    private Collider2D leftWall;
    [SerializeField]
    private Collider2D rightWall;
    [SerializeField]
    private Transform prefabsDump;

    [Header("Other Stuff")]
    [SerializeField]
    private PhysicsMaterial2D bounceMaterial;
    [SerializeField]
    private PhysicsMaterial2D noBounceMaterial;

    [Header("Prefabs")]
    [SerializeField]
    private TrajectoryDot prefabDot;

    [Header("Patterns")]
    [SerializeField]
    private BasketPattern[] basketPatterns;

    [Header("Audio")]
    [SerializeField]
    private AudioSettings gameOverSound;
    [SerializeField]
    private AudioSettings touchedBasketSound;
    [SerializeField]
    private AudioSettings throwSound;

    [Header("Game Settings")]
    [SerializeField]
    private Vector2 velocityMax = new Vector2(5.0f, 8.0f);
    [SerializeField]
    private Vector2 inputSensitivity = new Vector2(2.0f, 8.0f);
    [SerializeField]
    private float minBallAngularVelocity = 30.0f;
    [SerializeField]
    private float noInputLength = 1.0f;
    [SerializeField]
    private float transparencyMinLength = 1.0f;
    [SerializeField]
    [Tooltip("If velocity length is less than this then dots are semi-transparent")]
    private float transparencyMaxLength = 2.0f;
    [SerializeField]
    private float maxBasketPatternSpawnLength = 10.0f;

    [Header("Extra Settings")]
    [SerializeField]
    private int steps = 20;
    [SerializeField]
    [Tooltip("i.e. steps = 20, dotEverySteps = 2 -> 20 / 2 = 10 dots")]
    private int dotEverySteps = 2;
    [SerializeField]
    private float dotScaler = 0.98f;
    [SerializeField]
    private LayerMask physicsCopyLayerMask;
    [SerializeField]
    private float defaultZ = 0.0f;
    [SerializeField]
    private float basketBonesMaxDrag = -1.0f;
    [SerializeField]
    private float basketDestroyTime = 0.5f;
    [SerializeField]
    private float flyingTimeOut = 8.0f;
    #endregion

    public Ball Ball
    {
        get { return ball; }
    }

    public float DefaultZ
    {
        get { return defaultZ; }
    }
    public float BasketBonesMaxDrag
    {
        get { return basketBonesMaxDrag; }
    }

    public AudioSettings TouchedBasketSound
    {
        get { return touchedBasketSound; }
    }

    public Basket CurrentBasket
    {
        get;
        private set;
    }

    private int perfectCounter = 0;
    private int bounceCounter = 0;
    private bool touchedBasketSide = false;

    private CoroutineHandler ballRestPositionCoroutine;
    private CoroutineHandler flyingTimeoutCoroutine;

    private Rigidbody2D simulationBall;
    private Scene simulationScene;
    private PhysicsScene2D simulationPhysicsScene;

    private List<TrajectoryDot> dots = new List<TrajectoryDot>();

    ///<summary>in screen coordinates</summary> 
    private Vector3 inputReference;
    private Vector2 lastVelocity;

    private bool uiInput = false;

    private bool isFlying = true;
    private bool ballIsReady = false;
    private bool gameEnded = false;
    private bool switchingScene = false;
    private bool firstInput = false;
    protected override void SingletonAwake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Time.timeScale = 1.0f;

        ball.RG.sharedMaterial = bounceMaterial;

        ballRestPositionCoroutine = new CoroutineHandler(this);
        flyingTimeoutCoroutine = new CoroutineHandler(this);
    }
    protected override void SingletonDestroy()
    {

    }
    private void Start()
    {
        CreateSceneParameters _params = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        simulationScene = SceneManager.CreateScene("Simulation Scene", _params);
        simulationPhysicsScene = simulationScene.GetPhysicsScene2D();

        Scene current = SceneManager.GetActiveScene();
        foreach (GameObject go in current.GetRootGameObjects())
        {
            if(physicsCopyLayerMask != (physicsCopyLayerMask | (1 << go.layer)))
            {
                if (go != ball.gameObject)
                    continue;
            }

            GameObject newGO = Instantiate(go, go.transform.position, go.transform.rotation);
            SceneManager.MoveGameObjectToScene(newGO, simulationScene);

            Renderer[] renderers = newGO.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                Destroy(r);
            }

            if (go == ball.gameObject)
            {
                simulationBall = newGO.GetComponent<Rigidbody2D>();
                Destroy(newGO.GetComponent<Ball>());
            }
        }

        if(!FirstPlay)
        {
            StartCoroutine(UIMaster.Instance.FadeScreenCoroutine(0.2f, 1.0f, 0.0f));
        }
        FirstPlay = false;

        // doing this to force load so that there is no lag :)
        foreach (BasketPattern prefab in basketPatterns)
        {
            BasketPattern p = Instantiate(prefab, prefabsDump);
            p.transform.localPosition = Vector3.zero;
        }
        CreateDotsToCount();
    }
    private void Update()
    {
        if (IgnoreUpdates())
            return;

        bool wasInput = false;
        if (ReadInput())
        {
            if (Input.GetMouseButtonDown(0))
            {
                inputReference = Input.mousePosition;
                if (!firstInput)
                {
                    firstInput = true;
                    UIMaster.Instance.FadeInGameCanvas(0.2f);
                }
            }
            else if (Input.GetMouseButton(0))
            {
                wasInput = true;

                Vector3 worldInputReference = CameraMaster.Instance.Camera.ScreenToWorldPoint(inputReference);

                // calc velocity based on input
                Vector3 input = CameraMaster.Instance.Camera.InputToWorld(defaultZ);
                Vector2 inputVelocity = worldInputReference - input;
                Vector2 scaledInputVelocity = inputVelocity * inputSensitivity;
                lastVelocity = scaledInputVelocity;
                lastVelocity = lastVelocity.Clamp(-velocityMax, velocityMax);

                // do a little animation for basket dragging
                float yVelocityPerc = Mathf.InverseLerp(0.0f, velocityMax.y, Mathf.Abs(lastVelocity.y));
                CurrentBasket.BonesDrag(yVelocityPerc);

                // set transparency for trajectory dots based on velocity
                float length = lastVelocity.magnitude;
                if (length < transparencyMaxLength)
                {
                    float transparency = Mathf.InverseLerp(transparencyMinLength, transparencyMaxLength, length);
                    SetDotsAlpha(transparency);
                }
                else
                    SetDotsAlpha(1.0f);

                // rotate basket in throw direction
                Vector2 lookAtPos = (Vector2)CurrentBasket.transform.position + lastVelocity.normalized;
                if (lookAtPos.Approximately2D(CurrentBasket.transform.position))
                    return;

                Vector3 euler = CurrentBasket.transform.LookAt2DEuler(lookAtPos);
                euler.z -= 90.0f;
                CurrentBasket.transform.eulerAngles = euler;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                wasInput = true;

                HideDots();

                float length = lastVelocity.magnitude;
                if (length < noInputLength)
                {
                    return;
                }

                CurrentBasket.EndBonesDrag();

                ball.RestoreGraphicsParent();

                ball.RG.simulated = true;
                ball.RG.velocity = lastVelocity;
                ball.RG.angularVelocity = CalcAngularVelocity(ball.RG.velocity);

                SetFlying(true);

                AudioMaster.Instance.PlaySound(throwSound);
            }
        }
        if (!wasInput)
        {
            HideDots();
        }
        ball.RG.angularVelocity = CalcAngularVelocity(ball.RG.velocity);
    }
    // do physics in FixedUpdate for more consistency
    private void FixedUpdate()
    {
        if (IgnoreUpdates())
            return;

        if(ReadInput())
        {
            if(Input.GetMouseButton(0))
            {
                // run physics simulation to place dots
                SimulateAndMarkTrajectory(lastVelocity, CalcAngularVelocity(lastVelocity));
            }
        }
    }

    private bool IgnoreUpdates()
    {
        return gameEnded || this.IsPointerOverUIObject();
    }
    private bool ReadInput()
    {
        return CurrentBasket != null && !isFlying && ballIsReady && !UIMaster.Instance.IgnoreGameInput() && !switchingScene;
    }

    private float CalcAngularVelocity(Vector2 velocity)
    {
        float sign = -Mathf.Sign(velocity.x);
        float result = velocity.magnitude * sign * Mathf.Rad2Deg;
        if (Mathf.Abs(result) < minBallAngularVelocity)
            result = minBallAngularVelocity * sign;
        return result;
    }

    public void ResetScene()
    {
        StartCoroutine(SwitchToSceneCoroutine(SceneManager.GetActiveScene().buildIndex));
    }

    public void BallTouchedBounceCounter()
    {
        AudioMaster.Instance.PlaySound(ball.CurrentScriptableSkin.RandomHitSound);

        bounceCounter++;
    }
    public void BallTouchedBasketSide()
    {
        AudioMaster.Instance.PlaySound(ball.CurrentScriptableSkin.RandomHitSound);

        touchedBasketSide = true;
        perfectCounter = 0;
    }
    public void BallInsideAbyss()
    {
        AudioMaster.Instance.PlaySound(gameOverSound);

        if (UIMaster.Instance.CurrentScore == 0)
        {
            ResetBallToBasket(CurrentBasket);
        }
        else
        {
            gameEnded = true;

            ball.RG.simulated = false;

            int best = PlayerPrefs.GetInt(KEY_BEST_SCORE, 0);
            if (UIMaster.Instance.CurrentScore > best)
            {
                best = UIMaster.Instance.CurrentScore;
                PlayerPrefs.SetInt(KEY_BEST_SCORE, best);
            }
            UIMaster.Instance.ShowGameOver(best);

            SetFlying(false);
        }
    }
    public void BallInsideBasket(Basket basket)
    {
        SetFlying(false);

        if (basket != CurrentBasket)
        {
            if(basket.CountScore)
            {
                if (!touchedBasketSide)
                    perfectCounter++;

                int bounceMultiplier = bounceCounter > 0 ? 2 : 1;
                int scoreToAdd = (1 + perfectCounter) * bounceMultiplier;

                SetOrthographicsAsAABBSize aabb = CameraMaster.Instance.CameraAABB;
                float size = aabb.CalcCurrentSize();

                float leftMax = aabb.transform.position.x - size * 0.5f;
                float rightMin = aabb.transform.position.x + size * 0.5f;

                UIMaster.Instance.AddScore(scoreToAdd,perfectCounter, bounceCounter, leftMax, rightMin);
                basket.CountScore = false;

                SpawnNextBasketPattern(basket.transform.position, basket);
            }

            if(CurrentBasket != null)
            {
                // not doing pooling cz its easier to just destroy and recreate stuff here
                // esp since there is a very little amount of stuff that is being recreated
                if (CurrentBasket.Pattern != null)
                    CurrentBasket.Pattern.DestroySelf(basketDestroyTime);
                else
                    CurrentBasket.DestroySelf(basketDestroyTime);
            }

            CurrentBasket = basket;
            CameraMaster.Instance.MinPosition = CurrentBasket.CameraMinPosition;

            if (CurrentBasket.Pattern != null)
                CurrentBasket.Pattern.DestroyNonBasketObjects(basketDestroyTime);
        }

        touchedBasketSide = false;
        bounceCounter = 0;

        ball.RG.simulated = false;
        ball.RG.velocity = new Vector2(0.0f, 0.0f);
        ball.RG.angularVelocity = 0.0f;

        float minSpeed = 2.0f;
        float speed = ball.RG.velocity.magnitude;
        speed = Mathf.Max(speed, minSpeed);
        StartCoroutine(MoveBallToRestPositionCoroutine(speed, CurrentBasket.BallRestPosition, CurrentBasket.MiddleBottomBone.transform));
    }

    public void DisableBallBounciness()
    {
        ball.RG.sharedMaterial = noBounceMaterial;
    }
    public void RestoreBallBounciness()
    {
        ball.RG.sharedMaterial = bounceMaterial;
    }

    public void Unstuck()
    {
        if(isFlying)
        {
            UIMaster.Instance.HideUnstuck();

            if(CurrentBasket != null)
                ResetBallToBasket(CurrentBasket);
            else
            {
                // stuff went very wrong if this is called
                ResetScene();
            }
        }
    }
    private void ResetBallToBasket(Basket basket)
    {
        basket.BallInsideRotation(0.1f);
        ball.transform.position = CurrentBasket.transform.position + Vector3.up;
        ball.RG.velocity = new Vector2(0.0f, 0.0f);
        ball.RG.angularVelocity = 0.0f;
    }

    private void SetFlying(bool flying)
    {
        this.isFlying = flying;
        if(flying)
        {
            flyingTimeoutCoroutine.Start(FlyingTimeOutCoroutine(flyingTimeOut));
        }
        else
        {
            flyingTimeoutCoroutine.Stop();
            UIMaster.Instance.HideUnstuck();
        }
    }
    private void SetDotsAlpha(float alpha)
    {
        foreach (TrajectoryDot dot in dots)
        {
            dot.Renderer.SetAlpha(alpha);
        }
    }
    private void HideDots()
    {
        foreach (TrajectoryDot dot in dots)
        {
            dot.gameObject.SetActive(false);
        }
    }

    public void SpawnNextBasketPattern(Vector3 fromPosition, Basket fromBasket)
    {
        BasketSpawnPosition nextPosition = BasketSpawnPosition.None;
        switch (fromBasket.SpawnPosition)
        {
            case BasketSpawnPosition.Left:
                nextPosition = GetSpawnFromLeft();
                break;
            case BasketSpawnPosition.Middle:
                nextPosition = GetSpawnFromMiddle();
                break;
            case BasketSpawnPosition.Right:
                nextPosition = GetSpawnFromRight();
                break;
            case BasketSpawnPosition.None:
                Debug.LogError("Game broke while trying to spawn next basket");
                break;
            default:
                break;
        }

        BasketPattern patternPrefab = GetRandomBasketPrefabPatternWithSpawnPosition(nextPosition);

        BasketPattern pattern = Instantiate(patternPrefab);

        pattern.Basket.SpawnPosition = nextPosition;
        PlaceBasketPattern(pattern, nextPosition, fromPosition);
        pattern.PopUp(basketDestroyTime);
    }
    private BasketSpawnPosition GetSpawnFromLeft()
    {
        int rng = Random.Range(0, 2);
        if (rng == 0)
            return BasketSpawnPosition.Middle;
        return BasketSpawnPosition.Right;
    }
    private BasketSpawnPosition GetSpawnFromMiddle()
    {
        int rng = Random.Range(0, 2);
        if (rng == 0)
            return BasketSpawnPosition.Right;
        return BasketSpawnPosition.Left;
    }
    private BasketSpawnPosition GetSpawnFromRight()
    {
        int rng = Random.Range(0, 2);
        if (rng == 0)
            return BasketSpawnPosition.Middle;
        return BasketSpawnPosition.Left;
    }

    private void PlaceBasketPattern(BasketPattern pattern, BasketSpawnPosition position, Vector3 prevPosition)
    {
        Vector3 leftWallMax = leftWall.bounds.max;
        Vector3 rightWallMin = rightWall.bounds.min;
        float wallGap = rightWallMin.x - leftWallMax.x;

        float xOffsetPerc = 0.1f;
        switch (position)
        {
            case BasketSpawnPosition.Left:
                xOffsetPerc = UnityEngine.Random.Range(0.1f, 0.25f);
                break;
            case BasketSpawnPosition.Middle:
                xOffsetPerc = UnityEngine.Random.Range(0.45f, 0.55f);
                break;
            case BasketSpawnPosition.Right:
                xOffsetPerc = UnityEngine.Random.Range(0.75f, 0.9f);
                break;
            default:
                break;
        }
        float xPosition = leftWallMax.x + wallGap * xOffsetPerc;

        Vector3 newPosition = prevPosition;
        newPosition.x = xPosition;
        newPosition.y += pattern.RandomSpawnOffset;

        float patternMostLeft = newPosition.x + pattern.GetLeftMostOffset();
        if (patternMostLeft < leftWallMax.x)
            newPosition.x += leftWallMax.x - patternMostLeft;

        float patternMostRight = newPosition.x + pattern.GetRightMostOffset();
        if (patternMostRight > rightWallMin.x)
            newPosition.x += rightWallMin.x - patternMostRight;

        Vector3 dir = newPosition - prevPosition;
        float dirLength = dir.magnitude;
        if (dirLength > maxBasketPatternSpawnLength)
        {
            Vector3 dirNormalized = dir.normalized;
            newPosition = prevPosition + dirNormalized * maxBasketPatternSpawnLength;
        }

        pattern.transform.position = newPosition;
    }

    public BasketPattern GetRandomBasketPrefabPatternWithSpawnPosition(BasketSpawnPosition singlePosition)
    {
        List<BasketPattern> goodPatterns = new List<BasketPattern>();
        foreach (BasketPattern bp in basketPatterns)
        {
            if (bp.PosslbeSpawnPositions.HasFlag(singlePosition))
            {
                goodPatterns.Add(bp);
            }
        }
        if (goodPatterns.Count == 0)
            throw new System.Exception(string.Format("There is no basket pattern with {0} spawn position", singlePosition));
        return goodPatterns.Random();
    }

    private void CreateDotsToCount()
    {
        int totalDots = steps / dotEverySteps;
        while (dots.Count < totalDots)
        {
            dots.Add(Instantiate(prefabDot));
        }
    }
    private void SimulateAndMarkTrajectory(Vector2 velocity, float angularVelocity)
    {
        HideDots();
        CreateDotsToCount();

        simulationBall.transform.SetPositionAndRotation(ball.transform.position, ball.transform.rotation);
        simulationBall.velocity = velocity;
        simulationBall.angularVelocity = angularVelocity;

        Debug.DrawLine(simulationBall.transform.position, ball.transform.position, Color.red);

        Vector3 scale = new Vector3(1.0f, 1.0f, 1.0f);

        int dotCounter = 0;
        int dotsListCounter = 0;
        for (int i = 0; i < steps; i++)
        {
            simulationPhysicsScene.Simulate(Time.fixedDeltaTime);

            simulationBall.angularVelocity = CalcAngularVelocity(simulationBall.velocity);

            dotCounter++;
            if (dotCounter == dotEverySteps)
            {
                dotCounter = 0;

                TrajectoryDot currentDot = dots[dotsListCounter];
                currentDot.transform.position = simulationBall.transform.position;
                currentDot.transform.localScale = scale;
                scale *= dotScaler;
                currentDot.gameObject.SetActive(true);

                dotsListCounter++;
            }
        }
    }

    public void SwitchToShopScene()
    {
        StartCoroutine(SwitchToSceneCoroutine(1));
    }

    #region Coroutines
    private IEnumerator MoveBallToRestPositionCoroutine(float speed, Transform restPosition, Transform toParent)
    {
        ballIsReady = false;

        Vector3 initialPosition = ball.transform.position;
        Vector3 direction = restPosition.position - ball.transform.position;
        float distance = direction.magnitude;
        float totalTime = distance / speed;
        float t = 0.0f;
        while (t < totalTime)
        {
            // not using this.Lerp cz restPosition.position can change 
            t += Time.deltaTime;

            float perc = t / totalTime;
            ball.transform.position = Vector3.Lerp(initialPosition, restPosition.position, perc);

            yield return null;
        }
        ball.transform.position = restPosition.position;

        ball.SetGraphicsParent(toParent);

        CurrentBasket.StartBounce(1.0f, touchedBasketSound);
        yield return CurrentBasket.bounceCoroutine.coroutine;

        ballIsReady = true;
    }
    private IEnumerator SwitchToSceneCoroutine(int buildIndex)
    {
        switchingScene = true;

        AsyncOperation op = SceneManager.UnloadSceneAsync(simulationScene);
        yield return UIMaster.Instance.FadeScreenCoroutine(0.2f, 0.0f, 1.0f);
        while (!op.isDone)
        {
            yield return null;
        }
        SceneManager.LoadScene(buildIndex);
    }
    private IEnumerator FlyingTimeOutCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        UIMaster.Instance.ShowUnstuck();
    }
    #endregion
}
