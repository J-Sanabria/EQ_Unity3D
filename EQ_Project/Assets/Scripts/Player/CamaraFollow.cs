using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Referencias")]
    public Transform target;

    [Header("Seguimiento")]
    public float followSpeed = 8f;

    private Vector3 offset;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow: No hay target asignado.");
            return;
        }

        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );
    }
}
