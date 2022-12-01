using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DarkModeImage : DarkModeBase
{
    #region Inspector
    [SerializeField]
    private new Image renderer;
    #endregion

    public Image Renderer
    {
        get { return renderer; }
    }

    public override void OnDarkModeChanged(bool darkMode)
    {
        Color c = CameraMaster.Instance.Camera.backgroundColor;
        c.a = renderer.color.a;
        renderer.color = c;
    }

    #region Editor
    private void Reset()
    {
        renderer = GetComponent<Image>();
    }
    #endregion
}
