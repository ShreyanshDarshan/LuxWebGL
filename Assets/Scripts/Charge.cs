using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charge : MonoBehaviour
{
    public float charge;
    public Vector3 velocity;
    public Vector3 prevVelocity;
    public Vector3 acceleration;
    public float mass;
    Vector3 prevPosition;
    RenderTexture texture;
    RenderTexture texture_copy;
    public DebugVisualizer debugVisualizer;
    Simulator simulator;
    public Shader propagationShader;
    public Material propagationMat;
    Queue<Vector2> posQueue;
    int frameCount;
    public Texture2D debugTexture;
    Queue<Vector3> accQueue;
    // Start is called before the first frame update
    void Start()
    {
        simulator = FindObjectOfType<Simulator>();
        texture = new RenderTexture(simulator.gridSize.x, simulator.gridSize.y, 0, RenderTextureFormat.ARGBFloat);
        texture.filterMode = FilterMode.Point;
        texture_copy = new RenderTexture(simulator.gridSize.x, simulator.gridSize.y, 0, RenderTextureFormat.ARGBFloat);
        texture.filterMode = FilterMode.Point;
        propagationMat = new Material(propagationShader);
        posQueue = new Queue<Vector2>();
        accQueue = new Queue<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        velocity = (transform.position - prevPosition) / simulator.deltaTime;
        acceleration = (velocity-prevVelocity) / simulator.deltaTime;
        prevVelocity = velocity;
        prevPosition = transform.position;

        debugVisualizer.texture = texture;

        UpdateTexture();
    }

    Texture2D CreatePosTexture()
    {
        Texture2D posTexture = new Texture2D(1, posQueue.Count, TextureFormat.RGFloat, false);
        posTexture.filterMode = FilterMode.Point;
        Vector2[] posArray = posQueue.ToArray();
        for (int i = 0; i < posQueue.Count; i++)
        {
            Vector2 pos = posArray[posQueue.Count-i-1];
            Vector2 pos_grid = new Vector2(
                (pos.x - simulator.bounds.min.x) / simulator.cellSize,
                (pos.y - simulator.bounds.min.y) / simulator.cellSize
            );
            posTexture.SetPixel(0, i, new Color(pos_grid.x, pos_grid.y, 0, 1));
        }
        posTexture.Apply();
        // posTexture.SetPixel(0, 0, new Color(1, 1, 0, 1));
        // Debug.Log(posTexture.GetPixel(0, 0));
        // Debug.Log(posQueue.Count);
        debugTexture = posTexture;
        return posTexture;
    }

    void UpdateTexture()
    {
        Vector3Int cell = new Vector3Int(
            Mathf.FloorToInt((transform.position.x - simulator.bounds.min.x) / simulator.cellSize),
            Mathf.FloorToInt((transform.position.y - simulator.bounds.min.y) / simulator.cellSize),
            Mathf.FloorToInt((transform.position.z - simulator.bounds.min.z) / simulator.cellSize)
        );
        posQueue.Enqueue(new Vector2(transform.position.x, transform.position.y));

        if (cell.x < 0 || cell.x >= simulator.gridSize.x || cell.y < 0 || cell.y >= simulator.gridSize.y)
        {
            return;
        }
        // texture.SetPixel(cell.x, cell.y, new Color(Time.frameCount / 1000, 0, 0, 1));
        // texture.Apply();
        propagationMat.SetVector("_Cell", new Vector4(cell.x, cell.y, cell.z, 0));
        // Debug.Log(Time.frameCount / 1000.0f);
        propagationMat.SetFloat("_Charge", charge);
        propagationMat.SetInteger("_FrameCount", frameCount);
        propagationMat.SetTexture("_PosTexture", CreatePosTexture());
        Graphics.Blit(texture, texture_copy, propagationMat);
        Graphics.Blit(texture_copy, texture);
        // Debug.Log(cell);
        frameCount++;

        while (posQueue.Count > 100)
        {
            posQueue.Dequeue();
        }
    }
}
