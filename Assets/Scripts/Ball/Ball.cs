using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public const string KEY_LAST_SKIN_ID = "LastSkinID";

    #region Inspector
    [Header("Stuff")]
    [SerializeField]
    private Rigidbody2D rg;
    [SerializeField]
    private new Collider2D collider;
    [SerializeField]
    private Transform graphicsParent;

    [Header("Skins")]
    [SerializeField]
    private BallSkinScriptable defaultSkin;
    [SerializeField]
    private BallSkinsObject[] allSkins;
    #endregion

    public Rigidbody2D RG
    {
        get { return rg; }
    }
    public Collider2D Collider
    {
        get { return collider; }
    }

    public BallSkinScriptable DefaultSkin
    {
        get { return defaultSkin; }
    }
    public BallSkinsObject[] AllSkins
    {
        get { return allSkins; }
    }
    
    public BallSkinScriptable CurrentScriptableSkin
    {
        get;
        private set;
    }
    public BallSkin CurrentSkin
    {
        get;
        private set;
    }

    private void Start()
    {
        string lastID = GetLastSkinID();

        BallSkinScriptable lastSkin = null;
        foreach (BallSkinsObject obj in allSkins)
        {
            lastSkin = obj.FindBallSkinWithID(lastID);
            if (lastSkin != null)
                break;
        }
        if (lastSkin == null)
            lastSkin = defaultSkin;
        SetSkin(lastSkin);
    }

    public void SetSkin(BallSkinScriptable skin)
    {
        if(CurrentSkin != null)
        {
            Destroy(CurrentSkin.gameObject);
        }

        CurrentScriptableSkin = skin;
        CurrentSkin = Instantiate(skin.SkinPrefab, graphicsParent);
        CurrentSkin.transform.localPosition = Vector3.zero;

        Vector3 skinWorldMin = CurrentSkin.GetWorldMin2D();
        Vector3 skinWorldMax = CurrentSkin.GetWorldMax2D();

        Vector3 outScaler;
        Vector3 outWorldPosition;
        collider.bounds.CalcFitToBoxScalerAndWPosition2D(skinWorldMin, skinWorldMax,
            out outScaler, out outWorldPosition);

        // in case if actual skin size is not uniform
        float minScaler = Mathf.Min(outScaler.x, outScaler.y);

        CurrentSkin.transform.localScale *= minScaler;

        SetLastSkinID(skin.ID);
    }

    public void SetGraphicsParent(Transform parent)
    {
        CurrentSkin.transform.SetParent(parent);
    }
    public void RestoreGraphicsParent()
    {
        CurrentSkin.transform.SetParent(graphicsParent);
        CurrentSkin.transform.localPosition = Vector3.zero;
    }

    public static void SetLastSkinID(string id)
    {
        PlayerPrefs.SetString(KEY_LAST_SKIN_ID, id);
    }
    public static string GetLastSkinID()
    {
        return PlayerPrefs.GetString(KEY_LAST_SKIN_ID, "");
    }
}

