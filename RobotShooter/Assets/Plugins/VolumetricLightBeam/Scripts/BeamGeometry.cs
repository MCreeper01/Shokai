#if DEBUG
//#define DEBUG_SHOW_MESH_NORMALS
#endif
#define FORCE_CURRENT_CAMERA_DEPTH_TEXTURE_MODE

#if UNITY_2018_1_OR_NEWER
#define VLB_SRP_SUPPORT // Comment this to disable SRP support
#endif

#if VLB_SRP_SUPPORT
#if UNITY_2019_1_OR_NEWER
using AliasCurrentPipeline = UnityEngine.Rendering.RenderPipelineManager;
using AliasCameraEvents = UnityEngine.Rendering.RenderPipelineManager;
#else
using AliasCurrentPipeline = UnityEngine.Experimental.Rendering.RenderPipelineManager;
using AliasCameraEvents = UnityEngine.Experimental.Rendering.RenderPipeline;
#endif // UNITY_2019_1_OR_NEWER
#endif // VLB_SRP_SUPPORT

using UnityEngine;
using System.Collections;

#pragma warning disable 0429, 0162 // Unreachable expression code detected (because of Noise3D.isSupported on mobile)

namespace VLB
{
    [AddComponentMenu("")] // hide it from Component search
    [ExecuteInEditMode]
    [HelpURL(Consts.HelpUrlBeam)]
    public class BeamGeometry : MonoBehaviour
    {
        VolumetricLightBeam m_Master = null;
        Matrix4x4 m_ColorGradientMatrix;
        MeshType m_CurrentMeshType = MeshType.Shared;
        Material m_CustomMaterial = null;

        public MeshRenderer meshRenderer { get; private set; }
        public MeshFilter meshFilter { get; private set; }
        public Mesh coneMesh { get; private set; }

        public bool visible
        {
            get { return meshRenderer.enabled; }
            set { meshRenderer.enabled = value; }
        }

        public int sortingLayerID
        {
            get { return meshRenderer.sortingLayerID; }
            set { meshRenderer.sortingLayerID = value; }
        }

        public int sortingOrder
        {
            get { return meshRenderer.sortingOrder; }
            set { meshRenderer.sortingOrder = value; }
        }

        float ComputeFadeOutFactor(Transform camTransform)
        {
            if (m_Master.isFadeOutEnabled)
            {
                float distanceCamToBeam = Vector3.SqrMagnitude(meshRenderer.bounds.center - camTransform.position);
                return Mathf.InverseLerp(m_Master.fadeOutEnd * m_Master.fadeOutEnd, m_Master.fadeOutBegin * m_Master.fadeOutBegin, distanceCamToBeam);
            }
            else
            {
                return 1.0f;
            }
        }

        IEnumerator CoUpdateFadeOut()
        {
            while (m_Master.isFadeOutEnabled)
            {
                ComputeFadeOutFactor();
                yield return null;
            }

            SetFadeOutFactorProp(1.0f);
        }

        void ComputeFadeOutFactor()
        {
            var camTransform = Config.Instance.fadeOutCameraTransform;
            if (camTransform)
            {
                float fadeOutFactor = ComputeFadeOutFactor(camTransform);

                if (fadeOutFactor > 0)
                {
                    meshRenderer.enabled = true;
                    SetFadeOutFactorProp(fadeOutFactor);
                }
                else
                {
                    meshRenderer.enabled = false;
                }
            }
            else
            {
                SetFadeOutFactorProp(1.0f);
            }
        }

        void SetFadeOutFactorProp(float value)
        {
            MaterialChangeStart();
            SetMaterialProp(ShaderProperties.FadeOutFactor, value);
            MaterialChangeStop();
        }

        void RestartFadeOutCoroutine()
        {
        #if UNITY_EDITOR
            if (Application.isPlaying)
        #endif
            {
                StopAllCoroutines();

                if (m_Master && m_Master.isFadeOutEnabled)
                {
                    StartCoroutine(CoUpdateFadeOut());
                }
            }
        }

        void Start()
        {
            // Handle copy / paste the LightBeam in Editor
            if (!m_Master)
                DestroyImmediate(gameObject);
        }

        void OnDestroy()
        {
            if (m_CustomMaterial)
            {
                DestroyImmediate(m_CustomMaterial);
                m_CustomMaterial = null;
            }
        }

#if VLB_SRP_SUPPORT
        static bool IsUsingCustomRenderPipeline()
        {
            return AliasCurrentPipeline.currentPipeline != null || UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null;
        }

