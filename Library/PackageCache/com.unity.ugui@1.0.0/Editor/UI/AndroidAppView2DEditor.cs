using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.Android.AppView;
using Unity.Collections;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using UnityEditor.Build;
using Object = UnityEngine.Object;
using UnityEditor.AssetImporters;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(AndroidAppView2D), true)]
    [CanEditMultipleObjects]
    public class AndroidAppView2DEditor : GraphicEditor
    {
        SerializedProperty m_AndroidAppViewSettings;
        GUIContent m_AndroidAppViewSettingsContent;
        string experimentalWarning;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_AndroidAppViewSettingsContent = EditorGUIUtility.TrTextContent("Android App View Settings", "Select the Android App Setting that draws on this GameObject.");
            experimentalWarning = "This feature is experimental and will be officially released in a later version.";
            m_AndroidAppViewSettings = serializedObject.FindProperty("m_AndroidAppViewSettings");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(experimentalWarning, MessageType.Warning, true);

            serializedObject.Update();

            bool settingsDirty = false;
            var currentSettings = m_AndroidAppViewSettings.objectReferenceValue as AndroidAppViewSettings;
            EditorGUI.BeginChangeCheck();
            var newSettings = EditorGUILayout.ObjectField(m_AndroidAppViewSettingsContent, currentSettings, typeof(AndroidAppViewSettings), false);
            if (EditorGUI.EndChangeCheck() && newSettings != currentSettings)
            {
                string showSettingsChangingWarningDialog = nameof(showSettingsChangingWarningDialog);
                if (newSettings != null && !InternalEditorUtility.inBatchMode && SessionState.GetBool(showSettingsChangingWarningDialog, true))
                {
                    var message = "Change Android App View Settings will also replace main texture on this GameObject!";
                    if (EditorUtility.DisplayDialog("AndroidAppViewSettings Changing Warning", $"{message} Ignore and continue? (This dialog won't appear again in this Editor session if you'll choose Yes)", "Yes", "No"))
                    {
                        SessionState.SetBool(showSettingsChangingWarningDialog, false);
                        m_AndroidAppViewSettings.objectReferenceValue = newSettings;
                        settingsDirty = true;
                    }
                }
                else
                {
                    m_AndroidAppViewSettings.objectReferenceValue = newSettings;
                    settingsDirty = true;
                }
            }
    
            AppearanceControlsGUI();
            RaycastControlsGUI();

            serializedObject.ApplyModifiedProperties();
            
            if (settingsDirty)
            {
                AndroidAppView2D androidAppView2D = target as AndroidAppView2D;
                androidAppView2D.ApplyAndroidAppViewSettings();
            }
        }

        public override bool HasPreviewGUI()
        {
            AndroidAppView2D androidAppView2D = target as AndroidAppView2D;
            if (androidAppView2D == null)
                return false;

            var outer = new Rect(0f, 0f, 1f, 1f);
            return outer.width > 0 && outer.height > 0;
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            AndroidAppView2D androidAppView2D = target as AndroidAppView2D;
            Texture tex = androidAppView2D.mainTexture;

            if (tex == null)
                return;

            var outer = new Rect(0f, 0f, 1f, 1f);
            SpriteDrawUtility.DrawSprite(tex, rect, outer, outer, androidAppView2D.canvasRenderer.GetColor());
        }

        public override string GetInfoString()
        {
            AndroidAppView2D androidAppView2D = target as AndroidAppView2D;

            // Image size Text
            string text = string.Format("AndroidAppView2D Size: {0}x{1}",
                Mathf.RoundToInt(Mathf.Abs(androidAppView2D.rectTransform.rect.width)),
                Mathf.RoundToInt(Mathf.Abs(androidAppView2D.rectTransform.rect.height)));

            return text;
        }
    }
}
