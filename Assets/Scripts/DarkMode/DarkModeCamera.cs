using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DarkModeCamera : DarkModeBase
{
    #region Inspector
    [SerializeField]
    private new Camera camera;
    #endregion

    public override void OnDarkModeChanged(bool darkMode)
    {
        if (darkMode)
            camera.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
        else
            camera.backgroundColor = new Color(1.0f, 1.0f, 1.0f);
    }

    #region Editor
    private void Reset()
    {
        camera = GetComponent<Camera>();
    }
    #endregion
}

