using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using FishNet.Object;
using FishNet.Managing.Scened;
using FishNet;
using NavMeshPlus.Components;
using UnityEngine.AI;

public class MapManager : NetworkBehaviour
{
    [SerializeField] NavMeshSurface _surface2D;
    [SerializeField] List<GameObject> _surfaceMaps;
    [SerializeField] List<TileDatas> _tileDatas;

    Dictionary<TileBase, TileDatas> _dataFromTiles;
    void OnEnable()
    {
        InstanceFinder.SceneManager.OnLoadEnd += SceneManager_OnLoadEnd;
        FindSurfaces();

        _dataFromTiles = new Dictionary<TileBase, TileDatas>();
        foreach (var tileData in _tileDatas)
        {
            foreach (var tile in tileData.tiles)
            {
                _dataFromTiles.Add(tile, tileData);
            }
        }
    }
    private void SceneManager_OnLoadEnd(SceneLoadEndEventArgs args)
    {
        FindSurfaces();
    }

    public AudioClip GetCurrentFloorClip(Vector2 worldPosition)
    {
        foreach (var surfaceMap in _surfaceMaps)
        {
            if (!surfaceMap.activeInHierarchy) continue;
            var surfaceTilemap = surfaceMap.GetComponent<Tilemap>();
            var tile = surfaceTilemap.GetTile(surfaceTilemap.WorldToCell(worldPosition));
            if (tile != null && _dataFromTiles.ContainsKey(tile))
            {
                int index = Random.Range(0, _dataFromTiles[tile].clip.Length);
                return _dataFromTiles[tile].clip[index];
            }
        }
        return null;
    }

    public void FindSurfaces()
    {
        GameObject.FindGameObjectsWithTag("Surface", _surfaceMaps);
    }

    public void UpdateNavMesh()
    {
        if (_surface2D != null)
        {
            var objs = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var item in objs)
                item.GetComponent<NavMeshAgent>().enabled = false;

            _surface2D.UpdateNavMesh(_surface2D.navMeshData);

            foreach (var item in objs)
                item.GetComponent<NavMeshAgent>().enabled = true;
        }
    }

    public IEnumerator UpdateNavMeshAsync()
    {
        UpdateNavMesh();
        yield return null;
    }
}
