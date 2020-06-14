#if UNITY_EDITOR
#if UNITY_2019_1_OR_NEWER
#define UI_USE_FOLDOUT_HEADER_2019
#endif

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;

namespace VLB
{
    public class EditorCommon : Editor
    {
        protected virtual void OnEnable()
        {
            m_CurrentFoldableHeader = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
    #if UNITY_2019_3_OR_NEWER
            // no vertical space in 2019.3 looks better
    #else
            EditorGUILayout.Separator();
    #endif
        }

        protected static void Header(string label)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        protected bool HeaderFoldableBegin(string label)
        {
            return HeaderFoldableBegin(new GUIContent(label));
        }

        protected bool HeaderFoldableBegin(GUIContent label)
        {
            var uniqueString = this.ToString() + label.text;

            // this function can be called twice with the same label when we open the color/gradient picker tool
            Debug.Assert(m_CurrentFoldableHeader == null || m_CurrentFoldableHeader == uniqueString,
                string.Format("Trying to call HeaderFoldableBegin({0}) while a previous one '{1}' is not closed", label.text, m_CurrentFoldableHeader));

            if (ms_StyleHeaderFoldable == null)
            {
                ms_StyleHeaderFoldable = new GUIStyle(EditorStyles.foldout);
                ms_StyleHeaderFoldable.fontStyle = FontStyle.Bold;
            }

            m_CurrentFoldableHeader = uniqueString;

            bool folded = ms_FoldedHeaders.Contains(uniqueString);

#if UI_USE_FOLDOUT_HEADER_2019
            folded = !EditorGUILayout.BeginFoldoutHeaderGroup(!folded, label);
#elif UNITY_5_5_OR_NEWER
            folded = !EditorGUILayout.Foldout(!folded, label, toggleOnLabelClick: true, style: ms_StyleHeaderFoldable);
#else
            folded = !EditorGUILayout.Foldout(!folded, label, ms_StyleHeaderFoldable);
#endif

            if (folded) ms_FoldedHeaders.Add(uniqueString);
            else ms_FoldedHeaders.Remove(uniqueString);

            return !folded;
        }

        protected void HeaderFoldableEnd()
        {
            Debug.Assert(m_CurrentFoldableHeader != null, "Trying to call HeaderFoldableEnd() but there is no header opened");
            m_CurrentFoldableHeader = null;

#if UI_USE_FOLDOUT_HEADER_2019
            EditorGUILayout.EndFoldoutHeaderGroup();
    #if UNITY_2019_3_OR_NEWER
            EditorGUILayout.Separator();
    #endif
#else
            DrawLineSeparator();
#endif
        }

        protected static void DrawLineSeparator()
        {
            DrawLineSeparator(Color.grey, 1, 10);
        }

        static void DrawLineSeparator(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));

            r.x = 0;
            r.width = EditorGUIUtility.currentViewWidth;

            r.y += padding / 2;
            r.height = thickness;

            EditorGUI.DrawRect(r, color);
        }

        protected SerializedProperty FindProperty<T, TValue>(Expression<Func<T, TValue>> expr)
        {
            Debug.Assert(serializedObject != null);
            return serializedObject.FindProperty(ReflectionUtils.GetFieldPath(expr));
        }

        protected void ButtonOpenConfig(bool miniButton = true)
        {
            bool buttonClicked = false;
            if (miniButton) buttonClicked = GUILayout.Button(EditorStrings.ButtonOpenGlobalConfig, EditorStyles.miniButton);
            else            buttonClicked = GUILayout.Button(EditorStrings.ButtonOpenGlobalConfig);

            if (buttonClicked)
                Config.EditorSelectInstance();
        }

        string m_CurrentFoldableHeader = null;

        static HashSet<String> ms_FoldedHeaders = new HashSet<String>();
        static GUIStyle ms_StyleHeaderFoldable = null;
    }
}
#endif // UNITY_EDITOR

