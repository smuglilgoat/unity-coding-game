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
            case "push":
                Vector3Int curr_pos = gameObject.GetComponent<Movable>().position;
                int[,,] map = GameObject.Find("World").GetComponent<LevelLoader>().map;
                GameObject pushed_object_instance;
                Vector3Int dir;
                Vector3Int vector;
                switch (gameObject.GetComponent<Movable>().orientation)
                {
                    case 0:
                        if (curr_pos.x != (map.GetLength(0) - 1) && map[curr_pos.x + 1, curr_pos.y, curr_pos.z] != 0)
                        {
                            pushed_object_instance = GameObject.Find("World").GetComponent<LevelLoader>().map_instances[curr_pos.x + 1, curr_pos.y, curr_pos.z];
                            if (pushed_object_instance.GetComponent<Movable>())
                            {
                                dir = move_directions["right"];
                                vector = new Vector3Int(dir.x, 0, dir.y);
                                pushed_object_instance.GetComponent<Movable>().Move(vector);
                            }
                        }
                        break;
                    case 1:
                        if (curr_pos.z != 0 && map[curr_pos.x, curr_pos.y, curr_pos.z - 1] != 0)
                        {
                            pushed_object_instance = GameObject.Find("World").GetComponent<LevelLoader>().map_instances[curr_pos.x, curr_pos.y, curr_pos.z - 1];
                            if (pushed_object_instance.GetComponent<Movable>())
                            {
                                dir = move_directions["down"];
                                vector = new Vector3Int(dir.x, 0, dir.y);
                                pushed_object_instance.GetComponent<Movable>().Move(vector);    
                            }
                        }
                        break;
                    case 2:
                        if (curr_pos.x != 0 && map[curr_pos.x - 1, curr_pos.y, curr_pos.z] != 0)
                        {
                            pushed_object_instance = GameObject.Find("World").GetComponent<LevelLoader>().map_instances[curr_pos.x - 1, curr_pos.y, curr_pos.z];
                            if (pushed_object_instance.GetComponent<Movable>())
                            {
                                dir = move_directions["left"];
                                vector = new Vector3Int(dir.x, 0, dir.y);
                                pushed_object_instance.GetComponent<Movable>().Move(vector);    
                            }
                        }
                        break;
                    case 3:
                        if (curr_pos.z != (map.GetLength(2) - 1) && map[curr_pos.x, curr_pos.y, curr_pos.z + 1] != 0)
                        {
                            pushed_object_instance = GameObject.Find("World").GetComponent<LevelLoader>().map_instances[curr_pos.x, curr_pos.y, curr_pos.z + 1];
                            if (pushed_object_instance.GetComponent<Movable>())
                            {
                               dir = move_directions["up"];
                                vector = new Vector3Int(dir.x, 0, dir.y);
                                pushed_object_instance.GetComponent<Movable>().Move(vector);      
                            }
                        }
                        break;
                    default:
                        break;
                }
                break;
            case "look":
                if(operands.Count == 1){
                    if(move_directions.ContainsKey(operands[0])){
                        Vector3Int val = move_directions[operands[0]];
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
