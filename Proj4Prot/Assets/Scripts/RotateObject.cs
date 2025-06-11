using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField]
    private float maxRotationSpeed = 180f;

    private Vector3 chosenAxis;
    private float chosenSpeed;

    void Start()
    {
        int axisChoice = Random.Range(0, 3); // 0 for X, 1 for Y, 2 for Z

        switch (axisChoice)
        {
            case 0:
                chosenAxis = Vector3.right;
                break;
            case 1:
                chosenAxis = Vector3.up;
                break;
            case 2:
                chosenAxis = Vector3.forward;
                break;
            default:
                chosenAxis = Vector3.up;
                break;
        }

        chosenSpeed = Random.Range(90f, maxRotationSpeed);
    }

    void Update()
    {
        transform.Rotate(chosenAxis * chosenSpeed * Time.deltaTime);
    }
}
