using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;
using UnityEngine.AI;
using Unity.Cinemachine;

using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Managing;
using FishNet.Managing.Object;
using FishNet.Connection;
using FishNet.Component.Animating;
using FishNet.Component.Transforming;
using FishNet.Utility.Template;

using GameKit.Dependencies.Utilities;
using TMPro;
using UnityEngine.Tilemaps;

public class PlayerController : TickNetworkBehaviour
{
    readonly string AnimationMovementName = "Motion";

    #region Network Update Data

    [System.Serializable]
    public class VehicleUpdate
    {
        public bool isMounted = false;
        public Transform vehicleTransform = null;
        public List<Transform> guestTransforms = new();
        public float mountOffsetX = 0f;
        public float mountOffsetY = 0.5f;
    }

    #endregion

    #region Properties

    [Header("General")]
    [SerializeField] CinemachineCamera _camera;
    [SerializeField] Animator _bodyAnim;
    [SerializeField] NetworkAnimator _netBodyAnim;
    [SerializeField] string _spawnTreesId;
    [SerializeField] GameObject _spellPrefab;
    [SerializeField] AudioSource _audioSource;
    [SerializeField] SpriteLibraryAsset[] _spriteLibraryAssets;
    [SerializeField] GameObject _healthStatus;
    [SerializeField] GameObject _manaStatus;
    [SerializeField] GameObject _staminaStatus;
    [SerializeField] Sprite _damageStatusSprite;
    [SerializeField] Sprite _healthStatusSprite;
    [SerializeField] TMP_InputField _inputField;

    [Header("Mining")]
    [SerializeField] AudioClip _destroyRockSound;
    [SerializeField] AudioClip[] _pickaxeSounds;

    #endregion

    #region Member Fields

    bool _firstSpawn = true;
    bool _isWalking = false;
    bool _canMove = true;
    bool _bowReady = false;
    bool _isBowCharging = false;

    float _spawnTest = 0f;
    float _vehicleTimer = 0f;
    float _charChangeTimer = 0f;
    float _bowTimer = 0f;
    float _swingTimer = 0f;
    float _castTimer = 0f;
    float _healthRegenTimer = 0f;
    float _manaRegenTimer = 0f;
    float _staminaRegenTimer = 0f;
    int _treeType = 1;
    
    Vector3 _lastdir = Vector3.zero;
    Vector3 _lastpos = Vector3.zero;
    
    MapManager _mapManager;
    NavMeshAgent _navMeshAgent;
    Canvas _canvas;
    MessageFader _dialogueFader;
    MessageFader _messageFader;
    SpriteRenderer _healthStatusSpriteRenderer;
    SpriteRenderer _manaStatusSpriteRenderer;
    SpriteRenderer _staminaStatusSpriteRenderer;

    GameObject _healthBar;
    GameObject _manaBar;
    GameObject _staminaBar;
    TextMeshProUGUI _healthText;
    TextMeshProUGUI _manaText;

    #endregion

    #region SyncVars

    readonly SyncVar<int> _health = new SyncVar<int>(100);
    readonly SyncVar<int> _mana = new SyncVar<int>(100);
    readonly SyncVar<int> _stamina = new SyncVar<int>(100);
    readonly SyncVar<VehicleUpdate> _vehicleUpdate = new SyncVar<VehicleUpdate>(new VehicleUpdate());
    readonly SyncVar<int> _spriteLibraryIndex = new SyncVar<int>(0);

    #endregion

    #region Changed Events

    void OnHealthChanged(int oldValue, int newValue, bool asServer)
    {
        if (!asServer)
        {
            if (newValue <= 0)
                AnimSetBool(_bodyAnim, "Dead", true);
            else if (AnimGetBool(_bodyAnim, "Dead"))
                AnimSetBool(_bodyAnim, "Dead", false);
        }
    }

    void OnManaChanged(int oldValue, int newValue, bool asServer)
    {
        if (!asServer)
        {
            // Handle any animations or other mutual effects here.
        }
    }

    void OnStaminaChanged(int oldValue, int newValue, bool asServer)
    {
        if (!asServer)
        {
            // Handle any animations or other mutual effects here.
        }
    }

