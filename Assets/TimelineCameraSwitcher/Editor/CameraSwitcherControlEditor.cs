
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraSwitcherControl))]//拡張するクラスを指定
public class CameraSwitcherControlEditor : Editor {

    /// <summary>
    /// InspectorのGUIを更新
    /// </summary>
    public override void OnInspectorGUI(){
        //元のInspector部分を表示
        
        CameraSwitcherControl cameraSwitcherControl = target as CameraSwitcherControl;
        

        if (cameraSwitcherControl.cameraSwitcherSettings == null)
        {
            if (GUILayout.Button("Create settings"))
            {
                var baseSettings = Resources.Load<CameraSwitcherSettings>("CameraSwitcherSetting");
                var setting = CreateInstance<CameraSwitcherSettings>();
                var inputCameraA = new RenderTexture(1920, 1080,24);
                var inputCameraB = new RenderTexture(1920, 1080,24);
                var compoMat = new Material(baseSettings.material);
                if (!AssetDatabase.IsValidFolder("Assets/CameraSwitcherSettings"))
                {
                    string path = "Assets/CameraSwitcherSettings/";

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    AssetDatabase.ImportAsset(path);
                }

                var exportPath = "Assets/CameraSwitcherSettings/cameraSwitcherSettings.asset";
                var exportPath_rtA = "Assets/CameraSwitcherSettings/inputCameraA.asset";
                var exportPath_rtB = "Assets/CameraSwitcherSettings/inputCameraB.asset";
                var exportPath_compositeMat = "Assets/CameraSwitcherSettings/composite.mat";
                var asset = (CameraSwitcherSettings)AssetDatabase.LoadAssetAtPath(exportPath, typeof(CameraSwitcherSettings));
                var rtA = (CameraSwitcherSettings)AssetDatabase.LoadAssetAtPath(exportPath, typeof(RenderTexture));
                var rtB = (CameraSwitcherSettings)AssetDatabase.LoadAssetAtPath(exportPath, typeof(RenderTexture));
                var mat = (CameraSwitcherSettings)AssetDatabase.LoadAssetAtPath(exportPath_compositeMat, typeof(Material));

                
                if (asset == null){
                    // 指定のパスにファイルが存在しない場合は新規作成
                    AssetDatabase.CreateAsset(setting, exportPath);
                } else {
                    // 指定のパスに既に同名のファイルが存在する場合は更新
                    EditorUtility.CopySerialized(setting, asset);
                    AssetDatabase.SaveAssets();
                }
                AssetDatabase.Refresh();

                cameraSwitcherControl.cameraSwitcherSettings = AssetDatabase.LoadAssetAtPath<CameraSwitcherSettings>(exportPath);
                
                
                
                if (rtA == null){
                    AssetDatabase.CreateAsset(inputCameraA, exportPath_rtA);
                    
                } else {
                    // 指定のパスに既に同名のファイルが存在する場合は更新
                   EditorUtility.CopySerialized(inputCameraA, rtA);
                    AssetDatabase.SaveAssets();
                   
                }
                AssetDatabase.Refresh();

                
                cameraSwitcherControl.cameraSwitcherSettings.renderTextureA = AssetDatabase.LoadAssetAtPath<RenderTexture>(exportPath_rtA);
                
                
                
                
                if (rtB == null){
                    // 指定のパスにファイルが存在しない場合は新規作成
                    AssetDatabase.CreateAsset(inputCameraB, exportPath_rtB);
                } else {
                  

                    EditorUtility.CopySerialized(inputCameraB, rtB);
                    AssetDatabase.SaveAssets();
                }
                AssetDatabase.Refresh();

                cameraSwitcherControl.cameraSwitcherSettings.renderTextureB = AssetDatabase.LoadAssetAtPath<RenderTexture>(exportPath_rtB);
                
                
                
                if (mat == null){
                    // 指定のパスにファイルが存在しない場合は新規作成
                    AssetDatabase.CreateAsset(compoMat, exportPath_compositeMat);
                } else {
                  

                    EditorUtility.CopySerialized(compoMat, mat);
                    AssetDatabase.SaveAssets();
                }
                AssetDatabase.Refresh();

                cameraSwitcherControl.cameraSwitcherSettings.material = AssetDatabase.LoadAssetAtPath<Material>(exportPath_compositeMat);
            }
        }
        else
        {
            base.OnInspectorGUI ();
        }
       

        //targetを変換して対象を取得
       

        //PrivateMethodを実行する用のボタン
       

    }

    private void CreateMaterialAsset()
    {
        
    }

}  