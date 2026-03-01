using System.Collections;
using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerRespawner : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform playerRoot;
    CharacterController _cc;
    ThirdPersonController _tpc;
    StarterAssetsInputs _inputs;

    void Awake()
    {
        if (playerRoot == null) playerRoot = transform;
        _cc = playerRoot.GetComponentInChildren<CharacterController>();
        _tpc = playerRoot.GetComponentInChildren<ThirdPersonController>();
        _inputs = playerRoot.GetComponentInChildren<StarterAssetsInputs>();
    }

    public void RespawnAt(Transform spawnPoint)
    {
        if (spawnPoint == null) return;
        StopAllCoroutines();
        StartCoroutine(RespawnRoutine(spawnPoint.position, spawnPoint.rotation));
    }

    IEnumerator RespawnRoutine(Vector3 pos, Quaternion rot)
    {
        // limpiar inputs
        if (_inputs != null)
        {
            _inputs.move = Vector2.zero;
            _inputs.jump = false;
            _inputs.sprint = false;
            _inputs.look = Vector2.zero;
            _inputs.interact = false;
        }

        // desactiva controlador (más fuerte que MovementEnabled)
        if (_tpc != null) _tpc.enabled = false;

        if (_cc != null) _cc.enabled = false;

        playerRoot.SetPositionAndRotation(pos, rot);

        Physics.SyncTransforms();
        yield return null;

        if (_cc != null) _cc.enabled = true;

        yield return null;

        if (_tpc != null) _tpc.enabled = true;
    }
}