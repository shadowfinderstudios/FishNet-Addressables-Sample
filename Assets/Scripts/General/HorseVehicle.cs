using FishNet.Utility.Template;
using UnityEngine;
using UnityEngine.Rendering;

public class HorseVehicle : TickNetworkBehaviour
{
    [SerializeField] Animator _foreAnimator;
    [SerializeField] Animator _backAnimator;

    public string VehicleAnimationMovementName;

    bool _isMounted = false;
    float _speed = 10f;
    Vector3 _lastpos = Vector3.zero;
    Rigidbody2D _rigidbody;
    float _braketime = 0f;
    AudioSource _audioSource;
    SpriteRenderer _backSpriteRenderer;
    Transform _rider;

    void OnEnable()
    {
        _backSpriteRenderer = transform.Find("Horse_Back").GetComponent<SpriteRenderer>();
        _backSpriteRenderer.enabled = true;

        _audioSource = GetComponent<AudioSource>();
        Invoke("SetupRigidbody", 3);
    }

    void SetupRigidbody()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.simulated = true;
    }

    public void Mount()
    {
        Debug.Log("Horse Mounted");

        _rider = transform.Find("Player(Clone)");
        if (_rider != null)
        {
            var shadow = _rider.Find("Shadow");
            if (shadow != null) shadow.gameObject.SetActive(false);

            var sortingGroup = _rider.GetComponent<SortingGroup>();
            if (sortingGroup != null) sortingGroup.sortingOrder = 1;
        }

        AnimSetInteger(_foreAnimator, VehicleAnimationMovementName, -1);
        AnimSetInteger(_backAnimator, VehicleAnimationMovementName, -1);

        _isMounted = true;

        //if (_audioSource.time != 0) _audioSource.UnPause();
        //else _audioSource.Play();
    }

    public void Unmount()
    {
        Debug.Log("Vehicle Unmounted");

        _isMounted = false;

        if (_rider != null)
        {
            var shadow = _rider.Find("Shadow");
            if (shadow != null) shadow.gameObject.SetActive(true);

            var sortingGroup = _rider.GetComponent<SortingGroup>();
            if (sortingGroup != null) sortingGroup.sortingOrder = 0;

            _rider = null;
        }

        AnimSetInteger(_foreAnimator, VehicleAnimationMovementName, -1);
        AnimSetInteger(_backAnimator, VehicleAnimationMovementName, -1);
        //GetComponent<PolygonCollider2D>().TryUpdateShapeToAttachedSprite();

        _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody.simulated = true;
        _rigidbody.linearVelocity = Vector2.zero;

        //_audioSource.Pause();
    }

    void SetNotMoving(float dx, float dy)
    {
        AnimSetInteger(_backAnimator, VehicleAnimationMovementName, -1);
        AnimSetInteger(_foreAnimator, VehicleAnimationMovementName, -1);
        if (dx != 0f && dy == 0f) _backSpriteRenderer.enabled = false;
    }

    void SetRiderOffset(float x, float y)
    {
        if (_rider != null) _rider.localPosition = new Vector3(x, y, 0f);
    }

    protected override void TimeManager_OnTick()
    {
        if (!base.HasAuthority) return;
        if (!_isMounted) return;

        float axisx = Input.GetAxis("Horizontal");
        float axisy = Input.GetAxis("Vertical");

        const float deadzone = 0.25f;
        float dx = 0f; float dy = 0f;
        bool keepDirState = false;
        if (Mathf.Abs(axisx) > deadzone) dx = Mathf.Sign(axisx);
        else if (Mathf.Abs(axisy) > deadzone) dy = Mathf.Sign(axisy);
        else keepDirState = true;

        _rigidbody.linearVelocity = new Vector2(axisx * _speed * 25 * Time.fixedDeltaTime,
            axisy * _speed * 25 * Time.fixedDeltaTime);

        if (Time.fixedTime - _braketime > 0.5f)
        {
            _braketime = Time.fixedTime;

            if (0f == axisx && 0f == axisy)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                SetNotMoving(dx, dy);
            }
        }

        if (_lastpos != transform.position)
        {
            if (!keepDirState)
            {
                AnimSetFloat(_foreAnimator, "DX", dx);
                AnimSetFloat(_foreAnimator, "DY", dy);
                AnimSetFloat(_backAnimator, "DX", dx);
                AnimSetFloat(_backAnimator, "DY", dy);

                if (axisy != 0f && axisx == 0f)
                {
                    _backSpriteRenderer.enabled = true;
                    AnimSetInteger(_foreAnimator, VehicleAnimationMovementName, 0);
                    AnimSetInteger(_backAnimator, VehicleAnimationMovementName, 0);
                }
                else if (axisx != 0f && axisy == 0f)
                {
                    AnimSetInteger(_backAnimator, VehicleAnimationMovementName, -1);
                    AnimSetInteger(_foreAnimator, VehicleAnimationMovementName, 0);
                    _backSpriteRenderer.enabled = false;
                }

                if (dy == 0f)
                {
                    if (dy == 0f && dx > 0f)
                        SetRiderOffset(0.21f, 1.68f);
                    else if (dy == 0f && dx < 0f)
                        SetRiderOffset(-0.21f, 1.68f);
                }
                else
                {
                    if (dy > 0f)
                        SetRiderOffset(0f, 1.3f);
                    else if (dy < 0f)
                        SetRiderOffset(0f, 1.3f);
                }
            }
            else SetNotMoving(dx, dy);
            _lastpos = transform.position;
        }
    }

    #region Animations

    void AnimSetBool(Animator anim, string name, bool value)
    {
        if (anim != null && anim.gameObject.activeSelf)
            anim.SetBool(name, value);
    }

    void AnimSetFloat(Animator anim, string name, float value)
    {
        if (anim != null && anim.gameObject.activeSelf)
            anim.SetFloat(name, value);
    }

    void AnimSetInteger(Animator anim, string name, int value)
    {
        if (anim != null && anim.gameObject.activeSelf)
            anim.SetInteger(name, value);
    }

    #endregion
}
