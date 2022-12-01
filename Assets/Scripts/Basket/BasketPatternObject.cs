using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BasketPatternObject : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private Collider2D[] colliders;
    #endregion

    public Collider2D[] Colliders
    {
        get { return colliders; }
    }

    public void Disappear(float time)
    {
        StartCoroutine(DisappearCoroutine(time));
    }

    #region Inspector
    public IEnumerator DisappearCoroutine(float time)
    {
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        yield return this.Lerp(time, transform.localScale, Vector3.zero, Vector3.Lerp, transform.SetLocalScale);
        Destroy(gameObject);
    }
    #endregion
}