        void OnDisable()
        {
            if (IsUsingCustomRenderPipeline())
                AliasCameraEvents.beginCameraRendering -= OnBeginCameraRendering;
        }

        public static bool isCustomRenderPipelineSupported { get { return true; } }
#else
        public static bool isCustomRenderPipelineSupported { get { return false; } }
#endif

        void OnEnable()
        {
            // When a GAO is disabled, all its coroutines are killed, so renable them on OnEnable.
            RestartFadeOutCoroutine();

#if VLB_SRP_SUPPORT
            if (IsUsingCustomRenderPipeline())
                AliasCameraEvents.beginCameraRendering += OnBeginCameraRendering;
#endif
        }

        public void Initialize(VolumetricLightBeam master)
        {
            var hideFlags = Consts.ProceduralObjectsHideFlags;
            m_Master = master;

            transform.SetParent(master.transform, false);

            meshRenderer = gameObject.GetOrAddComponent<MeshRenderer>();
            meshRenderer.hideFlags = hideFlags;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
#if UNITY_5_4_OR_NEWER
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
#else
            meshRenderer.useLightProbes = false;
#endif

            if (Config.Instance.actualRenderingMode != RenderingMode.GPUInstancing)
            {
                m_CustomMaterial = MaterialManager.NewMaterial(gpuInstanced:false);
                ApplyMaterial();
            }

            if (SortingLayer.IsValid(m_Master.sortingLayerID))
                sortingLayerID = m_Master.sortingLayerID;
            else
                Debug.LogError(string.Format("Beam '{0}' has an invalid sortingLayerID ({1}). Please fix it by setting a valid layer.", Utils.GetPath(m_Master.transform), m_Master.sortingLayerID));

            sortingOrder = m_Master.sortingOrder;

            meshFilter = gameObject.GetOrAddComponent<MeshFilter>();
            meshFilter.hideFlags = hideFlags;

            gameObject.hideFlags = hideFlags;

#if UNITY_EDITOR
            // Apply the same static flags to the BeamGeometry than the VLB GAO
            var flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(master.gameObject);
            flags &= ~(UnityEditor.StaticEditorFlags.ContributeGI); // remove the Lightmap static flag since it will generate error messages when selecting the BeamGeometry GAO in the editor
            UnityEditor.GameObjectUtility.SetStaticEditorFlags(gameObject, flags);
#endif

            RestartFadeOutCoroutine();
        }

        /// <summary>
        /// Generate the cone mesh and calls UpdateMaterialAndBounds.
        /// Since this process involves recreating a new mesh, make sure to not call it at every frame during playtime.
        /// </summary>
        public void RegenerateMesh()
        {
            Debug.Assert(m_Master);

            if (Config.Instance.geometryOverrideLayer)
                gameObject.layer = Config.Instance.geometryLayerID;
            else
                gameObject.layer = m_Master.gameObject.layer;

            gameObject.tag = Config.Instance.geometryTag;

            if (coneMesh && m_CurrentMeshType == MeshType.Custom)
            {
                DestroyImmediate(coneMesh);
            }

            m_CurrentMeshType = m_Master.geomMeshType;

            switch (m_Master.geomMeshType)
            {
                case MeshType.Custom:
                    {
                        coneMesh = MeshGenerator.GenerateConeZ_Radius(1f, 1f, 1f, m_Master.geomCustomSides, m_Master.geomCustomSegments, m_Master.geomCap, Config.Instance.useSinglePassShader);
                        coneMesh.hideFlags = Consts.ProceduralObjectsHideFlags;
                        meshFilter.mesh = coneMesh;
                        break;
                    }
                case MeshType.Shared:
                    {
                        coneMesh = GlobalMesh.Get();
                        meshFilter.sharedMesh = coneMesh;
                        break;
                    }
                default:
                    {
                        Debug.LogError("Unsupported MeshType");
                        break;
                    }
            }

            UpdateMaterialAndBounds();
        }

        void ComputeLocalMatrix()
        {
            // In the VS, we compute the vertices so the whole beam fits into a fixed 2x2x1 box.
            // We have to apply some scaling to get the proper beam size.
            // This way we have the proper bounds without having to recompute specific bounds foreach beam.
            var maxRadius = Mathf.Max(m_Master.coneRadiusStart, m_Master.coneRadiusEnd);
            transform.localScale = new Vector3(maxRadius, maxRadius, m_Master.fallOffEnd);
        }

