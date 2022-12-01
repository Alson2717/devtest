using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public struct AudioSettings
{
    [SerializeField]
    public AudioClip clip;
    [SerializeField]
    public float volume;
    [SerializeField]
    public float pitch;
    [SerializeField]
    public float spatialBlend;
    [SerializeField]
    public bool loop;

    public void ApplyToSource(AudioSource s)
    {
        s.clip = clip;
        s.volume = volume;
        s.pitch = pitch;
        s.spatialBlend = spatialBlend;
        s.loop = loop;
    }
    public void ApplyToSourceAndPlay(AudioSource s)
    {
        ApplyToSource(s);
        s.Play();
    }
}

