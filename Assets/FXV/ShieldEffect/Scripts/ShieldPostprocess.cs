using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;

namespace FXV
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
    [InitializeOnLoad]
#endif
    [RequireComponent(typeof(Camera))]
    public class ShieldPostprocess : MonoBehaviour
    {
        static List<ShieldPostprocess> instances;

        internal class CameraBufferEvent
        {
            public CommandBuffer cmd;
            public CameraEvent camEvent;
        };

        internal class PostprocessContext
        {
            public RenderTargetIdentifier currentTarget;
            public RenderTargetIdentifier[] tempTarget;
            public int downSampleSteps = 0;
            public int targetWidth;
            public int targetHeight;
            public bool blitToScreen = true;
        }

        internal class InstancedPropertyArray
        {
            public InstancedPropertyArray(int propId) => propertyId = propId;

            public int propertyId;

            public virtual void SetTo(MaterialPropertyBlock properties)
            {

            }
        }

        internal class InstancedPropertyArrayFloat : InstancedPropertyArray
        {
            public InstancedPropertyArrayFloat(int propId) : base(propId)
            {
            }

            public float[] values = new float[256];

            public override void SetTo(MaterialPropertyBlock properties)
            {
                properties.SetFloatArray(propertyId, values);
            }
        }

        internal class InstancedPropertyArrayVector : InstancedPropertyArray
        {
            public InstancedPropertyArrayVector(int propId) : base(propId)
            {
            }

            public Vector4[] values = new Vector4[256];

            public override void SetTo(MaterialPropertyBlock properties)
            {
                properties.SetVectorArray(propertyId, values);
            }
        }

        internal class RenderInstanceInfo
        {
            public Mesh mesh;
            public int submeshIndex = 0;
            public Material material;
            public int shaderPass = 0;
            public Matrix4x4[] matrices = new Matrix4x4[256];
            public MaterialPropertyBlock properties = new MaterialPropertyBlock();
            public Dictionary<int, InstancedPropertyArray> propertyArrays = new Dictionary<int, InstancedPropertyArray>();

            public int matricesCount = 0;

            internal void SetProperty(int arrayId, int propertyId, float value)
            {
                ((InstancedPropertyArrayFloat)propertyArrays[propertyId]).values[arrayId] = value;
            }

            internal void SetProperty(int arrayId, int propertyId, Vector4 value)
            {
                ((InstancedPropertyArrayVector)propertyArrays[propertyId]).values[arrayId] = value;
            }

            internal void RegisterFloatProperyArray(int propId)
            {
                propertyArrays.Add(propId, new InstancedPropertyArrayFloat(propId));
            }

            internal void RegisterVectorProperyArray(int propId)
            {
                propertyArrays.Add(propId, new InstancedPropertyArrayVector(propId));
            }

            internal void UpdateMaterialPropertyBlock()
            {
                foreach (var val in propertyArrays.Values)
                {
                    val.SetTo(properties);
                }
            }

            internal bool IsFull()
            {
                return matricesCount >= matrices.Length;
            }
        }

        private static readonly int OPAQUE_TEXTURE_FOR_REFRACTION_ID = Shader.PropertyToID("_CameraOpaqueTextureBuiltin");

        [fxvHeader("Postprocess Params")]
        [Tooltip("Multiplier of postprocess intensity."), SerializeField, Range(0.001f, 10.0f)]
        internal float postprocessPower = 3.0f;

        [SerializeField, Range(1, 8)]
        [Tooltip("Number of blur iterations.")]
        internal int numberOfIterations = 4;

        [SerializeField, Range(0.0f, 0.5f)]
        [Tooltip("Number of blur iterations.")]
        internal float downSampleRate = 0.3f;

        [SerializeField, Range(1, 7)]
        [Tooltip("Blur kernel radius.")]
        int kernelRadius = 5;
        [SerializeField, Range(0.1f, 10.0f)]
        [Tooltip("Blur shape parameters.")]
        float sigma = 4.0f;
        float sampleStep = 1.0f;

        float[] gauss_coeff_H;

        private Material blendAddMaterial;
        private Material postprocessMaterial;

        private List<fxvRenderObject> allRenderObjects = new List<fxvRenderObject>();

        private Dictionary<int, RenderInstanceInfo> renderInstancingInfos = new Dictionary<int, RenderInstanceInfo>();

        private List<fxvRenderObject> renderInstancedObjects = new List<fxvRenderObject>();
        private List<fxvRenderObject> renderObjects = new List<fxvRenderObject>();

        private Color clearToTransparentColor;

        private Camera myCamera;

        private CameraEvent postprocessGrabOpaqueEvent = CameraEvent.AfterForwardOpaque;
        private CameraEvent postprocessDrawEvent = CameraEvent.AfterForwardAlpha;

        internal static ShieldPostprocess GetMainInstance()
        {
#if UNITY_EDITOR
            if (instances == null)
            {
                instances = new List<ShieldPostprocess>(FindObjectsByType<ShieldPostprocess>(FindObjectsSortMode.None));
            }
#endif
            for (int i = 0; i < instances.Count; ++i)
            {
                if (instances[i].transform == Camera.main.transform)
                {
                    return instances[i];
                }
            }

            return instances.Count > 0 ? instances[0] : null;
        }

        internal static void OnPipelineChanged()
        {
#if UNITY_EDITOR
            if (instances == null)
            {
                instances = new List<ShieldPostprocess>(FindObjectsByType<ShieldPostprocess>(FindObjectsSortMode.None));
            }
#endif

            for (int i = 0; i < instances.Count; ++i)
            {
                instances[i].Prepare();
            }
        }

        internal static int GetMaxDownSampleSteps()
        {
#if UNITY_EDITOR
            if (instances == null)
            {
                instances = new List<ShieldPostprocess>(FindObjectsByType<ShieldPostprocess>(FindObjectsSortMode.None));
            }
#endif

            int maxSteps = 0;
            for (int i = 0; i < instances.Count; ++i)
            {
                maxSteps = Mathf.Max(instances[i].numberOfIterations, maxSteps);
            }

            return maxSteps;
        }

        public static void AddShield(Shield s)
        {
#if UNITY_EDITOR
            if (instances == null)
            {
                instances = new List<ShieldPostprocess>(FindObjectsByType<ShieldPostprocess>(FindObjectsSortMode.None));
            }
#endif

            for (int i = 0; i < instances.Count; ++i)
            {
                instances[i].TryAddRenderObject(s);
            }
        }

        public static void RemoveShield(Shield s)
        {
#if UNITY_EDITOR
            if (instances == null)
            {
                instances = new List<ShieldPostprocess>(FindObjectsByType<ShieldPostprocess>(FindObjectsSortMode.None));
            }
#endif

            for (int i = 0; i < instances.Count; ++i)
            {
                instances[i].TryRemoveRenderObject(s);
            }
        }

        public static void UpdateAllObjects()
        {
#if UNITY_EDITOR
            if (instances == null)
            {
                instances = new List<ShieldPostprocess>(FindObjectsByType<ShieldPostprocess>(FindObjectsSortMode.None));
            }
#endif

            ShieldPostprocess[] postporcess = UnityEngine.Object.FindObjectsByType<ShieldPostprocess>(FindObjectsSortMode.None);
            for (int i = 0; i < postporcess.Length; ++i)
            {
                postporcess[i]._UpdateAllObjects();
            }
        }

        void Awake()
        {
            Prepare();

            if (instances == null)
            {
                instances = new List<ShieldPostprocess>();
            }

            instances.Add(this);
        }

        void Prepare()
        {
            myCamera = GetComponent<Camera>();

            context = null;

            if (blendAddMaterial == null)
            {
                blendAddMaterial = new Material(Shader.Find("Hidden/FXV/FXVPostprocessBlitAdd"));
            }
            blendAddMaterial.SetFloat("_ColorMultiplier", postprocessPower);

            if (postprocessMaterial == null)
            {
                postprocessMaterial = new Material(Shader.Find("Hidden/FXV/FXVPostprocessShield"));
            }

            clearToTransparentColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

            Camera.onPreRender -= RenderWithCamera;
            Camera.onPostRender -= AfterRenderWithCamera;
             
            if (fxvShieldAssetConfig.ActiveRenderPipeline == fxvShieldAssetConfig.Pipeline.FXV_SHIELD_BUILTIN)
            {
                Camera.onPreRender += RenderWithCamera;
                Camera.onPostRender += AfterRenderWithCamera;
            }
        }

        internal void DestroyAsset(UnityEngine.Object assetObject) 
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(assetObject);
            }
            else
