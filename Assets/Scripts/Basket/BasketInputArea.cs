using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BasketInputArea : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private Basket basket;
    #endregion

    public Basket Basket
    {
        get { return basket; }
    }

    private bool colliding = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (colliding)
            return;

        Rigidbody2D rg = collision.GetComponent<Rigidbody2D>();
        if (rg != GameMaster.Instance.Ball.RG)
            return;
        if (!rg.simulated)
            return;

        colliding = true;
        GameMaster.Instance.DisableBallBounciness();

        GameMaster.Instance.BallInsideBasket(basket);
        basket.BallInsideRotation();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!colliding)
            return;

        Rigidbody2D rg = collision.GetComponent<Rigidbody2D>();
        if (rg != GameMaster.Instance.Ball.RG)
            return;
        if (!rg.simulated)
            return;

        colliding = false;
        GameMaster.Instance.RestoreBallBounciness();
    }
}

