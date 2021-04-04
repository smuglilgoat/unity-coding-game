using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable: MonoBehaviour {
    [System.NonSerialized]
    public Vector3Int position;
    [System.NonSerialized]
    public int direction = 0;
    [System.NonSerialized]
    public bool pushable = false;

    public bool PositionInBound(Vector3Int position){
        int[, , ]map = GameObject.Find("World").GetComponent<LevelLoader>().map;
        return (position.x >= 0 && position.x < map.GetLength(0) &&
            position.y >= 0 && position.y < map.GetLength(1) &&
            position.z >= 0 && position.z < map.GetLength(2));
    }

    public Dictionary<string, Vector3Int> move_directions = new Dictionary<string, Vector3Int>{
        {"right", new Vector3Int(1, 0, 0)},
        {"down",  new Vector3Int(0, -1, 1)},
        {"left",  new Vector3Int(-1, 0, 2)},
        {"up",    new Vector3Int(0, 1, 3)},
    };
    public Dictionary<string, int> rotate_directions = new Dictionary<string, int>{
        {"right", 0},
        {"down", 1},
        {"left", 2},
        {"up", 3},
    };
    public Dictionary<int, Vector3Int> direction_to_vector = new Dictionary<int, Vector3Int>{
        {0, new Vector3Int(1, 0, 0)},
        {1, new Vector3Int(0, 0, -1)},
        {2, new Vector3Int(-1, 0, 0)},
        {3, new Vector3Int(0, 0, 1)},
    };

    public Vector3Int GetDirectionVector(){
        return direction_to_vector[direction];
    }

    public bool Move(Vector3Int direction) {
        Vector3Int new_pos = position+direction;
        int[, , ]map = GameObject.Find("World").GetComponent<LevelLoader>().map;

        if(PositionInBound(new_pos) && map[new_pos.x, new_pos.y, new_pos.z] == 0){
            Vector3Int pos_under = new_pos+new Vector3Int(0, -1, 0);
            if(PositionInBound(pos_under) && map[pos_under.x, pos_under.y, pos_under.z] == 0){
                return false;
            }

            int id = GameObject.Find("World").GetComponent<LevelLoader>().map[position.x, position.y, position.z];
            GameObject instance = GameObject.Find("World").GetComponent<LevelLoader>()
                                    .map_instances[position.x, position.y, position.z];

            GameObject.Find("World").GetComponent<LevelLoader>().map[position.x, position.y, position.z] = 0;
            GameObject.Find("World").GetComponent<LevelLoader>().map_instances[position.x, position.y, position.z] = null;

            position = new_pos;

            GameObject.Find("World").GetComponent<LevelLoader>().map[position.x, position.y, position.z] = id;
            GameObject.Find("World").GetComponent<LevelLoader>().map_instances[position.x, position.y, position.z] = instance;
            return true;
        }
        return false;
    }

    public void Rotate(int o) {
        direction = o;
    }
}