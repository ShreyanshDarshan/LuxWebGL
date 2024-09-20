using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulator : MonoBehaviour
{
    public float deltaTime;
    public Bounds bounds;
    public float cellSize;
    public Vector3Int gridSize;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        gridSize = new Vector3Int(
            Mathf.CeilToInt(bounds.size.x / cellSize),
            Mathf.CeilToInt(bounds.size.y / cellSize),
            Mathf.CeilToInt(bounds.size.z / cellSize)
        );
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
