#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VLB
{
    [CustomEditor(typeof(DynamicOcclusion))]
    [CanEditMultipleObjects]
    public class DynamicOcclusionEditor : EditorCommon
    {
        SerializedProperty dimensions, layerMask, considerTriggers, minOccluderArea, planeAlignment, maxSurfaceDot, planeOffset, fadeDistanceToPlane, waitFrameCount, minSurfaceRatio;

        public override bool RequiresConstantRepaint() { return Application.isPlaying || DynamicOcclusion.editorRaycastAtEachFrame; }

        protected override void OnEnable()
        {
            base.OnEnable();
            DynamicOcclusion.EditorLoadPrefs();

            dimensions = FindProperty((DynamicOcclusion x) => x.dimensions);
            layerMask = FindProperty((DynamicOcclusion x) => x.layerMask);
            considerTriggers = FindProperty((DynamicOcclusion x) => x.considerTriggers);
            minOccluderArea = FindProperty((DynamicOcclusion x) => x.minOccluderArea);
            planeAlignment = FindProperty((DynamicOcclusion x) => x.planeAlignment);
            planeOffset = FindProperty((DynamicOcclusion x) => x.planeOffset);
            fadeDistanceToPlane = FindProperty((DynamicOcclusion x) => x.fadeDistanceToPlane);
            waitFrameCount = FindProperty((DynamicOcclusion x) => x.waitFrameCount);
            minSurfaceRatio = FindProperty((DynamicOcclusion x) => x.minSurfaceRatio);
            maxSurfaceDot = FindProperty((DynamicOcclusion x) => x.maxSurfaceDot);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (HeaderFoldableBegin(EditorStrings.DynOcclusionHeaderRaycasting))
            {
                dimensions.CustomEnum<OccluderDimensions>(EditorStrings.DynOcclusionDimensions, EditorStrings.DynOcclusionDimensionsEnumDescriptions);
                EditorGUILayout.PropertyField(layerMask, EditorStrings.DynOcclusionLayerMask);
                EditorGUILayout.PropertyField(considerTriggers, EditorStrings.DynOcclusionConsiderTriggers);

                if (Physics2D.queriesHitTriggers == false)
                {
                    foreach (var ent in targets)
                    {
                        var instance = ent as DynamicOcclusion;
                        if (instance && instance.dimensions == OccluderDimensions.Occluders2D && instance.considerTriggers)
                        {
                            EditorGUILayout.HelpBox(EditorStrings.DynOcclusionConsiderTriggersNoPossible, MessageType.Error);
                            break;
                        }
                    }
                }

                EditorGUILayout.PropertyField(minOccluderArea, EditorStrings.DynOcclusionMinOccluderArea);
                EditorGUILayout.PropertyField(waitFrameCount, EditorStrings.DynOcclusionWaitFrameCount);
            }

            HeaderFoldableEnd();

            if (HeaderFoldableBegin(EditorStrings.DynOcclusionHeaderOccluderSurface))
            {
                minSurfaceRatio.FloatSlider(
                    EditorStrings.DynOcclusionMinSurfaceRatio,
                    Consts.DynOcclusionMinSurfaceRatioMin, Consts.DynOcclusionMinSurfaceRatioMax,
                    (value) => value * 100f,  // conversion value to slider
                    (value) => value / 100f   // conversion slider to value
                    );

                maxSurfaceDot.FloatSlider(
                    EditorStrings.DynOcclusionMaxSurfaceDot,
                    Consts.DynOcclusionMaxSurfaceAngleMin, Consts.DynOcclusionMaxSurfaceAngleMax,
                    (value) => Mathf.Acos(value) * Mathf.Rad2Deg,   // conversion value to slider
                    (value) => Mathf.Cos(value * Mathf.Deg2Rad)     // conversion slider to value
                    );
            }

            HeaderFoldableEnd();

            if (HeaderFoldableBegin(EditorStrings.DynOcclusionHeaderClippingPlane))
            {
                EditorGUILayout.PropertyField(planeAlignment, EditorStrings.DynOcclusionPlaneAlignment);
                EditorGUILayout.PropertyField(planeOffset, EditorStrings.DynOcclusionPlaneOffset);
                EditorGUILayout.PropertyField(fadeDistanceToPlane, EditorStrings.DynOcclusionFadeDistanceToPlane);
            }

            HeaderFoldableEnd();

            if (HeaderFoldableBegin(EditorStrings.DynOcclusionHeaderEditorDebug))
            {
                EditorGUI.BeginChangeCheck();
                DynamicOcclusion.editorShowDebugPlane = EditorGUILayout.Toggle(EditorStrings.DynOcclusionEditorShowDebugPlane, DynamicOcclusion.editorShowDebugPlane);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool("VLB_DYNOCCLUSION_SHOWDEBUGPLANE", DynamicOcclusion.editorShowDebugPlane);
                    SceneView.RepaintAll();
                }

                EditorGUI.BeginChangeCheck();
                DynamicOcclusion.editorRaycastAtEachFrame = EditorGUILayout.Toggle(EditorStrings.DynOcclusionEditorRaycastAtEachFrame, DynamicOcclusion.editorRaycastAtEachFrame);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool("VLB_DYNOCCLUSION_RAYCASTINEDITOR", DynamicOcclusion.editorRaycastAtEachFrame);
                    SceneView.RepaintAll();
                }

                if (Application.isPlaying || DynamicOcclusion.editorRaycastAtEachFrame)
                {
                    if (!serializedObject.isEditingMultipleObjects)
                    {
                        var instance = (target as DynamicOcclusion);
                        Debug.Assert(instance);
                        var hit = instance.currentHit;
                        var lastFrameUpdate = instance.editorDebugData.lastFrameUpdate;

                        var occluderInfo = string.Format("Last update {0} frame(s) ago\n", Time.frameCount - lastFrameUpdate);
                        occluderInfo += (hit != null) ? string.Format("Current occluder: '{0}'\nEstimated occluder area: {1} units²", hit.name, hit.bounds.GetMaxArea2D()) : "No occluder found";
                        EditorGUILayout.HelpBox(occluderInfo, MessageType.Info);
                    }
                }
            }
            HeaderFoldableEnd();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
