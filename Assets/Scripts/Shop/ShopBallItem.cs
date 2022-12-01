using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class ShopBallItem : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private SpriteRenderer graphicsBG;
    [SerializeField]
    private Collider2D graphicsAABB;
    [SerializeField]
    private Collider2D maxAABB;
    [SerializeField]
    private Transform graphicsParent;
    [SerializeField]
    private SpriteRenderer graphicsForeground;
    [SerializeField]
    private SpriteRenderer costBackground;
    [SerializeField]
    private TextMeshPro costText;
    #endregion

    public Bounds MaxAABB
    {
        get { return maxAABB.bounds; }
    }

    public BallSkinScriptable BallSkinItem
    {
        get;
        private set;
    }
    public int StarsCost
    {
        get;
        private set;
    } = 0;
    public bool Unlocked
    {
        get;
        private set;
    } = false;

    public void SetItem(BallSkinScriptable ballSkin, int starsCost, bool unlocked)
    {
        BallSkinItem = ballSkin;

        BallSkin graphics = Instantiate(ballSkin.SkinPrefab, graphicsParent);
        graphics.transform.localPosition = Vector3.zero;

        Vector3 outScaler;
        Vector3 outWPos;
        graphicsAABB.bounds.CalcFitToBoxScalerAndWPosition2D(
            graphics.GetWorldMin2D(),graphics.GetWorldMax2D(),
            out outScaler, out outWPos);

        float minScaler = Mathf.Min(outScaler.x, outScaler.y);
        graphics.transform.localScale *= minScaler;

        starsCost = Mathf.Max(0, starsCost);
        StarsCost = starsCost;

        costBackground.gameObject.SetActive(starsCost > 0 && !unlocked);
        if(starsCost > 0)
        {
            costText.text = starsCost.ToString();
        }

        Unlocked = unlocked;
        graphicsForeground.gameObject.SetActive(!unlocked);
    }

    private void OnMouseDown()
    {
        if (ShopMaster.Instance.IgnoreInput)
            return;

        if(!Unlocked)
        {
            int currentStars = UIMaster.GetStarsCount();
            if(currentStars >= StarsCost)
            {
                UIMaster.SetStarsCount(currentStars - StarsCost);
                ShopMaster.Instance.UpdateStarsText();

                ShopMaster.Instance.SaveUnlockedSkin(this.BallSkinItem);
                Unlocked = true;
                costBackground.gameObject.SetActive(false);
                graphicsForeground.gameObject.SetActive(false);

                SelectThisItem(false);

                AudioMaster.Instance.PlaySound(ShopMaster.Instance.SkinUnlockedSound);
            }
            else
            {
                AudioMaster.Instance.PlaySound(ShopMaster.Instance.NotEnoughtStarsSound);
            }
        }
        else
        {
            SelectThisItem(true);
        }
    }

    public void SelectThisItem(bool playSound)
    {
        if(ShopMaster.Instance.LastSelectedItem != null)
            ShopMaster.Instance.LastSelectedItem.RemoveSelection();
        ShopMaster.Instance.LastSelectedItem = this;
        MarkAsSelected();

        Ball.SetLastSkinID(BallSkinItem.ID);

        if(playSound)
        {
            if(AudioBank.Instance != null)
                AudioMaster.Instance.PlaySound(AudioBank.Instance.ButtonClickSound);
        }
    }

    public void MarkAsSelected()
    {
        graphicsBG.gameObject.SetActive(true);
    }
    public void RemoveSelection()
    {
        graphicsBG.gameObject.SetActive(false);
    }
}

