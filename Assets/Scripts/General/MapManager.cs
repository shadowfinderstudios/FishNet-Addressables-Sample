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
    [SerializeField] List<TileSurface> _tileDatas;
    [SerializeField] List<GameObject> _containerMaps;
    [SerializeField] List<TileContainer> _tileContainers;

    Dictionary<TileBase, TileSurface> _surfaceDescriptors;
    Dictionary<TileBase, TileContainer> _containerDescriptors;

    Dictionary<TileBase, ContainerState> _containerCorners;
    Dictionary<TileBase, ContainerState> _containers;

    void OnEnable()
    {
        InstanceFinder.SceneManager.OnLoadEnd += SceneManager_OnLoadEnd;

        FindMaps();

        _surfaceDescriptors = new Dictionary<TileBase, TileSurface>();
        foreach (var tileData in _tileDatas)
        {
            foreach (var tile in tileData.tiles)
            {
                _surfaceDescriptors.Add(tile, tileData);
            }
        }

        _containerCorners = new();
        _containers = new();

        _containerDescriptors = new Dictionary<TileBase, TileContainer>();
        foreach (var tileContainer in _tileContainers)
        {
            _containerCorners.Add(tileContainer.upperLeftTile, new ContainerState());

            foreach (var tile in tileContainer.tiles)
            {
                _containerDescriptors.Add(tile, tileContainer);
            }
        }

        SetupContainers();
    }

    void SetupContainers()
    {
        foreach (var containerMap in _containerMaps)
        {
            if (!containerMap.activeInHierarchy) continue;

            var containerTilemap = containerMap.GetComponent<Tilemap>();
            var bounds = containerTilemap.localBounds;

            int i = 0;
            var pos = new Vector3Int(0, 0, 0);
            for (int y = (int)bounds.min.y; y < (int)bounds.max.y; y++)
            {
                for (int x = (int)bounds.min.x; x < (int)bounds.max.x; x++)
                {
                    pos.x = x; pos.y = y;
                    var tile = containerTilemap.GetTile(pos);
                    if (tile != null && _containerCorners.ContainsKey(tile))
                    {
                        var state = _containerCorners[tile];

                        if (_containerDescriptors.ContainsKey(tile))
                        {
                            var desc = _containerDescriptors[tile];
                            state.containerDesc = desc;

                            var containerPos = new Vector3Int(x, y, 0);
                            for (int cy = 0; cy < desc.CellsHigh; cy++)
                            {
                                for (int cx = 0; cx < desc.CellsWide; cx++)
                                {
                                    containerPos.x = x + cx;
                                    containerPos.y = y - cy;

                                    var t = containerTilemap.GetTile(containerPos);

                                    if (t != null && !_containers.ContainsKey(t))
                                    {
                                        _containers.Add(t, state);
                                    }
                                }
                            }
                        }
                    }

                    i++;
                }
            }
        }
    }

    private void SceneManager_OnLoadEnd(SceneLoadEndEventArgs args)
    {
        FindMaps();
    }

    public AudioClip GetCurrentFloorClip(Vector2 worldPosition)
    {
        foreach (var surfaceMap in _surfaceMaps)
        {
            if (!surfaceMap.activeInHierarchy) continue;
            var surfaceTilemap = surfaceMap.GetComponent<Tilemap>();
            var tile = surfaceTilemap.GetTile(surfaceTilemap.WorldToCell(worldPosition));
            if (tile != null && _surfaceDescriptors.ContainsKey(tile))
            {
                int index = Random.Range(0, _surfaceDescriptors[tile].clip.Length);
                return _surfaceDescriptors[tile].clip[index];
            }
        }
        return null;
    }

    public ContainerState GetContainer(Vector2 worldPosition)
    {
        foreach (var containerMap in _containerMaps)
        {
            if (!containerMap.activeInHierarchy) continue;
            var containerTilemap = containerMap.GetComponent<Tilemap>();
            var tile = containerTilemap.GetTile(containerTilemap.WorldToCell(worldPosition));
            if (tile != null && _containers.ContainsKey(tile))
            {
                return _containers[tile];
            }
        }
        return null;
    }

    public void FindMaps()
    {
        GameObject.FindGameObjectsWithTag("Surface", _surfaceMaps);
        GameObject.FindGameObjectsWithTag("Container", _containerMaps);
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
