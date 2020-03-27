using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float offset;
    public float moveSpeed;
    public float zoomSpeed;

    // use ass min and max y
    
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    private float screenWidth;
    private float screenHeight;
    //get camera postion
    public Vector3 cameraMove;

    // hte lower size to more forward it is
    //how you zomed out
    public float minZoom;
    // how you zomed in 
    public float maxZoom;

    public Camera mainCamera;


    // Use this for initialization
    void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        mainCamera = GetComponent<Camera>();
        //set this object postion to vector 3 ass it is the camera
        cameraMove.x = mainCamera.transform.position.x;
        cameraMove.y = mainCamera.transform.position.y;
        cameraMove.z = mainCamera.transform.position.z;
    }
    // Update is called once per frame
    void Update()
    {
        //Move camera

        CameraMove();
        cameraZoom();
    }

    void CameraMove()
    {// get postion 
        if ((Input.mousePosition.x > screenWidth - offset) && transform.position.x < maxX)
        {
            cameraMove.x += MoveSpeed();
        }
        if ((Input.mousePosition.x < offset) && transform.position.x > minX)
        {
            cameraMove.x -= MoveSpeed();
        }
        if ((Input.mousePosition.y > screenHeight - offset) && transform.position.y < maxY)
        {
            cameraMove.y += MoveSpeed();
        }
        if ((Input.mousePosition.y < offset) && transform.position.y > minY)
        {
            cameraMove.y -= MoveSpeed();
        }
        transform.position = cameraMove;
    }
    void cameraZoom()
    {

        if (Input.GetAxis("Mouse ScrollWheel") > 0f && mainCamera.orthographicSize > maxZoom)
        {
            Debug.Log("i moved the up mousewheel");
            mainCamera.orthographicSize -= ZoomSpeed();
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f && mainCamera.orthographicSize < minZoom)
        {
            mainCamera.orthographicSize += ZoomSpeed();
            Debug.Log("i moved the down mousewheel");
        }

    }





    float MoveSpeed()
    {
        return moveSpeed * Time.deltaTime;
    }
    float ZoomSpeed()
    {
        return zoomSpeed * Time.deltaTime;
    }
}