#endif
            {
                Destroy(assetObject);
            }
        }

        private void OnDestroy()
        {
            if (instances != null)
            {
                instances.Remove(this);
            }

            Camera.onPreRender -= RenderWithCamera;
            Camera.onPostRender -= AfterRenderWithCamera;

            DestroyBuffers(shieldCommandBuffers);
            DestroyBuffers(grabCommandBuffers);

            if (blendAddMaterial)
            {
                DestroyAsset(blendAddMaterial);

                blendAddMaterial = null;
            }
            if (postprocessMaterial)
            {
                DestroyAsset(postprocessMaterial);

                postprocessMaterial = null;
            }
        }

        internal bool TryAddRenderObject(fxvRenderObject ro)
        {
            if (!allRenderObjects.Contains(ro))
            {
                allRenderObjects.Add(ro);

                ro.SetOwner(this);

                return true;
            }

            return false;
        }

        internal bool TryRemoveRenderObject(fxvRenderObject ro)
        {
            if (allRenderObjects.Contains(ro))
            {
                allRenderObjects.Remove(ro);

                ro.SetOwner(null);

                return true;
            }

            return false;
        }

        void OnEnable()
        {
            Prepare();

#if !UNITY_ANDROID
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
#endif

            _UpdateAllObjects();

            UpdateGaussianCoeff();
        }

        private void OnDisable()
        {
            if (fxvShieldAssetConfig.ActiveRenderPipeline == fxvShieldAssetConfig.Pipeline.FXV_SHIELD_BUILTIN)
            {
                Camera.onPreRender -= RenderWithCamera;
                Camera.onPostRender -= AfterRenderWithCamera;

                DestroyBuffers(shieldCommandBuffers);
                DestroyBuffers(grabCommandBuffers);
            }
        }

        internal void _UpdateAllObjects()
        {
            allRenderObjects.Clear();

            renderInstancedObjects.Clear();
            renderObjects.Clear();

            fxvRenderObject[] renderObjectsOnScene = UnityEngine.Object.FindObjectsByType<fxvRenderObject>(FindObjectsSortMode.None);
            foreach (fxvRenderObject ro in renderObjectsOnScene)
            {
                if (TryAddRenderObject(ro))
                {
                    Renderer r = ro.GetComponent<Renderer>();
                    bool wasEnabled = r.enabled;
                    r.enabled = false;
                    ro.Prepare();
                    r.enabled = wasEnabled;
                }
            }
        }

        void UpdateGaussianCoeff()
        {
            int kernelSize = kernelRadius * 2 + 1;
            if (kernelSize > 16) kernelSize = 16;

            gauss_coeff_H = gaussian_kernel(kernelSize, sigma);

            postprocessMaterial.SetFloatArray("GAUSSIAN_COEFF", gauss_coeff_H);
            postprocessMaterial.SetInt("GAUSSIAN_KERNEL_RADIUS", kernelRadius);
            postprocessMaterial.SetFloat("GAUSSIAN_TEXEL_SIZE", sampleStep);
        }

        internal void _UpdateGaussianCoeff(Material mat)
        {
            int kernelSize = kernelRadius * 2 + 1;
            if (kernelSize > 16) kernelSize = 16;

            gauss_coeff_H = gaussian_kernel(kernelSize, sigma);
            mat.SetFloatArray("GAUSSIAN_COEFF", gauss_coeff_H);
            mat.SetInt("GAUSSIAN_KERNEL_RADIUS", kernelRadius);
            mat.SetFloat("GAUSSIAN_TEXEL_SIZE", sampleStep);
        }

        RenderInstanceInfo GetRenderList(int renderKey)
        {
            return renderInstancingInfos[renderKey];
        }

        internal void AddToRenderList(fxvRenderObject ro)
        {
            if (ro.IsInstancingSupported())
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (renderInstancedObjects.Contains(ro))
                    {
                        return;
                    }
                }

                if (UnityEditor.GameObjectUtility.GetStaticEditorFlags(ro.gameObject).HasFlag(UnityEditor.StaticEditorFlags.BatchingStatic))
                {
                    Debug.Log("[FXV.ShieldPostprocess] Rendering FXV object that is static " + ro.gameObject.name, ro.gameObject);
                    return;
                }
