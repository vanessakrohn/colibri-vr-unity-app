using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;


public class LaserScript : MonoBehaviour
{
    public SteamVR_Input_Sources handType;
    public SteamVR_Behaviour_Pose controllerPose;
    public SteamVR_Action_Boolean grabAction;
    public SteamVR_Action_Boolean clickAction;
    public SteamVR_Action_Boolean menuAction;

    public GameObject laserPrefab; 
    private GameObject laser; 
    private Transform laserTransform; 
    private Vector3 hitPoint; 

    private Material shaderRed;
    private Material shaderWhite;

    private Renderer laserRenderer;

    public GameObject canvas;
    public GameObject renderer;
    
    void Start()
    {
        
        laser = Instantiate(laserPrefab);
        
        laserTransform = laser.transform;

        shaderRed = new Material(Shader.Find("Unlit/Color"));
        shaderRed.color = Color.red;
        shaderWhite = new Material(Shader.Find("Unlit/Color"));
        shaderWhite.color = Color.white;
        
        laserRenderer = laser.GetComponent<Renderer>();

    }
    
    void Update()
    {
        RaycastHit hit;

        
        if (Physics.Raycast(controllerPose.transform.position, transform.forward, out hit, 1000))
        {
            if(clickAction.GetState(handType)){
                var button = hit.collider.GetComponent<Button>();
                if(button){
                    button.onClick.Invoke();
                }
            }
            ShowLaser(hit.point, hit.distance);
            laserRenderer.material = shaderWhite;
        }
        else 
        {
            ShowLaser(controllerPose.transform.position + transform.forward * 30, 30);
            laserRenderer.material = shaderRed;
        }

        if(menuAction.GetStateDown(handType)){
            canvas.SetActive(!canvas.activeSelf);
            renderer.SetActive(!renderer.activeSelf);
        }
    }




    private void ShowLaser(Vector3 point, float distance)
    {
        
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(controllerPose.transform.position, point, .5f);
        laserTransform.LookAt(point);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x,
                                                laserTransform.localScale.y,
                                                distance);
    }

}
