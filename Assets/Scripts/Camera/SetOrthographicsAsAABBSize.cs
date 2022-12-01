using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SetOrthographicsAsAABBSize : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private new Camera camera;
    [SerializeField]
    private bool setOrthographicsInAwakeOrStart = true;
    [SerializeField]
    private Vector2 targetAspect = new Vector2(18, 9);
    [SerializeField]
    private float size = 1.0f;
    [SerializeField]
    private Vector2[] setOnlyForAspects;
    [SerializeField]
    private float horizontalLine = -5.0f; // its actually horizontal
    [SerializeField]
    private bool alignWithHorizontalLine = false;
    [SerializeField]
    private bool setOnlyWhenSmaller = false;
    [SerializeField]
    private bool setOnlyWhenBigger = false;
    [SerializeField]
    private bool dontUseSafeArea = false;
    #endregion

    public Camera Camera
    {
        get { return camera; }
    }


    private void Awake()
    {
        if (!setOrthographicsInAwakeOrStart)
            return;

        float current = CalcCurrentSize();
        if (setOnlyForAspects != null && setOnlyForAspects.Length > 0)
        {
            float aspect = Screen.safeArea.width / Screen.safeArea.height;
            if (dontUseSafeArea)
                aspect = ((float)Screen.width) / ((float)Screen.height);
            foreach (Vector2 a in setOnlyForAspects)
            {
                if (Mathf.Abs(aspect - (a.x / a.y)) < 0.005f)
                {
                    if (setOnlyWhenSmaller)
                    {
                        if (current < size)
                            SetOrthoGraphicSize();
                    }
                    else if (setOnlyWhenBigger)
                    {
                        if (current > size)
                            SetOrthoGraphicSize();
                    }
                    else
                        SetOrthoGraphicSize();
                    break;
                }
            }
        }
        else
        {
            if (setOnlyWhenSmaller)
            {
                if (current < size)
                    SetOrthoGraphicSize();
            }
            else if (setOnlyWhenBigger)
            {
                if (current > size)
                    SetOrthoGraphicSize();
            }
            else
                SetOrthoGraphicSize();
        }

        if (alignWithHorizontalLine)
            AlignWithVerticalLine();
    }

    public float CalcCurrentSize()
    {
        float height = Screen.safeArea.height;
        float width = Screen.safeArea.width;
        if (dontUseSafeArea)
        {
            height = Screen.height;
            width = Screen.width;
        }
        return camera.orthographicSize / (height / width) * 2.0f;
    }

    public void SetOrthoGraphicSize()
    {
        float height = Screen.safeArea.height;
        float width = Screen.safeArea.width;
        if (dontUseSafeArea)
        {
            height = Screen.height;
            width = Screen.width;
        }
        camera.orthographicSize = size * (height / width) * 0.5f;
    }
    public void AlignWithVerticalLine()
    {
        if (horizontalLine > camera.transform.position.y)
            camera.transform.SetWorldPositionY(horizontalLine - camera.orthographicSize);
        else
            camera.transform.SetWorldPositionY(horizontalLine + camera.orthographicSize);
    }

    #region ContextMenu
    [ContextMenu("Calc aabb size from current camera and screen")]
    private void ContextMenu_CalcSize()
    {
        size = camera.orthographicSize / (targetAspect.y / targetAspect.x) * 2f;
    }
    #endregion

    #region Editor
    private void Reset()
    {
        camera = GetComponent<Camera>();
        if (camera != null)
        {
            ContextMenu_CalcSize();
            horizontalLine = camera.transform.position.y;
        }
        else
            horizontalLine = transform.position.y;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        UnityEngine.Camera c = UnityEngine.Camera.current;
        if (c == null)
            return;

        Gizmos.color = Color.red;
        Vector3 lb = c.ScreenToWorldPoint(Vector3.zero);
        Vector3 tr = c.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0.0f));

        Vector3 first = new Vector3(transform.position.x + size / 2, lb.y, 0.0f);
        Vector3 second = new Vector3(transform.position.x + size / 2, tr.y, 0.0f);
        Gizmos.DrawLine(first, second);

        first.x = transform.position.x - size / 2;
        second.x = transform.position.x - size / 2;
        Gizmos.DrawLine(first, second);

        first = new Vector3(lb.x, horizontalLine, 0.0f);
        second = new Vector3(tr.x, horizontalLine, 0.0f);
        Gizmos.DrawLine(first, second);
    }
    #endregion
}

