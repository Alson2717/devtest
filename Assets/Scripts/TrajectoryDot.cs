using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrajectoryDot : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private new SpriteRenderer renderer;
    #endregion

    public SpriteRenderer Renderer
    {
        get { return renderer; }
    }
}

