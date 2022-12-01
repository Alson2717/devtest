using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CameraMaster : Singleton<CameraMaster>
{
    #region Inspector
    [SerializeField]
    private new Camera camera;
    [SerializeField]
    private Transform toFollow;
    [SerializeField]
    private float speed = 5.0f;
    [SerializeField]
    private float offset = 2.0f;
    [SerializeField]
    private SetOrthographicsAsAABBSize cameraAABB;
    #endregion

    public Camera Camera
    {
        get { return camera; }
    }
    public Transform MinPosition
    {
        get;
        set;
    }
    public SetOrthographicsAsAABBSize CameraAABB
    {
        get { return cameraAABB; }
    }

    [HideInInspector]
    public List<FollowCameraYPosition> followers = new List<FollowCameraYPosition>();

    protected override void SingletonAwake()
    {

    }
    protected override void SingletonDestroy()
    {
        
    }

    private void LateUpdate()
    {
        Vector3 position = transform.position;
        position.y = Mathf.Lerp(transform.position.y, toFollow.position.y + offset, speed * Time.deltaTime);
        if(MinPosition != null)
            position.y = Mathf.Max(MinPosition.position.y, position.y);
        transform.position = position;

        foreach (FollowCameraYPosition follower in followers)
        {
            follower.transform.SetWorldPositionY(position.y);
        }
    }
}

