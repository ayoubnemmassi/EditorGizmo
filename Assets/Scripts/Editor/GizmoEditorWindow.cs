using System;
using System.Collections.Generic;
using System.Linq;
using technical.test.editor;
using UnityEditor;
using UnityEngine;

public class GizmoEditorWindow : EditorWindow
{

    private SceneGizmoAsset _gizmoAsset;
    private Gizmo _selectedGizmo;
    private int _selectedGizmoIndex;
    private static SceneGizmoAsset _selectedGizmoAsset;
   
    private List<Gizmo> _undoList = new List<Gizmo>();
    private int _undoIndex = -1;

    private static GizmoEditorWindow _window;

    bool isEditing = false;


    [MenuItem("Window/Custom/Show Gizmos")]
    public static void ShowGizmoEditor()
    {
        // Check if a SceneGizmoAsset is selected
        _selectedGizmoAsset = Selection.activeObject as SceneGizmoAsset;
        if (_selectedGizmoAsset != null)
        {
            // Show the Gizmo Editor window with the selected SceneGizmoAsset
            ShowWindow(_selectedGizmoAsset);
        }
        else
        {
            // Show the Gizmo Editor window without a selected SceneGizmoAsset
            GizmoEditorWindow window = (GizmoEditorWindow)EditorWindow.GetWindow(typeof(GizmoEditorWindow));
            window.Show();
        }
    }
    public static void ShowWindow(SceneGizmoAsset gizmoAsset)
    {
        _window = (GizmoEditorWindow)EditorWindow.GetWindow(typeof(GizmoEditorWindow));
        _window._gizmoAsset = gizmoAsset;
        _window.Show();
    }

    // Reference to the previously selected SceneGizmoAsset
    private SceneGizmoAsset _previousSelectedGizmoAsset;

    private void Update()
    {
        // Check if the selected SceneGizmoAsset has changed
        var selectedGizmoAsset = Selection.activeObject as SceneGizmoAsset;

        if (selectedGizmoAsset != _previousSelectedGizmoAsset)
        {
            _previousSelectedGizmoAsset = selectedGizmoAsset;

            if (selectedGizmoAsset != null)
            {
                _selectedGizmoIndex = -1;
            }
        }
    }

