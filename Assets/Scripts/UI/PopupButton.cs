using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PopupButton : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private Button button;
    #endregion

    public Button Button
    {
        get { return button; }
    }

    public CoroutineHandler rescaleCoroutine;

    private void Awake()
    {
        rescaleCoroutine = new CoroutineHandler(this);
    }
    public void Popup(float time, Vector3 scale)
    {
        gameObject.SetActive(true);
        button.interactable = true;
        rescaleCoroutine.Start(RescaleCoroutine(time, Vector3.zero, scale, false));
    }
    public void Hide(float time)
    {
        if (!gameObject.activeSelf)
            return;
        button.interactable = false;
        rescaleCoroutine.Start(RescaleCoroutine(time, transform.localScale, Vector3.zero, true));
    }

    #region Coroutines
    public IEnumerator RescaleCoroutine(float time, Vector3 fromScale, Vector3 toScale, bool disableAfter)
    {
        yield return this.Lerp(time, fromScale, toScale, Vector3.Lerp, transform.SetLocalScale);
        if (disableAfter)
            gameObject.SetActive(false);
    }
    #endregion

    #region Editor
    private void Reset()
    {
        button = GetComponent<Button>();
    }
    #endregion
}

