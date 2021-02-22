/*
    Modifications:
        21/02 - Commented out Fading
    -------------------------------------------- 
    TODO: Fading Impl
    TODO: Serialisable level ?
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour {
    // public RawImage fade;
    // private float fade_speed = 0.5f;
    // private float fade_progress = 0f;
    // private bool fade_in;

    public GameObject[] map_objects;
    public int[, , ] map;

    [System.NonSerialized]
    public string current_level_id;

    [System.NonSerialized]
    public bool level_ready = false;

    GameObject SpawnObject (GameObject prefab, Vector3Int position) {
        GameObject obj = Instantiate (prefab, position, Quaternion.identity);
        if (obj.GetComponent<Movable> ()) obj.GetComponent<Movable> ().position = position;
        return obj;
    }

    // void FadeIn(bool i){
    //     if(!i) fade.enabled = true;
    //     fade_in = i;
    //     fade_progress = 0;
    // }

    void Start () {
        LoadLevel ("1");
    }

    void LoadLevel (string level_id) {
        level_ready = false;

        StreamReader level_file = new StreamReader (Application.dataPath + "/Levels/" + level_id + ".level");
        string line = level_file.ReadLine ();

        string[] line_split = line.Split (',');
        int[] world_size = new int[line_split.Length];
        for (int i = 0; i < line_split.Length; i++) {
            world_size[i] = int.Parse (line_split[i]);
        }
        map = new int[world_size[0], world_size[1], world_size[2]];

        int z_counter = 0;
        int y_counter = 0;
        while (!level_file.EndOfStream) {
            line = level_file.ReadLine ();
            if (line == "") continue;

            line_split = line.Split (',');

            for (int i = 0; i < line_split.Length; i++) {
                map[i, y_counter, world_size[2] - 1 - z_counter] = int.Parse (line_split[i]);
            }
            z_counter++;
            if (z_counter == world_size[2]) {
                z_counter = 0;
                y_counter++;
            }
        }

        level_file.Close ();

        GameObject objects_container = new GameObject ("ObjectsContainer");
        objects_container.transform.position = new Vector3 (-Mathf.Floor (map.GetLength (0)) / 2 + 0.5f,
            0, -Mathf.Floor (map.GetLength (2)) / 2 + 0.5f);

        for (int x = 0; x < map.GetLength (0); x++) {
            for (int y = 0; y < map.GetLength (1); y++) {
                for (int z = 0; z < map.GetLength (2); z++) {
                    if (map[x, y, z] == 0) continue;

                    GameObject obj = SpawnObject (map_objects[map[x, y, z]], new Vector3Int (x, y, z));
                    obj.transform.SetParent (objects_container.transform, false);

                    if (obj.GetComponent<Movable> ()) {
                        GetComponent<WorldManager> ().objects_to_update.Add (obj);
                    }
                }
            }
        }

        current_level_id = level_id;
        level_ready = true;
    }

    void Update () {
        // if(fade_progress < 1){
        //     Color c = fade.color;

        //     if(fade_in){
        //         c.a = fade_progress;
        //     }
        //     else{
        //         c.a = 1-fade_progress;
        //     }

        //     fade.color = c;
        //     fade_progress += Time.deltaTime*fade_speed;

        //     if(fade_progress >= 1 && !fade_in){
        //         fade.enabled = false;
        //     }
        // }
    }
}