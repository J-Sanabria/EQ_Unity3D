using UnityEngine;

public class PickupBob : MonoBehaviour
{
    public float bobAmplitude = 0.15f;
    public float bobSpeed = 2f;
    public float rotateSpeed = 45f;

    Vector3 _startPos;

    void Awake()
    {
        _startPos = transform.localPosition;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.localPosition = _startPos + new Vector3(0f, y, 0f);
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
    }
}
