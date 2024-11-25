using UnityEngine;
using Unity.Cinemachine;
using FishNet;
using FishNet.Component.Animating;
using FishNet.Utility.Template;
using FishNet.Managing.Object;
using GameKit.Dependencies.Utilities;
using System.Collections;
using FishNet.Managing;
using UnityEngine.UI;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Component.Transforming;
using UnityEngine.U2D.Animation;
using UnityEngine.AI;

public class PlayerMovement : TickNetworkBehaviour
{
    const float MountOffsetY = 0.5f;

    #region Network Update Data

    [System.Serializable]
    public class VehicleUpdate
    {
        public bool isMounted = false;
        public Transform vehicleTransform = null;
        public List<Transform> guestTransforms = new();
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

    [Header("Mining")]
    [SerializeField] AudioClip _destroyRockSound;
    [SerializeField] AudioClip[] _pickaxeSounds;

    #endregion

    #region Member Fields

    bool _firstSpawn = true;
    bool _isWalking = false;
    bool _canMove = true;
    float _spawnTest = 0f;
    float _sailTimer = 0f;
    float _charChangeTimer = 0f;
    int _treeType = 1;
    Vector3 _lastdir = Vector3.zero;
    Vector3 _lastpos = Vector3.zero;
    MapManager _mapManager;
    NavMeshAgent _navMeshAgent;

    #endregion

    #region SyncVars

    readonly SyncVar<VehicleUpdate> _vehicleUpdate = new SyncVar<VehicleUpdate>(new VehicleUpdate());
    [ServerRpc] private void SetVehicle(VehicleUpdate value) => _vehicleUpdate.Value = value;

    readonly SyncVar<int> _spriteLibraryIndex = new SyncVar<int>(0);
    [ServerRpc] private void SetSpriteLibraryIndex(int value) => _spriteLibraryIndex.Value = value;

    #endregion

    #region RPC

    void OnVehicleChanged(VehicleUpdate oldValue, VehicleUpdate newValue, bool asServer)
    {
        var vt = newValue.vehicleTransform;

        if (newValue.isMounted)
        {
            var sr = vt.GetComponent<SpriteRenderer>();
            sr.sortingOrder = -1;

            newValue.guestTransforms[0].position = vt.position + new Vector3(0, sr.bounds.extents.y * MountOffsetY, 0);

            foreach (var tr in newValue.guestTransforms)
            {
                tr.GetComponent<Collider2D>().enabled = false;
                tr.GetComponent<NetworkTransform>().enabled = false;
                tr.GetComponent<Rigidbody2D>().simulated = false;
                tr.parent = vt.transform;
            }

            vt.gameObject.SendMessage("Mount");
            _navMeshAgent.enabled = false;
        }
        else
        {
            var sr = vt.GetComponent<SpriteRenderer>();

            foreach (var tr in newValue.guestTransforms)
            {
                tr.parent = null;
                tr.GetComponent<Collider2D>().enabled = true;
                tr.GetComponent<NetworkTransform>().enabled = true;
                tr.GetComponent<Rigidbody2D>().simulated = true;
            }

            newValue.guestTransforms[0].position = vt.position + new Vector3(0, sr.bounds.extents.y * MountOffsetY, 0);

            sr.sortingOrder = 0;
            vt.gameObject.SendMessage("Unmount");
            _navMeshAgent.enabled = true;

            if (asServer) vt.GetComponent<NetworkObject>().RemoveOwnership();
        }
    }

    private void OnSpriteLibraryIndexChanged(int prev, int next, bool asServer)
    {
        GetComponent<SpriteLibrary>().spriteLibraryAsset = _spriteLibraryAssets[next];
    }


    #endregion

    #region Setup

    void OnEnable()
    {
        _mapManager = FindFirstObjectByType<MapManager>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _vehicleUpdate.OnChange += OnVehicleChanged;
        _spriteLibraryIndex.OnChange += OnSpriteLibraryIndexChanged;

        // If you don't set the following, the 2d navmeshagent will fall over.
        _navMeshAgent.enabled = true;
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.updateUpAxis = false;
        _navMeshAgent.autoRepath = true;
    }

