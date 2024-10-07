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
    RenderTexture combinedAccTexture;
    RenderTexture combinedAccTextureCopy;
    [SerializeField] Shader combineAccShader;
    [SerializeField] Shader zeroAccShader;
    Material combineAccMat;
    Material zeroAccMat;
    [SerializeField] List<Charge> charges;
    [SerializeField] bool visualize = false;
    DebugVisualizer debugVisualizer;
    public float lightSpeed = 1.0f;
    
    Texture2D combinedAccTexture2D;
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
        combinedAccTexture = new RenderTexture(gridSize.x, gridSize.y, 0, RenderTextureFormat.ARGBFloat);
        combinedAccTexture.filterMode = FilterMode.Point;
        combinedAccTextureCopy = new RenderTexture(gridSize.x, gridSize.y, 0, RenderTextureFormat.ARGBFloat);
        combinedAccTextureCopy.filterMode = FilterMode.Point;
        combineAccMat = new Material(combineAccShader);
        zeroAccMat = new Material(zeroAccShader);
        debugVisualizer = FindAnyObjectByType<DebugVisualizer>();
        lightSpeed = cellSize / deltaTime;
        combinedAccTexture2D = new Texture2D(gridSize.x, gridSize.y, TextureFormat.RGBAFloat, false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Graphics.Blit(combinedAccTexture, combinedAccTextureCopy, zeroAccMat);
        Graphics.Blit(combinedAccTextureCopy, combinedAccTexture);
        for (int i = 0; i < charges.Count; i++)
        {
            combineAccMat.SetTexture("_AccTexture", charges[i].texture);
            Graphics.Blit(combinedAccTexture, combinedAccTextureCopy, combineAccMat);
            Graphics.Blit(combinedAccTextureCopy, combinedAccTexture);
        }

        RenderTexture.active = combinedAccTexture;
        combinedAccTexture2D.ReadPixels(new Rect(0, 0, gridSize.x, gridSize.y), 0, 0);
        combinedAccTexture2D.Apply();
        RenderTexture.active = null;

        for (int i = 0; i < charges.Count; i++)
        {
            Vector3Int chargeGridPos = new Vector3Int(
                Mathf.FloorToInt((charges[i].transform.position.x - bounds.min.x) / cellSize),
                Mathf.FloorToInt((charges[i].transform.position.y - bounds.min.y) / cellSize),
                Mathf.FloorToInt((charges[i].transform.position.z - bounds.min.z) / cellSize)
            );
            Color accColor = combinedAccTexture2D.GetPixel(chargeGridPos.x, chargeGridPos.y);
            Vector3 acc = new Vector3(accColor.r, accColor.g, 0) / charges[i].mass;
            charges[i].acceleration += acc;
        }

        if (visualize)
        {
            debugVisualizer.texture = combinedAccTexture;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
