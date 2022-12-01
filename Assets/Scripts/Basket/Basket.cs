using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Basket : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private BasketInputArea inputArea;
    [SerializeField]
    private Transform middleBottomBone;
    [SerializeField]
    private Transform ballRestPosition;
    [SerializeField]
    private Transform cameraMinPosition;
    [SerializeField]
    private bool countScore = true;
    [SerializeField]
    private BasketPattern pattern;
    [SerializeField]
    private BasketSpawnPositionBSWorkaround initialSpawnPosition;
    #endregion

    public Transform MiddleBottomBone
    {
        get { return middleBottomBone; }
    }
    public Transform BallRestPosition
    {
        get { return ballRestPosition; }
    }
    public Transform CameraMinPosition
    {
        get { return cameraMinPosition; }
    }
    public bool CountScore
    {
        get { return countScore; }
        set { countScore = value; }
    }
    public BasketPattern Pattern
    {
        get { return pattern; }
    }

    public BasketSpawnPosition SpawnPosition
    {
        get;
        set;
    }

    public bool IsResetingAfterDrag
    {
        get;
        private set;
    } = false;

    public CoroutineHandler bounceCoroutine;
    public CoroutineHandler rotationCoroutine;

    private float bonesOffset = 0.0f;
    private void Awake()
    {
        SpawnPosition = initialSpawnPosition.Combine();
    }
    private void Start()
    {
        bounceCoroutine = new CoroutineHandler(this);
        bounceCoroutine.OnCoroutineStopped = Callback_OnBounceCoroutineStopped;

        rotationCoroutine = new CoroutineHandler(this);
    }

    public void StartBounce(float direction, AudioSettings sound)
    {
        if(sound.clip != null)
            AudioMaster.Instance.PlaySound(sound);
        bounceCoroutine.Start(BallInsideBounceCoroutine(0.1f, direction));
    }
    public void BallInsideRotation(float time = 0.2f)
    {
        rotationCoroutine.Start(BallInsideRotationCoroutine(time));
    }

    public void BonesDrag(float yVelocityPerc)
    {
        AddBonesOffset(-bonesOffset);
        bonesOffset = GameMaster.Instance.BasketBonesMaxDrag * yVelocityPerc;
        AddBonesOffset(bonesOffset);
    }
    public void EndBonesDrag()
    {
        bounceCoroutine.Start(EndDragCoroutine());
    }

    public void Popup(float time, Vector3 scale)
    {
        StartCoroutine(PopupCoroutine(time, scale));
    }
    public void DestroySelf(float time)
    {
        StartCoroutine(DestroySelfCoroutine(time));
    }

    private void AddBonesOffset(float offset)
    {
        middleBottomBone.localPosition += new Vector3(0.0f, offset, 0.0f);
    }

    private void Callback_OnBounceCoroutineStopped()
    {
        BonesDrag(0.0f);
    }

    #region Coroutines
    private IEnumerator EndDragCoroutine()
    {
        IsResetingAfterDrag = false;

        float perc = bonesOffset / GameMaster.Instance.BasketBonesMaxDrag;
        float percPerc = perc * 0.33f;
        yield return this.Lerp(0.06f, perc, -percPerc, Mathf.Lerp, BonesDrag);
        yield return this.Lerp(0.2f, -percPerc, percPerc, Mathf.Lerp, BonesDrag);
        yield return this.Lerp(0.2f, percPerc, 0.0f, Mathf.Lerp, BonesDrag);

        bonesOffset = 0;

        IsResetingAfterDrag = true;
    }
    private IEnumerator BallInsideBounceCoroutine(float time, float direction)
    {
        float firstPartPerc = 0.7f;

        LerpInstruction<float> bounceInstruction = new LerpInstruction<float>();
        bounceInstruction.lerp = Mathf.Lerp;
        bounceInstruction.set = BonesDrag;
        bounceInstruction.firstValue = 0.0f;
        bounceInstruction.lastValue = 0.4f * direction;

        yield return this.Lerp(time * firstPartPerc, bounceInstruction);

        bounceInstruction.firstValue = bounceInstruction.lastValue;
        bounceInstruction.lastValue = 0.0f;
        yield return this.Lerp(time * (1.0f - firstPartPerc), bounceInstruction);
    }
    private IEnumerator BallInsideRotationCoroutine(float time)
    {
        yield return this.Lerp(time, transform.rotation, Quaternion.identity, Quaternion.Lerp, transform.SetRotation);
    }
    private IEnumerator PopupCoroutine(float time, Vector3 scale)
    {
        yield return this.Lerp(time, Vector3.zero, scale, Vector3.Lerp, transform.SetLocalScale);
    }
    private IEnumerator DestroySelfCoroutine(float time)
    {
        yield return this.Lerp(time, transform.localScale, Vector3.zero, Vector3.Lerp, transform.SetLocalScale);
        Destroy(gameObject);
    }
    #endregion
}
