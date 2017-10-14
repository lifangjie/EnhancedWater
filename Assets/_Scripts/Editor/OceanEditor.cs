using System.Threading;
using UnityEditor;
using UnityEngine;
using WaterVersionTest;

namespace _Scripts.Editor {
    [CustomEditor(typeof(Ocean))]
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

        private float _startTime;
        private float _progress;
        private float _seconds;

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_size);
            EditorGUILayout.PropertyField(_length);
            EditorGUILayout.PropertyField(_wind);
            EditorGUILayout.PropertyField(_sampleCount);
            EditorGUILayout.PropertyField(_cycle);
            EditorGUILayout.PropertyField(_layerCount);
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Bake into texture")) {
                _seconds = _size.intValue * _size.intValue * _sampleCount.intValue / 65536f;
                _startTime = Time.realtimeSinceStartup;
                EditorApplication.update -= DisplayProgressBar;
                EditorApplication.update += DisplayProgressBar;
                var ocean = (Ocean) target;
                ocean.StopAllCoroutines();
                ocean.UpdateMesh();
                ocean.StartCoroutine(ocean.BakeIntoTexture());
            }
        }

        private void DisplayProgressBar() {
            _progress = Time.realtimeSinceStartup - _startTime;
            if (_progress < _seconds && ((Ocean) target).Baking) {
                EditorUtility.DisplayProgressBar("Bake into texture", "Bake fft waves into textures",
                    _progress / _seconds);
            } else {
                EditorUtility.ClearProgressBar();
                EditorApplication.update -= DisplayProgressBar;
            }
        }
    }
}