        bool isNoiseEnabled { get { return m_Master.isNoiseEnabled && m_Master.noiseIntensity > 0f && Noise3D.isSupported; } } // test Noise3D.isSupported the last
        bool isClippingPlaneEnabled { get { return dynamicOcclusion ? (dynamicOcclusion.planeEquationWS.normal.sqrMagnitude > 0) : false; } }
#pragma warning disable 0162
        bool isDepthBlendEnabled { get { return GpuInstancing.forceEnableDepthBlend || m_Master.depthBlendDistance > 0f; } }
#pragma warning restore 0162

        bool ApplyMaterial()
        {
            var colorGradient = MaterialManager.ColorGradient.Off;
            if (m_Master.colorMode == ColorMode.Gradient)
            {
                var precision = Utils.GetFloatPackingPrecision();
                colorGradient = precision == Utils.FloatPackingPrecision.High ? MaterialManager.ColorGradient.MatrixHigh : MaterialManager.ColorGradient.MatrixLow;
            }

            Debug.Assert((int)BlendingMode.Additive == (int)MaterialManager.BlendingMode.Additive);
            Debug.Assert((int)BlendingMode.SoftAdditive == (int)MaterialManager.BlendingMode.SoftAdditive);
            Debug.Assert((int)BlendingMode.TraditionalTransparency == (int)MaterialManager.BlendingMode.TraditionalTransparency);

            var staticProps = new MaterialManager.StaticProperties
            {
                blendingMode = (MaterialManager.BlendingMode)m_Master.blendingMode,
                noise3D = isNoiseEnabled ? MaterialManager.Noise3D.On : MaterialManager.Noise3D.Off,
                depthBlend = isDepthBlendEnabled ? MaterialManager.DepthBlend.On : MaterialManager.DepthBlend.Off,
                colorGradient = colorGradient,
                clippingPlane = isClippingPlaneEnabled ? MaterialManager.ClippingPlane.On : MaterialManager.ClippingPlane.Off
            };

            Material mat = null;
            if (Config.Instance.actualRenderingMode != RenderingMode.GPUInstancing)
            {
                mat = m_CustomMaterial;
                if(mat)
                    staticProps.ApplyToMaterial(mat);
            }
            else
            {
                mat = MaterialManager.GetInstancedMaterial(m_Master._INTERNAL_InstancedMaterialGroupID, staticProps);
            }

            meshRenderer.material = mat;
            return mat != null;
        }

        void SetMaterialProp(int nameID, float value)
        {
            if (m_CustomMaterial)
                m_CustomMaterial.SetFloat(nameID, value);
            else
                MaterialManager.materialPropertyBlock.SetFloat(nameID, value);
        }

        void SetMaterialProp(int nameID, Vector4 value)
        {
            if (m_CustomMaterial)
                m_CustomMaterial.SetVector(nameID, value);
            else
                MaterialManager.materialPropertyBlock.SetVector(nameID, value);
        }

        void SetMaterialProp(int nameID, Color value)
        {
            if (m_CustomMaterial)
                m_CustomMaterial.SetColor(nameID, value);
            else
                MaterialManager.materialPropertyBlock.SetColor(nameID, value);
        }

        void SetMaterialProp(int nameID, Matrix4x4 value)
        {
            if (m_CustomMaterial)
                m_CustomMaterial.SetMatrix(nameID, value);
            else
                MaterialManager.materialPropertyBlock.SetMatrix(nameID, value);
        }

        void MaterialChangeStart()
        {
            if (m_CustomMaterial == null)
                meshRenderer.GetPropertyBlock(MaterialManager.materialPropertyBlock);
        }

        void MaterialChangeStop()
        {
            if (m_CustomMaterial == null)
                meshRenderer.SetPropertyBlock(MaterialManager.materialPropertyBlock);
        }

        void SendMaterialClippingPlaneProp()
        {
            Debug.Assert(dynamicOcclusion != null);
            var planeWS = dynamicOcclusion.planeEquationWS;
            SetMaterialProp(ShaderProperties.ClippingPlaneWS, new Vector4(planeWS.normal.x, planeWS.normal.y, planeWS.normal.z, planeWS.distance));
            SetMaterialProp(ShaderProperties.ClippingPlaneProps, dynamicOcclusion.fadeDistanceToPlane);
        }

