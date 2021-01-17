using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotBehaviour : MonoBehaviour
{
    public string program;
    public int current_line = 0;
    private int[] world_size;
    public bool on = false;
    public bool error = false;
    public Vector2 position;

    public void Start(){
        world_size = GameObject.Find("World").GetComponent<GenerateWorld>().world_size;
    }

    public bool DoesntCollide(Vector2 new_pos){
        if(new_pos.x < world_size[0]/2 && new_pos.x >= -world_size[0]/2
        && new_pos.y < world_size[1]/2 && new_pos.y >= -world_size[1]/2){

            /* We need something way better than this */
            List<GameObject> other_objects = GameObject.Find("World").GetComponent<GenerateWorld>().to_update;
            foreach(GameObject other in other_objects){
                if(new_pos == other.GetComponent<RobotBehaviour>().position){
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    public void Tick(){
        if(!on) return;
        error = false;

        string[] lines = program.Split('\n');
        string line = lines[current_line].ToLower();
        List<string> instruction = new List<string>(line.Split(' '));

        string operation = instruction[0];
        List<string> operands = new List<string>();

        if(instruction.Count > 1) operands = instruction.GetRange(1, instruction.Count-1);

        switch(operation){
        case "mov":
            if(operands.Count == 2){
                int x;
                bool success = int.TryParse(operands[0], out x);
                if(success){
                    int y;
                    success = int.TryParse(operands[1], out y);
                    if(success){
                        Vector2 move = new Vector2(x, y);
                        if(DoesntCollide(position+move)) position += move;
                        break;
                    }
                }
            }
            error = true;
            break;
        default:
            error = true;
            break;
        }

        if(!error){
            current_line++;
            if(current_line == lines.Length){
                current_line = 0;
            }
        }
        else{
            on = false;
        }
    }
}
