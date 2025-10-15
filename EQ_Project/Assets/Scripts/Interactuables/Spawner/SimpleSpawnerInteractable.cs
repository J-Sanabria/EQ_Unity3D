using UnityEngine;
using TMPro; // IMPORTANTE

public class SimpleSpawnerInteractable : Interactable
{
    [Header("Spawn")]
    public GameObject prefab;
    public Transform spawnPoint;
    public float cooldown = 0.5f;

    [Header("Cantidad por Collectible")]
    public int spawnAmount = 1;
    public int minAmount = 1;
    public int maxAmount = 99;

    [Header("UI opcional")]
    [SerializeField] TMP_Text amountLabel; // TextMesh Pro (UGUI o 3D)

    float timer;

    void Update()
    {
        if (timer > 0f) timer -= Time.deltaTime;
    }

    public override void Interact(Transform interactor)
    {
        if (prefab == null) return;
        if (timer > 0f) return;

        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint ? spawnPoint.rotation : transform.rotation;

        var go = Instantiate(prefab, pos, rot);

        var col = go.GetComponent<Collectible>();
        if (col != null)
            col.amount = Mathf.Clamp(spawnAmount, minAmount, maxAmount);

        timer = cooldown;
    }

    public void AdjustAmount(int delta)
    {
        spawnAmount = Mathf.Clamp(spawnAmount + delta, minAmount, maxAmount);
        RefreshUI();
    }

    public void SetAmount(int value)
    {
        spawnAmount = Mathf.Clamp(value, minAmount, maxAmount);
        RefreshUI();
    }

    void OnValidate()
    {
        spawnAmount = Mathf.Clamp(spawnAmount, minAmount, maxAmount);
        RefreshUI();
    }

    void Start()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        if (amountLabel != null)
            amountLabel.SetText("x{0}", spawnAmount); // TMP: más eficiente que .text
    }

    // Prompt dinámico (opcional)
    public new string Prompt => "E - Generar x" + spawnAmount.ToString();
}
