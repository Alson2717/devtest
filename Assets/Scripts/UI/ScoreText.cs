using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class ScoreText : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private TextMeshPro text;
    #endregion

    public TextMeshPro Text
    {
        get { return text; }
    }

    public void SetScore(int value)
    {
        text.text = string.Format("+{0}", value);
    }
}

