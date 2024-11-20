using UnityEngine;
using FishNet.Object;
using System.Collections;

public class SpellCast : NetworkBehaviour
{
    [SerializeField] public AudioClip _audioClip;

    private void Start()
    {
        StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        gameObject.GetComponent<AudioSource>().PlayOneShot(_audioClip);

        // Wait for the spell effect animation then despawn the effect.
        yield return new WaitForSeconds(1.5f);
        Despawn();
    }
}
