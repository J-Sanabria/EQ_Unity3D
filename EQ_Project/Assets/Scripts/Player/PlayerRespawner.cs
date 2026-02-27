using StarterAssets;
using UnityEngine;

public class PlayerRespawner : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform playerRoot;
    [SerializeField] CharacterController characterController;
    [SerializeField] ThirdPersonController thirdPersonController;
    [SerializeField] StarterAssetsInputs starterInputs;

    void Awake()
    {
        // Si no se asignó en inspector, auto-detecta en runtime
        if (playerRoot == null) playerRoot = transform;

        if (characterController == null)
            characterController = playerRoot.GetComponentInChildren<CharacterController>();

        if (thirdPersonController == null)
            thirdPersonController = playerRoot.GetComponentInChildren<ThirdPersonController>();

        if (starterInputs == null)
            starterInputs = playerRoot.GetComponentInChildren<StarterAssetsInputs>();
    }

    public void RespawnAt(Transform spawnPoint)
    {
        if (playerRoot == null || spawnPoint == null) return;

        // Limpia inputs
        if (starterInputs != null)
        {
            starterInputs.move = Vector2.zero;
            starterInputs.jump = false;
            starterInputs.sprint = false;
            starterInputs.look = Vector2.zero;
            starterInputs.interact = false;
        }

        // Apaga CC para evitar colisiones raras
        bool hadCC = characterController != null && characterController.enabled;
        if (characterController != null) characterController.enabled = false;

        playerRoot.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

        if (characterController != null) characterController.enabled = hadCC;

        // Asegura movimiento habilitado (GameMode lo controla luego igual)
        if (thirdPersonController != null)
            thirdPersonController.MovementEnabled = true;
    }
}