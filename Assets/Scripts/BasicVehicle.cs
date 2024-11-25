using FishNet.Utility.Template;
using UnityEngine;
using UnityEngine.AI;

public class BasicVehicle : TickNetworkBehaviour
{
    [SerializeField] Animator _vehicleAnimator;
    [SerializeField] Animator _secondaryAnimator;

    public string vehicleRunningBool;

    bool _isMounted = false;
    float _speed = 20f;
    Vector3 _lastpos = Vector3.zero;
    Rigidbody2D _rigidbody;
    float _braketime = 0f;

    private void OnEnable()
    {
        Invoke("SetupRigidbody", 3);
    }

    void SetupRigidbody()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.simulated = true;
    }

    public void Mount()
    {
        Debug.Log("Vehicle Mounted");

        // If this was a car you could swing open the car door, start the car, etc.

        AnimSetBool(_vehicleAnimator, vehicleRunningBool, true);
        AnimSetBool(_secondaryAnimator, vehicleRunningBool, true);

        transform.Find("WaterEffect").gameObject.SetActive(true);

        _isMounted = true;
    }

    public void Unmount()
    {
        Debug.Log("Vehicle Unmounted");

        _isMounted = false;

        AnimSetBool(_vehicleAnimator, vehicleRunningBool, false);
        AnimSetBool(_secondaryAnimator, vehicleRunningBool, false);
        GetComponent<PolygonCollider2D>().TryUpdateShapeToAttachedSprite();

        _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody.simulated = true;
        _rigidbody.linearVelocity = Vector2.zero;
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

        _rigidbody.linearVelocity = new Vector2(axisx * _speed * 25 * Time.fixedDeltaTime, axisy * _speed * 25 * Time.fixedDeltaTime);

        if (Time.fixedTime - _braketime > 0.5f)
        {
            if (0f == axisx && 0f == axisy)
            {
                _braketime = Time.fixedTime;
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }

        if (_lastpos != transform.position)
        {
            if (!keepDirState)
            {
                AnimSetFloat(_vehicleAnimator, "DX", dx);
                AnimSetFloat(_vehicleAnimator, "DY", dy);
                AnimSetFloat(_secondaryAnimator, "DX", dx);
                AnimSetFloat(_secondaryAnimator, "DY", dy);
            }

            AnimSetBool(_secondaryAnimator, vehicleRunningBool, !keepDirState);
            _lastpos = transform.position;
        }
    }

    #region Animations

    bool AnimGetBool(Animator anim, string name)
    {
        if (anim != null && anim.gameObject.activeSelf)
            return anim.GetBool(name);
        return false;
    }

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

    #endregion
}
