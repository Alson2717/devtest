using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class DarkModeChangeText : DarkModeBase
{
    #region Inspector
    [SerializeField]
    private TextMeshProUGUI text;
    [SerializeField]
    private string lightModeText = "Darkmode is off";
    [SerializeField]
    private string darkModeText = "Darkmode is on";
    #endregion

    public override void OnDarkModeChanged(bool darkMode)
    {
        if (darkMode)
            text.text = darkModeText;
        else
            text.text = lightModeText;
    }

    #region Editor
    private void Reset()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
    #endregion
}