    void OnDisable()
    {
        _vehicleUpdate.OnChange -= OnVehicleChanged;
        _spriteLibraryIndex.OnChange -= OnSpriteLibraryIndexChanged;
    }

    #endregion

    #region Updates

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        if (!base.HasAuthority) return;

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
            SpawnTree(_treeType);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (!UseVehicle())
            {
                // If we didn't enter a vehicle, then we activated our pickaxe instead.
                AnimSetTrigger(_netBodyAnim, "Slash");
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

    [ServerRpc] void VehicleTakeOwnership(NetworkObject nob, NetworkConnection connection)
    {
        nob.GiveOwnership(connection);
    }

    bool UseVehicle()
    {
        if (Time.fixedTime - _sailTimer > 1.5f)
        {
            if (_vehicleUpdate.Value.isMounted)
            {
                foreach (var tr in _vehicleUpdate.Value.guestTransforms)
                {
                    if (tr == transform)
                    {
                        _sailTimer = Time.fixedTime;

                        AnimSetBool(_bodyAnim, "Sail", false);
                        _vehicleUpdate.Value.isMounted = false;
                        SetVehicle(_vehicleUpdate.Value);
                        _navMeshAgent.enabled = true;
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
                        _sailTimer = Time.fixedTime;

                        VehicleTakeOwnership(hit.gameObject.GetComponent<NetworkObject>(), base.Owner);
                        AnimSetBool(_bodyAnim, "Walk", false);
                        AnimSetBool(_bodyAnim, "Sail", true);
                        _vehicleUpdate.Value.isMounted = true;
                        _vehicleUpdate.Value.vehicleTransform = hit.transform;
                        _vehicleUpdate.Value.guestTransforms.Clear(); // TODO: handle multiple passengers
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
            AnimSetBool(_bodyAnim, "Walk", _canMove = _isWalking = false);
            _spawnTest = Time.fixedTime;

            var nm = InstanceFinder.NetworkManager;
            if (nm != null) StartCoroutine(SummonTreeSpell(nm, transform.position + _lastdir * 3f, index, 0.8f));
        }
    }

    [ServerRpc] void ServerCastSpell(Vector3 position, float delayTime)
    {
        // Spawn the spell effect.
        var go = Instantiate(_spellPrefab, position, Quaternion.identity);
        InstanceFinder.ServerManager.Spawn(go);
    }

    [ServerRpc] void ServerSummonTree(int index, Vector3 position)
    {
        ushort id = _spawnTreesId.GetStableHashU16();
        var spawnables = (SinglePrefabObjects)InstanceFinder.NetworkManager.GetPrefabObjects<SinglePrefabObjects>(id, true);
        var prefab = spawnables.Prefabs[index];
        var nob = Instantiate(prefab, position, Quaternion.identity);
        InstanceFinder.ServerManager.Spawn(nob);
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

    public void SetNav(bool state)
    {
        if (_vehicleUpdate.Value.isMounted) return;
        _navMeshAgent.enabled = state;
    }

    void SetDest(Vector3 pos)
    {
        if (_navMeshAgent.enabled)
            _navMeshAgent.SetDestination(pos);
    }

    #endregion

    #region Tick

    protected override void TimeManager_OnTick()
    {
        if (!base.HasAuthority) return;

        if (_firstSpawn)
        {
            _firstSpawn = false;
            _lastpos = transform.position;
        }

        if (!_canMove) return;

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

            if (!_isWalking && !_vehicleUpdate.Value.isMounted) AnimSetBool(_bodyAnim, "Walk", _isWalking=true);
        }
        else
        {
            if (_isWalking && !_vehicleUpdate.Value.isMounted) AnimSetBool(_bodyAnim, "Walk", _isWalking=false);
        }
    }

    #endregion

    #region Network Events

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.Owner.IsLocalClient)
        {
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

    #endregion
}

