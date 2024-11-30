using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System;
using UnityEngine;

namespace FishNet.Component.Spawning
{

    /// <summary>
    /// Spawns a player object for clients when they connect.
    /// Must be placed on or beneath the NetworkManager object.
    /// </summary>
    [AddComponentMenu("FishNet/Component/VehicleSpawner")]
    public class VehicleSpawner : MonoBehaviour
    {
        #region Public.
        /// <summary>
        /// Called on the server when a player is spawned.
        /// </summary>
        public event Action<NetworkObject> OnSpawned;
        #endregion

        #region Serialized.
        /// <summary>
        /// Prefab to spawn for the vehicle.
        /// </summary>
        [Tooltip("Prefab to spawn for the vehicle.")]
        [SerializeField]
        private NetworkObject _vehiclePrefab;
        /// <summary>
        /// Sets the VehiclePrefab to use.
        /// </summary>
        /// <param name="nob"></param>
        public void SetVehiclePrefab(NetworkObject nob) => _vehiclePrefab = nob;
        /// <summary>
        /// True to add player to the active scene when no global scenes are specified through the SceneManager.
        /// </summary>
        [Tooltip("True to add player to the active scene when no global scenes are specified through the SceneManager.")]
        [SerializeField]
        private bool _addToDefaultScene = true;
        /// <summary>
        /// Areas in which players may spawn.
        /// </summary>
        [Tooltip("Areas in which players may spawn.")]
        public Transform[] Spawns = new Transform[0];
        #endregion

        #region Private.
        /// <summary>
        /// NetworkManager on this object or within this objects parents.
        /// </summary>
        private NetworkManager _networkManager;
        /// <summary>
        /// Next spawns to use.
        /// </summary>
        private int _nextSpawn;
        #endregion

        private void Start()
        {
            InitializeOnce();
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
                _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
        }


        /// <summary>
        /// Initializes this script for use.
        /// </summary>
        private void InitializeOnce()
        {
            _networkManager = InstanceFinder.NetworkManager;
            if (_networkManager == null)
            {
                NetworkManagerExtensions.LogWarning($"VehicleSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
                return;
            }

            _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
        }

        bool _loaded;

        /// <summary>
        /// Called when a client loads initial scenes after connecting.
        /// </summary>
        private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
        {
            if (!asServer)
                return;
            if (_vehiclePrefab == null)
            {
                NetworkManagerExtensions.LogWarning($"Vehicle prefab is empty and cannot be spawned for connection {conn.ClientId}.");
                return;
            }

            if (_loaded) return;
            _loaded = true;

            Vector3 position;
            Quaternion rotation;
            SetSpawn(_vehiclePrefab.transform, out position, out rotation);

            NetworkObject nob = _networkManager.GetPooledInstantiated(_vehiclePrefab, position, rotation, true);
            _networkManager.ServerManager.Spawn(nob, null);

            //If there are no global scenes 
            if (_addToDefaultScene)
                _networkManager.SceneManager.AddOwnerToDefaultScene(nob);

            OnSpawned?.Invoke(nob);
        }


        /// <summary>
        /// Sets a spawn position and rotation.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        private void SetSpawn(Transform prefab, out Vector3 pos, out Quaternion rot)
        {
            //No spawns specified.
            if (Spawns.Length == 0)
            {
                SetSpawnUsingPrefab(prefab, out pos, out rot);
                return;
            }

            Transform result = Spawns[_nextSpawn];
            if (result == null)
            {
                SetSpawnUsingPrefab(prefab, out pos, out rot);
            }
            else
            {
                pos = result.position;
                rot = result.rotation;
            }

            //Increase next spawn and reset if needed.
            _nextSpawn++;
            if (_nextSpawn >= Spawns.Length)
                _nextSpawn = 0;
        }

        /// <summary>
        /// Sets spawn using values from prefab.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        private void SetSpawnUsingPrefab(Transform prefab, out Vector3 pos, out Quaternion rot)
        {
            pos = prefab.position;
            rot = prefab.rotation;
        }

    }


}