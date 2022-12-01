using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BounceCounter : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == GameMaster.Instance.Ball.gameObject)
            GameMaster.Instance.BallTouchedBounceCounter();
    }
}

