using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject playerPrefab;

    public void SelectTree_Apple()
    {
        if (playerPrefab != null)
            playerPrefab.GetComponent<PlayerMovement>()?.SetTree(0);
    }

    public void SelectTree_Cherry()
    {
        if (playerPrefab != null)
            playerPrefab.GetComponent<PlayerMovement>()?.SetTree(1);
    }
}
