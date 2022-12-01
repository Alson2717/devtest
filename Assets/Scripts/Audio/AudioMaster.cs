using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;  

public class AudioMaster : Singleton<AudioMaster>
{
    public const string KEY_AUDIO_IS_ON = "AudioIsOn";
    public const int AUDIO_ON_VALUE = 0;
    public const int AUDIO_OFF_VALUE = 1;

    #region Inspector
    [SerializeField]
    private AudioObject prefabAudioObject;
    #endregion

    public Pool<AudioObject> audioObjectsPool = new Pool<AudioObject>();

    public bool SoundIsOn
    {
        get;
        private set;
    } = true;

    [HideInInspector]
    public List<AudioChangedBase> audioChangedObjects = new List<AudioChangedBase>();

    protected override void SingletonAwake()
    {
        if(prefabAudioObject == null)
        {
            GameObject go = new GameObject("PrefabAudioObject");
            prefabAudioObject = go.AddComponent<AudioObject>();
        }
    }
    protected override void SingletonDestroy()
    {
        
    }
    private void Start()
    {
        // doing this to ensure that all audiochangedbase stuff is subscribed
        StartCoroutine(NextFrameSetAudio());
    }

    public void ToggleSounds()
    {
        EnableSounds(!SoundIsOn);
    }
    public void EnableSounds(bool enable)
    {
        SoundIsOn = enable;
        PlayerPrefs.SetInt(KEY_AUDIO_IS_ON, enable ? AUDIO_ON_VALUE : AUDIO_OFF_VALUE);

        foreach (AudioChangedBase a in audioChangedObjects)
        {
            a.OnAudioEnabledChanged(SoundIsOn);
        }
    }

    public AudioObject PlaySound(AudioSettings audio)
    {
        if (!SoundIsOn)
            return null;
        AudioObject obj = audioObjectsPool.ExtractFirst(prefabAudioObject);
        obj.Play(audio);
        return obj;
    }

    #region Coroutines
    private IEnumerator NextFrameSetAudio()
    {
        yield return null;
        bool set = PlayerPrefs.GetInt(KEY_AUDIO_IS_ON, AUDIO_ON_VALUE) == AUDIO_ON_VALUE;
        EnableSounds(set);
    }
    #endregion
}

