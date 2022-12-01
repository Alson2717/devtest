using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class DarkModeBase : MonoBehaviour
{
    #region Inspector

    #endregion

    private void Start()
    {
        if(UIMaster.Instance != null)
        {
            UIMaster.Instance.darkModeThings.Add(this);
            OnDarkModeChanged(UIMaster.Instance.DarkMode);
        }
        else
        {
            OnDarkModeChanged(UIMaster.IsDarkMode());
        }
    }
    private void OnDestroy()
    {
        if (UIMaster.Instance != null)
            UIMaster.Instance.darkModeThings.Remove(this);
    }


    public abstract void OnDarkModeChanged(bool darkMode);
}

