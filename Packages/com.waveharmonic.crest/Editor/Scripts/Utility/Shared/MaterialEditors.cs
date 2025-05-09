// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace WaveHarmonic.Crest.Editor
{
    /// <summary>
    /// Adds a deprecated message to shaders.
    ///
    /// USAGE
    /// Add to bottom of Shader block:
    /// CustomEditor "Crest.ObsoleteShaderGUI"
    /// Optionally add to Properties block:
    /// [HideInInspector] _ObsoleteMessage("The additional message.", Float) = 0
    /// </summary>
    sealed class ObsoleteShaderGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
        {
            // Enable rich text in help boxes. Store original so we can revert since this might be a "hack".
            var styleRichText = GUI.skin.GetStyle("HelpBox").richText;
            GUI.skin.GetStyle("HelpBox").richText = true;

            var message = "";

            {
                var property = FindProperty("_Message", properties, propertyIsMandatory: false);
                if (property != null)
                {
                    message += property.displayName;
                }
            }

            {
                var property = FindProperty("_ObsoleteMessage", properties, propertyIsMandatory: false);
                if (property != null)
                {
                    message += "This shader is deprecated and will be removed in a future version. " + property.displayName;
                }
            }

            EditorGUILayout.HelpBox(message, MessageType.Warning);
            EditorGUILayout.Space(3f);

            // Revert skin since it persists.
            GUI.skin.GetStyle("HelpBox").richText = styleRichText;

            // Render the default GUI.
            base.OnGUI(editor, properties);
        }
    }
}
