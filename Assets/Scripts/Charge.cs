using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Charge : MonoBehaviour
{
    public float charge;
    public Vector3 velocity;
    public Vector3 prevVelocity;
    public Vector3 acceleration;
    public float mass;
    Vector3 prevPosition;
    public RenderTexture texture;
    RenderTexture textureCopy;
    DebugVisualizer debugVisualizer;
    Simulator simulator;
    public Shader propagationShader;
    public Material propagationMat;
    Queue<Vector3> posQueue;
    int frameCount;
    public Queue<Vector3> accQueue;
    [SerializeField] bool visualize = false;
    public bool userControlled = false;
    Transform chargeControl;
    [SerializeField] float decelerationDistance = 5.0f;
    // Start is called before the first frame update
    void Start()
    {
        simulator = FindObjectOfType<Simulator>();
        texture = new RenderTexture(simulator.gridSize.x, simulator.gridSize.y, 0, RenderTextureFormat.ARGBFloat);
        texture.filterMode = FilterMode.Point;
        textureCopy = new RenderTexture(simulator.gridSize.x, simulator.gridSize.y, 0, RenderTextureFormat.ARGBFloat);
        texture.filterMode = FilterMode.Point;
        propagationMat = new Material(propagationShader);
        posQueue = new Queue<Vector3>();
        accQueue = new Queue<Vector3>();
        debugVisualizer = FindAnyObjectByType<DebugVisualizer>();
        chargeControl = GameObject.FindGameObjectWithTag("ChargeController").transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdatePhysics();
        UpdateTexture();

        if (visualize)
        {
            debugVisualizer.texture = texture;
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }

    private void UpdatePhysics()
    {
        if (userControlled) {
            Vector3 dir_vec = chargeControl.position - transform.position;
            if (dir_vec.magnitude > simulator.cellSize * decelerationDistance) {
                dir_vec = dir_vec.normalized * simulator.cellSize * 5;
            }
            dir_vec = dir_vec / simulator.cellSize / decelerationDistance;
            velocity = Vector3.Lerp(velocity, dir_vec * simulator.lightSpeed, 0.1f);
            transform.position += velocity * simulator.deltaTime;
            acceleration = (velocity - prevVelocity) / simulator.deltaTime;
            prevVelocity = velocity;
            prevPosition = transform.position;
        } else {
            velocity += acceleration * simulator.deltaTime;
            transform.position += velocity * simulator.deltaTime;
            prevVelocity = velocity;
        }
    }

    Texture2D CreatePosTexture()
    {
        Texture2D posTexture = new Texture2D(1, posQueue.Count, TextureFormat.RGBAFloat, false);
        posTexture.filterMode = FilterMode.Point;
        Vector3[] posArray = posQueue.ToArray();
        for (int i = 0; i < posQueue.Count; i++)
        {
            Vector3 pos = posArray[posQueue.Count-i-1];
            Vector3 pos_grid = new Vector3(
                (pos.x - simulator.bounds.min.x) / simulator.cellSize,
                (pos.y - simulator.bounds.min.y) / simulator.cellSize,
                (pos.z - simulator.bounds.min.z) / simulator.cellSize
            );
            posTexture.SetPixel(0, i, new Color(pos_grid.x, pos_grid.y, pos_grid.z, 1));
        }
        posTexture.Apply();
        // posTexture.SetPixel(0, 0, new Color(1, 1, 0, 1));
        // Debug.Log(posTexture.GetPixel(0, 0));
        // Debug.Log(posQueue.Count);
        return posTexture;
    }

    Texture2D CreateAccTexture()
    {
        Texture2D accTexture = new Texture2D(1, accQueue.Count, TextureFormat.RGBAFloat, false);
        accTexture.filterMode = FilterMode.Point;
        Vector3[] accArray = accQueue.ToArray();
        for (int i = 0; i < accQueue.Count; i++)
        {
            Vector3 acc = accArray[accQueue.Count-i-1];
            accTexture.SetPixel(0, i, new Color(acc.x, acc.y, acc.z, 1));
        }
        accTexture.Apply();
        return accTexture;
    }

    void UpdateTexture()
    {
        Vector3Int cell = new Vector3Int(
            Mathf.FloorToInt((transform.position.x - simulator.bounds.min.x) / simulator.cellSize),
            Mathf.FloorToInt((transform.position.y - simulator.bounds.min.y) / simulator.cellSize),
            Mathf.FloorToInt((transform.position.z - simulator.bounds.min.z) / simulator.cellSize)
        );
        posQueue.Enqueue(transform.position);
        accQueue.Enqueue(acceleration);
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
        propagationMat.SetTexture("_AccTexture", CreateAccTexture());
        Graphics.Blit(texture, textureCopy, propagationMat);
        Graphics.Blit(textureCopy, texture);
        // Debug.Log(cell);
        frameCount++;

        while (posQueue.Count > simulator.gridSize.magnitude + 1)
        {
            posQueue.Dequeue();
        }
        while (accQueue.Count > simulator.gridSize.magnitude + 1)
        {
            accQueue.Dequeue();
        }
    }
}
