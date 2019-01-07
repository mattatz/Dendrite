using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace VolumeSampler
{

    public class VolumeSamplerWindow : EditorWindow
    {

        protected string path = "Assets/Volume.asset";
        protected int resolution = 16;
        protected float radius = 1f;
        Object mesh;

        [MenuItem("Window/VolumeSampler")]
        static void Init()
        {
            var window = EditorWindow.GetWindow(typeof(VolumeSamplerWindow));
            window.Show();
        }

        protected void OnGUI()
        {
            const float headerSize = 120f;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Source mesh file", GUILayout.Width(headerSize));
                mesh = EditorGUILayout.ObjectField(mesh, typeof(Mesh), true);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Grid resolution", GUILayout.Width(headerSize));
                resolution = EditorGUILayout.IntField(resolution);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Sample distance", GUILayout.Width(headerSize));
                radius = EditorGUILayout.FloatField(radius);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Output path", GUILayout.Width(headerSize));
                path = EditorGUILayout.TextField(path);
            }

            GUI.enabled = (mesh != null);
            if (GUILayout.Button("Build"))
            {
                Build(mesh as Mesh, path, resolution, radius);
            }
        }

        protected void Build(
            Mesh mesh,
            string path,
            int resolution, float radius
        )
        {
            var volume = VolumeSampler.Sample(mesh, resolution, radius);
            AssetDatabase.CreateAsset(volume, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

    }

}


