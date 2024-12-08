using UnityEngine.Tilemaps;
using UnityEngine;

public enum FloorType
{
    None,
    Grass,
    Stone,
    Dirt,
    Carpet,
    Wood,
    Tiling,
    Snow,
    Gravel,
    Water
}

[CreateAssetMenu(fileName = "TileSurface", menuName = "Scriptable Objects/TileSurface")]
public class TileSurface : ScriptableObject
{
    public TileBase[] tiles;
    public AudioClip[] clip;
    public FloorType floorType;
}