using FishNet.Object;
using UnityEngine;

public class StashArea : NetworkBehaviour
{
    [SerializeField] Renderer[] _stashObjects;

    int _stashCount;

    private void OnEnable()
    {
        int value = -1;
        for (int i = _stashObjects.Length - 1; i >= 0; --i)
        {
            if (_stashObjects[i].enabled)
            {
                value = i;
                break;
            }
        }
        _stashCount = value;
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
        return _stashCount + 1;
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
        if (GetStashedCount() < _stashObjects.Length)
        {
            HideAll();
            _stashObjects[GetStashedCount()].enabled = true;
            ++_stashCount;
            return true;
        }
        return false;
    }

    public bool TakeStash()
    {
        if (_stashCount < 0) return false;

        if (_stashObjects[_stashCount].enabled)
        {
            _stashObjects[_stashCount].enabled = false;
            _stashCount--;

            if (_stashCount >= 0)
                _stashObjects[_stashCount].enabled = true;

            return true;
        }
        return false;
    }
}
