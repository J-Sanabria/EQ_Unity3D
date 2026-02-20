using UnityEngine;
using TMPro;
using static UnityEditor.Progress;


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

    ItemDefinition item;

    [Header("Base de datos")]
    [SerializeField] InventoryDatabase inventoryDatabase;

    [Header("UI opcional")]
    [SerializeField] TMP_Text amountLabel; // TextMesh Pro (UGUI o 3D)
    [SerializeField] SpriteRenderer elementIcon; 

    float timer;
    void Update()
    {
        if (timer > 0f)
            timer -= Time.deltaTime;
    }

    public void Configure(ItemDefinition def)
    {
        item = def;

        if (item == null || item.collectiblePrefab == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        RefreshUI();
        RefreshElementUI();
    }
    public override void Interact(Transform interactor)
    {
        if (item == null || item.collectiblePrefab == null) return;
        if (timer > 0f) return;

        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint ? spawnPoint.rotation : transform.rotation;

        var col = Instantiate(item.collectiblePrefab, pos, rot);
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
        RefreshElementUI();
    }

    void Start()
    {
        RefreshUI();
        RefreshElementUI();
    }

    void RefreshUI()
    {
        if (amountLabel != null)
            amountLabel.SetText("x{0}", spawnAmount);
    }

    void RefreshElementUI()
    {
        if (elementIcon == null) return;

        elementIcon.sprite = item.icon;
        elementIcon.enabled = item.icon != null;
    }

    // Prompt dinámico (opcional)
    public new string Prompt => "E - Generar x" + spawnAmount.ToString();
}
