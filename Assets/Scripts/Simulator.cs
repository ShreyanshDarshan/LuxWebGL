using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulator : MonoBehaviour
{
    public float deltaTime;
    public Bounds bounds;
    public float cellSize;
    public Vector3Int gridSize;

    [SerializeField] List<Charge> charges;
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
        charges = new List<Charge>(FindObjectsOfType<Charge>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Vector3 CalculateElectricField() {
        
    // }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
