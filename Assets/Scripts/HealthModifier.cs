using UnityEngine;
using FishNet.Object;

[RequireComponent(typeof(BoxCollider2D))]
public class HealthModifier : NetworkBehaviour
{
    [SerializeField] int _healthModifier = 1;
    [SerializeField] bool _affectWhilePresent = false;
    [SerializeField] float _frequency = 0.5f;
    [SerializeField] AudioClip _audioClip;

    bool _isPresent = false;
    Collider2D _collision;
    float _timer = 0f;
    AudioSource _audioSource;

    void OnEnable()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (_affectWhilePresent)
            {
                _isPresent = true;
                _collision = collision;
            }

            ModifyHealth(collision.gameObject, _healthModifier);
            _audioSource?.PlayOneShot(_audioClip);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_affectWhilePresent && collision.CompareTag("Player"))
        {
            _isPresent = false;
            _collision = null;
            collision.gameObject.GetComponent<PlayerController>().ReleaseHealth();
        }
    }

    void ModifyHealth(GameObject obj, int amount)
    {
        obj.GetComponent<PlayerController>().ModifyHealth(amount);
    }

    void Update()
    {
        // Note: OnTriggerStay2D wasn't working properly, so switched to this method.

        if (_affectWhilePresent && _isPresent)
        {
            _timer += Time.deltaTime;
            if (_timer >= _frequency)
            {
                ModifyHealth(_collision.gameObject, _healthModifier);
                _timer = 0f;
            }
        }
    }
}
