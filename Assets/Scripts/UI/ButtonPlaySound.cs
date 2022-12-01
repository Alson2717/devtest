using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPlaySound : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private Button button;
    #endregion

    private void Start()
    {
        button.onClick.AddListener(Clicked);
    }
    private void Clicked()
    {
        if (AudioBank.Instance != null)
            AudioMaster.Instance.PlaySound(AudioBank.Instance.ButtonClickSound);
    }

    #region Editor
    private void Reset()
    {
        button = GetComponent<Button>();
    }
    #endregion
}

