using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour, Tickable
{
    private Dictionary<string, Vector3Int> move_directions = new Dictionary<string, Vector3Int>{
            {"right", new Vector3Int(1, 0, 0)},
            {"down",  new Vector3Int(0, -1, 1)},
            {"left",  new Vector3Int(-1, 0, 2)},
            {"up",    new Vector3Int(0, 1, 3)},
        };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(string direction){
        Vector3Int val = move_directions[direction];
        Vector3Int move = new Vector3Int(val.x, 0, val.y);
        gameObject.GetComponent<Movable>().Move(move);
        gameObject.GetComponent<Movable>().Rotate((int)val.z);
    }
    public bool Tick()
    {
        return true;
    }
}
