using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BasketBounceCollider : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private Basket basket;
    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!basket.IsResetingAfterDrag)
            basket.StartBounce(-1.0f, GameMaster.Instance.TouchedBasketSound);
    }
}

