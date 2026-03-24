using UnityEngine;

public class CameraLockedBackground : MonoBehaviour
{
    [System.Serializable]
    public class CameraBackgroundSettings
    {
        public Transform cameraTarget;
        public float distanceFromCamera = 30f;
        public Vector3 localOffset = Vector3.zero;
        public Vector3 scale = Vector3.one;
        public Vector3 eulerOffset = Vector3.zero;
    }

    [Header("Per Camera Settings")]
    [SerializeField] private CameraBackgroundSettings explorationSettings;
    [SerializeField] private CameraBackgroundSettings balanceSettings;

    [Header("Rotation")]
    [SerializeField] private bool copyCameraRotation = true;

    [Header("Update")]
    [SerializeField] private bool useLateUpdate = true;

    private void Start()
    {
        ApplyTransform();
    }

    private void Update()
    {
        if (!useLateUpdate)
            ApplyTransform();
    }

    private void LateUpdate()
    {
        if (useLateUpdate)
            ApplyTransform();
    }

    private void ApplyTransform()
    {
        CameraBackgroundSettings activeSettings = GetActiveSettings();
        if (activeSettings == null || activeSettings.cameraTarget == null)
            return;

        Transform cam = activeSettings.cameraTarget;

        transform.position = cam.position
                           + cam.forward * activeSettings.distanceFromCamera
                           + cam.TransformVector(activeSettings.localOffset);

        if (copyCameraRotation)
            transform.rotation = cam.rotation * Quaternion.Euler(activeSettings.eulerOffset);
        else
            transform.rotation = Quaternion.Euler(activeSettings.eulerOffset);

        transform.localScale = activeSettings.scale;
    }

    private CameraBackgroundSettings GetActiveSettings()
    {
        if (balanceSettings != null &&
            balanceSettings.cameraTarget != null &&
            balanceSettings.cameraTarget.gameObject.activeInHierarchy)
        {
            return balanceSettings;
        }

        if (explorationSettings != null &&
            explorationSettings.cameraTarget != null &&
            explorationSettings.cameraTarget.gameObject.activeInHierarchy)
        {
            return explorationSettings;
        }

        if (explorationSettings != null && explorationSettings.cameraTarget != null)
            return explorationSettings;

        if (balanceSettings != null && balanceSettings.cameraTarget != null)
            return balanceSettings;

        return null;
    }
}