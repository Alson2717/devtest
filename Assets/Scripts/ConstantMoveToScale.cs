using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ConstantMoveToScale : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private Vector3 targetScale;
    [SerializeField]
    private float speed = 2.0f;
    #endregion

    public Vector3 TargetScale
    {
        get { return targetScale; }
        set { targetScale = value; }
    }
    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }



    private void Update()
    {
        transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, speed * Time.deltaTime);
    }

    #region Editor
    private void Reset()
    {
        targetScale = transform.localScale;
    }
    #endregion  
}

