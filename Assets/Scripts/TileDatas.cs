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

[CreateAssetMenu(fileName = "TileDatas", menuName = "Scriptable Objects/TileDatas")]
public class TileDatas : ScriptableObject
{
    public TileBase[] tiles;
    public AudioClip[] clip;
    public FloorType floorType;
}