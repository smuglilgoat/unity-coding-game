using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable: MonoBehaviour {
  [System.NonSerialized]
  public Vector3Int position;
  [System.NonSerialized]
  public int orientation = 0;
  [System.NonSerialized]
  public int id;
  public GameObject instance;

  public bool Move(Vector3Int direction) {
    Vector3Int new_pos = position + direction;
    int[, , ] map = GameObject.Find("World").GetComponent < LevelLoader > ().map;

    if (new_pos.x >= 0 && new_pos.x < map.GetLength(0) &&
      new_pos.y >= 0 && new_pos.y < map.GetLength(1) &&
      new_pos.z >= 0 && new_pos.z < map.GetLength(2) &&
      map[new_pos.x, new_pos.y, new_pos.z] == 0) {

      id = GameObject.Find("World").GetComponent < LevelLoader > ().map[position.x, position.y, position.z];
      instance = GameObject.Find("World").GetComponent < LevelLoader > ().map_instances[position.x, position.y, position.z];

      GameObject.Find("World").GetComponent < LevelLoader > ().map[position.x, position.y, position.z] = 0;
      GameObject.Find("World").GetComponent < LevelLoader > ().map_instances[position.x, position.y, position.z] = null;

      position = new_pos;
      
      GameObject.Find("World").GetComponent < LevelLoader > ().map[position.x, position.y, position.z] = id;
      GameObject.Find("World").GetComponent < LevelLoader > ().map_instances[position.x, position.y, position.z] = instance;
      return true;
    }
    return false;
  }

  public void Rotate(int o) {
    orientation = o;
  }
}