        public void UpdateMaterialAndBounds()
        {
            Debug.Assert(m_Master);

            if (ApplyMaterial() == false)
            {
                return;
            }

            MaterialChangeStart();
            {
                if (isClippingPlaneEnabled && m_CustomMaterial == null)
                {
                    SendMaterialClippingPlaneProp();
                }

                float slopeRad = (m_Master.coneAngle * Mathf.Deg2Rad) / 2; // use coneAngle (instead of spotAngle) which is more correct with the geometry
                SetMaterialProp(ShaderProperties.ConeSlopeCosSin, new Vector2(Mathf.Cos(slopeRad), Mathf.Sin(slopeRad)));

                // kMinRadius and kMinApexOffset prevents artifacts when fresnel computation is done in the vertex shader
                const float kMinRadius = 0.0001f;
                var coneRadius = new Vector2(Mathf.Max(m_Master.coneRadiusStart, kMinRadius), Mathf.Max(m_Master.coneRadiusEnd, kMinRadius));
                SetMaterialProp(ShaderProperties.ConeRadius, coneRadius);

                const float kMinApexOffset = 0.0001f;
                float nonNullApex = Mathf.Sign(m_Master.coneApexOffsetZ) * Mathf.Max(Mathf.Abs(m_Master.coneApexOffsetZ), kMinApexOffset);
                SetMaterialProp(ShaderProperties.ConeApexOffsetZ, nonNullApex);

                if (m_Master.colorMode == ColorMode.Flat)
                {
                    SetMaterialProp(ShaderProperties.ColorFlat, m_Master.color);
                }
                else
                {
                    var precision = Utils.GetFloatPackingPrecision();
                    m_ColorGradientMatrix = m_Master.colorGradient.SampleInMatrix((int)precision);
                    // pass the gradient matrix in OnWillRenderObject()
                }

                SetMaterialProp(ShaderProperties.AlphaInside, m_Master.intensityInside);
                SetMaterialProp(ShaderProperties.AlphaOutside, m_Master.intensityOutside);
                SetMaterialProp(ShaderProperties.AttenuationLerpLinearQuad, m_Master.attenuationLerpLinearQuad);
                SetMaterialProp(ShaderProperties.DistanceFadeStart, m_Master.fallOffStart);
                SetMaterialProp(ShaderProperties.DistanceFadeEnd, m_Master.fallOffEnd);
                SetMaterialProp(ShaderProperties.DistanceCamClipping, m_Master.cameraClippingDistance);
                SetMaterialProp(ShaderProperties.FresnelPow, Mathf.Max(0.001f, m_Master.fresnelPow)); // no pow 0, otherwise will generate inf fresnel and issues on iOS
                SetMaterialProp(ShaderProperties.GlareBehind, m_Master.glareBehind);
                SetMaterialProp(ShaderProperties.GlareFrontal, m_Master.glareFrontal);
                SetMaterialProp(ShaderProperties.DrawCap, m_Master.geomCap ? 1 : 0);

                if (isDepthBlendEnabled)
                {
                    SetMaterialProp(ShaderProperties.DepthBlendDistance, m_Master.depthBlendDistance);
                }

                if (isNoiseEnabled)
                {
                    Noise3D.LoadIfNeeded();
                    SetMaterialProp(ShaderProperties.NoiseLocal, new Vector4(
                        m_Master.noiseVelocityLocal.x,
                        m_Master.noiseVelocityLocal.y,
                        m_Master.noiseVelocityLocal.z,
                        m_Master.noiseScaleLocal));

                    SetMaterialProp(ShaderProperties.NoiseParam, new Vector4(
                        m_Master.noiseIntensity,
                        m_Master.noiseVelocityUseGlobal ? 1f : 0f,
                        m_Master.noiseScaleUseGlobal ? 1f : 0f,
                        m_Master.noiseMode == NoiseMode.WorldSpace ? 0f : 1f));
                }

                ComputeLocalMatrix(); // compute matrix before sending it to the shader

#if VLB_SRP_SUPPORT
                if (IsUsingCustomRenderPipeline() && Config.Instance.actualRenderingMode == RenderingMode.GPUInstancing)
                {
                    SetMaterialProp(ShaderProperties.LocalToWorldMatrix, transform.localToWorldMatrix);
                    SetMaterialProp(ShaderProperties.WorldToLocalMatrix, transform.worldToLocalMatrix);
                }
#endif
            }
            MaterialChangeStop();

#if DEBUG_SHOW_MESH_NORMALS
            for (int vertexInd = 0; vertexInd < coneMesh.vertexCount; vertexInd++)
            {
                var vertex = coneMesh.vertices[vertexInd];

                // apply modification done inside VS
                vertex.x *= Mathf.Lerp(coneRadius.x, coneRadius.y, vertex.z);
                vertex.y *= Mathf.Lerp(coneRadius.x, coneRadius.y, vertex.z);
                vertex.z *= m_Master.fallOffEnd;

                var cosSinFlat = new Vector2(vertex.x, vertex.y).normalized;
                var normal = new Vector3(cosSinFlat.x * Mathf.Cos(slopeRad), cosSinFlat.y * Mathf.Cos(slopeRad), -Mathf.Sin(slopeRad)).normalized;

                vertex = transform.TransformPoint(vertex);
                normal = transform.TransformDirection(normal);
                Debug.DrawRay(vertex, normal * 0.25f);
            }
#endif
        }

