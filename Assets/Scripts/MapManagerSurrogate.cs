using FishNet.Object;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManagerSurrogate : NetworkBehaviour
{
    //MapManager _mapManager;
    [SerializeField] Tilemap surfaceMap;

    void OnEnable()
    {
        //_mapManager = FindFirstObjectByType<MapManager>();
        
        //if (_mapManager != null && !_mapManager._surfaceMaps.Contains(surfaceMap))
        //{
        //    _mapManager._surfaceMaps.Add(surfaceMap);
        //}
    }

    private void OnDisable()
    {
        //if (_mapManager != null && _mapManager._surfaceMaps.Contains(surfaceMap))
        //{
        //    _mapManager._surfaceMaps.Remove(surfaceMap);
        //}
    }
}
