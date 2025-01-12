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
    public Vector3 force;
    public float mass;
    Vector3 prevPosition;
    public RenderTexture fieldTexture;
    RenderTexture fieldTextureCopy;
    RenderTexture historyTexture;
    RenderTexture historyTextureCopy;
    DebugVisualizer debugVisualizer;
    Simulator simulator;
    public Shader propagationShader;
    public Material propagationMat;
    public Shader fieldShader;
    public Material fieldMat;

    Queue<Vector3> posQueue;
    int frameCount;
    public Queue<Vector3> accQueue;
    [SerializeField] bool visualize = false;
    public bool userControlled = false;
    Transform chargeControl;
    [SerializeField] float decelerationDistance = 5.0f;
    Texture2D posTexture;
    Texture2D accTexture;
    public float dampingCoeff = 0.1f;
    public bool push;
    // Start is called before the first frame update
    void Start()
    {
        simulator = FindObjectOfType<Simulator>();
        fieldTexture = new RenderTexture(simulator.gridSize.x, simulator.gridSize.y * simulator.gridSize.z, 0, RenderTextureFormat.ARGBFloat);
        fieldTexture.volumeDepth = simulator.gridSize.z;
        fieldTexture.filterMode = FilterMode.Point;
        fieldTextureCopy = new RenderTexture(simulator.gridSize.x, simulator.gridSize.y * simulator.gridSize.z, 0, RenderTextureFormat.ARGBFloat);
        fieldTextureCopy.filterMode = FilterMode.Point;

        propagationMat = new Material(propagationShader);
        fieldMat = new Material(fieldShader);
        posQueue = new Queue<Vector3>();
        accQueue = new Queue<Vector3>();
        debugVisualizer = FindAnyObjectByType<DebugVisualizer>();
        chargeControl = GameObject.FindGameObjectWithTag("ChargeController").transform;
        posTexture = new Texture2D(1, (int)simulator.gridSize.magnitude + 2, TextureFormat.RGBAFloat, false);
        posTexture.filterMode = FilterMode.Point;
        accTexture = new Texture2D(1, (int)simulator.gridSize.magnitude + 2, TextureFormat.RGBAFloat, false);
        accTexture.filterMode = FilterMode.Point;
        historyTexture = new RenderTexture(simulator.gridSize.x, simulator.gridSize.y * simulator.gridSize.z, 0, RenderTextureFormat.RFloat);
        historyTexture.filterMode = FilterMode.Point;
        historyTextureCopy = new RenderTexture(simulator.gridSize.x, simulator.gridSize.y * simulator.gridSize.z, 0, RenderTextureFormat.RFloat);
        historyTextureCopy.filterMode = FilterMode.Point;
        push = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdatePhysics();
        UpdateTexture();

        if (visualize)
        {
            debugVisualizer.texture = fieldTexture;
        }
    }
    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawSphere(transform.position, 0.1f);
    // }

    private void UpdatePhysics()
    {
        if (userControlled) {
            // Vector3 dir_vec = chargeControl.position - transform.position;
            // if (dir_vec.magnitude > simulator.cellSize * decelerationDistance) {
            //     dir_vec = dir_vec.normalized * simulator.cellSize * 5;
            // }
            // dir_vec = dir_vec / simulator.cellSize / decelerationDistance;
            // velocity = Vector3.Lerp(velocity, dir_vec * simulator.lightSpeed, 0.1f);
            // transform.position += velocity * simulator.deltaTime;
            // acceleration = (velocity - prevVelocity) / simulator.deltaTime;
            // prevVelocity = velocity;
            // prevPosition = transform.position;
            if (push) {
                acceleration = Vector3.up * 0.01f;
                push = false;
                Debug.Log(push);
            } else {
                acceleration = Vector3.zero;
            }
            velocity += acceleration * simulator.deltaTime;
            // velocity = Mathf.Sqrt(Mathf.Max(velocity.magnitude * velocity.magnitude - acceleration.magnitude*acceleration.magnitude * dampingCoeff, 0)) * velocity.normalized;
            transform.position += velocity * simulator.deltaTime;
            prevVelocity = velocity;
            force = Vector3.zero;
        } else {
            // velocity = 
            acceleration = force / mass;
            velocity += acceleration * simulator.deltaTime;
            // velocity = Mathf.Sqrt(Mathf.Max(velocity.magnitude * velocity.magnitude - acceleration.magnitude*acceleration.magnitude * dampingCoeff, 0)) * velocity.normalized;
            transform.position += velocity * simulator.deltaTime;
            prevVelocity = velocity;
            force = Vector3.zero;
        }
    }

    Texture2D UpdatePosTexture()
    {
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

    Texture2D UpdateAccTexture()
    {
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
        acceleration = Vector3.zero;

        if (cell.x < 0 || cell.x >= simulator.gridSize.x || cell.y < 0 || cell.y >= simulator.gridSize.y)
        {
            return;
        }
        // texture.SetPixel(cell.x, cell.y, new Color(Time.frameCount / 1000, 0, 0, 1));
        // texture.Apply();
        
        propagationMat.SetVector("_Cell", new Vector4(cell.x, cell.y, cell.z, 0));
        propagationMat.SetVector("_GridSize", new Vector4(simulator.gridSize.x, simulator.gridSize.y, simulator.gridSize.z, 0));
        // Debug.Log(Time.frameCount / 1000.0f);
        propagationMat.SetFloat("_Charge", charge);
        propagationMat.SetInteger("_FrameCount", frameCount);
        propagationMat.SetTexture("_PosTexture", UpdatePosTexture());
        propagationMat.SetTexture("_AccTexture", UpdateAccTexture());
        Graphics.Blit(historyTexture, historyTextureCopy, propagationMat);
        Graphics.Blit(historyTextureCopy, historyTexture);

        fieldMat.SetVector("_Cell", new Vector4(cell.x, cell.y, cell.z, 0));
        fieldMat.SetVector("_GridSize", new Vector4(simulator.gridSize.x, simulator.gridSize.y, simulator.gridSize.z, 0));
        fieldMat.SetFloat("_Charge", charge);
        fieldMat.SetFloat("_FrameCount", frameCount);
        fieldMat.SetTexture("_PosTexture", posTexture);
        fieldMat.SetTexture("_AccTexture", accTexture);
        Graphics.Blit(historyTexture, fieldTexture, fieldMat);
        // Graphics.Blit(fieldTextureCopy, fieldTexture);
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
