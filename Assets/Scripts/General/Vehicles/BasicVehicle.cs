using FishNet.Utility.Template;
using UnityEngine;
using Shadowfinder.Extensions;

namespace Shadowfinder.Vehicles
{
    public class BasicVehicle : TickNetworkBehaviour
    {
        [SerializeField] Animator _vehicleAnimator;
        [SerializeField] Animator _secondaryAnimator;

        public string VehicleAnimationMovementName;

        bool _isMounted = false;
        float _speed = 20f;
        Vector3 _lastpos = Vector3.zero;
        Rigidbody2D _rigidbody;
        float _braketime = 0f;
        AudioSource _audioSource;

        private void OnEnable()
        {
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
            Debug.Log("Vehicle Mounted");

            // If this was a car you could swing open the car door, start the car, etc.

            AnimSetInteger(_vehicleAnimator, VehicleAnimationMovementName, 0);
            AnimSetInteger(_secondaryAnimator, VehicleAnimationMovementName, 0);

            var waterEffect = transform.Find("WaterEffect");
            if (waterEffect != null)
                waterEffect.gameObject.GetComponent<SpriteRenderer>().enabled = true;

            _isMounted = true;

            if (_audioSource.time != 0)
                _audioSource.UnPause();
            else
                _audioSource.Play();
        }

        public void Unmount()
        {
            Debug.Log("Vehicle Unmounted");

            _isMounted = false;

            AnimSetInteger(_vehicleAnimator, VehicleAnimationMovementName, -1);
            AnimSetInteger(_secondaryAnimator, VehicleAnimationMovementName, -1);
            GetComponent<PolygonCollider2D>().TryUpdateShapeToAttachedSprite();

            var waterEffect = transform.Find("WaterEffect");
            if (waterEffect != null)
                waterEffect.gameObject.GetComponent<SpriteRenderer>().enabled = true;

            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.simulated = true;
            _rigidbody.linearVelocity = Vector2.zero;

            _audioSource.Pause();
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
                _braketime = Time.fixedTime;

                if (0f == axisx && 0f == axisy)
                    _rigidbody.linearVelocity = Vector2.zero;
            }

            if (_lastpos != transform.position)
            {
                if (!keepDirState)
                {
                    AnimSetFloat(_vehicleAnimator, "DX", dx);
                    AnimSetFloat(_vehicleAnimator, "DY", dy);
                    AnimSetFloat(_secondaryAnimator, "DX", dx);
                    AnimSetFloat(_secondaryAnimator, "DY", dy);
                    AnimSetInteger(_secondaryAnimator, VehicleAnimationMovementName, 0);
                }
                else
                {
                    AnimSetInteger(_secondaryAnimator, VehicleAnimationMovementName, -1);
                }

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

        void AnimSetInteger(Animator anim, string name, int value)
        {
            if (anim != null && anim.gameObject.activeSelf)
                anim.SetInteger(name, value);
        }

        #endregion
    }
}
