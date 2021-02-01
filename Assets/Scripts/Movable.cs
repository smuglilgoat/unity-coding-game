using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour{
    [System.NonSerialized]
    public Vector3Int position;
    [System.NonSerialized]
    public int orientation = 0;

    public bool Move(Vector3Int direction){
        Vector3Int new_pos = position+direction;
        int[,,] map = GameObject.Find("World").GetComponent<LevelLoader>().map;

        if(new_pos.x >= 0 && new_pos.x < map.GetLength(0)
            && new_pos.y >= 0 && new_pos.y < map.GetLength(1)
            && new_pos.z >= 0 && new_pos.z < map.GetLength(2)
            && map[new_pos.x, new_pos.y, new_pos.z] == 0){
            GameObject.Find("World").GetComponent<LevelLoader>().map[position.x, position.y, position.z] = 0;
            position = new_pos;
            GameObject.Find("World").GetComponent<LevelLoader>().map[position.x, position.y, position.z] = 1;
            return true;
        }
        return false;
    }

    public void Rotate(int o){
        orientation = o;
    }
}
