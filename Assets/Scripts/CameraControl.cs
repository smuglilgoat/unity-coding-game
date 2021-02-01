using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour{
    private float camera_zoom = 0f;
    private float camera_zoom_speed = 0.6f;
    private float camera_pan_speed = 10f;

    private float camera_min_pos = 21.25f;
    private Vector3 camera_min_rotation = new Vector3(60f, 0f, 0f);
    private float camera_max_pos = 12f;
    private Vector3 camera_max_rotation = new Vector3(47f, 0f, 0f);

    void Update(){
        if(Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0f){
            camera_zoom += Input.GetAxis("Mouse ScrollWheel")*camera_zoom_speed;
            camera_zoom = Mathf.Clamp(camera_zoom, 0, 1);
            transform.position = Vector3.Lerp(new Vector3(transform.position.x, camera_min_pos, transform.position.z),
                                            new Vector3(transform.position.x, camera_max_pos, transform.position.z),
                                            camera_zoom);
            transform.rotation = Quaternion.Lerp(
                                    Quaternion.Euler(camera_min_rotation),
                                    Quaternion.Euler(camera_max_rotation), camera_zoom);
        }

        if(Input.mousePosition.x <= 0){
            transform.position += Vector3.left*camera_pan_speed*Time.deltaTime;
        }
        else if(Input.mousePosition.x > Screen.width){
            transform.position += Vector3.right*camera_pan_speed*Time.deltaTime;
        }
        if(Input.mousePosition.y <= 0){
            transform.position += Vector3.back*camera_pan_speed*Time.deltaTime;
        }
        else if(Input.mousePosition.y > Screen.height){
            transform.position += Vector3.forward*camera_pan_speed*Time.deltaTime;
        }

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, -10, 10),
            transform.position.y,
            Mathf.Clamp(transform.position.z, -20, -5));
    }
}
