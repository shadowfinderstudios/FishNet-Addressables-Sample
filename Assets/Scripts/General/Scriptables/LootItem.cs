using System;
using UnityEngine;

[Serializable]
public struct LootData
{
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "Loot", menuName = "Scriptable Objects/Loot")]
public class Loot : ScriptableObject
{
    public LootData[] lootItems;
}