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
    private Dictionary<string, Vector3Int> move_directions = new Dictionary<string, Vector3Int>{
            {"right", new Vector3Int(1, 0, 0)},
            {"down",  new Vector3Int(0, -1, 1)},
            {"left",  new Vector3Int(-1, 0, 2)},
            {"up",    new Vector3Int(0, 1, 3)},
        };

    void Start(){

    }

    void Update(){
        
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
            // case "laser":
            //     if(operands.Count == 1){
            //         if(operands[0] == "on"){
            //             TurnLaserOn();
            //             break;
            //         }
            //         else if(operands[0] == "off"){
            //             TurnLaserOff();
            //             break;
            //         }
            //     }
            //     success = false;
            //     break;
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
