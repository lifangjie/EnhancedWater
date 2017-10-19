using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using WaterVersionTest;

namespace _Scripts.Editor {
    [CustomEditor(typeof(Ocean))]
    [CanEditMultipleObjects]
    public class OceanEditor : UnityEditor.Editor {
        SerializedProperty _sampleCount;
        SerializedProperty _wind;
        SerializedProperty _size;
        SerializedProperty _length;
        SerializedProperty _cycle;
        SerializedProperty _layerCount;

        void OnEnable() {
            // Setup the SerializedProperties.
            _sampleCount = serializedObject.FindProperty("SampleCount");
            _wind = serializedObject.FindProperty("Wind");
            _size = serializedObject.FindProperty("Size");
            _length = serializedObject.FindProperty("Length");
            _cycle = serializedObject.FindProperty("Cycle");
            _layerCount = serializedObject.FindProperty("LayerCount");
        }


        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_size);
            EditorGUILayout.PropertyField(_length);
            EditorGUILayout.PropertyField(_wind);
            EditorGUILayout.PropertyField(_sampleCount);
            EditorGUILayout.PropertyField(_cycle);
            EditorGUILayout.PropertyField(_layerCount);
            serializedObject.ApplyModifiedProperties();

            var ocean = (Ocean) target;
            if (GUILayout.Button("Update mesh")) {
                //ocean.GetComponent<MeshFilter>().sharedMesh = ocean.UpdateMesh();
                var mesh = ocean.UpdateMesh();
                if (!AssetDatabase.Contains(mesh)) {
                    AssetDatabase.CreateAsset(mesh, "Assets/Resources/mesh.asset");
                }
                //ocean.UpdateMesh();
            }
            if (GUILayout.Button("Bake into texture")) {
                EditorApplication.update -= DisplayProgressBar;
                EditorApplication.update += DisplayProgressBar;
                ocean.StopAllCoroutines();
                ocean.UpdateMesh();
                ocean.StartCoroutine(ocean.BakeIntoTexture());
            }
        }


        private void DisplayProgressBar() {
            var ocean = (Ocean) target;
            if (ocean.Progress < _sampleCount.intValue || ocean.Baking) {
                EditorUtility.DisplayProgressBar("Bake into texture", "Bake fft waves into textures",
                    ocean.Progress * 1f / _sampleCount.intValue);
            } else {
                EditorUtility.ClearProgressBar();
                EditorApplication.update -= DisplayProgressBar;
            }
        }
    }
}