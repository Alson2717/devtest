using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BasketPattern : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private Basket basket;
    [SerializeField]
    private Collider2D[] allColliders;
    [SerializeField]
    private BasketSpawnPositionBSWorkaround possibleSpawnPositions;
    [SerializeField]
    private float minSpawnOffset = 2.0f;
    [SerializeField]
    private float maxSpawnOffset = 5.0f;
    [SerializeField]
    private BasketPatternObject[] nonBasketObjects;
    [SerializeField]
    private BasketPatternStar star;
    [SerializeField]
    private float starChance = 0.15f;
    #endregion

    public Basket Basket
    {
        get { return basket; }
    }
    public BasketSpawnPosition PosslbeSpawnPositions
    {
        get { return possibleSpawnPositions.Combine(); }
    }
    public float MinSpawnOffset
    {
        get { return minSpawnOffset; }
    }
    public float MaxSpawnOffset
    {
        get { return maxSpawnOffset; }
    }
    public float RandomSpawnOffset
    {
        get { return UnityEngine.Random.Range(minSpawnOffset, maxSpawnOffset); }
    }

    public CoroutineHandler rescaleCoroutine;


    private void Awake()
    {
        rescaleCoroutine = new CoroutineHandler(this);

        if(star != null)
        {
            // star failed to spawn
            if (Random.Range(0.0f, 1.0f) > starChance)
                star.gameObject.SetActive(false);
        }
    }

    public void DestroySelf(float time)
    {
        PopOut(time);
    }
    public void DestroyNonBasketObjects(float time)
    {
        foreach (BasketPatternObject o in nonBasketObjects)
        {
            o.Disappear(time);
        }
    }

    public float GetLeftMostOffset()
    {
        float min = float.MaxValue;
        foreach (Collider2D collider in allColliders)
        {
            float offset = collider.bounds.min.x - transform.position.x;
            min = Mathf.Min(min, offset);
        }
        return min;
    }
    public float GetRightMostOffset()
    {
        float max = float.MinValue;
        foreach (Collider2D collider in allColliders)
        {
            float offset = collider.bounds.max.x - transform.position.x;
            max = Mathf.Max(max, offset);
        }
        return max;
    }

    public void PopUp(float time)
    {
        rescaleCoroutine.Start(RescaleCoroutine(time, Vector3.zero, transform.localScale, false));
    }
    public void PopOut(float time)
    {
        rescaleCoroutine.Start(RescaleCoroutine(time, transform.localScale, Vector3.zero, true));
    }

    #region Coroutines
    public IEnumerator RescaleCoroutine(float time, Vector3 fromScale, Vector3 toScale, bool destroyAfter)
    {
        yield return this.Lerp(time, fromScale, toScale, Vector3.Lerp, transform.SetLocalScale);
        if(destroyAfter)
            Destroy(gameObject);
    }
    #endregion

    #region Editor
    private void Reset()
    {
        ContextMenu_GrabColliders();
    }
    [ContextMenu("GrabColliders")]
    public void ContextMenu_GrabColliders()
    {
        this.allColliders = GetComponentsInChildren<Collider2D>();
    }

    private void OnDrawGizmos()
    {
        float left = GetLeftMostOffset();
        float right = GetRightMostOffset();

        Vector3 position = transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(position + new Vector3(left, 0.0f), position + new Vector3(right, 0.0f));
    }
    #endregion
}

