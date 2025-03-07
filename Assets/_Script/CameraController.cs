using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CameraController.cs
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -5);
    [SerializeField] private float smoothSpeed = 5f;

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
        transform.position = smoothedPosition;
        transform.LookAt(target);
    }
}
