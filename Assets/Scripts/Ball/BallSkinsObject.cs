using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "BallSkins", menuName = "Ball Skins Object")]
public class BallSkinsObject : ScriptableObject
{
    #region Inspector
    [SerializeField]
    private BallSkinScriptable[] skins;
    #endregion

    public BallSkinScriptable[] Skins
    {
        get { return skins; }
    }

    public BallSkinScriptable FindBallSkinWithID(string id)
    {
        foreach (BallSkinScriptable skin in skins)
        {
            if (skin.ID.Equals(id))
                return skin;
        }
        return null;
    }
}
