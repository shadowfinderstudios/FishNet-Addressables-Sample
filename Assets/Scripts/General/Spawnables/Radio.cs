using UnityEngine;

namespace Shadowfinder.Spawnables
{
    public class Radio : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                GetComponent<AudioSource>().Play();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                GetComponent<AudioSource>().Pause();
            }
        }
    }
}
