using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class StashArea : NetworkBehaviour
{
    [SerializeField] Renderer[] _stashObjects;

    readonly SyncVar<int> _stashCount = new(-1);

    [ServerRpc(RequireOwnership = false)]
    public void SetStashCount(int value) => _stashCount.Value = value;

    void OnStashCountChanged(int oldValue, int newValue, bool asServer)
    {
        if (!asServer)
        {
            for (int i = 0; i < _stashObjects.Length; i++)
                _stashObjects[i].enabled = i == newValue;
        }
    }

    void UpdateStashCount(int value)
    {
        if (base.IsClientInitialized)
            SetStashCount(value);
        else
            _stashCount.Value = value;
    }

    protected void OnEnable()
    {
        _stashCount.OnChange += OnStashCountChanged;
    }

    protected void OnDisable()
    {
        _stashCount.OnChange -= OnStashCountChanged;
    }

    void HideAll()
    {
        for (int i = 0; i < _stashObjects.Length; i++)
        {
            if (_stashObjects[i].enabled)
                _stashObjects[i].enabled = false;
        }
    }

    public int GetStashedCount()
    {
        return _stashCount.Value + 1;
    }

    public bool HasStash()
    {
        return GetStashedCount() > 0;
    }

    public bool HasFree()
    {
        return GetStashedCount() < _stashObjects.Length;
    }

    public bool PlaceStash()
    {
        if (!base.IsServerInitialized) return false;

        if (GetStashedCount() < _stashObjects.Length)
        {
            HideAll();
            _stashObjects[GetStashedCount()].enabled = true;
            UpdateStashCount(_stashCount.Value + 1);
            return true;
        }
        return false;
    }

    public bool TakeStash()
    {
        if (!base.IsServerInitialized) return false;

        var value = _stashCount.Value;
        if (value < 0) return false;

        if (_stashObjects[value].enabled)
        {
            _stashObjects[value].enabled = false;
            if (--value >= 0) _stashObjects[value].enabled = true;
            UpdateStashCount(value);
            return true;
        }
        return false;
    }

    public override void OnStartServer()
    {
        GiveOwnership(base.Owner);
        int value = -1;
        for (int i = _stashObjects.Length - 1; i >= 0; --i)
            if (_stashObjects[i].enabled) { value = i; break; }
        _stashCount.Value = value;
    }
}
