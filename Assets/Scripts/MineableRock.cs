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

    void OnEnable()
    {
        _mapManager = FindFirstObjectByType<MapManager>();
    }

    public void ResetResource()
    {
        _health.Value = 100;
        GetComponent<SpriteRenderer>().sprite = _defaultSprite;
        GetComponent<PolygonCollider2D>().enabled = true;
        GetComponent<NavMeshModifier>().overrideArea = true;
        StartCoroutine(_mapManager.UpdateNavMeshAsync());
    }

    public int MineResource(int damage)
    {
        if (_health.Value > 0)
        {
            _health.Value -= damage;
            GetComponent<ParticleSystem>().Play();
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = _rubbleSprite;
            GetComponent<PolygonCollider2D>().enabled = false;
            GetComponent<NavMeshModifier>().overrideArea = false;
            StartCoroutine(_mapManager.UpdateNavMeshAsync());
        }
        return _health.Value;
    }
}
