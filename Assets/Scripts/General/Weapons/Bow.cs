using UnityEngine;
using FishNet.Object;
using FishNet.Utility.Template;
using FishNet;

namespace Shadowfinder.Weapons
{
    class Bow : TickNetworkBehaviour
    {
        const float MAX_PASSED_TIME = 0.3f;

        [SerializeField] GameObject _projectile;
        [SerializeField] Transform _source;

        public bool BowReady = false;

        public void ShootArrow()
        {
            if (!IsOwner) return;
            if (!BowReady) return;

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var position = _source.position;
            var direction = (mousePosition - position).normalized;
            direction.z = 0;

            var rotation = Quaternion.LookRotation(Vector3.forward, direction);
            ClientFire(_projectile, position, direction, rotation);
        }

        protected override void TimeManager_OnTick()
        {
        }

        void ClientFire(GameObject prefab, Vector3 position, Vector3 direction, Quaternion rotation)
        {
            ServerFire(prefab, position, direction, rotation, base.TimeManager.Tick);
        }

        void SpawnProjectile(GameObject prefab, Vector3 position, Vector3 direction, Quaternion rotation, float passedTime)
        {
            var pp = Instantiate(prefab, position, rotation);
            pp.GetComponent<Arrow>().Initialize(direction, passedTime);
            InstanceFinder.ServerManager.Spawn(pp);
        }

        [ServerRpc(RunLocally = false)]
        void ServerFire(GameObject prefab, Vector3 position, Vector3 direction, Quaternion rotation, uint tick)
        {
            float passedTime = (float)base.TimeManager.TimePassed(tick, false);
            passedTime = Mathf.Min(MAX_PASSED_TIME / 2f, passedTime);
            SpawnProjectile(prefab, position, direction, rotation, passedTime);
            ObserversFire(prefab, position, direction, rotation, tick);
        }

        [ObserversRpc(ExcludeOwner = true)]
        void ObserversFire(GameObject prefab, Vector3 position, Vector3 direction, Quaternion rotation, uint tick)
        {
            float passedTime = (float)base.TimeManager.TimePassed(tick, false);
            passedTime = Mathf.Min(MAX_PASSED_TIME, passedTime);
        }
    }
}
