using UnityEngine;
using FishNet.Object;
using System.Collections;
using FishNet.Connection;

public class SpellCast : NetworkBehaviour
{
    [SerializeField] public AudioClip _audioClip;

    private void Start()
    {
        StartCoroutine(Die());
    }

    [ServerRpc(RequireOwnership = false)]
    void ProcessDespawn(NetworkObject nob, NetworkConnection conn = null)
    {
        Despawn();
    }

    IEnumerator Die()
    {
        gameObject.GetComponent<AudioSource>().PlayOneShot(_audioClip);

        // Wait for the spell effect animation then despawn the effect.
        yield return new WaitForSeconds(1.5f);

        ProcessDespawn(GetComponent<NetworkObject>(), base.Owner);
    }
}