    bool GetVehicleBoundsAndRenderer(Transform vt, out Bounds bounds, out SpriteRenderer sr, out TilemapRenderer tr)
    {
        sr = null;
        tr = null;
        bounds = new Bounds();

        sr = vt.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            bounds = sr.bounds;
            sr.sortingOrder = -1;
        }
        else
        {
            // Could be the horse.
            sr = vt.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                bounds = sr.bounds;
            }
            else
            {
                tr = vt.GetComponentInChildren<TilemapRenderer>();
                if (tr == null) return false;

                bounds = tr.bounds;
                tr.sortingOrder = -1;
            }
        }
        return true;
    }

    void OnVehicleChanged(VehicleUpdate oldValue, VehicleUpdate newValue, bool asServer)
    {
        var vt = newValue.vehicleTransform;

        if (newValue.isMounted)
        {
            if (!GetVehicleBoundsAndRenderer(vt, out Bounds bounds,
                out SpriteRenderer srend, out TilemapRenderer trend)) return;

            newValue.guestTransforms[0].position = vt.position +
                new Vector3(newValue.mountOffsetX,
                            bounds.extents.y * newValue.mountOffsetY, 0);

            foreach (var tr in newValue.guestTransforms)
            {
                tr.GetComponent<Collider2D>().enabled = false;
                tr.GetComponent<NetworkTransform>().enabled = false;
                tr.GetComponent<Rigidbody2D>().simulated = false;
                tr.parent = vt.transform;
            }

            vt.gameObject.SendMessage("Mount");
            UpdateNav(false);
        }
        else
        {
            if (!GetVehicleBoundsAndRenderer(vt, out Bounds bounds,
                out SpriteRenderer srend, out TilemapRenderer trend)) return;

            foreach (var tr in newValue.guestTransforms)
            {
                tr.parent = null;
                tr.GetComponent<Collider2D>().enabled = true;
                tr.GetComponent<NetworkTransform>().enabled = true;
                tr.GetComponent<Rigidbody2D>().simulated = true;
            }

            newValue.guestTransforms[0].position = vt.position +
                new Vector3(newValue.mountOffsetX,
                            bounds.extents.y * newValue.mountOffsetY, 0);

            if (srend != null) srend.sortingOrder = 0;
            else if (trend != null) trend.sortingOrder = 0;

            vt.gameObject.SendMessage("Unmount");
            UpdateNav(true);

            if (asServer) vt.GetComponent<NetworkObject>().RemoveOwnership();
        }
    }

    private void OnSpriteLibraryIndexChanged(int prev, int next, bool asServer)
    {
        GetComponent<SpriteLibrary>().spriteLibraryAsset = _spriteLibraryAssets[next];
    }

    #endregion

    #region RPC

    [ServerRpc(RequireOwnership = false)]
    public void SetNav(bool state)
    {
        SetMyNav(state);
        ObserversSetMyNav(state);
    }

    [ServerRpc]
    void SetDest(Vector3 pos)
    {
        SetMyDest(pos);
        ObserversSetMyDest(pos);
    }

    public void UpdateNav(bool state)
    {
        if (IsClientInitialized) SetNav(state);
    }

    [ObserversRpc] void ObserversSetMyNav(bool state) => SetMyNav(state);

    [ObserversRpc] void ObserversSetMyDest(Vector3 pos) => SetMyDest(pos);

    [ServerRpc] void SetHealth(int value) => _health.Value = value;

    [ServerRpc] void SetMana(int value) => _mana.Value = value;

    [ServerRpc] void SetStamina(int value) => _stamina.Value = value;

    [ServerRpc] void SetVehicle(VehicleUpdate value) => _vehicleUpdate.Value = value;

    [ServerRpc] void SetSpriteLibraryIndex(int value) => _spriteLibraryIndex.Value = value;

    [ServerRpc]
    void VehicleTakeOwnership(NetworkObject nob, NetworkConnection connection)
    {
        nob.GiveOwnership(connection);
    }

    [ServerRpc]
    void ServerCastSpell(Vector3 position, float delayTime)
    {
        // Spawn the spell effect.
        var go = Instantiate(_spellPrefab, position, Quaternion.identity);
        InstanceFinder.ServerManager.Spawn(go);
    }

    [ServerRpc]
    void ServerSummonTree(int index, Vector3 position)
    {
        ushort id = _spawnTreesId.GetStableHashU16();
        var spawnables = (SinglePrefabObjects)InstanceFinder.NetworkManager.GetPrefabObjects<SinglePrefabObjects>(id, true);
        var prefab = spawnables.Prefabs[index];
        var nob = Instantiate(prefab, position, Quaternion.identity);
        InstanceFinder.ServerManager.Spawn(nob);
    }

    #endregion

    #region Setup

    void OnEnable()
    {
        _mapManager = FindFirstObjectByType<MapManager>();

        // If you don't set the following, the 2d navmeshagent will fall over.
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.updateUpAxis = false;
        _navMeshAgent.autoRepath = true;

        _vehicleUpdate.OnChange += OnVehicleChanged;
        _spriteLibraryIndex.OnChange += OnSpriteLibraryIndexChanged;
        _health.OnChange += OnHealthChanged;
        _mana.OnChange += OnManaChanged;
        _stamina.OnChange += OnStaminaChanged;
    }

    void OnDisable()
    {
        _vehicleUpdate.OnChange -= OnVehicleChanged;
        _spriteLibraryIndex.OnChange -= OnSpriteLibraryIndexChanged;
        _health.OnChange -= OnHealthChanged;
        _mana.OnChange -= OnManaChanged;
        _stamina.OnChange -= OnStaminaChanged;
    }

    #endregion

    #region Updates

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        if (!base.HasAuthority) return;
        if (AnimGetBool(_bodyAnim, "Dead")) return;

        if (_inputField != null && _inputField.isFocused) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            var button1 = GameObject.Find("Button_SelectCherryTree");
            button1?.GetComponent<Button>().onClick.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            var button2 = GameObject.Find("Button_SelectAppleTree");
            button2?.GetComponent<Button>().onClick.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.fixedTime - _castTimer > 2.5f)
            {
                _castTimer = Time.fixedTime;
                if (_mana.Value > 25)
                {
                    SpawnTree(_treeType);
                    SetMana(_mana.Value - 25);
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (!UseVehicle())
            {
                // If we didn't enter a vehicle, then we activated our pickaxe instead.
                if (Time.fixedTime - _swingTimer > 1.5f)
                {
                    _swingTimer = Time.fixedTime;
                    if (_stamina.Value > 15)
                    {
                        AnimSetTrigger(_netBodyAnim, "Slash");
                        SetStamina(_stamina.Value - 15);
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            if (Time.fixedTime - _charChangeTimer > 0.5f)
            {
                _charChangeTimer = Time.fixedTime;
                var index = (_spriteLibraryIndex.Value + 1) % _spriteLibraryAssets.Length;
                SetSpriteLibraryIndex(index);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            if (Time.fixedTime - _bowTimer > 0.25f)
            {
                _bowTimer = Time.fixedTime;
                _bowReady = !_bowReady;
                AnimSetBool(_bodyAnim, "Bow", _bowReady);
                GetComponent<Bow>().BowReady = _bowReady;
            }
        }

        if (!_isBowCharging && Input.GetMouseButtonDown(0))
        {
            if (_bowReady && _stamina.Value > 15)
            {
                AnimSetTrigger(_netBodyAnim, "Shoot");
                _isBowCharging = true;
                SetStamina(_stamina.Value - 15);
            }
        }
        else if (_isBowCharging && Input.GetMouseButtonUp(0))
        {
            _isBowCharging = false;
            if (_bowReady)
            {
                AnimSetTrigger(_netBodyAnim, "Release");
                Invoke("ReleaseArrow", 0.8f);
            }
        }
    }

    #endregion

    #region Public Methods

    public void ReleaseHealth()
    {
        _healthStatusSpriteRenderer.sprite = null;
    }

    public void ReleaseMana()
    {
        _manaStatusSpriteRenderer.sprite = null;
    }

    public void ReleaseStamina()
    {
        _staminaStatusSpriteRenderer.sprite = null;
    }

    public void ModifyHealth(int amount, bool updateIndicator=true)
    {
        var oldValue = _health.Value;
        var newValue = _health.Value + amount;
        if (newValue < 0) newValue = 0;
        else if (newValue > 100) newValue = 100;

        if (oldValue != newValue)
        {
            SetHealth(newValue);

            _healthText.text = newValue.ToString();
            _healthBar.GetComponent<Image>().fillAmount = newValue / 100f;

            if (newValue == 0 || newValue == 100) ReleaseHealth();
            else if (updateIndicator)
            {
                if (newValue < oldValue)
                    _healthStatusSpriteRenderer.sprite = _damageStatusSprite;
                else
                    _healthStatusSpriteRenderer.sprite = _healthStatusSprite;
            }
        }
    }

    public void ModifyMana(int amount, bool updateIndicator=true)
    {
        var oldValue = _mana.Value;
        var newValue = _mana.Value + amount;
        if (newValue < 0) newValue = 0;
        else if (newValue > 100) newValue = 100;

        if (oldValue != newValue)
        {
            SetMana(newValue);

            _manaText.text = newValue.ToString();
            _manaBar.GetComponent<Image>().fillAmount = newValue / 100f;

            if (newValue == 0 || newValue == 100) ReleaseMana();
            else if (updateIndicator)
            {
                //if (newValue < oldValue)
                //    _manaStatusSpriteRenderer.sprite = _manaLossStatusSprite;
                //else
                //    _manaStatusSpriteRenderer.sprite = _manaGainStatusSprite;
            }
        }
    }

    public void ModifyStamina(int amount, bool updateIndicator=true)
    {
        var oldValue = _stamina.Value;
        var newValue = _stamina.Value + amount;
        if (newValue < 0) newValue = 0;
        else if (newValue > 100) newValue = 100;

        if (oldValue != newValue)
        {
            SetStamina(newValue);

            _staminaBar.GetComponent<Image>().fillAmount = newValue / 100f;

            if (newValue == 0 || newValue == 100) ReleaseStamina();
            else if (updateIndicator)
            {
                //if (newValue < oldValue)
                //    _staminaStatusSpriteRenderer.sprite = _staminaLossStatusSprite;
                //else
                //    _staminaStatusSpriteRenderer.sprite = _staminaGainStatusSprite;
            }
        }
    }

    public void Slash()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Rock"))
            {
                var mrock = hit.GetComponent<MineableRock>();
                var rock = mrock as IMineable;
                if (rock != null)
                {
                    int rockHealth = rock.MineResource(10);
                    if (rockHealth <= 0)
                    {
                        _audioSource.PlayOneShot(_destroyRockSound);
                        //TODO: give player ore
                    }
                    else
                    {
                        _audioSource.PlayOneShot(_pickaxeSounds[Random.Range(0, _pickaxeSounds.Length)]);
                    }
                }
                break;
            }
        }
    }

    public void SetTree(int type)
    {
        _treeType = type;
    }

    #endregion

    #region Private Methods

    void ReleaseArrow()
    {
        GetComponent<Bow>().ShootArrow();
    }

    bool UseVehicle()
    {
        if (Time.fixedTime - _vehicleTimer > 1.5f)
        {
            if (_vehicleUpdate.Value.isMounted)
            {
                foreach (var tr in _vehicleUpdate.Value.guestTransforms)
                {
                    if (tr == transform)
                    {
                        _vehicleTimer = Time.fixedTime;

                        if (AnimGetBool(_bodyAnim, "Sail"))
                            AnimSetBool(_bodyAnim, "Sail", false);

                        _navMeshAgent.enabled = true;
                        _vehicleUpdate.Value.isMounted = false;
                        SetVehicle(_vehicleUpdate.Value);
                        UpdateNav(true);
                        return true;
                    }
                }
            }
            else if (transform.parent == null)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f);
                foreach (Collider2D hit in hits)
                {
                    if (hit.CompareTag("Boat"))
                    {
                        _vehicleTimer = Time.fixedTime;
                        VehicleTakeOwnership(hit.gameObject.GetComponent<NetworkObject>(), base.Owner);
                        AnimSetInteger(_bodyAnim, AnimationMovementName, -1);
                        AnimSetBool(_bodyAnim, "Sail", true);
                        _navMeshAgent.enabled = false;
                        _vehicleUpdate.Value.isMounted = true;
                        _vehicleUpdate.Value.mountOffsetX = 0f;
                        _vehicleUpdate.Value.mountOffsetY = 0.5f;
                        _vehicleUpdate.Value.vehicleTransform = hit.transform;
                        _vehicleUpdate.Value.guestTransforms.Clear(); // TODO: handle multiple passengers
                        _vehicleUpdate.Value.guestTransforms.Add(transform);
                        SetVehicle(_vehicleUpdate.Value);
                        return true;
                    }
                    else if (hit.CompareTag("Vehicle"))
                    {
                        _vehicleTimer = Time.fixedTime;
                        VehicleTakeOwnership(hit.gameObject.GetComponent<NetworkObject>(), base.Owner);
                        AnimSetInteger(_bodyAnim, AnimationMovementName, -1);
                        _navMeshAgent.enabled = false;
                        _vehicleUpdate.Value.isMounted = true;
                        _vehicleUpdate.Value.mountOffsetX = 0f;
                        _vehicleUpdate.Value.mountOffsetY = 0.4f;
                        _vehicleUpdate.Value.vehicleTransform = hit.transform;
                        _vehicleUpdate.Value.guestTransforms.Clear();
                        _vehicleUpdate.Value.guestTransforms.Add(transform);
                        SetVehicle(_vehicleUpdate.Value);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void SpawnTree(int index)
    {
        if (Time.fixedTime - _spawnTest > 1f)
        {
            _netBodyAnim.SetTrigger("Cast");
            _canMove = _isWalking = false;
            AnimSetInteger(_bodyAnim, AnimationMovementName, -1);
            _spawnTest = Time.fixedTime;

            var nm = InstanceFinder.NetworkManager;
            if (nm != null) StartCoroutine(SummonTreeSpell(nm, transform.position + _lastdir * 3f, index, 0.8f));
        }
    }

    IEnumerator SummonTreeSpell(NetworkManager nm, Vector3 position, int index, float delayTime)
    {
        // Wait for the player casting animation.
        yield return new WaitForSeconds(delayTime);

        ServerCastSpell(position, delayTime);

        ushort id = _spawnTreesId.GetStableHashU16();
        var spawnables = (SinglePrefabObjects)InstanceFinder.NetworkManager.GetPrefabObjects<SinglePrefabObjects>(id, true);
        if (spawnables != null && index < spawnables.Prefabs.Count)
        {
            var prefab = spawnables.Prefabs[index];
            ServerSummonTree(index, position);
        }

        _lastpos = transform.position; // BUGFIX: This prevents walking after casting if idling after a walk cast
        _canMove = true;
    }

    #endregion

    #region Agent

    void SetMyNav(bool state)
    {
        if (_vehicleUpdate.Value.isMounted) return;
        _navMeshAgent.enabled = state;
    }

    void SetMyDest(Vector3 pos)
    {
        if (_navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
            _navMeshAgent.SetDestination(pos);
    }

    #endregion

    #region Tick

    protected override void TimeManager_OnTick()
    {
        if (!base.HasAuthority) return;
        if (AnimGetBool(_bodyAnim, "Dead")) return;

        if (_firstSpawn)
        {
            _firstSpawn = false;
            _lastpos = transform.position;
        }

        if (Time.fixedTime - _healthRegenTimer > 1f)
        {
            _healthRegenTimer = Time.fixedTime;
            if (_health.Value < 100) ModifyHealth(1, false);
        }
        if (Time.fixedTime - _manaRegenTimer > 1f)
        {
            _manaRegenTimer = Time.fixedTime;
            if (_mana.Value < 100) ModifyMana(1, false);
        }
        if (Time.fixedTime - _staminaRegenTimer > 1f)
        {
            _staminaRegenTimer = Time.fixedTime;
            if (_stamina.Value < 100) ModifyStamina(1, false);
        }

        if (!_canMove) return;

        if (_inputField != null && _inputField.isFocused) return;

        float axisx = Input.GetAxis("Horizontal");
        float axisy = Input.GetAxis("Vertical");

        const float deadzone = 0.25f;
        float dx = 0f; float dy = 0f;
        bool keepDirState = false;
        if      (Mathf.Abs(axisx) > deadzone) dx = Mathf.Sign(axisx);
        else if (Mathf.Abs(axisy) > deadzone) dy = Mathf.Sign(axisy);
        else keepDirState = true;

        if (!_vehicleUpdate.Value.isMounted)
        {
            if (0f != axisx || 0f != axisy)
                SetDest(transform.position + new Vector3(axisx, axisy));
        }
        else
        {
            if (!keepDirState)
            {
                AnimSetFloat(_bodyAnim, "DX", dx);
                AnimSetFloat(_bodyAnim, "DY", dy);
            }
            return;
        }

        if (_lastpos != transform.position)
        {
            _lastpos = transform.position;

            if (!keepDirState)
            {
                AnimSetFloat(_bodyAnim, "DX", dx);
                AnimSetFloat(_bodyAnim, "DY", dy);
                _lastdir = new Vector3(dx, dy);
            }

            if (!_isWalking && !_vehicleUpdate.Value.isMounted)
            {
                _isWalking = true;
                AnimSetInteger(_bodyAnim, AnimationMovementName, 0);
            }
        }
        else
        {
            if (_isWalking && !_vehicleUpdate.Value.isMounted)
            {
                _isWalking = false;
                AnimSetInteger(_bodyAnim, AnimationMovementName, -1);
            }
        }
    }

    #endregion

    #region Network Events

    public override void OnStartClient()
    {
        var found = GameObject.Find("ChatField");
        var cfield = GameObject.Find("ChatField");
        if (cfield != null) _inputField = cfield.GetComponent<TMP_InputField>();

        _healthStatusSpriteRenderer = _healthStatus.GetComponent<SpriteRenderer>();
        _manaStatusSpriteRenderer = _manaStatus.GetComponent<SpriteRenderer>();
        _staminaStatusSpriteRenderer = _staminaStatus.GetComponent<SpriteRenderer>();

        _healthBar = GameObject.Find("HealthBarFill");
        _manaBar = GameObject.Find("ManaBarFill");
        _staminaBar = GameObject.Find("StaminaBarFill");
        _healthText = GameObject.Find("HealthText").GetComponent<TextMeshProUGUI>();
        _manaText = GameObject.Find("ManaText").GetComponent<TextMeshProUGUI>();

        if (base.Owner.IsLocalClient)
        {
            var found2 = GameObject.Find("ChatField");
            _castTimer = Time.fixedTime;
            _swingTimer = Time.fixedTime;
            _bowTimer = Time.fixedTime;

            _canvas = FindFirstObjectByType<Canvas>();
            var messageFaders = _canvas.GetComponentsInChildren<MessageFader>();
            foreach (var mf in messageFaders)
            {
                if (mf.Name == "Dialogue")
                {
                    _dialogueFader = mf;
                    _dialogueFader.Title.text = "...";
                    _dialogueFader.Content.text = "Welcome to the showcase.";
                    _dialogueFader.FadeIn = true;
                    _dialogueFader.DelayedFadeout(5f);
                }
                else if (mf.Name == "Message")
                    _messageFader = mf;
            }

            _audioSource = GetComponent<AudioSource>();

            _camera = FindFirstObjectByType<CinemachineCamera>();
            if (_camera != null)
            {
                _camera.Follow = transform;
                _camera.LookAt = transform;
            }

            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                canvas.gameObject.GetComponent<SpawnManager>().playerPrefab = gameObject;
            }

            StartCoroutine(_mapManager.UpdateNavMeshAsync());
        }
    }

    public override void OnStopClient()
    {
        if (base.IsOwner)
        {
            if (_camera != null)
            {
                _camera.Follow = null;
                _camera.LookAt = null;
            }
        }
        base.OnStopClient();
    }

    #endregion

    #region Animations

    bool AnimGetBool(Animator anim, string name)
    {
        if (anim == null) return false;
        if (anim.gameObject.activeSelf)
        {
            return anim.GetBool(name);
        }
        return false;
    }

    float AnimGetFloat(Animator anim, string name)
    {
        if (anim == null) return 0f;
        if (anim.gameObject.activeSelf)
        {
            return anim.GetFloat(name);
        }
        return 0f;
    }

    int AnimGetInteger(Animator anim, string name)
    {
        if (anim == null) return 0;
        if (anim.gameObject.activeSelf)
        {
            return anim.GetInteger(name);
        }
        return 0;
    }

    void AnimSetTrigger(NetworkAnimator anim, string name)
    {
        if (anim == null) return;
        if (anim.gameObject.activeSelf)
        {
            anim.SetTrigger(name);
        }
    }

    void AnimSetBool(Animator anim, string name, bool value)
    {
        if (anim == null) return;
        if (anim.gameObject.activeSelf)
        {
            anim.SetBool(name, value);
        }
    }

    void AnimSetFloat(Animator anim, string name, float value)
    {
        if (anim == null) return;
        if (anim.gameObject.activeSelf)
        {
            anim.SetFloat(name, value);
        }
    }

    void AnimSetInteger(Animator anim, string name, int value)
    {
        if (anim == null) return;
        if (anim.gameObject.activeSelf)
        {
            anim.SetInteger(name, value);
        }
    }

    #endregion

    #region Event Handlers

    public void EnterRegionArea(string regionName, string regionDescription, string regionType)
    {
        if (_messageFader != null)
        {
            _messageFader.Title.text = regionName;
            _messageFader.Content.text = regionDescription;
            _messageFader.FadeIn = true;
            _messageFader.DelayedFadeout(5f);
        }
    }

    public void ExitRegionArea(string regionName, string regionDescription, string regionType)
    {
    }

    #endregion
}

