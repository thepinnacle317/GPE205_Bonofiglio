using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FXV
{
    internal class ShieldHit : MonoBehaviour
    {
        static ShieldHit()
        {
            HIT_COLOR_PROP = Shader.PropertyToID("_HitColor");
            HIT_T_PROP = Shader.PropertyToID("_HitT");
            HIT_RADIUS_PROP = Shader.PropertyToID("_HitRadius");
            HIT_POS_PROP = Shader.PropertyToID("_HitPos");
            HIT_POWER_PROP = Shader.PropertyToID("_HitPower");
        }

        private static int HIT_COLOR_PROP;
        private static int HIT_T_PROP;
        private static int HIT_RADIUS_PROP;
        private static int HIT_POS_PROP;
        private static int HIT_POWER_PROP;

        private float lifeTime = 0.5f;
        private float lifeStart = 0.5f;

        private float radius = 1.0f;

        private MeshRenderer myRenderer;

        private AnimationCurve hitDecalFadeAnimation;
        private AnimationCurve hitDecalSizeAnimation;

        private Color color;

        private MaterialPropertyBlock propertyBlock;

        private void OnDestroy()
        {
            hitDecalFadeAnimation = null;
            hitDecalSizeAnimation = null;
            myRenderer = null;
            propertyBlock = null;
        }

        void Update()
        {
            lifeTime -= Time.deltaTime;

            float anim = hitDecalFadeAnimation.Evaluate(1.0f - GetLifeT());

            Color c = color;
            c.a = Mathf.Max(0.0f, anim);
            propertyBlock.SetColor(HIT_COLOR_PROP, c);

            propertyBlock.SetFloat(HIT_T_PROP, (1.0f - GetLifeT()));

            float size = Mathf.Max(0.0f, hitDecalSizeAnimation.Evaluate(1.0f - GetLifeT()));
            propertyBlock.SetFloat(HIT_RADIUS_PROP, radius * size);

            myRenderer.SetPropertyBlock(propertyBlock);

            if (lifeTime <= 0.0f)
            {
                lifeTime = 0.0f;
            }
        }

        internal bool IsFinished()
        {
            return lifeTime <= 0.0f;
        }

        internal float GetLifeT()
        {
            return lifeTime / lifeStart;
        }

        internal void DestroyHitEffect()
        {
            Destroy(gameObject);
        }

        internal void StartHitFX(Vector3 hitLocalPosition, Color hitColor, float power, float time, float radius, AnimationCurve fadeAnim, AnimationCurve sizeAnim)
        {
            myRenderer = GetComponent<MeshRenderer>();

            propertyBlock = new MaterialPropertyBlock();

            myRenderer.GetPropertyBlock(propertyBlock);

            propertyBlock.SetVector(HIT_POS_PROP, hitLocalPosition);
            propertyBlock.SetFloat(HIT_POWER_PROP, power);

            this.radius = radius;

            hitDecalFadeAnimation = fadeAnim;
            hitDecalSizeAnimation = sizeAnim;

            lifeTime = lifeStart = time;

            float anim = hitDecalFadeAnimation.Evaluate(0.0f);

            color = hitColor;
            Color c = color;
            c.a = anim;
            propertyBlock.SetColor(HIT_COLOR_PROP, c);

            propertyBlock.SetFloat(HIT_T_PROP, 0.0f);

            float size = Mathf.Max(0.0f, hitDecalSizeAnimation.Evaluate(0.0f));
            propertyBlock.SetFloat(HIT_RADIUS_PROP, radius * size);

            myRenderer.SetPropertyBlock(propertyBlock);
        }
    }

}