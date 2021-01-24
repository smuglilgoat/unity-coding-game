using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GenerateWorld : MonoBehaviour
{
    private GameObject floor_container;

    [System.NonSerialized]
    public int[] world_size = {8, 2, 8};
    public int[, ,] collision_map;

    public GameObject floor_unit_prefab;
    public GameObject robot_prefab;
    public GameObject rock_prefab;

    public GameObject console;
    public TMP_InputField console_input;
    public RectTransform console_current_line;
    public Toggle robot_toggle;
    private bool console_up = false;
    private float console_l = 0f;

    private GameObject selected_robot;

    private float tick = 1f;
    private float move_progress = 0f;
    private float move_speed = 3f;
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
    private Dictionary<GameObject, ToMove> to_move = new Dictionary<GameObject, ToMove>();
    public List<GameObject> to_update = new List<GameObject>();

    GameObject SpawnObject(GameObject prefab, Vector3Int position){
        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        collision_map[position.x, position.y, position.z] = 1;
        obj.GetComponent<Thing>().position = position;
        return obj;
    }

    void Start(){
        console_input.onValueChanged.AddListener(delegate {ProgramChange(); });
        robot_toggle.onValueChanged.AddListener(delegate {ToggleRobot(); });

        floor_container = new GameObject("FloorContainer");
        floor_container.transform.position = new Vector3(0, 0, 0);

        collision_map = new int[world_size[0], world_size[1], world_size[2]];
        for(int i = 0; i < world_size[0]; i++){
            for(int j = 0; j < world_size[1]; j++){
                for(int k = 0; k < world_size[2]; k++){
                    collision_map[i, j, k] = 0;
                }
            }
        }

        for(int i = 0; i < world_size[0]; i++){
            for(int j = 0; j < world_size[2]; j++){
                GameObject floor_unit = SpawnObject(floor_unit_prefab, new Vector3Int(i, 0, j));
                floor_unit.transform.SetParent(floor_container.transform, false);
            }
        }

        GameObject robot = SpawnObject(robot_prefab, new Vector3Int(0, 1, 1));
        to_update.Add(robot);

        SpawnObject(rock_prefab, new Vector3Int(4, 1, 1));
        SpawnObject(rock_prefab, new Vector3Int(0, 1, 4));

        /*GameObject rock = Instantiate(rock_prefab, Vector3.zero, Quaternion.identity);
        collision_map[2, 1, 1] = 1;*/
    }

    void ProgramChange(){
        selected_robot.GetComponent<Robot>().program = console_input.text;
    }

    void ToggleRobot(){
        if(selected_robot == null) return;
        selected_robot.GetComponent<Robot>().is_on = robot_toggle.isOn;
        console_input.readOnly = selected_robot.GetComponent<Robot>().is_on;
    }

    void Update(){
        if(Input.GetMouseButtonDown(0)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)){
                if(hit.transform.GetComponent<Robot>()){
                    console_up = true;
                    selected_robot = hit.transform.gameObject;
                    console_input.text = selected_robot.GetComponent<Robot>().program;
                    console_current_line.localPosition = new Vector3(console_current_line.localPosition.x,
                                                        -selected_robot.GetComponent<Robot>().current_line*35+225, 0);
                    console_current_line.gameObject.GetComponent<Image>().color = new Color32(255, 255, 225, 114);
                    robot_toggle.isOn = selected_robot.GetComponent<Robot>().is_on;
                    console_input.readOnly = selected_robot.GetComponent<Robot>().is_on;
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Tab)){
            console_up = false;
        }

        if(console_up){
            if(console_l < 0.5) console_l += Time.deltaTime*2;
        }
        else{
            if(console_l > 0) console_l -= Time.deltaTime*2;
        }
        console.GetComponent<RectTransform>().localPosition = Vector3.Lerp(
            new Vector3(0, -671, 0),
            new Vector3(0, 0, 0), console_l*2);

        foreach(KeyValuePair<GameObject, ToMove> o in to_move){
            Transform t = o.Key.transform;
            t.position = Vector3.Lerp(o.Value.position_from, o.Value.position_to, move_progress);
            t.rotation = Quaternion.Lerp(o.Value.rotation_from, o.Value.rotation_to, rotation_progress);
        }

        tick -= Time.deltaTime;
        move_progress += Time.deltaTime*move_speed;
        rotation_progress += Time.deltaTime*rotate_speed;

        if(tick <= 0){
            if(selected_robot != null){
                console_current_line.localPosition = new Vector3(console_current_line.localPosition.x,
                                                        -selected_robot.GetComponent<Robot>().current_line*35+225, 0);
            }
            foreach(KeyValuePair<GameObject, ToMove> o in to_move){
                o.Key.transform.position = o.Value.position_to;
            }
            to_move.Clear();

            foreach(GameObject o in to_update){
                if(o.GetComponent<Robot>() != null && o.GetComponent<Robot>().Tick()){
                    Vector3Int thing_pos = o.GetComponent<Thing>().position;
                    Quaternion rotation_to = Quaternion.Euler(new Vector3(0, o.GetComponent<Thing>().orientation*90, 0));
                    to_move[o] = new ToMove(o.transform.position, thing_pos, o.transform.rotation, rotation_to);
                }
            }
            if(selected_robot != null){
                robot_toggle.isOn = selected_robot.GetComponent<Robot>().is_on;
                Color32 color;
                if(selected_robot.GetComponent<Robot>().in_error){
                    color = new Color32(255, 0, 0, 114);
                }
                else{
                    color = new Color32(255, 255, 225, 114);
                }
                console_current_line.gameObject.GetComponent<Image>().color = color;
            }

            tick = 1f;
            move_progress = 0f;
            rotation_progress = 0f;
        }
    }
}
