using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestEditorWindow : EditorWindow
{
    [MenuItem("Window/TestEditorWindow")]
    static void Init()
    {
        TestEditorWindow window = (TestEditorWindow)EditorWindow.GetWindow(typeof(TestEditorWindow));
        window.Show();
    }
    void OnGUI()
    {
        GUILayout.Label("Hello World");
    }
}
