using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Abyss : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == GameMaster.Instance.Ball.gameObject)
        {
            GameMaster.Instance.BallInsideAbyss();
        }
        //else
        //{
        //    BasketInputArea basket = collision.GetComponent<BasketInputArea>();
        //    if(basket != null && basket.Basket != GameMaster.Instance.CurrentBasket)
        //    {
        //        // not doing pooling cz too much stuff to reset and keep track of, easier to just recreat
        //        // esp since there is very little amount of baskets being created
        //        Destroy(basket.Basket.gameObject);
        //    }
        //}
    }
}

