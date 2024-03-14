using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FXV.ShieldDemo
{
    public class fxvBullet : MonoBehaviour
    {
        public LayerMask mask;

        public float speed = 10.0f;
        public float lifetime = 2.0f;

        public float bulletHitSize = 1.0f;
        public float bulletHitDuration = 2.0f;

        public GameObject hitParticles;

        private Vector3 moveDir = Vector3.zero;
        private Ray ray;

        private float currentTime = 0.0f;

        bool isToDestroy = false;

        void Start()
        {

        }

        void Update()
        {
            if (isToDestroy)
            {
                return;
            }

            Vector3 newPos = transform.position + moveDir * speed * Time.deltaTime;

            ray.origin = transform.position;
            RaycastHit rhi;

            Vector3 offset = newPos - transform.position;

            currentTime += Time.deltaTime;

            bool needDestroy = false;

            if (currentTime > lifetime)
            {
                needDestroy = true;
            }

            if (Physics.Raycast(ray, out rhi, offset.magnitude, mask.value, QueryTriggerInteraction.Ignore))
            {
                needDestroy = true;

                transform.position = rhi.point;

                if (hitParticles)
                {
                    GameObject ps = Instantiate(hitParticles, rhi.point, Quaternion.identity);
                    ps.transform.position = transform.position;
                    ps.GetComponent<ParticleSystem>().Emit(25);
                    Destroy(ps, 3.0f);
                }

                Shield shield = rhi.collider.gameObject.GetComponentInParent<Shield>();

                if (shield && !shield.GetIsDuringActivationAnim())
                {
                    shield.OnHit(rhi.point, bulletHitSize, bulletHitDuration);
                }
            }
            else
            {
                transform.position = newPos;
            }

            if (needDestroy)
            {
                isToDestroy = true;
                Destroy(gameObject, 3.0f);
            }
        }

        public void Shoot(Vector3 dir)
        {
            moveDir = dir;
            currentTime = 0.0f;

            ray = new Ray(transform.position, moveDir);
        }
    }

}