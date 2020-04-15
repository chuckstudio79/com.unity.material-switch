﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.MaterialSwitch
{

    [CustomEditor(typeof(MaterialSwitchClip))]
    public class MaterialSwitchClipEditor : Editor
    {
        bool showTextureProperties;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var palettePropertyMap = serializedObject.FindProperty("palettePropertyMap");
            for (var i = 0; i < palettePropertyMap.arraySize; i++)
            {
                var ppm = palettePropertyMap.GetArrayElementAtIndex(i);
                GUILayout.BeginVertical("box");
                EditorGUI.indentLevel --;
                EditorGUILayout.PropertyField(ppm.FindPropertyRelative("material"));
                var textureProperty = ppm.FindPropertyRelative("texture");
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(textureProperty, new GUIContent("Palette Texture"));
                if (textureProperty.objectReferenceValue != null)
                {
                    var t = textureProperty.objectReferenceValue as Texture;
                    if (!t.isReadable)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox("Texture is not marked as readable!", MessageType.Error);
                        if (GUILayout.Button("Fix"))
                            MakeTextureReadable(t);
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        var showCoords = ppm.FindPropertyRelative("showCoords");
                        EditorGUI.indentLevel += 1;
                        showCoords.boolValue = EditorGUILayout.Foldout(showCoords.boolValue, "Color Coordinates");
                        if (showCoords.boolValue)
                        {
                            GUILayout.BeginVertical("box");
                            var ccs = ppm.FindPropertyRelative("colorCoordinates");
                            for (var j = 0; j < ccs.arraySize; j++)
                            {
                                var cc = ccs.GetArrayElementAtIndex(j);
                                GUILayout.BeginVertical("box");
                                EditorGUILayout.LabelField($"Property: {cc.FindPropertyRelative("propertyName").stringValue}");
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Sampled Color");
                                var rect = GUILayoutUtility.GetRect(18, 18);
                                EditorGUI.DrawRect(rect, cc.FindPropertyRelative("sampledColor").colorValue);
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(cc.FindPropertyRelative("uv"));
                                GUI.enabled = textureProperty.objectReferenceValue != null;
                                if (GUILayout.Button("Pick"))
                                {
                                    rect = GUIUtility.GUIToScreenRect(rect);
                                    CoordPickerWindow.Open(this, ppm.FindPropertyRelative("texture").objectReferenceValue as Texture2D, cc, rect);
                                }
                                GUI.enabled = true;
                                GUILayout.EndHorizontal();
                                GUILayout.EndVertical();
                            }
                            GUILayout.EndVertical();

                        }
                    }
                }
                showTextureProperties = EditorGUILayout.Foldout(showTextureProperties, "Texture Properties");
                if (showTextureProperties)
                {
                    GUILayout.BeginVertical("box");
                    var textureProperties = ppm.FindPropertyRelative("textureProperties");
                    if (textureProperties != null)
                        for (var j = 0; j < textureProperties.arraySize; j++)
                        {
                            var tp = textureProperties.GetArrayElementAtIndex(j);
                            GUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField($"Property: {tp.FindPropertyRelative("propertyName").stringValue}");

                            EditorGUILayout.PropertyField(tp.FindPropertyRelative("originalValue"));
                            EditorGUILayout.PropertyField(tp.FindPropertyRelative("targetValue"), new GUIContent("New Value"));
                            GUILayout.EndVertical();
                        }
                    GUILayout.EndVertical();
                }

                GUILayout.EndVertical();

            }

            // EditorGUILayout.PropertyField(palettePropertyMap);

            serializedObject.ApplyModifiedProperties();
        }

        void MakeTextureReadable(Texture texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            AssetImporter.GetAtPath(path);
            var importer = (TextureImporter)TextureImporter.GetAtPath(path);
            importer.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }
}
