using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotateObject : MonoBehaviour
{

    [SerializeField]
    private float rotationSpeed = 50f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
    }
}