#endif
                int renderKey = ro.GetInstancingKey();
                if (!renderInstancingInfos.ContainsKey(renderKey))
                {
                    RenderInstanceInfo rii = new RenderInstanceInfo();
                    renderInstancingInfos.Add(renderKey, rii);
                    ro.RegisterInstancedProperties(rii);
                }

                renderInstancedObjects.Add(ro);
            }
            else
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (renderObjects.Contains(ro))
                    {
                        return;
                    }
                }
#endif
                renderObjects.Add(ro);
            }
        }

        internal void OnInstancingKeyUpdate(fxvRenderObject ro, int renderKey)
        {
            if (!renderInstancingInfos.ContainsKey(renderKey))
            {
                RenderInstanceInfo rii = new RenderInstanceInfo();
                renderInstancingInfos.Add(renderKey, rii);
                ro.RegisterInstancedProperties(rii);
            }
        }

        private void Update()
        {

        }

        internal void _PushRenderObjectsToCommandBuffer(CommandBuffer cmd)
        {
            cmd.ClearRenderTarget(false, true, clearToTransparentColor);

            int count = renderInstancedObjects.Count;
            for (int i = 0; i < count; ++i)
            {
                fxvRenderObject ro = renderInstancedObjects[i];

                if (ro == null || !ro.WillRender())
                {
                    renderInstancedObjects.RemoveAt(i);
                    i--;
                    count--;
                    continue;
                }

                RenderInstanceInfo rii = GetRenderList(ro.GetInstancingKey());
                if (!rii.IsFull())
                {
                    ro.AddRenderInstanceInfo(rii);
                }
            }


            count = renderObjects.Count;
            for (int i = 0; i < count; ++i)
            {
                fxvRenderObject ro = renderObjects[i];

                if (ro == null || !ro.WillRender())
                {
                    renderObjects.RemoveAt(i);
                    i--;
                    count--;
                    continue;
                }

                ro.RenderNoInstancing(cmd);
            }

            foreach (RenderInstanceInfo rii in renderInstancingInfos.Values)
            {
                if (rii.matricesCount == 0)
                {
                    continue;
                }

                rii.UpdateMaterialPropertyBlock();

                cmd.DrawMeshInstanced(rii.mesh, rii.submeshIndex, rii.material, rii.shaderPass, rii.matrices, rii.matricesCount, rii.properties);

                rii.matricesCount = 0;
            }

        }

        internal void _AddPostprocessToCommandBuffer(CommandBuffer cmd, PostprocessContext context)
        {
            UpdateGaussianCoeff();

            cmd.SetGlobalVector("_CameraDepthTexture_TexelSize", new Vector4(1.0f / context.targetWidth, 1.0f / context.targetHeight, context.targetWidth, context.targetHeight));

            int pass = 2;
            for (int i = 0; i < context.downSampleSteps; ++i)
            {
                cmd.Blit(context.tempTarget[i], context.tempTarget[i + 1], postprocessMaterial, pass);
                pass = pass == 2 ? 3 : 2;
            }

            for (int i = 0; i < context.downSampleSteps; ++i)
            {
                cmd.Blit(context.tempTarget[numberOfIterations - i], context.tempTarget[numberOfIterations - i - 1], postprocessMaterial, 1);
            }

            if (context.blitToScreen)
            {
                blendAddMaterial.SetFloat("_ColorMultiplier", postprocessPower);

                cmd.Blit(context.tempTarget[0], context.currentTarget, blendAddMaterial);
            }
        }

        PostprocessContext context;
        int[] tempTargetTextureId;

        private readonly Dictionary<Camera, CameraBufferEvent> shieldCommandBuffers = new Dictionary<Camera, CameraBufferEvent>();
        private readonly Dictionary<Camera, CameraBufferEvent> grabCommandBuffers = new Dictionary<Camera, CameraBufferEvent>();

        RenderTexture opaqueRT = null;
        RenderTargetIdentifier opaqueTextureRT;

        private CameraBufferEvent GetBuffer(Dictionary<Camera, CameraBufferEvent> map, Camera cam, CameraEvent camEvent)
        {
            CameraBufferEvent buffer;
            if (map.ContainsKey(cam))
            {
                buffer = map[cam];
                if (buffer.camEvent != camEvent)
                {
                    cam.RemoveCommandBuffer(buffer.camEvent, buffer.cmd);
                    cam.AddCommandBuffer(camEvent, buffer.cmd);
                    buffer.camEvent = camEvent;
                }
            }
            else
            {
                CommandBuffer cmd = cam.GetCommandBuffers(camEvent).FirstOrDefault(b => b.name == "FXVShieldPostprocess");
                buffer = new CameraBufferEvent();
                buffer.camEvent = camEvent;

                if (cmd != null)
                {
                    buffer.cmd = cmd;
                    map[cam] = buffer;
                    return buffer;
                }

                cmd = new CommandBuffer();
                cmd.name = "FXVShieldPostprocess";
                buffer.cmd = cmd;
                map[cam] = buffer;

                cam.AddCommandBuffer(camEvent, cmd);
            }

            buffer.cmd.Clear();
            return buffer;
        }

        private void DestroyBuffers(Dictionary<Camera, CameraBufferEvent> map)
        {
            foreach(var entry in map)
            {
                Camera cam = entry.Key;
                CameraBufferEvent buffer = map[cam];
                if (cam != null)
                {
                    cam.RemoveCommandBuffer(buffer.camEvent, buffer.cmd);
                }
                buffer.cmd.Clear();
            }

            map.Clear();
        }

        public void RenderWithCamera(Camera cam)
        {
            if (!cam)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (myCamera == null)
                {
                    Prepare();
                }

                UpdateGaussianCoeff();
            }