        DynamicOcclusion _dynamicOcclusion;

        public DynamicOcclusion dynamicOcclusion
        {
            get { return _dynamicOcclusion; }
            set
            {
                _dynamicOcclusion = value;
                if (m_CustomMaterial)
                {
                    bool hasDynOcclusion = _dynamicOcclusion != null;
                    m_CustomMaterial.SetKeywordEnabled("VLB_CLIPPING_PLANE", hasDynOcclusion);
                    if (hasDynOcclusion)
                        SendMaterialClippingPlaneProp();
                }
                else
                    UpdateMaterialAndBounds();
            }
        }

#if VLB_SRP_SUPPORT
    #if UNITY_2019_1_OR_NEWER
        void OnBeginCameraRendering(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam)
    #else
        void OnBeginCameraRendering(Camera cam)
    #endif
        {
            UpdateCameraRelatedProperties(cam);
        }
#endif

        void OnWillRenderObject()
        {
#if VLB_SRP_SUPPORT
            if (!IsUsingCustomRenderPipeline())
#endif
            {
                var cam = Camera.current;
                if (cam != null)
                    UpdateCameraRelatedProperties(cam);
            }
        }

        static bool IsEditorCamera(Camera cam)
        {
#if UNITY_EDITOR
            var sceneView = UnityEditor.SceneView.currentDrawingSceneView;
            if (sceneView)
            {
                return cam == sceneView.camera;
            }
#endif
            return false;
        }

        void UpdateCameraRelatedProperties(Camera cam)
        {
            if (cam && m_Master)
            {
                MaterialChangeStart();
                {
                    var camPosOS = m_Master.transform.InverseTransformPoint(cam.transform.position);
                    var camForwardVectorOSN = transform.InverseTransformDirection(cam.transform.forward).normalized;
                    float camIsInsideBeamFactor = cam.orthographic ? -1f : m_Master.GetInsideBeamFactorFromObjectSpacePos(camPosOS);
                    SetMaterialProp(ShaderProperties.CameraParams, new Vector4(camForwardVectorOSN.x, camForwardVectorOSN.y, camForwardVectorOSN.z, camIsInsideBeamFactor));

#if UNITY_2017_3_OR_NEWER && VLB_SRP_SUPPORT // ScalableBufferManager introduced in Unity 2017.3
                    if (IsUsingCustomRenderPipeline())
                    {
                        var bufferSize = cam.allowDynamicResolution ? new Vector2(ScalableBufferManager.widthScaleFactor, ScalableBufferManager.heightScaleFactor) : Vector2.one;
                        SetMaterialProp(ShaderProperties.CameraBufferSizeSRP, bufferSize);
                    }
#endif

                    if (m_Master.colorMode == ColorMode.Gradient)
                    {
                        // Send the gradient matrix every frame since it's not a shader's property
                        SetMaterialProp(ShaderProperties.ColorGradientMatrix, m_ColorGradientMatrix);
                    }
                }
                MaterialChangeStop();

#if FORCE_CURRENT_CAMERA_DEPTH_TEXTURE_MODE
                if (m_Master.depthBlendDistance > 0f)
                    cam.depthTextureMode |= DepthTextureMode.Depth;
#endif
            }
        }
    }
}
