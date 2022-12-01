using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class AudioChangedBase : MonoBehaviour
{
    private void Start()
    {
        AudioMaster.Instance.audioChangedObjects.Add(this);
        OnAudioEnabledChanged(AudioMaster.Instance.SoundIsOn);        
    }
    private void OnDestroy()
    {
        if (AudioMaster.Instance != null)
            AudioMaster.Instance.audioChangedObjects.Remove(this);
    }

    public abstract void OnAudioEnabledChanged(bool enable);
}

