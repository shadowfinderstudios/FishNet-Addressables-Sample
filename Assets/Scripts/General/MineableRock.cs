using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.CodeGenerating;
using NavMeshPlus.Components;

public class MineableRock : NetworkBehaviour, IMineable
{
    [SerializeField] Sprite _defaultSprite;
    [SerializeField] Sprite _rubbleSprite;

    [AllowMutableSyncType]
    [SerializeField]
    SyncVar<int> _health = new SyncVar<int>(100);

    MapManager _mapManager;

    [ServerRpc(RequireOwnership = false)]
    void SetHealth(int value) => _health.Value = value;

    [ObserversRpc(ExcludeOwner = true)]
    void ObserversSetHealth(int value) => UpdateEffects(value);

    private void OnHealthChanged(int oldValue, int newValue, bool asServer)
    {
        if (asServer) ObserversSetHealth(newValue);
        else UpdateEffects(newValue);
    }

    void UpdateEffects(int newValue)
    {
        var isReset = newValue == 100;
        if (!isReset && newValue > 0)
        {
            GetComponent<ParticleSystem>().Play();
        }
        else
        {
            var sprite = isReset ? _defaultSprite : _rubbleSprite;
            GetComponent<SpriteRenderer>().sprite = sprite;
            GetComponent<PolygonCollider2D>().enabled = isReset;
            GetComponent<NavMeshModifier>().overrideArea = isReset;
            StartCoroutine(_mapManager.UpdateNavMeshAsync());
        }
    }

    protected void OnEnable()
    {
        _mapManager = FindFirstObjectByType<MapManager>();
        _health.OnChange += OnHealthChanged;
    }

    protected void OnDisable()
    {
        _health.OnChange -= OnHealthChanged;
    }

    public void ResetResource()
    {
        SetHealth(100);
    }

    public int MineResource(int damage)
    {
        var health = _health.Value;
        if (health > 0)
        {
            health -= damage;
            SetHealth(health);
        }
        return health;
    }
}
