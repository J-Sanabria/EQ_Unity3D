using System.Collections;
using UnityEngine;
using StarterAssets; // para ThirdPersonController

[RequireComponent(typeof(CharacterController))]
public class LastSafePlatformRespawn : MonoBehaviour
{
    [Header("Detección de plataforma segura")]
    [Tooltip("Capas consideradas como plataformas/suelo ‘seguro’")]
    public LayerMask platformLayers;

    [Tooltip("Altura que se añade al punto de impacto para evitar incrustarse")]
    public float standUpOffset = 0.1f;

    [Tooltip("Distancia del raycast vertical hacia abajo desde el centro del CharacterController")]
    public float groundRayDistance = 1.0f;

    [Header("Condiciones de caída")]
    [Tooltip("Si Y < killYThreshold  respawn")]
    public float killYThreshold = -20f;

    //[Tooltip("Opcional: activar respawn al tocar un trigger con esta tag (ej: KillZone)")]
    //public string killZoneTag = "KillZone";

    [Header("Opcional")]
    [Tooltip("Si está marcado, dibuja gizmos del último punto seguro")]
    public bool drawGizmos = true;

    private ThirdPersonController _tpc;
    private CharacterController _cc;

    private Vector3 _lastSafePosition;
    private Quaternion _lastSafeRotation;
    private bool _hasSafePoint;

    private void Awake()
    {
        _tpc = GetComponent<ThirdPersonController>();
        _cc = GetComponent<CharacterController>();

        // Fallback inicial: posición de spawn
        _lastSafePosition = transform.position;
        _lastSafeRotation = transform.rotation;
        _hasSafePoint = true;
    }

    private void Update()
    {
        // Guardar último punto seguro cuando está en suelo
        if (_tpc != null && _tpc.Grounded)
        {
            // Raycast hacia abajo desde el centro del controller
            Vector3 origin = transform.position + Vector3.up * 0.1f; 
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundRayDistance, platformLayers, QueryTriggerInteraction.Ignore))
            {
                _lastSafePosition = hit.point + Vector3.up * (standUpOffset + _cc.skinWidth);
                _lastSafeRotation = transform.rotation;
                _hasSafePoint = true;
            }
        }

        // Detectar caída por umbral Y
        if (transform.position.y < killYThreshold)
        {
            Respawn();
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!string.IsNullOrEmpty(killZoneTag) && other.CompareTag(killZoneTag))
    //    {
    //        Respawn();
    //    }
    //}

    public void Respawn()
    {
        if (!_hasSafePoint) return;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        // Desactiva el controlador de movimiento para que no intente mover durante el respawn
        if (_tpc != null) _tpc.enabled = false;

        // Deshabilita el CharacterController para reposicionar sin choques
        _cc.enabled = false;

        // Reposiciona y orienta
        transform.position = _lastSafePosition;
        transform.rotation = _lastSafeRotation;

        // Sincroniza transformaciones y espera un frame
        Physics.SyncTransforms();
        yield return null;

        // Vuelve a habilitar el CharacterController
        _cc.enabled = true;

        // Espera un frame adicional para que Grounded se actualice sin conflictos
        yield return null;

        // Reactiva el ThirdPersonController
        if (_tpc != null) _tpc.enabled = true;
    }
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(_lastSafePosition, 0.1f);
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        Gizmos.DrawWireSphere(_lastSafePosition, 0.25f);
    }
}
