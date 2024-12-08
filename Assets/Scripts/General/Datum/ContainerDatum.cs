using UnityEngine;
using UnityEditor;
using Shadowfinder.Scriptables;

namespace Shadowfinder.Datum
{
    public class LootItem
    {
        public GameObject prefab = null;
        public int quantity = 1;
    }

    public class ContainerDatum
    {
        public TileContainer containerDesc = null;
        public LootItem[] loot;
        public int durability = 100;
        public bool isLocked = false;
        public int requiredKey = 0;
        public bool hasLoot = false;
        public bool isOpen = false;
        public bool shouldGenerateLoot = true;

        public void GenerateRandomLoot(int minimumLootQuantity, int maximumLootQuantity)
        {
            var lootPrefabs = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Resources/Loot" });

            int lootCount = Random.Range(minimumLootQuantity, maximumLootQuantity);
            loot = new LootItem[lootCount];
            for (int i = 0; i < lootCount; i++)
            {
                loot[i] = new LootItem();

                int index = Random.Range(0, lootPrefabs.Length);
                loot[i].prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(lootPrefabs[index]));

                loot[i].quantity = Random.Range(1, 5);
            }

            hasLoot = true;
        }

        public void UnpackLoot(Transform transform)
        {
            if (!hasLoot) return;

            Debug.Log("Loot:");
            foreach (var loot in loot)
            {
                Debug.Log("Loot: " + loot.prefab.name);
            }
            Debug.Log("End Loot");

            shouldGenerateLoot = false;
        }

        public void Unlock(int key, AudioSource source, AudioClip unlockClip)
        {
            if (!isLocked) return;

            if (durability > 0 && key == requiredKey)
            {
                isLocked = false;
                source.PlayOneShot(unlockClip);
            }
        }

        public void Lock(int key, AudioSource source, AudioClip lockClip)
        {
            if (isLocked) return;

            if (durability > 0 && key == requiredKey)
            {
                isLocked = true;
                source.PlayOneShot(lockClip);
            }
        }

        public bool Open(AudioSource source, AudioClip openClip, AudioClip lockedClip)
        {
            if (isOpen) return true;
            if (isLocked)
            {
                source.PlayOneShot(lockedClip);
                return false;
            }

            isOpen = true;
            source.PlayOneShot(openClip);
            return true;
        }

        public void Close(AudioSource source, AudioClip closeClip)
        {
            if (!isOpen) return;
            isOpen = false;
            source.PlayOneShot(closeClip);
        }
    }
}
