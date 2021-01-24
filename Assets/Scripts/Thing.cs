using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thing : MonoBehaviour
{
    [System.NonSerialized]
    public Vector3Int position;
    [System.NonSerialized]
    public int orientation = 0;

    public bool Move(Vector3Int direction){
        Vector3Int new_pos = position+direction;
        int[,,] collision_map = GameObject.Find("World").GetComponent<GenerateWorld>().collision_map;
        int[] world_size = GameObject.Find("World").GetComponent<GenerateWorld>().world_size;

        if(new_pos.x >= 0 && new_pos.x < world_size[0]
            && new_pos.y >= 0 && new_pos.y < world_size[1]
            && new_pos.z >= 0 && new_pos.z < world_size[2]
            && collision_map[new_pos.x, new_pos.y, new_pos.z] == 0){
            GameObject.Find("World").GetComponent<GenerateWorld>().collision_map[position.x, position.y, position.z] = 0;
            position = new_pos;
            GameObject.Find("World").GetComponent<GenerateWorld>().collision_map[position.x, position.y, position.z] = 1;
            return true;
        }
        return false;
    }

    public void Rotate(int o){
        orientation = o;
    }
}
