using UnityEngine;

public class RegionArea : MonoBehaviour
{
    [SerializeField] string _regionName;
    [SerializeField] string _regionDescription;
    [SerializeField] string _regionType;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerController>().EnterRegionArea(_regionName, _regionDescription, _regionType);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerController>().ExitRegionArea(_regionName, _regionDescription, _regionType);
        }
    }
}
