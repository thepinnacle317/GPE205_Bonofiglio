using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FXV
{
    [ExecuteInEditMode]
    public class fxvRenderObject : MonoBehaviour
    {
        protected Renderer myRenderer = null;
        protected MeshFilter myMeshFilter = null;
        protected Material renderPostprocessMaterial = null;

        string renderInstancingKey;
        int renderInstancingHash = 0;

        bool shouldRender = false;
        bool isPrepared = false;
        bool forceDisableInstancing = false;
        bool isMaterialCopy = false;

        ShieldPostprocess owner = null;

        private void Awake()
        {
            if (!isPrepared)
            {
                Prepare();
            }
        }

        protected virtual void OnDestroy()
        {
            myRenderer = null;
            myMeshFilter = null;

            CleanupMaterial();
        }

        internal void DestroyAsset(Object assetObject)
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

        internal void SetOwner(ShieldPostprocess o)
        {
            owner = o;
        }

        internal virtual void SetRenderPostprocessMaterial(Material material, bool makeCopy)
        {
            CleanupMaterial();

            isMaterialCopy = makeCopy;
            if (makeCopy)
            {
                renderPostprocessMaterial = new Material(material);
            }
            else
            {
                renderPostprocessMaterial = material;
            }

            UpdateInstancingKey();
        }

        internal virtual void CleanupMaterial()
        {
            if (renderPostprocessMaterial)
            {
                if (isMaterialCopy)
                {
                    DestroyAsset(renderPostprocessMaterial);
                }
                isMaterialCopy = false;
                renderPostprocessMaterial = null;
            }
        }

        internal virtual void Prepare()
        {
            myRenderer = GetComponent<Renderer>();
            myMeshFilter = GetComponent<MeshFilter>();

            forceDisableInstancing = false;

            Vector3 scale = transform.lossyScale;
            if (scale.x <= 0.0f || scale.y <= 0.0f || scale.z <= 0.0f)
            {
                forceDisableInstancing = true;
            }

            UpdateInstancingKey();

            isPrepared = true;
        }

        internal virtual void RegisterInstancedProperties(ShieldPostprocess.RenderInstanceInfo rii)
        {

        }

        private void OnDisable()
        {
            shouldRender = false;
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                CleanupMaterial();
                Prepare();
            }
#endif
        }

        protected virtual void UpdateInstancingKey()
        {
            renderInstancingKey = null;
            renderInstancingHash = 0;

            if (myMeshFilter)
            {
                if (myMeshFilter.sharedMesh == null)
                {
                    return;
                }

                renderInstancingKey = myMeshFilter.sharedMesh.GetInstanceID().ToString();
            }

            if (renderPostprocessMaterial != null)
            {
                renderInstancingKey += "_" + renderPostprocessMaterial.name;
            }
            else if (myRenderer.sharedMaterial != null)
            {
                renderInstancingKey += "_" + myRenderer.sharedMaterial.name;
            }

            if (renderInstancingKey != null && renderInstancingKey.Length > 0)
            {
                renderInstancingHash = renderInstancingKey.GetHashCode();
            }

            if (owner)
            {
                owner.OnInstancingKeyUpdate(this, renderInstancingHash);
            }
        }

        public int GetInstancingKey()
        {
            return renderInstancingHash;
        }

        public Matrix4x4 GetRenderMatrix()
        {
            return transform.localToWorldMatrix;
        }

        public bool IsInstancingSupported()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return false;
            }
#endif
            return !forceDisableInstancing && myMeshFilter != null && renderPostprocessMaterial.enableInstancing;
        }

        internal bool WillRender()
        {
            return shouldRender;
        }

        internal virtual void AddRenderInstanceInfo(ShieldPostprocess.RenderInstanceInfo info)
        {
            info.mesh = myMeshFilter.sharedMesh;
            info.material = renderPostprocessMaterial ? renderPostprocessMaterial : myRenderer.sharedMaterial;
            info.matrices[info.matricesCount] = GetRenderMatrix();
            info.matricesCount++;
        }

        internal void RenderNoInstancing(CommandBuffer cmd)
        {
            cmd.DrawRenderer(myRenderer, myRenderer.sharedMaterial, 0 ,1);
        }

        private void OnBecameVisible()
        {
            if (owner)
            {
                if (!isPrepared)
                {
                    Prepare();
                }

                if (renderInstancingHash == 0)
                {
                    UpdateInstancingKey();
                }

                owner.AddToRenderList(this);
                shouldRender = true;
            }
        }

        private void OnBecameInvisible()
        {
            shouldRender = false;
        }
    }
}
