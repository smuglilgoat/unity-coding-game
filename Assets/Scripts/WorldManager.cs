using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldManager : MonoBehaviour{
    public GameObject console;
    public TMP_InputField console_input;
    public RectTransform console_current_line;
    public Button start_button;
    public Button restart_button;
    private bool console_up = false;
    private float console_l = 0f;

    private GameObject selected_robot;

    private float tick = 1f;
    private float move_progress = 0f;
    private float move_speed = 2.0f;
    private float rotation_progress = 0f;
    private float rotate_speed = 3f;

    public class ToMove{
        public Vector3 position_from;
        public Vector3 position_to;
        public Quaternion rotation_from;
        public Quaternion rotation_to;

        public ToMove(Vector3 position_from, Vector3 position_to, Quaternion rotation_from, Quaternion rotation_to){
            this.position_from = position_from;
            this.position_to = position_to;
            this.rotation_from = rotation_from;
            this.rotation_to = rotation_to;
        }
    }
    private Dictionary<GameObject, ToMove> objects_to_reposition = new Dictionary<GameObject, ToMove>();
    [System.NonSerialized]
    public List<GameObject> objects_to_update = new List<GameObject>();

    void Start(){
        console_input.onValueChanged.AddListener(delegate { ProgramChange(); });
        start_button.onClick.AddListener(delegate { StartLevel(); });
        restart_button.onClick.AddListener(delegate { RestartLevel(); });
    }

    void ProgramChange(){
        if(selected_robot.GetComponent<RobotLaser>()){
            selected_robot.GetComponent<RobotLaser>().program = console_input.text;
        }
        else{
            selected_robot.GetComponent<RobotPush>().program = console_input.text;
        }
    }

    private bool restart_next_frame = false;
    void RestartLevel(){
        restart_next_frame = true;
    }

    void RestartLevel2(){
        objects_to_reposition = new Dictionary<GameObject, ToMove>();
        objects_to_update = new List<GameObject>();
        GetComponent<LevelLoader>().level_ready = false;
        GetComponent<LevelLoader>().LoadLevel(GetComponent<LevelLoader>().current_level_id);
        console_input.readOnly = false;
    }

    void StartLevel(){
        console_input.readOnly = true;

        foreach(GameObject o in objects_to_update){
            if(o.GetComponent<RobotLaser>()){
                o.GetComponent<RobotLaser>().is_on = true;
            }
            else if(o.GetComponent<RobotPush>()){
                o.GetComponent<RobotPush>().is_on = true;
            }
        }
    }

    void CheckConsole(){
        if(Input.GetKeyDown(KeyCode.Tab)){
            console_up = false;
        }

        if(!console_up){
            if(console_l < 0.5) console_l += Time.deltaTime*2;
        }
        else{
            if(console_l > 0) console_l -= Time.deltaTime*2;
        }
        console.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(
            new Vector3(0, 580, 0),
            new Vector3(0, 0, 0), console_l*2);
    }

    void Update(){
        if(!GetComponent<LevelLoader>().level_ready) return;
        if(restart_next_frame){
            restart_next_frame = false;
            RestartLevel2();
        }

        if(Input.GetMouseButtonDown(0)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)){
                if(hit.transform.GetComponent<RobotLaser>() && !hit.transform.GetComponent<RobotLaser>().burned){
                    console_up = true;
                    selected_robot = hit.transform.gameObject;
                    console_input.text = selected_robot.GetComponent<RobotLaser>().program;
                    console_current_line.localPosition = new Vector3(console_current_line.localPosition.x,
                                                        -selected_robot.GetComponent<RobotLaser>().current_line*35+225, 0);
                    console_current_line.gameObject.GetComponent<Image>().color = new Color32(255, 255, 225, 114);
                }
                if(hit.transform.GetComponent<RobotPush>() && !hit.transform.GetComponent<RobotPush>().burned){
                    console_up = true;
                    selected_robot = hit.transform.gameObject;
                    console_input.text = selected_robot.GetComponent<RobotPush>().program;
                    console_current_line.localPosition = new Vector3(console_current_line.localPosition.x,
                                                        -selected_robot.GetComponent<RobotPush>().current_line*35+225, 0);
                    console_current_line.gameObject.GetComponent<Image>().color = new Color32(255, 255, 225, 114);
                }
            }
        }

        CheckConsole();

        foreach(KeyValuePair<GameObject, ToMove> o in objects_to_reposition){
            Transform t = o.Key.transform;
            t.localPosition = Vector3.Lerp(o.Value.position_from, o.Value.position_to, move_progress);
            t.rotation = Quaternion.Lerp(o.Value.rotation_from, o.Value.rotation_to, rotation_progress);
        }

        tick -= Time.deltaTime;
        move_progress += Time.deltaTime*move_speed;
        rotation_progress += Time.deltaTime*rotate_speed;

        if(GetComponent<LevelLoader>().level_ready && tick <= 0){
            // Move code cursor, doesn't work
            if(selected_robot != null){
                if(selected_robot.GetComponent<RobotLaser>()){
                    console_current_line.localPosition = new Vector3(console_current_line.localPosition.x,
                                                        -selected_robot.GetComponent<RobotLaser>().current_line*35+225, 0);
                }
                else{
                    console_current_line.localPosition = new Vector3(console_current_line.localPosition.x,
                                                        -selected_robot.GetComponent<RobotPush>().current_line*35+225, 0);
                }
            }

            // Fix the position whether lerp finished or not
            foreach(KeyValuePair<GameObject, ToMove> o in objects_to_reposition){
                o.Key.transform.localPosition = o.Value.position_to;
            }
            objects_to_reposition.Clear();

            // Add objects to lerp to list
            foreach(GameObject o in objects_to_update){
                if(o.GetComponent<Tickable>() != null){
                    o.GetComponent<Tickable>().Tick();
                }

                Vector3Int new_pos = o.GetComponent<Movable>().position;
                Quaternion new_rotation = Quaternion.Euler(new Vector3(0, o.GetComponent<Movable>().direction*90, 0));
                objects_to_reposition[o] = new ToMove(o.transform.localPosition, new_pos, o.transform.rotation, new_rotation);
            }

            // Red colored cursor for error
            if(selected_robot != null){
                if (selected_robot.GetComponent<RobotLaser>())
                {
                    Color32 color;
                    if(selected_robot.GetComponent<RobotLaser>().in_error){
                        color = new Color32(255, 0, 0, 114);
                    }
                    else{
                        color = new Color32(255, 255, 225, 114);
                    }
                    console_current_line.gameObject.GetComponent<Image>().color = color;selected_robot.GetComponent<RobotLaser>().program = console_input.text; 
                } else {
                    Color32 color;
                    if(selected_robot.GetComponent<RobotPush>().in_error){
                        color = new Color32(255, 0, 0, 114);
                    }
                    else{
                        color = new Color32(255, 255, 225, 114);
                    }
                    console_current_line.gameObject.GetComponent<Image>().color = color;
                }
            }

            tick = 1f;
            move_progress = 0f;
            rotation_progress = 0f;
        }
    }
}
