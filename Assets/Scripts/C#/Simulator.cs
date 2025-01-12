using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulator : MonoBehaviour
{
    public float deltaTime;
    public Bounds bounds;
    public float cellSize;
    public Vector3Int gridSize;
    // Texture3D combinedAccTexture;
    RenderTexture combinedFieldTexture;
    RenderTexture combinedFieldTextureCopy;
    [SerializeField] Shader combineFieldShader;
    [SerializeField] Shader zeroFieldShader;
    Material combineFieldMat;
    Material zeroFieldMat;
    [SerializeField] List<Charge> charges;
    [SerializeField] bool visualize = false;
    DebugVisualizer debugVisualizer;
    public float lightSpeed = 1.0f;
    
    Texture2D combinedFieldTexture2D;
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
        combinedFieldTexture = new RenderTexture(gridSize.x, gridSize.y, 0, RenderTextureFormat.ARGBFloat);
        combinedFieldTexture.filterMode = FilterMode.Point;
        combinedFieldTextureCopy = new RenderTexture(gridSize.x, gridSize.y, 0, RenderTextureFormat.ARGBFloat);
        combinedFieldTextureCopy.filterMode = FilterMode.Point;
        combineFieldMat = new Material(combineFieldShader);
        zeroFieldMat = new Material(zeroFieldShader);
        debugVisualizer = FindAnyObjectByType<DebugVisualizer>();
        lightSpeed = cellSize / deltaTime;
        combinedFieldTexture2D = new Texture2D(gridSize.x, gridSize.y, TextureFormat.RGBAFloat, false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Graphics.Blit(combinedFieldTexture, combinedFieldTextureCopy, zeroFieldMat);
        // Graphics.Blit(combinedFieldTextureCopy, combinedFieldTexture);
        // for (int i = 0; i < charges.Count; i++)
        // {
        //     combineFieldMat.SetTexture("_FieldTexture", charges[i].fieldTexture);
        //     Graphics.Blit(combinedFieldTexture, combinedFieldTextureCopy, combineFieldMat);
        //     Graphics.Blit(combinedFieldTextureCopy, combinedFieldTexture);
        // }

        // RenderTexture.active = combinedFieldTexture;
        // combinedFieldTexture2D.ReadPixels(new Rect(0, 0, gridSize.x, gridSize.y), 0, 0);
        // combinedFieldTexture2D.Apply();
        // RenderTexture.active = null;

        // for (int i = 0; i < charges.Count; i++)
        // {
        //     Vector3Int chargeGridPos = new Vector3Int(
        //         Mathf.FloorToInt((charges[i].transform.position.x - bounds.min.x) / cellSize),
        //         Mathf.FloorToInt((charges[i].transform.position.y - bounds.min.y) / cellSize),
        //         Mathf.FloorToInt((charges[i].transform.position.z - bounds.min.z) / cellSize)
        //     );
        //     Color fieldColor = combinedFieldTexture2D.GetPixel(chargeGridPos.x, chargeGridPos.y);
        //     Vector3 force = new Vector3(fieldColor.r, fieldColor.g, 0);
        //     charges[i].force += force;
        // }

        // float energy = 0;
        // for (int px=0; px<gridSize.x; px++) {
        //     for (int py=0; py<gridSize.y; py++) {
        //         Color fieldColor = combinedFieldTexture2D.GetPixel(px, py);
        //         Vector3 field_val = new Vector3(fieldColor.r, fieldColor.g, 0);
        //         energy += field_val.sqrMagnitude;
        //     }
        // }
        // Debug.Log("Energy: " + energy);

        if (visualize)
        {
            debugVisualizer.texture = combinedFieldTexture;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