#endif

            bool isProperCamera = false;
#if UNITY_EDITOR
            isProperCamera = (cam.name == "SceneCamera");
#endif
            isProperCamera = cam == myCamera || isProperCamera;

            if (!isProperCamera)
            {
                return;
            }

            var rtW = Screen.width;
            var rtH = Screen.height;

            if (opaqueRT)
            {
                RenderTexture.ReleaseTemporary(opaqueRT);
                opaqueRT = null;
            }

            postprocessGrabOpaqueEvent = (cam.renderingPath == RenderingPath.Forward || cam.orthographic) ? CameraEvent.AfterForwardOpaque : CameraEvent.AfterLighting;
            CameraBufferEvent grabBuffer = GetBuffer(grabCommandBuffers, cam, postprocessGrabOpaqueEvent);
            opaqueRT = RenderTexture.GetTemporary(rtW, rtH, 0);
            opaqueTextureRT = new RenderTargetIdentifier(opaqueRT);
            grabBuffer.cmd.Blit(BuiltinRenderTextureType.CurrentActive, opaqueTextureRT);

            Shader.SetGlobalTexture(OPAQUE_TEXTURE_FOR_REFRACTION_ID, opaqueRT);

            CameraBufferEvent bufferEvent = GetBuffer(shieldCommandBuffers, cam, postprocessDrawEvent);

            bufferEvent.cmd.Clear();

            if (context == null)
            {
                context = new ShieldPostprocess.PostprocessContext();
                context.tempTarget = new RenderTargetIdentifier[numberOfIterations + 1];

                tempTargetTextureId = new int[numberOfIterations + 1];
                for (int i = 0; i < tempTargetTextureId.Length; i++)
                {
                    tempTargetTextureId[i] = Shader.PropertyToID("_FXVTemporaryBuffer_" + i);
                }
            }

            context.downSampleSteps = numberOfIterations;
            context.targetWidth = rtW;
            context.targetHeight = rtH;
            context.currentTarget = BuiltinRenderTextureType.CameraTarget;

            bufferEvent.cmd.GetTemporaryRT(tempTargetTextureId[0], new RenderTextureDescriptor(rtW, rtH, RenderTextureFormat.ARGBFloat, 0));
            context.tempTarget[0] = new RenderTargetIdentifier(tempTargetTextureId[0]);

            int w = context.targetWidth;
            int h = context.targetHeight;

            float downsample = (1.0f - downSampleRate);

            for (int i = 1; i < tempTargetTextureId.Length; i++)
            {
                w = (int)(w * downsample);
                h = (int)(h * downsample);

                bufferEvent.cmd.GetTemporaryRT(tempTargetTextureId[i], w, h, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
                context.tempTarget[i] = new RenderTargetIdentifier(tempTargetTextureId[i]);
            }

            bufferEvent.cmd.SetRenderTarget(context.tempTarget[0]);

            _PushRenderObjectsToCommandBuffer(bufferEvent.cmd);

            _AddPostprocessToCommandBuffer(bufferEvent.cmd, context);

            for (int i = 0; i < tempTargetTextureId.Length; i++)
            {
                bufferEvent.cmd.ReleaseTemporaryRT(tempTargetTextureId[i]);
            }
        }

        void AfterRenderWithCamera(Camera cam)
        {
            if (!cam)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (myCamera == null)
                {
                    Prepare();
                }

                UpdateGaussianCoeff();
            }
