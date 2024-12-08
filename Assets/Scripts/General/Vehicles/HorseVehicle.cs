using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Utility.Template;
using UnityEngine;
using UnityEngine.Rendering;

namespace Shadowfinder.Vehicles
{
    public class HorseVehicle : TickNetworkBehaviour
    {
        #region Properties

        [SerializeField] Animator _foreAnimator;
        [SerializeField] Animator _backAnimator;

        public string VehicleAnimationMovementName;

        #endregion

        #region Member Fields

        bool _isMounted = false;
        float _speed = 10f;
        Vector3 _lastpos = Vector3.zero;
        Rigidbody2D _rigidbody;
        float _braketime = 0f;
        AudioSource _audioSource;
        SpriteRenderer _backSpriteRenderer;
        Transform _rider;

        #endregion

        #region SyncVars

        readonly SyncVar<bool> _hideBackRenderer = new SyncVar<bool>(false);
        readonly SyncVar<bool> _isMoving = new SyncVar<bool>(false);

        #endregion

        #region Network Callbacks

        void OnHideBackChanged(bool oldValue, bool newValue, bool asServer)
        {
            _backSpriteRenderer.enabled = newValue;
        }

        void OnIsMovingChanged(bool oldValue, bool newValue, bool asServer)
        {
            if (newValue) PlayHorseMoveSFX();
            else _audioSource.Pause();
        }

        #endregion

        #region RPCs

        [ServerRpc(RequireOwnership = false)]
        void SetHideBack(bool value) => _hideBackRenderer.Value = value;

        [ServerRpc(RequireOwnership = false)]
        void SetMoving(bool value) => _isMoving.Value = value;

        #endregion

        #region Setup

        void OnEnable()
        {
            _hideBackRenderer.OnChange += OnHideBackChanged;
            _isMoving.OnChange += OnIsMovingChanged;
            Invoke("SetupRigidbody", 3);
        }

        #endregion

        #region Public Methods

        public void Mount()
        {
            Debug.Log("Horse Mounted");

            _backSpriteRenderer = transform.Find("Horse_Back").GetComponent<SpriteRenderer>();
            _audioSource = GetComponent<AudioSource>();

            _rider = transform.Find("Player(Clone)");
            if (_rider != null)
            {
                var shadow = _rider.Find("Shadow");
                if (shadow != null) shadow.gameObject.SetActive(false);

                var sortingGroup = _rider.GetComponent<SortingGroup>();
                if (sortingGroup != null) sortingGroup.sortingOrder = 1;
            }

            AnimSetBool(_foreAnimator, "Mounted", true);
            AnimSetInteger(_foreAnimator, VehicleAnimationMovementName, -1);
            AnimSetInteger(_backAnimator, VehicleAnimationMovementName, -1);

            transform.Find("Horse_Fore").GetComponent<SpriteRenderer>().sortingOrder = 1;
            _isMounted = true;
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

            AnimSetBool(_foreAnimator, "Mounted", false);
            AnimSetInteger(_foreAnimator, VehicleAnimationMovementName, -1);
            AnimSetInteger(_backAnimator, VehicleAnimationMovementName, -1);

            transform.Find("Horse_Fore").GetComponent<SpriteRenderer>().sortingOrder = 0;

            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.simulated = true;
            _rigidbody.linearVelocity = Vector2.zero;
        }

        #endregion

        #region OnTick

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
                if (AnimGetInteger(_foreAnimator, VehicleAnimationMovementName) >= 0)
                    SetMoving(true);

                if (!keepDirState)
                {
                    AnimSetFloat(_foreAnimator, "DX", dx);
                    AnimSetFloat(_foreAnimator, "DY", dy);
                    AnimSetFloat(_backAnimator, "DX", dx);
                    AnimSetFloat(_backAnimator, "DY", dy);

                    if (axisy != 0f && axisx == 0f)
                    {
                        SetHideBack(true);
                        AnimSetInteger(_foreAnimator, VehicleAnimationMovementName, 0);
                        AnimSetInteger(_backAnimator, VehicleAnimationMovementName, 0);
                    }
                    else if (axisx != 0f && axisy == 0f)
                    {
                        AnimSetInteger(_backAnimator, VehicleAnimationMovementName, -1);
                        AnimSetInteger(_foreAnimator, VehicleAnimationMovementName, 0);
                        SetHideBack(false);
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

        #endregion

        #region Private Methods

        void SetupRigidbody()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.simulated = true;
        }

        void SetNotMoving(float dx, float dy)
        {
            AnimSetInteger(_backAnimator, VehicleAnimationMovementName, -1);
            AnimSetInteger(_foreAnimator, VehicleAnimationMovementName, -1);
            if (dx != 0f && dy == 0f) SetHideBack(false);
            SetMoving(false);
        }

        void SetRiderOffset(float x, float y)
        {
            if (_rider != null) _rider.localPosition = new Vector3(x, y, 0f);
        }

        void PlayHorseMoveSFX()
        {
            if (!_audioSource.isPlaying)
            {
                if (_audioSource.time != 0) _audioSource.UnPause();
                else _audioSource.Play();
            }
        }

        #endregion

        #region Animations

        int AnimGetInteger(Animator anim, string name)
        {
            if (anim == null) return 0;
            if (anim.gameObject.activeSelf)
            {
                return anim.GetInteger(name);
            }
            return 0;
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
