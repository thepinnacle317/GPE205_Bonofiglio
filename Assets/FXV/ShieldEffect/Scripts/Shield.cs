#define FXV_SHIELD_USE_PROPERTY_BLOCKS

using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System;

namespace FXV
{
    public class ShieldDraggablePoint : PropertyAttribute { }

    public class Shield : fxvRenderObject
    {
        static Shield()
        {
            ACTIVATION_TIME_PROP = Shader.PropertyToID("_ActivationTime");
            ACTIVATION_TIME_PROP01 = Shader.PropertyToID("_ActivationTime01");
            ACTIVATION_RIM_PROP = Shader.PropertyToID("_ActivationRim");
            SHIELD_DIRECTION_PROP = Shader.PropertyToID("_ShieldDirection");
            HIT_EFFECT_VALUE_PROP = Shader.PropertyToID("_HitEffectValue");

            MAIN_TEX_COLOR_PROP = Shader.PropertyToID("_Color");
            TEX_COLOR_PROP = Shader.PropertyToID("_TextureColor");
            PATTERN_COLOR_PROP = Shader.PropertyToID("_PatternColor");
        }

        private static int ACTIVATION_TIME_PROP;
        private static int ACTIVATION_TIME_PROP01;
        private static int ACTIVATION_RIM_PROP;
        private static int SHIELD_DIRECTION_PROP;
        private static int HIT_EFFECT_VALUE_PROP;

        private static int MAIN_TEX_COLOR_PROP;
        private static int TEX_COLOR_PROP;
        private static int PATTERN_COLOR_PROP;

#region ShieldProperties
        [fxvHeader("Shield Config")]
        [Tooltip("Is shield active at start. This will have effect when entering play mode.")]
        public bool shieldActive = true;

        [Tooltip("How fast shield activation animation is.")]
        public float shieldActivationSpeed = 1.0f;

        [Tooltip("Specify the range of for activation time so that shield is invisible at time 0, and fully visible at time 1.")]
        public Vector2 shieldActivationRange = new Vector2(0.0f, 1.0f);

        [Tooltip("Sometimes you dont want to make shield a child of an object - specify GameObject here you want the shield to follow without being a child of it.")]
        public GameObject followObject;

        [Tooltip("Specify the light object the shield should affect when turned on/off.")]
        public Light shieldLight;

        private float shieldActivationRim = 0.2f;
#endregion ShieldProperties

#region HitProperties
        [fxvHeader("Hit Config"), SerializeField]
        Color hitColor = Color.white;

        [FormerlySerializedAs("hitRippleTexture"), SerializeField]
        Texture2D hitDecalTexture = null;

        [FormerlySerializedAs("hitRippleDistortion"), SerializeField]
        float hitEffectDistortion = 1.0f;

        [SerializeField]
        float hitColorPower = 1.0f;

        [SerializeField]
        AnimationCurve hitShieldAnimation = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

        [FormerlySerializedAs("hitDecalAnimation"), SerializeField]
        AnimationCurve hitDecalFadeAnimation = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 0.0f);

        [SerializeField]
        AnimationCurve hitDecalSizeAnimation = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 0.0f);

        [SerializeField]
        float hitDurationModifier = 1.0f;

        [SerializeField]
        bool hitAffectsColor = false;

        [SerializeField]
        bool hitAffectsRimTexture = true;

        [SerializeField]
        bool hitAffectsPatternTexture = true;
#endregion HitProperties

#region PreviewProperties
        [fxvHeader("Preview Options (Edit Mode Only)")]
        [SerializeField, ShieldDraggablePoint,Tooltip("Use gizmo in scene view to change position of fade point. This is only used when material have Direction Based Visibility Enabled.")]
        private Vector3 ShieldFadePoint = Vector3.up;

        [SerializeField, Range(0.0f, 1.0f), Tooltip("Use this slider for activation animation preview in edit mode.")]
        float activationAnimationPreview = 1.0f;
