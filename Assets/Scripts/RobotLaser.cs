using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotLaser : MonoBehaviour, Tickable{
    [System.NonSerialized]
    public bool is_on = false;
    [System.NonSerialized]
    public bool laser_on = false;
    public string program;
    [System.NonSerialized]
    public int current_line = 0;
    public bool in_error = false;
    private GameObject laser;

    private LineRenderer laser_line;
    private Vector3 laser_target_pos;
    private float max_laser_distance = 500f;
    private GameObject previous_prism;

    [System.NonSerialized]
    public bool burned = false;

    public void Burn(){
        laser_on = false; // bug where previous_prism isn't turned off
        burned = true;
    }

    void Start(){
        laser_line = transform.Find("Laser").GetComponent<LineRenderer>();
        laser_line.positionCount = 2;
        laser_line.SetPosition(0, laser_line.gameObject.transform.position);
        laser_line.SetPosition(1, laser_line.gameObject.transform.position);
    }

    void Update(){
        if(laser_on){
            RaycastHit[] hits;
            hits = Physics.RaycastAll(laser_line.gameObject.transform.position, transform.right, max_laser_distance);
            System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

            if(hits.Length > 0){
                laser_target_pos = hits[0].point;
            }
            else{
                laser_target_pos = transform.right*max_laser_distance;
            }
            laser_line.SetPosition(0, laser_line.gameObject.transform.position);
            laser_line.SetPosition(1, laser_target_pos);
        }
        else{
            laser_line.SetPosition(0, laser_line.gameObject.transform.position);
            laser_line.SetPosition(1, laser_line.gameObject.transform.position);
        }
    }

    public bool Tick(){
        bool success = true;

        if(!is_on || burned){
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
                    if(GetComponent<Movable>().move_directions.ContainsKey(operands[0])){
                        if(laser_on) break;
                        Vector3Int val = GetComponent<Movable>().move_directions[operands[0]];
                        Vector3Int move = new Vector3Int(val.x, 0, val.y);
                        gameObject.GetComponent<Movable>().Move(move);
                        gameObject.GetComponent<Movable>().Rotate((int)val.z);
                        break;
                    }
                }
                success = false;
                break;
            case "turn":
                if(operands.Count == 1){
                    if(GetComponent<Movable>().rotate_directions.ContainsKey(operands[0])){
                        if(laser_on) break;
                        int val = GetComponent<Movable>().rotate_directions[operands[0]];
                        gameObject.GetComponent<Movable>().Rotate(val);
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
                        laser_on = true;
                        break;
                    }
                    else if(operands[0] == "off"){
                        laser_on = false;
                        break;
                    }
                }
                success = false;
                break;
            default:
                success = false;
                break;
        }

        if(laser_on){
            Vector3Int direction_vector = GetComponent<Movable>().GetDirectionVector();
            Vector3Int current_pos = GetComponent<Movable>().position+direction_vector;
            bool didnt_find_anything = true;
            while(GetComponent<Movable>().PositionInBound(current_pos)){
                GameObject current_cell = GameObject.Find("World").GetComponent<LevelLoader>()
                                        .map_instances[current_pos.x, current_pos.y, current_pos.z];
                if(current_cell != null){
                    didnt_find_anything = false;

                    if(current_cell != previous_prism && previous_prism != null){
                        previous_prism.GetComponent<LaserReceiver>().activated = false;
                    }

                    if(current_cell.GetComponent<LaserReceiver>()){
                        previous_prism = current_cell;
                        previous_prism.GetComponent<LaserReceiver>().activated = true;
                    }
                    else if(current_cell.GetComponent<RobotLaser>()){
                        current_cell.GetComponent<RobotLaser>().Burn();
                    }
                    else if(current_cell.GetComponent<RobotPush>()){
                        current_cell.GetComponent<RobotPush>().Burn();
                    }
                    break;
                }
                current_pos += direction_vector;
            }

            if(didnt_find_anything && previous_prism != null){
                previous_prism.GetComponent<LaserReceiver>().activated = false;
            }
        }
        else{
            if(previous_prism != null){
                previous_prism.GetComponent<LaserReceiver>().activated = false;
            }
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
