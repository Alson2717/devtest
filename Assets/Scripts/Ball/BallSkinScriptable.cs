using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "BallSkin", menuName = "Ball Skin")]
public class BallSkinScriptable : ScriptableObject
{
    #region Inspector
    [SerializeField]
    private BallSkin skinPrefab;
    [SerializeField]
    private AudioSettings[] hitSounds;
    [SerializeField]
    private Color color = Color.white;
    [SerializeField]
    private string id = System.Guid.NewGuid().ToString();
    #endregion

    public BallSkin SkinPrefab
    {
        get { return skinPrefab; }
    }
    public AudioSettings[] HitSounds
    {
        get { return hitSounds; }
    }
    public Color Color
    {
        get { return color; }
    }
    public string ID
    {
        get { return id; }
    }

    public AudioSettings RandomHitSound
    {
        get { return hitSounds.Random(); }
    }
}