#endregion PreviewProperties

        private float shieldActivationTime;
        private float shieldActivationDir;

        private Color lightColor;

        private Material baseMaterial;
        private Material activationMaterial;
        private Material instancedMaterial = null;
        private Material postprocessMaterial;
        private Material postprocessActivationMaterial;
        private Material hitMaterial;

        private Collider myCollider;

#if FXV_SHIELD_USE_PROPERTY_BLOCKS
        private MaterialPropertyBlock propertyBlock;
        private bool isDirty = false;
#endif

        private int currentHitIndex = 1;

        private float enabledTimer = 0.0f;

        private bool isVisuallyActive = true;

        private List<ShieldHit> activeHits = new List<ShieldHit>();

       // public Shield copyHitPAramsFrom;

        internal void CopyHitValuesFrom(Shield shield)
        {
            hitColor = shield.hitColor;
            hitDecalTexture = shield.hitDecalTexture;
            hitEffectDistortion = shield.hitEffectDistortion;
            hitColorPower = shield.hitColorPower;
            hitShieldAnimation = new AnimationCurve(shield.hitShieldAnimation.keys);
            hitDecalFadeAnimation = new AnimationCurve(shield.hitDecalFadeAnimation.keys);
            hitDecalSizeAnimation = new AnimationCurve(shield.hitDecalSizeAnimation.keys);
            hitDurationModifier = shield.hitDurationModifier;
            hitAffectsColor = shield.hitAffectsColor;
            hitAffectsRimTexture = shield.hitAffectsRimTexture;
            hitAffectsPatternTexture = shield.hitAffectsPatternTexture;
        }

        internal override void Prepare()
        {
            base.Prepare();

#if FXV_SHIELD_USE_PROPERTY_BLOCKS
            propertyBlock = new MaterialPropertyBlock();
#endif

            myRenderer = GetComponent<Renderer>();

            ShieldPostprocess.AddShield(this);

            shieldActivationDir = 0.0f;

            if (shieldLight)
            {
                lightColor = shieldLight.color;
            }

            myCollider = transform.GetComponent<Collider>();

            if (shieldActive)
            {
                shieldActivationTime = 1.0f;
                if (myCollider)
                {
                    myCollider.enabled = true;
                }
                myRenderer.enabled = true;
            }
            else
            {
                shieldActivationTime = 0.0f;
                if (myCollider)
                {
                    myCollider.enabled = false;
                }
                myRenderer.enabled = false;
            }

            if (shieldLight && Application.isPlaying)
            {
                shieldLight.color = Color.Lerp(Color.black, lightColor, shieldActivationTime);
            }


            SetMaterial(myRenderer.sharedMaterial);
        }

        internal override void RegisterInstancedProperties(ShieldPostprocess.RenderInstanceInfo rii)
        {
#if FXV_SHIELD_USE_PROPERTY_BLOCKS
            rii.RegisterFloatProperyArray(ACTIVATION_TIME_PROP);
            rii.RegisterFloatProperyArray(ACTIVATION_TIME_PROP01);
            rii.RegisterFloatProperyArray(HIT_EFFECT_VALUE_PROP);
            rii.RegisterVectorProperyArray(SHIELD_DIRECTION_PROP);
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ShieldPostprocess.RemoveShield(this);

#if !FXV_SHIELD_USE_PROPERTY_BLOCKS
            if (Application.isPlaying)
            {
                if (baseMaterial)
                {
                    DestroyAsset(baseMaterial);
                }
            }
#endif
            if (postprocessMaterial)
            {
                DestroyAsset(postprocessMaterial);
            }
            if (postprocessActivationMaterial)
            {
                DestroyAsset(postprocessActivationMaterial);
            }
            if (activationMaterial)
            {
                DestroyAsset(activationMaterial);
            }
            if (hitMaterial)
            {
                DestroyAsset(hitMaterial);
            }
            if (instancedMaterial)
            {
                DestroyAsset(instancedMaterial);
            }

            baseMaterial = null;
            postprocessMaterial = null;
            postprocessActivationMaterial = null;
            activationMaterial = null;
            instancedMaterial = null;
        }

        private void OnValidate()
        {
           /* if (copyHitPAramsFrom != null)
            {
                CopyHitValuesFrom(copyHitPAramsFrom);
                copyHitPAramsFrom = null;
            }*/

            if (!Application.isPlaying)
            {
                shieldActivationTime = activationAnimationPreview;
            }

            UpdateActivationTimeProps();
            SetShieldEffectDirection(ShieldFadePoint);
        }

        public void SetMaterial(Material newMat)
        {
            if (newMat == null)
            {
                return;
            }

            if (postprocessMaterial)
            {
                DestroyAsset(postprocessMaterial);
            }
            if (postprocessActivationMaterial)
            {
                DestroyAsset(postprocessActivationMaterial);
            }
            if (activationMaterial)
            {
                DestroyAsset(activationMaterial);
            }
            if (hitMaterial)
            {
                DestroyAsset(hitMaterial);
            }

            float t = shieldActivationRange.x + shieldActivationTime * (shieldActivationRange.y - shieldActivationRange.x);

#if FXV_SHIELD_USE_PROPERTY_BLOCKS
            propertyBlock.SetFloat(ACTIVATION_TIME_PROP, t);
            propertyBlock.SetFloat(ACTIVATION_TIME_PROP01, shieldActivationTime);
            propertyBlock.SetFloat(HIT_EFFECT_VALUE_PROP, 1.0f);
            propertyBlock.SetVector(SHIELD_DIRECTION_PROP, new Vector4(ShieldFadePoint.x, ShieldFadePoint.y, ShieldFadePoint.z, 0.0f));

            baseMaterial = newMat;
            baseMaterial.EnableKeyword("USE_MATERIAL_PROPERTY_BLOCKS");
            baseMaterial.EnableKeyword("ACTIVATION_EFFECT_ON");
            baseMaterial.DisableKeyword("HIT_EFFECT_ON");
            
            myRenderer.SetPropertyBlock(propertyBlock);
            isDirty = false;
#else

            if (Application.isPlaying)
            {
                if (baseMaterial)
                {
                    DestroyAsset(baseMaterial);
                }

                baseMaterial = new Material(newMat);
                baseMaterial.SetFloat(ACTIVATION_TIME_PROP, t);
                baseMaterial.DisableKeyword("HIT_EFFECT_ON");
            }
            else
            {
                baseMaterial = newMat;
                baseMaterial.SetFloat(ACTIVATION_TIME_PROP, t);
            }
#endif

            baseMaterial.SetShaderPassEnabled("Postprocess", false);

            postprocessMaterial = new Material(baseMaterial);
            postprocessMaterial.DisableKeyword("USE_REFRACTION");

            postprocessActivationMaterial = new Material(baseMaterial);
            postprocessActivationMaterial.DisableKeyword("USE_REFRACTION");
            postprocessActivationMaterial.EnableKeyword("ACTIVATION_EFFECT_ON");

            activationMaterial = new Material(baseMaterial);
            activationMaterial.EnableKeyword("ACTIVATION_EFFECT_ON");

            hitMaterial = new Material(baseMaterial);
            hitMaterial.DisableKeyword("USE_MAIN_TEXTURE_ANIMATION");
            hitMaterial.DisableKeyword("USE_PATTERN_TEXTURE_ANIMATION");
            if (!hitAffectsRimTexture)
            {
                hitMaterial.DisableKeyword("USE_MAIN_TEXTURE");
            }

            if (!hitAffectsPatternTexture)
            {
                hitMaterial.DisableKeyword("USE_PATTERN_TEXTURE");
            }

            hitMaterial.EnableKeyword("HIT_EFFECT_ON");
            hitMaterial.SetInt("_BlendSrcMode", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            hitMaterial.SetInt("_BlendDstMode", (int)UnityEngine.Rendering.BlendMode.One);
            hitMaterial.SetVector("_WorldScale", transform.lossyScale);
            hitMaterial.SetFloat("_HitPower", 1.0f);

            if (hitDecalTexture != null)
            {
                hitMaterial.EnableKeyword("USE_HIT_RIPPLE");
                hitMaterial.SetTexture("_HitRippleTex", hitDecalTexture);
                hitMaterial.SetFloat("_HitRippleDistortion", hitEffectDistortion);
                hitMaterial.SetFloat("_HitColorAffect", hitAffectsColor ? 1.0f : 0.0f);
            }

            hitMaterial.renderQueue = hitMaterial.renderQueue + currentHitIndex;

            shieldActivationRim = activationMaterial.GetFloat(ACTIVATION_RIM_PROP);

            myRenderer.sharedMaterial = baseMaterial;

            SetShieldEffectDirection(ShieldFadePoint);

            SetRenderPostprocessMaterial(GetIsDuringActivationAnim() ? postprocessActivationMaterial : postprocessMaterial, false);
        }

        internal void CreateInstancedMaterialIfNeeded()
        {
            if (instancedMaterial == null)
            {
                instancedMaterial = new Material(myRenderer.sharedMaterial);
                SetMaterial(instancedMaterial);
            }
        }

        public void SetRimColor(Color c)
        {
            CreateInstancedMaterialIfNeeded();

            activationMaterial.color = c;
            baseMaterial.color = c;
            myRenderer.sharedMaterial.color = c;
            postprocessMaterial.color = c;
            postprocessActivationMaterial.color = c;
        }

        public void SetTextureRimColor(Color c)
        {
            CreateInstancedMaterialIfNeeded();

            activationMaterial.SetColor(TEX_COLOR_PROP, c);
            baseMaterial.SetColor(TEX_COLOR_PROP, c);
            myRenderer.sharedMaterial.SetColor(TEX_COLOR_PROP, c);
            postprocessMaterial.SetColor(TEX_COLOR_PROP, c);
            postprocessActivationMaterial.SetColor(TEX_COLOR_PROP, c);
        }

        public void SetPatternColor(Color c)
        {
            CreateInstancedMaterialIfNeeded();

            activationMaterial.SetColor(PATTERN_COLOR_PROP, c);
            baseMaterial.SetColor(PATTERN_COLOR_PROP, c);
            myRenderer.sharedMaterial.SetColor(PATTERN_COLOR_PROP, c);
            postprocessMaterial.SetColor(PATTERN_COLOR_PROP, c);
            postprocessActivationMaterial.SetColor(PATTERN_COLOR_PROP, c);
        }

        public void SetHitColor(Color c)
        {
            hitColor = c;
        }

        public bool IsHitDecalTextureEnabled()
        {
            return hitDecalTexture != null;
        }

        void Update()
        {
            if ((shieldActivationTime == 1.0f) || (shieldActivationDir != 0.0f))
            {
                if (myRenderer.enabled != isVisuallyActive)
                {
                    myRenderer.enabled = isVisuallyActive;
                }
            }

            if (shieldActivationDir > 0.0f)
            {
                shieldActivationTime += shieldActivationSpeed * Time.deltaTime;
                if (shieldActivationTime >= 1.0f)
                {
                    shieldActivationTime = 1.0f;
                    shieldActivationDir = 0.0f;
                    myRenderer.sharedMaterial = baseMaterial;

                    SetRenderPostprocessMaterial(GetIsDuringActivationAnim() ? postprocessActivationMaterial : postprocessMaterial, false);
                }

                UpdateActivationTimeProps();

                if (shieldLight && Application.isPlaying)
                {
                    shieldLight.color = Color.Lerp(Color.black, lightColor, shieldActivationTime);
                }
            }
            else if (shieldActivationDir < 0.0f)
            {
                shieldActivationTime -= shieldActivationSpeed * Time.deltaTime;
                if (shieldActivationTime <= -shieldActivationRim)
                {
                    shieldActivationTime = -shieldActivationRim;
                    shieldActivationDir = 0.0f;
                    myRenderer.enabled = false;
                    myRenderer.sharedMaterial = baseMaterial;

                    SetRenderPostprocessMaterial(GetIsDuringActivationAnim() ? postprocessActivationMaterial : postprocessMaterial, false);
                }

                UpdateActivationTimeProps();

                if (shieldLight && Application.isPlaying)
                {
                    shieldLight.color = Color.Lerp(Color.black, lightColor, shieldActivationTime);
                }
            }

            if (followObject)
            {
                transform.position = followObject.transform.position;
            }

            if (GetIsShieldActive())
            {
                enabledTimer += Time.deltaTime;
            }
            else
            {
                enabledTimer = 0.0f;
            }

            if (activeHits.Count > 0)
            {
                float maxHitT = 0.0f;
                for (int i = 0; i < activeHits.Count; ++i)
                {
                    if (activeHits[i].IsFinished())
                    {
                        activeHits[i].DestroyHitEffect();
                        activeHits.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        maxHitT = Mathf.Max(maxHitT, activeHits[i].GetLifeT());
                    }
                }

                UpdateHitEffectProps(hitShieldAnimation.Evaluate(1.0f - maxHitT));
            }

#if FXV_SHIELD_USE_PROPERTY_BLOCKS
            if (isDirty)
            {
                myRenderer.SetPropertyBlock(propertyBlock);
                isDirty = false;
            }
#endif
        }

        void UpdateHitEffectProps(float v)
        {
#if FXV_SHIELD_USE_PROPERTY_BLOCKS
            if (propertyBlock != null)
            {

                propertyBlock.SetFloat(HIT_EFFECT_VALUE_PROP, v);

                isDirty = true;
            }
#else
            myRenderer.sharedMaterial.SetFloat(HIT_EFFECT_VALUE_PROP, v);
            postprocessActivationMaterial.SetFloat(HIT_EFFECT_VALUE_PROP, v);
#endif
        }

        void UpdateActivationTimeProps()
        {
            float t = shieldActivationRange.x + shieldActivationTime * (shieldActivationRange.y - shieldActivationRange.x);

#if FXV_SHIELD_USE_PROPERTY_BLOCKS
            if (propertyBlock != null)
            {
                propertyBlock.SetFloat(ACTIVATION_TIME_PROP, t);
                propertyBlock.SetFloat(ACTIVATION_TIME_PROP01, shieldActivationTime);

                isDirty = true;
            }
#else
            if (myRenderer != null)
            {
                myRenderer.sharedMaterial.SetFloat(ACTIVATION_TIME_PROP, t);
            }
            if (postprocessActivationMaterial)
            {
                postprocessActivationMaterial.SetFloat(ACTIVATION_TIME_PROP, t);
            }
#endif
        }

        public bool GetIsShieldActive()
        {
            return (shieldActivationTime == 1.0f) || (shieldActivationDir == 1.0f);
        }

        public bool GetIsDuringActivationAnim()
        {
            return shieldActivationDir != 0.0f;
        }

        public float GetShieldEnabledTimer()
        {
            return enabledTimer;
        }

        public void SetShieldVisuallyActive(bool active)
        {
            isVisuallyActive = active;
        }

        public void SetShieldActive(bool active, bool animated = true)
        {
            if (active && !GetIsShieldActive())
            {
                enabledTimer = 0.0f;
            }

            if (myRenderer == null) //let Awake set begin state
            {
                shieldActive = active;
                myRenderer = GetComponent<Renderer>();
                myCollider = transform.GetComponent<Collider>();
            }

            if (animated)
            {
                shieldActivationDir = (active) ? 1.0f : -1.0f;
                if (activationMaterial)
                {
                    myRenderer.sharedMaterial = activationMaterial;

                    UpdateActivationTimeProps();
                }

                if (active)
                {
                    myRenderer.enabled = isVisuallyActive;
                }
            }
            else
            {
                shieldActivationTime = (active) ? 1.0f : 0.0f;
                shieldActivationDir = 0.0f;

                myRenderer.enabled = active && isVisuallyActive;

                UpdateActivationTimeProps();
            }

            SetRenderPostprocessMaterial(GetIsDuringActivationAnim() ? postprocessActivationMaterial : postprocessMaterial, false);

            if (myCollider)
            {
                myCollider.enabled = active;
            }
        }

        public void SetShieldEffectFadePointPosition(Vector3 localPos)
        {
            ShieldFadePoint = localPos;

            SetShieldEffectDirection(new Vector4(localPos.x, localPos.y, localPos.z, 0.0f));
        }

        void SetShieldEffectDirection(Vector4 dir)
        {
#if FXV_SHIELD_USE_PROPERTY_BLOCKS
            if (propertyBlock != null)
            {
                propertyBlock.SetVector(SHIELD_DIRECTION_PROP, dir);

                isDirty = true;
            }
#else
            if (myRenderer != null)
            {
                myRenderer.sharedMaterial.SetVector(SHIELD_DIRECTION_PROP, dir);
            }
            if (baseMaterial != null)
            {
                baseMaterial.SetVector(SHIELD_DIRECTION_PROP, dir);
                activationMaterial.SetVector(SHIELD_DIRECTION_PROP, dir);
                postprocessMaterial.SetVector(SHIELD_DIRECTION_PROP, dir);
                postprocessActivationMaterial.SetVector(SHIELD_DIRECTION_PROP, dir);
            }
#endif
        }

        public void OnHit(Vector3 hitPos, float hitScale, float hitDuration)
        {
            AddHitMeshAtPos(gameObject.GetComponent<MeshFilter>().sharedMesh, hitPos, hitScale, hitDuration);
        }

        private void AddHitMeshAtPos(Mesh mesh, Vector3 hitPos, float hitScale, float hitDuration)
        {
            GameObject hitObject = new GameObject("hitFX");
            hitObject.transform.parent = transform;
            hitObject.transform.position = transform.position;
            hitObject.transform.localScale = Vector3.one;
            hitObject.transform.rotation = transform.rotation;

            Vector3 hitLocalSpace = transform.InverseTransformPoint(hitPos);

            MeshRenderer mr = hitObject.AddComponent<MeshRenderer>();
            MeshFilter mf = hitObject.AddComponent<MeshFilter>();

            mf.sharedMesh = mesh;
            mr.sharedMaterial = hitMaterial;

            mr.shadowCastingMode = myRenderer.shadowCastingMode;

#if FXV_SHIELD_USE_PROPERTY_BLOCKS
            mr.SetPropertyBlock(propertyBlock);
#else
            mr.sharedMaterial.SetVector(SHIELD_DIRECTION_PROP, ShieldFadePoint);
#endif

            ShieldHit hit = hitObject.AddComponent<ShieldHit>();
            hit.StartHitFX(hitLocalSpace, hitColor, hitColorPower, hitDuration * hitDurationModifier, hitScale, hitDecalFadeAnimation, hitDecalSizeAnimation);

            activeHits.Add(hit);

            currentHitIndex++;
            if (currentHitIndex > 100)
                currentHitIndex = 1;
        }

        internal override void AddRenderInstanceInfo(ShieldPostprocess.RenderInstanceInfo info)
        {
            info.mesh = myMeshFilter.sharedMesh;
            info.material = myRenderer.sharedMaterial;
            info.matrices[info.matricesCount] = GetRenderMatrix();
            info.shaderPass = 1;
#if FXV_SHIELD_USE_PROPERTY_BLOCKS
            info.SetProperty(info.matricesCount, ACTIVATION_TIME_PROP, propertyBlock.GetFloat(ACTIVATION_TIME_PROP));
            info.SetProperty(info.matricesCount, ACTIVATION_TIME_PROP01, propertyBlock.GetFloat(ACTIVATION_TIME_PROP01));
            info.SetProperty(info.matricesCount, HIT_EFFECT_VALUE_PROP, propertyBlock.GetFloat(HIT_EFFECT_VALUE_PROP));
            info.SetProperty(info.matricesCount, SHIELD_DIRECTION_PROP, propertyBlock.GetVector(SHIELD_DIRECTION_PROP));
#endif
            info.matricesCount++;
        }
    }
}