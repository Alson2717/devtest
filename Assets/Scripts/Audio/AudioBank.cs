using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AudioBank : Singleton<AudioBank>
{
    #region Inspector
    [SerializeField]
    private AudioSettings buttonClickSound;
    #endregion

    public AudioSettings ButtonClickSound
    {
        get { return buttonClickSound; }
    }

    protected override void SingletonAwake()
    {
        DontDestroyOnLoad(gameObject);
    }
    protected override void SingletonDestroy()
    {
        
    }
}

