using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
    // [SerializeField] float oscillationSpeed;
    [SerializeField] float springForceConstant;
    [SerializeField] float dampingConstant;
    // [SerializeField] Vector3 oscillationDirection;
    Charge charge;
    [SerializeField] Vector3 initPosition;
    Simulator simulator;
    // Start is called before the first frame update
    void Start()
    {
        charge = GetComponent<Charge>();
        initPosition = charge.transform.position;
        simulator = FindObjectOfType<Simulator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        charge.acceleration += -springForceConstant * (charge.transform.position - initPosition) - dampingConstant * charge.velocity / simulator.deltaTime;
    }
}
