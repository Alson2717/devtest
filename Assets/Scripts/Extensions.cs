using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public static class Extensions
{
    public static T ExtractRandom<T>(this IList<T> me)
    {
        return me.ExtractAt(me.RandomIndex());
    }
    public static T ExtractAt<T>(this IList<T> me, int index)
    {
        T value = me[index];
        me.RemoveAt(index);
        return value;
    }
    public static int RandomIndex<T>(this IList<T> me)
    {
        return UnityEngine.Random.Range(0, me.Count);
    }
    public static T Random<T>(this IList<T> me)
    {
        return me[me.RandomIndex()];
    }

    public static Vector3 InputToWorld(this Camera me)
    {
        return me.ScreenToWorldPoint(Input.mousePosition);
    }
    public static Vector3 InputToWorld(this Camera me, float z)
    {
        Vector3 value = InputToWorld(me);
        value.z = z;
        return value;
    }

    public static Vector3 LookAt2DEuler(this Transform me, Vector2 lookAtPosition)
    {
        lookAtPosition = new Vector2(lookAtPosition.x - me.position.x, lookAtPosition.y - me.position.y);
        return new Vector3(0.0f, 0.0f, Mathf.Atan2(lookAtPosition.y, lookAtPosition.x) * Mathf.Rad2Deg);
    }

    public static Vector2 Clamp(this Vector2 me, Vector2 min, Vector2 max)
    {
        Vector2 result;
        result.x = Mathf.Clamp(me.x, min.x, max.x);
        result.y = Mathf.Clamp(me.y, min.y, max.y);
        return result;
    }
    public static bool Approximately2D(this Vector3 me, Vector3 other)
    {
        return Mathf.Abs(me.x - other.x) < 0.01f && Mathf.Abs(me.y - other.y) < 0.01f;
    }
    public static bool Approximately2D(this Vector2 me, Vector2 other)
    {
        return Mathf.Abs(me.x - other.x) < 0.01f && Mathf.Abs(me.y - other.y) < 0.01f;
    }

    public static void SetAlpha(this SpriteRenderer me, float alpha)
    {
        Color c = me.color;
        c.a = alpha;
        me.color = c;
    }
    public static void SetAlpha(this TextMeshPro me, float alpha)
    {
        Color c = me.color;
        c.a = alpha;
        me.color = c;
    }
    public static void SetAlpha(this Image me, float alpha)
    {
        Color c = me.color;
        c.a = alpha;
        me.color = c;
    }
    public static void SetAlpha(this CanvasGroup me, float alpha)
    {
        me.alpha = alpha;
    }

    public static void SetWorldPosition(this Transform me, Vector3 value)
    {
        me.transform.position = value;
    }
    public static void SetWorldPositionY(this Transform me, float value)
    {
        Vector3 position = me.position;
        position.y = value;
        me.position = position;
    }
    public static void SetRotation(this Transform me, Quaternion value)
    {
        me.rotation = value;
    }
    public static void SetLocalScale(this Transform me, Vector3 scale)
    {
        me.localScale = scale;
    }

    public static void CalcFitToBoxScalerAndWPosition2D(this Bounds aabb,
    Vector3 worldMin, Vector3 worldMax,
    out Vector3 scaler, out Vector3 worldPosition)
    {
        CalcFitToBoxScalerAndWPosition2D(aabb, aabb.min, aabb.max,
            worldMin, worldMax,
            out scaler, out worldPosition);
    }
    public static void CalcFitToBoxScalerAndWPosition2D(this object me,
        Vector3 currentWorldMin, Vector3 currentWorldMax,
        Vector3 worldMin, Vector3 worldMax,
        out Vector3 scaler, out Vector3 worldPosition)
    {
        if (worldMin.x > worldMax.x)
        {
            float t = worldMin.x;
            worldMin.x = worldMax.x;
            worldMax.x = t;
        }
        if (worldMin.y > worldMax.y)
        {
            float t = worldMin.y;
            worldMin.y = worldMax.y;
            worldMax.y = t;
        }

        Vector3 aabbDif = currentWorldMax - currentWorldMin;
        Vector3 boxDif = worldMax - worldMin;

        scaler = new Vector3(boxDif.x / aabbDif.x, boxDif.y / aabbDif.y);
        worldPosition = (worldMax + worldMin) * 0.5f;

        scaler.x = 1.0f / scaler.x;
        scaler.y = 1.0f / scaler.y;
    }

    public static bool IsPointerOverUIObject(this object me)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return EventSystem.current.IsPointerOverGameObject();
#elif UNITY_ANDROID || UNITY_IOS
        foreach (Touch touch in Input.touches)
        {
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return true;
            }
        }
        return false;
#endif
    }
}