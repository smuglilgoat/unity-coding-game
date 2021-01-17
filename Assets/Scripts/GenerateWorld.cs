using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GenerateWorld : MonoBehaviour
{
    private GameObject floor_container;
    public int[] world_size = {5, 8};

    public GameObject floor_unit_prefab;
    public GameObject robot_prefab;
    public GameObject console;
    public TMP_InputField console_input;
    public RectTransform console_current_line;
    public Toggle robot_toggle;
    private bool console_up = false;
    private float console_l = 0f;

    private GameObject selected_robot;

    private float tick = 1f;
    private float progress = 0f;
    private float lerp_speed = 3f;

    public class ToMove{
        public Vector3 from;
        public Vector3 to;
        public ToMove(Vector3 from, Vector3 to){
            this.from = from;
            this.to = to;
        }
    }
    private Dictionary<GameObject, ToMove> to_move = new Dictionary<GameObject, ToMove>();
    public List<GameObject> to_update = new List<GameObject>();

    void Start(){
        console_input.onValueChanged.AddListener(delegate {ProgramChange(); });
        robot_toggle.onValueChanged.AddListener(delegate {ToggleRobot(); });

        floor_container = new GameObject("floor_container");
        floor_container.transform.position = new Vector3(0, 0, 0);

        for(int i = 0; i < world_size[0]; i++){
            for(int j = 0; j < world_size[1]; j++){
                GameObject floor_unit = Instantiate(floor_unit_prefab,
                    new Vector3(-(world_size[0]/2)+i, 0, -(world_size[1]/2)+j),
                    Quaternion.identity);
                floor_unit.transform.SetParent(floor_container.transform, false);
            }
        }

        //////
        GameObject robot = Instantiate(robot_prefab, new Vector3(0, 1, 0), Quaternion.identity);
        robot.GetComponent<RobotBehaviour>().position = new Vector2(0, 0);
        to_update.Add(robot);

        GameObject robot2 = Instantiate(robot_prefab, new Vector3(2, 1, 0), Quaternion.identity);
        robot2.GetComponent<RobotBehaviour>().position = new Vector2(2, 0);
        to_update.Add(robot2);
        //////
    }

    void ProgramChange(){
        selected_robot.GetComponent<RobotBehaviour>().program = console_input.text;
    }

    void ToggleRobot(){
        if(selected_robot == null) return;
        selected_robot.GetComponent<RobotBehaviour>().on = robot_toggle.isOn;
        selected_robot.GetComponent<RobotBehaviour>().current_line = 0;
        console_input.readOnly = selected_robot.GetComponent<RobotBehaviour>().on;
    }

    void Update(){
        if(Input.GetMouseButtonDown(0)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)){
                if(hit.transform.GetComponent<RobotBehaviour>()){
                    console_up = true;
                    selected_robot = hit.transform.gameObject;
                    selected_robot.GetComponent<RobotBehaviour>().error = false;
                    console_input.text = selected_robot.GetComponent<RobotBehaviour>().program;
                    console_current_line.localPosition = new Vector3(console_current_line.localPosition.x,
                                                        -selected_robot.GetComponent<RobotBehaviour>().current_line*35+225, 0);
                    console_current_line.gameObject.GetComponent<Image>().color = new Color32(255, 255, 225, 114);
                    robot_toggle.isOn = selected_robot.GetComponent<RobotBehaviour>().on;
                    console_input.readOnly = selected_robot.GetComponent<RobotBehaviour>().on;
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
            o.Key.transform.position = Vector3.Lerp(o.Value.from, o.Value.to, progress);
        }

        tick -= Time.deltaTime;
        progress += Time.deltaTime*lerp_speed;
        if(tick <= 0){
            if(selected_robot != null){
                console_current_line.localPosition = new Vector3(console_current_line.localPosition.x,
                                                        -selected_robot.GetComponent<RobotBehaviour>().current_line*35+225, 0);
            }
            foreach(KeyValuePair<GameObject, ToMove> o in to_move){
                o.Key.transform.position = o.Value.to;
            }
            to_move.Clear();

            foreach(GameObject o in to_update){
                if(o.GetComponent<RobotBehaviour>()){
                    o.GetComponent<RobotBehaviour>().Tick();
                    Vector2 robot_pos = o.GetComponent<RobotBehaviour>().position;
                    Vector3 next_pos = new Vector3(robot_pos.x, 1, robot_pos.y);
                    to_move[o] = new ToMove(o.transform.position, next_pos);
                }
            }
            if(selected_robot != null){
                robot_toggle.isOn = selected_robot.GetComponent<RobotBehaviour>().on;
                if(selected_robot.GetComponent<RobotBehaviour>().error){
                    console_current_line.gameObject.GetComponent<Image>().color = new Color32(255, 0, 0, 114);
                }
            }

            tick = 1f;
            progress = 0f;
        }
    }
}
