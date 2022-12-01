using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FollowCameraYPosition : MonoBehaviour
{
    private void Start()
    {
        CameraMaster.Instance.followers.Add(this);
    }
    private void OnDestroy()
    {
        if (CameraMaster.Instance != null)
            CameraMaster.Instance.followers.Remove(this);
    }
}

