using UnityEngine;
using Unity.Cinemachine;
using FishNet;
using FishNet.Component.Animating;
using FishNet.Utility.Template;
using FishNet.Managing.Object;
using GameKit.Dependencies.Utilities;
using System.Collections;
using FishNet.Managing;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using FishNet.Object;
using System;

public class PlayerMovement : TickNetworkBehaviour
{
    #region Properties

    [Header("General")]
    [SerializeField] float _speed = 6f;
    [SerializeField] CinemachineCamera _camera;
    [SerializeField] Animator _bodyAnim;
    [SerializeField] NetworkAnimator _netBodyAnim;
    [SerializeField] string _spawnTreesId;
    [SerializeField] GameObject _spellPrefab;
    [SerializeField] AudioClip[] _audioGrassFootsteps;

    #endregion

    #region Member Fields

    bool _firstSpawn = true;
    bool _isWalking = false;
    bool _canMove = true;
    float _spawnTest = 0f;
    int _treeType = 1;
    Vector3 _lastdir = Vector3.zero;
    Rigidbody2D _rigidbody;
    Vector3 _lastpos = Vector3.zero;
    AudioSource _audioSource;

    #endregion

    #region Setup

    void OnEnable()
    {
    }

    void OnDisable()
    {
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
    }
    public void SetTree(int type)
    {
        _treeType = type;
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

        var obj = Instantiate(prefab, position, Quaternion.identity);
        InstanceFinder.ServerManager.Spawn(obj);
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

    public void OnFootstep()
    {
        if (_audioSource != null && _audioGrassFootsteps != null && _audioGrassFootsteps.Length > 0)
        {
            _audioSource.PlayOneShot(_audioGrassFootsteps[Random.Range(0, _audioGrassFootsteps.Length)]);
            // TODO: replicate the sound on all the clients
        }
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

        _rigidbody.linearVelocity = new Vector2(axisx * _speed * 25 * Time.fixedDeltaTime, axisy * _speed * 25 * Time.fixedDeltaTime);

        if (_lastpos != transform.position)
        {
            _lastpos = transform.position;

            if (!keepDirState)
            {
                AnimSetFloat(_bodyAnim, "DX", dx);
                AnimSetFloat(_bodyAnim, "DY", dy);
                _lastdir = new Vector3(dx, dy);
            }

            if (!_isWalking) AnimSetBool(_bodyAnim, "Walk", _isWalking=true);
        }
        else
        {
            if (_isWalking) AnimSetBool(_bodyAnim, "Walk", _isWalking=false);
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
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.Owner.IsLocalClient)
        {
            _audioSource = GetComponent<AudioSource>();
            _rigidbody = GetComponent<Rigidbody2D>();

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

