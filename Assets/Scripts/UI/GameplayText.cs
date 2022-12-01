using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class GameplayText : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private TextMeshPro text;
    [SerializeField]
    private string quote = "PERFECT";
    #endregion

    public TextMeshPro Text
    {
        get { return text; }
    }

    public void SetValue(int value)
    {
        if (value == 1)
        {
            text.text = string.Format("{0}!", quote);
        }
        else
        {
            text.text = string.Format("{0} x{1}", quote, value);
        }
    }
}

