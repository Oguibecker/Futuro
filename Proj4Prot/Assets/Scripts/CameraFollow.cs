using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float smoothSpeed = 10f;

    public float rotationDuration;
    private bool isRotating = false;

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    public void SmoothRotateCamera(float degrees , int axis)
    {
        if (!isRotating)
        {
            StartCoroutine(RotateCameraCoroutine(degrees, axis));
        }
    }

    private IEnumerator RotateCameraCoroutine(float degreesR, int axisR)
    {
        isRotating = true;

        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
        if (axisR == 1){ targetRotation = startRotation * Quaternion.Euler(degreesR, 0, 0); }
        if (axisR == 2){ targetRotation = startRotation * Quaternion.Euler(0, degreesR, 0); }
        if (axisR == 3){ targetRotation = startRotation * Quaternion.Euler(0, 0, degreesR); }

        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        isRotating = false;
    }
}
