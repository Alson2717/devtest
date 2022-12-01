using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// nothing fancy here
public class CoroutineHandler
{
    public CoroutineHandler(MonoBehaviour component)
    {
        this.linkedComponent = component;
    }

    public MonoBehaviour linkedComponent;
    public Coroutine coroutine;

    public System.Action OnCoroutineStopped;

    public void Start(IEnumerator coroutine)
    {
        Stop();
        this.coroutine = linkedComponent.StartCoroutine(coroutine);
    }
    public void Stop()
    {
        if (this.coroutine != null)
        {
            linkedComponent.StopCoroutine(this.coroutine);
            this.coroutine = null;

            OnCoroutineStopped?.Invoke();
        }
    }
}

