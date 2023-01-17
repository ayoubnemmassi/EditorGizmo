using UnityEditor;
using UnityEngine;

namespace technical.test.editor
{
    [CreateAssetMenu(fileName = "Scene Gizmo Asset", menuName = "Custom/Create Scene Gizmo Asset")]
    public class SceneGizmoAsset : ScriptableObject
    {
        [SerializeField] public Gizmo[] _gizmos = default;

    
        public override string ToString()
        {
            return "Gizmo count : " + _gizmos.Length;
        }
    }

}