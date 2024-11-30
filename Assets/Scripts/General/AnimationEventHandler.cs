using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    [SerializeField] AudioSource _audioSource;
    [SerializeField] MapManager _mapManager;

    void Awake()
    {
        _mapManager = FindFirstObjectByType<MapManager>();
        _audioSource = GetComponentInChildren<AudioSource>();
    }

    public void Step()
    {
        if (_mapManager != null && _audioSource != null)
        {
            var currentFloorClip = _mapManager.GetCurrentFloorClip(transform.position);
            if (currentFloorClip != null)
            {
                if (_audioSource.isPlaying == false)
                    _audioSource.PlayOneShot(currentFloorClip);
            }
        }
    }
}
