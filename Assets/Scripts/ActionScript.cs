using System;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class ActionScript : MonoBehaviour
{
    public SteamVR_Input_Sources handType;
    public SteamVR_Behaviour_Pose controllerPose;
    public SteamVR_Action_Boolean grabAction;
    public SteamVR_Action_Vector2 touchPosition;
    public GameObject scrollbarGameObject;
    private Scrollbar scrollbar;
    private Vector3 hitPoint;
    private Transform hitTransform;
    private RaycastHit? nullableHit;

    void Start()
    {
        scrollbar = scrollbarGameObject.GetComponent<Scrollbar>();
    }
    
    void Update()
    {
        var y = touchPosition.axis.y;
        scrollbar.value = (float)Math.Max(0, Math.Min(1, scrollbar.value + y * 0.01));
        
        if (grabAction.GetStateDown(handType))
        {
            RaycastHit rayHit;
            if (Physics.Raycast(controllerPose.transform.position, transform.forward, out rayHit, 1000))
            {
                nullableHit = rayHit;
            }
            else
            {
                nullableHit = null;
            }
        }
        if (grabAction.GetStateUp(handType))
        {
            nullableHit = null;
        }

        if (nullableHit is RaycastHit hit)
        {
            // Controller movement (new position - old position)
            var controllerMovement = controllerPose.GetVelocity();
            // Controller rotation
            var quaternion = Quaternion.LookRotation(transform.forward, transform.up);
            var rotationMatrix = Matrix4x4.Rotate(quaternion).inverse;
            var normalizedControllerMovement = rotationMatrix.MultiplyPoint3x4(controllerMovement);
            
            hit.transform.RotateAround(hit.collider.bounds.center, Vector3.right, 4f * normalizedControllerMovement.y);
            hit.transform.RotateAround(hit.collider.bounds.center, Vector3.up, 4f * -normalizedControllerMovement.x);
        }

    }

    public bool GetGrab()
    {
        return grabAction.GetState(handType);
    }

}

