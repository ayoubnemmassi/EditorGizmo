using UnityEditor;
using UnityEngine;
using technical.test.editor;
[CustomEditor(typeof(SceneGizmoAsset))]
public class SceneGizmoAssetEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SceneGizmoAsset asset = (SceneGizmoAsset)target;

        if (GUILayout.Button("Open Gizmo Editor"))
        {
            GizmoEditorWindow.ShowWindow(asset);
        }
    }
}
