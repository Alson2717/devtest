using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BasketPatternStar : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private new Collider2D collider;
    [SerializeField]
    private new SpriteRenderer renderer;
    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == GameMaster.Instance.Ball.gameObject)
        {
            UIMaster.Instance.AddStar(this);
            StartCoroutine(DisappearCoroutine(0.75f));
        }
    }

    #region Coroutines
    private IEnumerator DisappearCoroutine(float time)
    {
        transform.SetParent(null);

        LerpInstruction<float> alpha = new LerpInstruction<float>(renderer.color.a, 0.0f, Mathf.Lerp, renderer.SetAlpha);
        LerpInstruction<float> position = new LerpInstruction<float>();

        position.firstValue = transform.position.y;
        position.lastValue = position.firstValue + 0.5f;
        position.lerp = Mathf.Lerp;
        position.set = transform.SetWorldPositionY;

        yield return this.Lerp(time, alpha, position);
        Destroy(gameObject);
    }
    #endregion
}
