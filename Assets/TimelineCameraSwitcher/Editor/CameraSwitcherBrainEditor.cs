using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

[CustomEditor(typeof(CameraSwitcherBrain))]//拡張するクラスを指定
public class CameraSwitcherBrainEditor : Editor {

    static string targetPackage;
    static EmbedRequest Request;
    static ListRequest LRequest;
    static void GetPackageName()
    {
        // まず、インストールされたパッケージの名前を取得します
        LRequest = Client.List();
        EditorApplication.update += LProgress;
    }

    static void LProgress()
    {
        if (LRequest.IsCompleted)
        {
            if (LRequest.Status == StatusCode.Success)
            {
                foreach (var package in LRequest.Result)
                {
                    
                    if (package.isDirectDependency && package.source
                        != PackageSource.BuiltIn && package.source
                        != PackageSource.Embedded)
                    {
                        targetPackage = package.name;
                        break;
                    }
                }

            }
            else
                Debug.Log(LRequest.Error.message);

            EditorApplication.update -= LProgress;

            Embed(targetPackage);

        }
    }

    static void Embed(string inTarget)
    {
        // プロジェクトにパッケージを埋め込みます
        Debug.Log("Embed('" + inTarget + "') called");
        Request = Client.Embed(inTarget);
        EditorApplication.update += Progress;

    }

    static void Progress()
    {
        if (Request.IsCompleted)
        {
            if (Request.Status == StatusCode.Success)
                Debug.Log("Embedded: " + Request.Result.packageId);
            else if (Request.Status >= StatusCode.Failure)
                Debug.Log(Request.Error.message);

            EditorApplication.update -= Progress;
        }
    }
    /// <summary>
    /// InspectorのGUIを更新
    /// </summary>
    public override void OnInspectorGUI(){
        
        
        CameraSwitcherBrain cameraSwitcherBrain = target as CameraSwitcherBrain;
        
        // if (cameraSwitcherControl.cameraSwitcherSettings == null)
        // {
            
            if (GUILayout.Button("Create settings"))
            {
                
                
                
                var filePath = EditorUtility.SaveFilePanelInProject(
                    "Create Camera Switcher Settings",
                    "NewCameraSwitcherSettings",
                    "asset",
                    "Please enter a file name to save the texture to");

                Debug.Log(filePath);
                if (string.IsNullOrEmpty(filePath))
                    return;
                
                
                
                var exportPath = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(filePath))
                    return;
                Debug.Log(exportPath);
                
           
                
                var baseName = Path.GetFileName(filePath).Split(".").First();
                Debug.Log(baseName);
                
                // return;
                var baseSettings = Resources.Load<CameraSwitcherSettings>("TimelineCameraSwitcherResources/CameraSwitcherSetting");
                var setting = CreateInstance<CameraSwitcherSettings>();
                
                var inputCameraA = new RenderTexture(1920, 1080,24);
                var inputCameraB = new RenderTexture(1920, 1080,24);
                var compoMat = new Material(baseSettings.material);
                

                var exportPath_settings = filePath;
                var exportPath_rtA = exportPath+"/"+baseName+"_inputCameraA.asset";
                var exportPath_rtB = exportPath+"/"+baseName+"inputCameraB.asset";
                var exportPath_compositeMat = exportPath+"/"+baseName+"_composite.mat";
                // var exportPath_compositShader = "Assets/CameraSwitcherSettings/CameraSwitcherCompositeShader.shadergraph";

                var asset = (CameraSwitcherSettings)AssetDatabase.LoadAssetAtPath(exportPath_settings, typeof(CameraSwitcherSettings));
                var rtA = (RenderTexture)AssetDatabase.LoadAssetAtPath(exportPath_rtA, typeof(RenderTexture));
                var rtB = (RenderTexture)AssetDatabase.LoadAssetAtPath(exportPath_rtB, typeof(RenderTexture));
                var mat = (Material)AssetDatabase.LoadAssetAtPath(exportPath_compositeMat, typeof(Material));
                // var shaderGraph = (CameraSwitcherSettings)AssetDatabase.LoadAssetAtPath(exportPath_compositShader, typeof(Shader));
                
                if (asset == null){
                    // 指定のパスにファイルが存在しない場合は新規作成
                    AssetDatabase.CreateAsset(setting, exportPath_settings);
                } else {
                    // 指定のパスに既に同名のファイルが存在する場合は更新
                    EditorUtility.CopySerialized(setting, asset);
                    AssetDatabase.SaveAssets();
                }
                AssetDatabase.Refresh();

                cameraSwitcherBrain.cameraSwitcherSettings = AssetDatabase.LoadAssetAtPath<CameraSwitcherSettings>(exportPath_settings);
                
                
                
                if (rtA == null){
                    AssetDatabase.CreateAsset(inputCameraA, exportPath_rtA);
                    
                } else {
                    // 指定のパスに既に同名のファイルが存在する場合は更新
                   EditorUtility.CopySerialized(inputCameraA, rtA);
                    AssetDatabase.SaveAssets();
                   
                }
                AssetDatabase.Refresh();

                
                cameraSwitcherBrain.cameraSwitcherSettings.renderTextureA = AssetDatabase.LoadAssetAtPath<RenderTexture>(exportPath_rtA);
                
                
                
                
                if (rtB == null){
                    // 指定のパスにファイルが存在しない場合は新規作成
                    AssetDatabase.CreateAsset(inputCameraB, exportPath_rtB);
                } else {
                  

                    EditorUtility.CopySerialized(inputCameraB, rtB);
                    AssetDatabase.SaveAssets();
                }
                AssetDatabase.Refresh();

                cameraSwitcherBrain.cameraSwitcherSettings.renderTextureB = AssetDatabase.LoadAssetAtPath<RenderTexture>(exportPath_rtB);
                
                
                
                if (mat == null){
                    // 指定のパスにファイルが存在しない場合は新規作成
                    AssetDatabase.CreateAsset(compoMat, exportPath_compositeMat);
                } else {
                  

                    EditorUtility.CopySerialized(compoMat, mat);
                    AssetDatabase.SaveAssets();
                }
                AssetDatabase.Refresh();
                
                cameraSwitcherBrain.cameraSwitcherSettings.material = AssetDatabase.LoadAssetAtPath<Material>(exportPath_compositeMat);

                // cameraSwitcherBrain.material = cameraSwitcherBrain.cameraSwitcherSettings.material;

            }
       //  }
       //  else
       //  {
       //      
       //  }   
       base.OnInspectorGUI ();

        //targetを変換して対象を取得
       

        //PrivateMethodを実行する用のボタン
       

    }

    private void CreateMaterialAsset()
    {
        
    }

}  


