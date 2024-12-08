using UnityEngine;
using FishNet;
using FishNet.Object;

namespace Shadowfinder.Weapons
{
    public class Arrow : NetworkBehaviour
    {
        const float MOVE_RATE = 5f;
        const float LIFE = 8f;

        Vector3 _direction;
        float _passedTime = 0f;
        float _currentTime = 0f;

        public void Initialize(Vector3 direction, float passedTime)
        {
            _direction = direction;
            _passedTime = passedTime;
        }

        void Update()
        {
            float delta = Time.deltaTime;

            _currentTime += delta;
            if (_currentTime > LIFE)
            {
                Despawn();
                return;
            }

            float passedTimeDelta = 0f;
            if (_passedTime > 0f)
            {
                float step = (_passedTime * 0.08f);
                _passedTime -= step;

                if (_passedTime <= (delta / 2f))
                {
                    step += _passedTime;
                    _passedTime = 0f;
                }

                passedTimeDelta = step;
            }

            transform.position += _direction * (MOVE_RATE * (delta + passedTimeDelta));
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (InstanceFinder.IsClientStarted)
            {
                //client-side VFX and audio
            }

            if (InstanceFinder.IsServerStarted)
            {
                //PlayerController p = collision.gameObject.GetComponent<PlayerController>();
                //if (p != null) p.Health -= 10f;
            }

            Despawn();
        }
    }
}
