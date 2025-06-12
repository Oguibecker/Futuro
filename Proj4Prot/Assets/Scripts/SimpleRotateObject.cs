using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotateObject : MonoBehaviour
{

    [SerializeField]
    private float rotationSpeed = 50f; // Speed of rotation
    [SerializeField]
    private float maxRotationAngle = 45f; // Maximum angle to rotate in one direction (X degrees)

    [SerializeField]
    private bool RotateAroundZAxis = true;
    [SerializeField]
    private bool RotateAroundYAxis = false;

    private float initialYRotation; // To store the starting Y rotation
    private float currentPingPongTime = 0f;

    // Update is called once per frame
    void Update()
    {
        if (RotateAroundZAxis == true){
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
        if (RotateAroundYAxis == true){
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
        
        if (RotateAroundZAxis == false && RotateAroundYAxis == false){
            currentPingPongTime += Time.deltaTime * rotationSpeed / maxRotationAngle;
            float desiredYRotationOffset = Mathf.PingPong(currentPingPongTime, 1f) * maxRotationAngle * 2 - maxRotationAngle;
            transform.localEulerAngles = new Vector3(
                transform.localEulerAngles.x,
                initialYRotation + desiredYRotationOffset,
                transform.localEulerAngles.z
            );
        }

    }
}
