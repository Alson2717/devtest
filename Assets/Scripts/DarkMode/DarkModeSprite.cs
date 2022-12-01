using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DarkModeSprite : DarkModeBase
{
    #region Inspector
    [SerializeField]
    private new SpriteRenderer renderer;
    #endregion

    public SpriteRenderer Renderer
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
        renderer = GetComponent<SpriteRenderer>();
    }
    #endregion
}
