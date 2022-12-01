using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChangeSortingOrder : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private new Renderer renderer;
    [SerializeField]
    private int sortingOrder;
    #endregion


    #region Editor
    private void Reset()
    {
        renderer = GetComponent<Renderer>();
        if (renderer != null)
            sortingOrder = renderer.sortingOrder;
    }
    private void OnValidate()
    {
        if (renderer != null)
        {
            renderer.sortingOrder = sortingOrder;
        }
    }
    #endregion
}

