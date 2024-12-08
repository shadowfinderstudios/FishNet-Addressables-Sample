using UnityEngine.Tilemaps;
using UnityEngine;

namespace Shadowfinder.Scriptables
{
    public enum ContainerType
    {
        None,
        Chest,
        Crate,
        Barrel
    }

    [CreateAssetMenu(fileName = "TileContainers", menuName = "Scriptable Objects/TileContainers")]
    public class TileContainer : ScriptableObject
    {
        public int CellsWide = 1;
        public int CellsHigh = 1;
        public TileBase upperLeftTile;
        public TileBase[] tiles;
        public AudioClip openClip;
        public AudioClip closeClip;
        public AudioClip lockClip;
        public AudioClip unlockClip;
        public AudioClip lockedClip;
        public ContainerType containerType;
        public int lockDifficulty = 0;
        public int minimumLootQuantity = 1;
        public int maximumLootQuantity = 5;
    }
}
