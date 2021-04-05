using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotPush : MonoBehaviour, Tickable{
    [System.NonSerialized]
    public bool is_on = false;
    public string program;
    [System.NonSerialized]
    public int current_line = 0;
    public bool in_error = false;

    [System.NonSerialized]
    public bool burned = false;

    public void Burn(){
        burned = true;
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
                        Vector3Int val = GetComponent<Movable>().move_directions[operands[0]];
                        Vector3Int move = new Vector3Int(val.x, 0, val.y);
                        Vector3Int new_pos = GetComponent<Movable>().position+move;
                        Vector3Int push_pos = GetComponent<Movable>().position+move*2;

                        if(GetComponent<Movable>().PositionInBound(new_pos) &&
                            GetComponent<Movable>().PositionInBound(push_pos) &&
                            GameObject.Find("World").GetComponent<LevelLoader>().map[
                                push_pos.x, push_pos.y, push_pos.z] == 0){
                            GameObject object_to_push = GameObject.Find("World").GetComponent<LevelLoader>().map_instances[
                                new_pos.x, new_pos.y, new_pos.z];

                            if(object_to_push != null &&
                                object_to_push.GetComponent<Movable>() &&
                                object_to_push.GetComponent<Movable>().pushable){
                                object_to_push.GetComponent<Movable>().Move(move);
                            }
                        }
                        GetComponent<Movable>().Move(move);
                        GetComponent<Movable>().Rotate((int)val.z);
                        break;
                    }
                }
                success = false;
                break;
            case "look":
                if(operands.Count == 1){
                    if(GetComponent<Movable>().move_directions.ContainsKey(operands[0])){
                        Vector3Int val = GetComponent<Movable>().move_directions[operands[0]];
                        GetComponent<Movable>().Rotate((int)val.z);
                        break;
                    }
                }
                success = false;
                break;
            case "nop":
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
