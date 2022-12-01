using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BallSkin : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private Renderer[] renderers;
    #endregion

    public Renderer[] Renderers
    {
        get { return renderers; }
    }

    public Vector3 GetWorldMin2D()
    {
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0.0f);
        foreach (Renderer renderer in renderers)
        {
            Bounds aabb = renderer.bounds;
            Vector3 aabbMin = aabb.min;

            min.x = Mathf.Min(min.x, aabbMin.x);
            min.y = Mathf.Min(min.y, aabbMin.y);
        }
        return min;
    }
    public Vector3 GetWorldMax2D()
    {
        Vector3 max = new Vector3(float.MinValue, float.MinValue);
        foreach (Renderer renderer in renderers)
        {
            Bounds aabb = renderer.bounds;
            Vector3 aabbMax = aabb.max;

            max.x = Mathf.Max(max.x, aabbMax.x);
            max.y = Mathf.Max(max.y, aabbMax.y);
        }
        return max;
    }

    #region Editor
    private void Reset()
    {
        ContextMenu_GrabRenderers();
    }
    [ContextMenu("Grab Renderers")]
    public void ContextMenu_GrabRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }
    #endregion
}

