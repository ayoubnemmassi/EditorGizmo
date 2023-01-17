
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace technical.test.editor
{
    [InitializeOnLoad]
    public class InitialSceneLoader
    {
        static string KEY_SHOWN_INITIAL_SCENE_EDITOR_PREFS_NAME = "FirstLaunch.shownInitialScene";

        static InitialSceneLoader()
        {
            EditorApplication.delayCall += OpenInitialSceneAutomatically;
        }

        private static void OpenInitialSceneAutomatically()
        {
            if (!EditorPrefs.GetBool(KEY_SHOWN_INITIAL_SCENE_EDITOR_PREFS_NAME, false))
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(0);
                Debug.Log("Open initial scene : " + scenePath);
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                EditorPrefs.SetBool(KEY_SHOWN_INITIAL_SCENE_EDITOR_PREFS_NAME, true);
            }
        }
    }
}
