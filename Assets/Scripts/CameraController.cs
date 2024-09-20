using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform cameraPivotTargetX;
    [SerializeField] Transform cameraPivotTargetY;
    [SerializeField] Transform cameraTargetTransform;
    [SerializeField] Transform cameraPivotX;
    [SerializeField] Transform cameraPivotY;
    [SerializeField] float rotationSpeed = 1;
    [SerializeField] float zoomSpeed = 1;
    [SerializeField] float zoomPower = 1;
    [SerializeField] float panSpeed = 1;
    [SerializeField] float smoothnessCoeff = 1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Mouse input
        float horizontal = Input.GetAxis("Mouse X");
        float vertical = Input.GetAxis("Mouse Y");
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetMouseButton(0))
        {
            Orbit(horizontal, vertical);
        }

        Zoom(scroll);

        cameraPivotX.localRotation = Quaternion.Lerp(cameraPivotX.localRotation, cameraPivotTargetX.localRotation, Time.deltaTime * smoothnessCoeff);
        cameraPivotY.localRotation = Quaternion.Lerp(cameraPivotY.localRotation, cameraPivotTargetY.localRotation, Time.deltaTime * smoothnessCoeff);
        Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, cameraTargetTransform.localPosition, Time.deltaTime * smoothnessCoeff);
    }

    void Orbit(float horizontal, float vertical)
    {
        cameraPivotTargetX.Rotate(Vector3.up, horizontal * rotationSpeed * Time.deltaTime, Space.World);
        cameraPivotTargetY.Rotate(Vector3.right, -vertical * rotationSpeed * Time.deltaTime, Space.Self);
    }

    void Zoom(float scroll)
    {
        Vector3 camPos = cameraTargetTransform.localPosition;
        cameraTargetTransform.localPosition = new Vector3(camPos.x, camPos.y, camPos.z * Mathf.Pow(zoomPower, -zoomSpeed * scroll * Time.deltaTime));
    }
}