    private void OnGUI()
    {
        //Check if a SceneGizmoAsset has been assigned to the _gizmoAsset variable
        if (_gizmoAsset == null)
        {
            //If not display a message to the user
            EditorGUILayout.LabelField("No Scene Gizmo Asset selected.");
            return;
        }
        //Begin a horizontal layout to display the labels for the text and Position fields
        EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
        //Display the "Text" label
        EditorGUILayout.LabelField("Text", EditorStyles.boldLabel);
        //Display the "Position" label
        EditorGUILayout.LabelField("Position", EditorStyles.boldLabel);

        EditorGUILayout.EndHorizontal();


        for (int i = 0; i < _gizmoAsset._gizmos.Length; i++)
        {
            //Assign the current Gizmo 
            var gizmo = _gizmoAsset._gizmos[i];
            //Begin a horizontal layout to display the Gizmo fields
            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            // var isSelected = _selectedGizmo.Equals(gizmo);

            //Check if the current Gizmo is the selected Gizmo and if we are editing it 
            if (_selectedGizmoIndex == i && isEditing)
            {
                //If so, change the background color of the fields to red
                GUI.backgroundColor = Color.red;

                //Display a text field for the Gizmo Name
                _selectedGizmo.Name = EditorGUILayout.TextField(gizmo.Name);

                //Create a new SerializedObject for the Gizmo class
                SerializedObject gizmoNameObject = new SerializedObject(_gizmoAsset);
                SerializedProperty nameProperty = gizmoNameObject.FindProperty("_gizmos.Array.data[" + _selectedGizmoIndex + "].Name");

                nameProperty.stringValue = _selectedGizmo.Name;

                EditorGUILayout.Space(20);
                _selectedGizmo.Position = EditorGUILayout.Vector3Field("", _selectedGizmo.Position);

                // Create a new SerializedObject for the Gizmo class
                SerializedObject gizmoPosObject = new SerializedObject(_gizmoAsset);

                // Create a SerializedProperty for the Position property
                SerializedProperty positionProperty = gizmoPosObject.FindProperty("_gizmos.Array.data[" + _selectedGizmoIndex + "].Position");

                
                positionProperty.vector3Value = _selectedGizmo.Position;

                //check if the values changed if so add it to the undo list
                if (_selectedGizmo.Position != gizmo.Position)
                {
                    _undoList.RemoveRange(_undoIndex + 1, _undoList.Count - _undoIndex - 1);                 
                    _undoList.Add(new Gizmo(_selectedGizmo.Name, _selectedGizmo.Position));
                    _undoIndex++;
                }
              
                //Apply all the modifications
                gizmoNameObject.ApplyModifiedProperties();
                gizmoPosObject.ApplyModifiedProperties();
                GUI.backgroundColor = Color.white;
            }
            else
            {
                //if we dont edit just display the info in read-only mode 

                EditorGUILayout.TextField(gizmo.Name);
                EditorGUILayout.Space(20);
                EditorGUILayout.Vector3Field("", gizmo.Position);
            }



            if (GUILayout.Button("Edit"))
            {
                _selectedGizmoIndex = i;
                _selectedGizmo = gizmo;
                isEditing = !isEditing;
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
        }


        if (!_selectedGizmo.Equals(default(Gizmo)))
        {
            if (GUILayout.Button("Reset Position"))
            {
                ResetGizmoPosition();
            }
            if (GUILayout.Button("Delete"))
            {
                DeleteGizmo();
            }
        }

        if (GUILayout.Button("Undo"))
        {
            Undo();
        }
        if (GUILayout.Button("Redo"))
        {
            Redo();
        }

    }


    private void SaveChanges()
    {
        // Save the changes to the asset
        EditorUtility.SetDirty(_gizmoAsset);
        AssetDatabase.SaveAssets();
    }


    private bool _isDragging;
    private bool _saveTheUndo;
    private void OnSceneGUI(SceneView sceneView)
    {
        if (_gizmoAsset == null)
        {
            return;
        }

        Handles.color = Color.white;
        for (int i = 0; i < _gizmoAsset._gizmos.Length; i++)
        {

            var gizmo = _gizmoAsset._gizmos[i];
            Handles.SphereHandleCap(0, gizmo.Position, Quaternion.identity, 0.2f, EventType.Repaint);
            var textPos = sceneView.camera.WorldToScreenPoint(gizmo.Position);
            if (textPos.z > 0)
            {
                var textSize = GUI.skin.label.CalcSize(new GUIContent(gizmo.Name));
                var textRect = new Rect(textPos.x - textSize.x / 2, textPos.y, textSize.x, textSize.y);
                GUI.contentColor = Color.black;
                GUI.Label(textRect, gizmo.Name);

                var handleSize = HandleUtility.GetHandleSize(gizmo.Position) * 0.05f;
                if (Handles.Button(gizmo.Position, Quaternion.identity, handleSize, handleSize, Handles.SphereHandleCap))
                {
                    _selectedGizmo = gizmo;
                    SceneView.RepaintAll();
                }

                if (_selectedGizmo.Equals(gizmo))
                {

                    if (Event.current.type == EventType.MouseDrag)
                    {
                        
                        _isDragging = true;
                    }
                    else if (Event.current.type == EventType.MouseUp)
                    {
                       
                        _isDragging = false;
                    }
                    var newPos = Handles.PositionHandle(gizmo.Position, Quaternion.identity);
                    if (newPos != gizmo.Position)
                    {
                        if (_saveTheUndo)
                        {
                           
                            _undoList.RemoveRange(_undoIndex + 1, _undoList.Count - _undoIndex - 1);
                            _undoList.Add(new Gizmo(gizmo.Name, gizmo.Position));
                            _undoIndex++;
                        }

                        _gizmoAsset._gizmos[i] = new Gizmo(gizmo.Name, newPos);
                        _selectedGizmo = _gizmoAsset._gizmos[i];

                    }
                }
            }
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Reset Position"), false, ResetGizmoPosition);
            menu.AddItem(new GUIContent("Delete"), false, DeleteGizmo);
            menu.ShowAsContext();
            Event.current.Use();
        }

        if (_isDragging)
        {

            EditorApplication.update += CheckForEndDrag;
            
            _saveTheUndo = false;
        }
        else
        {
           
            _saveTheUndo = true;

        }
    }

    private void CheckForEndDrag()
    {
        if (Event.current != null && Event.current.type == EventType.MouseUp)
        {
           
            _isDragging = false;
            SaveChanges();
         

        }
    }

    private void OnSelectionChange()
    {
        var selectedObject = Selection.activeObject;
        if (selectedObject is SceneGizmoAsset)
        {
            _gizmoAsset = (SceneGizmoAsset)selectedObject;
            _selectedGizmo = new Gizmo();
            Repaint();
        }
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void ResetGizmoPosition()
    {
        if (_undoList.Count > 0)
        {
            var OriginalPosition = _undoList[0];
            var currentGizmo = _gizmoAsset._gizmos.FirstOrDefault(g => g.Equals(_selectedGizmo));

            for (int i = 0; i < _gizmoAsset._gizmos.Length; i++)
            {
                if (_gizmoAsset._gizmos[i].Equals(_selectedGizmo))
                {
                    _gizmoAsset._gizmos[i] = OriginalPosition;
                    break;
                }
            }
            _undoList.Clear();
            _undoIndex = _undoList.Count - 1;
            SaveChanges();
            SceneView.RepaintAll();
        }
    }

    private void DeleteGizmo()
    {
        var gizmoList = new List<Gizmo>(_gizmoAsset._gizmos);
        gizmoList.Remove((Gizmo)_selectedGizmo);
        _gizmoAsset._gizmos = gizmoList.ToArray();

        _selectedGizmo = new Gizmo();
        SceneView.RepaintAll();
    }

    private void Undo()
    {
        if (_undoIndex <= 0)
            return;
        _undoIndex--;
        var previousGizmo = _undoList[_undoIndex];
        for (int i = 0; i < _gizmoAsset._gizmos.Length; i++)
        {
            if (_gizmoAsset._gizmos[i].Equals(_selectedGizmo))
            {
                _gizmoAsset._gizmos[i] = previousGizmo;
                break;
            }
        }
        _selectedGizmo = previousGizmo;
        SaveChanges();
        SceneView.RepaintAll();
    }

    private void Redo()
    {
        if (_undoIndex >= _undoList.Count - 1)
            return;
        _undoIndex++;
        var nextGizmo = _undoList[_undoIndex];
        var currentGizmo = _gizmoAsset._gizmos.FirstOrDefault(g => g.Equals(_selectedGizmo));
        var currentGizmoIndex = Array.IndexOf(_gizmoAsset._gizmos, currentGizmo);
        _gizmoAsset._gizmos[currentGizmoIndex] = new Gizmo(currentGizmo.Name, nextGizmo.Position);
        _selectedGizmo = _gizmoAsset._gizmos[currentGizmoIndex];
        SaveChanges();
        SceneView.RepaintAll();
    }
    private void OnEnable()
    {

        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

}