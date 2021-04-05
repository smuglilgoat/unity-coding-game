using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserReceiver : MonoBehaviour
{
    [System.NonSerialized]
    public bool activated = false;

    public Material prism_material;
    public Material prism_material_activated;
    public Color light_color;
    public Color light_color_activated;

    private Transform prism;
    private Light light_;
    private float prism_rotate_speed = 50f;
    private float prism_move_speed = 1f;
    private float lerp_speed = 5f;
    private float lerp = 0f;

    private Vector3 prism_pos_bottom;
    private Vector3 prism_pos_top;

    void Start(){
        prism = transform.Find("Prism");
        light_ = transform.Find("Light").GetComponent<Light>();
        prism_pos_bottom = prism.position;
        prism_pos_top = prism.position+new Vector3(0, 0.1f, 0);
    }

    void Update(){
        float pos_lerp = Mathf.PingPong(Time.time*prism_move_speed, 1);
        prism.transform.position = Vector3.Slerp(prism_pos_bottom, prism_pos_top, pos_lerp);

        if(activated){
            prism.Rotate(new Vector3(0, prism_rotate_speed, 0)*Time.deltaTime);
            lerp += lerp_speed*Time.deltaTime;
        }
        else{
            lerp -= lerp_speed*Time.deltaTime;
        }
        lerp = Mathf.Clamp(lerp, 0, 1);

        prism.GetComponent<Renderer>().material.Lerp(prism_material, prism_material_activated, lerp);
        light_.color = Color.Lerp(light_color, light_color_activated, lerp);
    }
}
