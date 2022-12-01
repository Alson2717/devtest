using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AudioObject : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private AudioSource source;
    #endregion

    public AudioSource Source
    {
        get { return source; }
    }

    public CoroutineHandler soundCoroutine;
    public bool returnToPool = true;

    private void Awake()
    {
        soundCoroutine = new CoroutineHandler(this);

        if (source == null)
            source = gameObject.AddComponent<AudioSource>();
    }

    public void Play(AudioClip clip, AudioSettings settings = default)
    {
        settings.clip = clip;
        Play(settings);
    }
    public void Play(AudioSettings settings)
    {
        settings.ApplyToSource(source);
        soundCoroutine.Start(PlaySoundCoroutine());
    }
    public void Stop()
    {
        soundCoroutine.Stop();
        Internal_Stop();
    }

    private void Internal_Stop()
    {
        source.Stop();
        if(returnToPool)
            AudioMaster.Instance.audioObjectsPool.Add(this);
    }

    public bool Callback_IsPlaying()
    {
        return source.isPlaying;
    }

    #region Coroutines
    public IEnumerator PlaySoundCoroutine()
    {
        source.Play();
        yield return new WaitWhile(Callback_IsPlaying);
        Internal_Stop();
    }
    #endregion
}