#endif
            if (opaqueRT)
            {
                RenderTexture.ReleaseTemporary(opaqueRT);
                opaqueRT = null;

                Shader.SetGlobalTexture(OPAQUE_TEXTURE_FOR_REFRACTION_ID, Texture2D.blackTexture);
            }
        }

        float erf(float x)
        {
            const float a1 = 0.254829592f;
            const float a2 = -0.284496736f;
            const float a3 = 1.421413741f;
            const float a4 = -1.453152027f;
            const float a5 = 1.061405429f;
            const float p = 0.3275911f;

            float t = 1.0f / (1.0f + p * Mathf.Abs(x));
            float y = 1.0f - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Mathf.Exp(-x * x);

            return Mathf.Sign(x) * y;
        }

        float def_int_gaussian(float x, float mu, float sigma)
        {
            return 0.5f * erf((x - mu) / (1.41421356237f * sigma));
        }

        float[] gaussian_kernel(int kernel_size = 5, float sigma = 1.0f, float mu = 0.0f, int step = 1)
        {
            float end = 0.5f * kernel_size;
            float start = -end;
            List<float> coeff = new List<float>();

            float sum = 0.0f;
            float x = start;
            float last_int = def_int_gaussian(x, mu, sigma);

            while (x < end)
            {
                x += step;
                float new_int = def_int_gaussian(x, mu, sigma);
                float c = new_int - last_int;
                coeff.Add(c);
                sum += c;
                last_int = new_int;
            }

            sum = 1 / sum;
            for (int i = 0; i < coeff.Count; i++)
            {
                coeff[i] *= sum;
            }

            while(coeff.Count < 16)
            {
                coeff.Add(0.0f);
            }

            return coeff.ToArray();
        }
    }
}