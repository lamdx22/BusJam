using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Hiker.UI
{
    [CustomEditor(typeof(HikerAspectFitter), true)]
    [CanEditMultipleObjects]
    public class HikerAspectRatioFitterEditor : SelfControllerEditor
    {
        SerializedProperty m_ActiveCondition;
        SerializedProperty m_AspectMode;
        SerializedProperty m_AspectRatio;

        protected virtual void OnEnable()
        {
            m_ActiveCondition = serializedObject.FindProperty("m_ActiveCondition");
            m_AspectMode = serializedObject.FindProperty("m_AspectMode");
            m_AspectRatio = serializedObject.FindProperty("m_AspectRatio");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_ActiveCondition);
            EditorGUILayout.PropertyField(m_AspectMode);
            EditorGUILayout.PropertyField(m_AspectRatio);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}

