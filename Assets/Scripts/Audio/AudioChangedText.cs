using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class AudioChangedText : AudioChangedBase
{
    #region Inspector
    [SerializeField]
    private TextMeshProUGUI text;
    [SerializeField]
    private string soundOnText = "Audio is on";
    [SerializeField]
    private string soundOffText = "Audio is off"; 
    #endregion

    public override void OnAudioEnabledChanged(bool enable)
    {
        if (enable)
            text.text = soundOnText;
        else
            text.text = soundOffText;
    }

    #region Editor
    private void Reset()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
    #endregion
}

