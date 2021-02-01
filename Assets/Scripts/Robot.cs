using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour, Tickable{
    [System.NonSerialized]
    public bool is_on = false;
    [System.NonSerialized]
    public bool laser_on = false;
    public string program;
    [System.NonSerialized]
    public int current_line = 0;
    public bool in_error = false;
    private GameObject laser;

    private Dictionary<string, Vector3Int> move_directions = new Dictionary<string, Vector3Int>{
            {"right", new Vector3Int(1, 0, 0)},
            {"down",  new Vector3Int(0, -1, 1)},
            {"left",  new Vector3Int(-1, 0, 2)},
            {"up",    new Vector3Int(0, 1, 3)},
        };

    private LineRenderer laser_line;
    private Vector3 laser_target_pos;
    private float laser_progress = 0;
    private float laser_speed = 40f;

    void Start(){
        laser_line = transform.Find("Laser").GetComponent<LineRenderer>();
        laser_line.positionCount = 2;
    }

    void TurnLaserOn(){
        if(laser_on) return;
        laser_on = true;

        Vector3 laser_start = laser_line.gameObject.transform.position;
        Vector3[] points = new Vector3[2];
        points[0] = laser_start;
        points[1] = laser_start;
        laser_line.SetPositions(points);

        RaycastHit[] hits;
        float max_laser_distance = 300f;
        hits = Physics.RaycastAll(laser_start, transform.right, max_laser_distance);
        if(hits.Length > 0){
            laser_target_pos = hits[0].point;
        }
        else{
            laser_target_pos = transform.right*max_laser_distance;
        }
        laser_progress = 0;
    }

    void TurnLaserOff(){
        if(!laser_on) return;
        laser_line.SetPosition(1, laser_target_pos);
        laser_on = false;
        laser_progress = 0;
    }

    void Update(){
        if(laser_progress < 1){
            if(laser_on){
                laser_line.SetPosition(1, Vector3.Lerp(laser_line.GetPosition(0), laser_target_pos, laser_progress));
            }
            else{
                laser_line.SetPosition(0, Vector3.Lerp(laser_line.gameObject.transform.position, laser_target_pos, laser_progress));
            }

            float s = Vector3.Distance(laser_line.gameObject.transform.position, laser_target_pos);
            laser_progress += laser_speed/s*Time.deltaTime;
            if(laser_progress >= 1){
                if(laser_on){
                    laser_line.SetPosition(1, Vector3.Lerp(laser_line.GetPosition(0), laser_target_pos, 1));
                }
                else{
                    laser_line.SetPosition(0, laser_line.gameObject.transform.position);
                    laser_line.SetPosition(1, laser_line.gameObject.transform.position);
                }
            }
        }
    }

    public bool Tick(){
        bool success = true;

        if(!is_on){
            return false;
        }

        string[] lines = program.Split('\n');

        if(current_line >= lines.Length){
            current_line = 0;
            return false;
        }

        string line = lines[current_line].ToLower().Trim();

        List<string> instruction = new List<string>(line.Split(' '));

        string operation = instruction[0];
        List<string> operands = new List<string>();

        if(instruction.Count > 1) operands = instruction.GetRange(1, instruction.Count-1);

        switch(operation){
            case "mov":
                if(operands.Count == 1){
                    if(move_directions.ContainsKey(operands[0])){
                        if(laser_on) break;
                        Vector3Int val = move_directions[operands[0]];
                        Vector3Int move = new Vector3Int(val.x, 0, val.y);
                        gameObject.GetComponent<Movable>().Move(move);
                        gameObject.GetComponent<Movable>().Rotate((int)val.z);
                        break;
                    }
                }
                success = false;
                break;
            case "nop":
                break;
            case "laser":
                if(operands.Count == 1){
                    if(operands[0] == "on"){
                        TurnLaserOn();
                        break;
                    }
                    else if(operands[0] == "off"){
                        TurnLaserOff();
                        break;
                    }
                }
                success = false;
                break;
            default:
                success = false;
                break;
        }

        if(success){
            current_line++;
            if(current_line == lines.Length){
                current_line = 0;
            }
        }
        else{
            is_on = false;
        }

        in_error = !success;
        return success;
    }
}
