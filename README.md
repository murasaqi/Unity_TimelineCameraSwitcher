# Unity_TimelineCameraSwitcher
[![Unity 2020.3+](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg?logo=unity)](https://unity3d.com/jp/unity/qa/lts-releases?version=2020.3)

> ## v0.1.2 (29/06/2021)
> 
> - [UnityPackage](https://github.com/murasaqi/Unity_TimelineCameraSwitcher/releases/tag/v0.1.2)   
<br>

![demoImage](https://github.com/murasaqi/Unity_TimelineCameraSwitcher/blob/main/Docs/bandicam%202021-06-28%2002-02-49-927_1.gif)  

UnityのTimeline上でカメラスイッチングを簡易的に行えるアーティスト向けの拡張です。    
映像やアニメーション制作において煩雑だったカメラワーク作成を大幅に簡略化することが可能です。  
**現状はURPのみでお使いいただけます。HDRPでもおそらく動きます。**

## インストールガイド  
### UnityPackage版
[UnityPackage](https://github.com/murasaqi/Unity_TimelineCameraSwitcher/releases/tag/v0.1.2)

### PackageManager版
[youtube](https://www.youtube.com/watch?v=IY_4eE4b-B4)
  
UnityProjectのディレクトリにあるPackages/manifest.jsonを開き  
dependenciesに一行追加してください。
```
{
"dependencies": {
    "com.murasaqi.camera_switcher_control": "https://github.com/murasaqi/Unity_TimelineCameraSwitcher.git?path=/Assets/TimelineCameraSwitcher#v0.1.2",
    ...
}
```

## 設定
### Canvas
![demoImage](https://github.com/murasaqi/Unity_TimelineCameraSwitcher/blob/main/Docs/canvas_setting.png)  
Canvasを作成しRawImage要素を作成して、Materialのみをアタッチします。  
MaterialにはShader Graph/Composite Shader をアタッチしてください。  
Example/PrefabディレクトリにPrefab化されたものを用意してあるのでSceneに配置して利用することが可能です。
　　
### Timeline
![demoImage](https://github.com/murasaqi/Unity_TimelineCameraSwitcher/blob/main/Docs/track_setting.gif)  
CameraSwitcherControlTrackを追加して、Trackの設定欄に先程用意したRawImage要素とRenderTextureを2枚設定します。  
RenderTextureはご自身で使いたい設定のモノを2枚用意してアタッチしてください。  
あとはカメラをドロップしていけばClipが作成されます。  
クリップのブレンディングでリニアなフェードが可能です。  
基本的にUIをレンダリングするカメラ以外は全部こちらに配置しておいてください。
　　
### Clip(Wiggler)
![demoImage](https://github.com/murasaqi/Unity_TimelineCameraSwitcher/blob/main/Docs/clip_setting.gif)  
Clipごとに手ブレを追加することが可能です。

## License
MIT.
商用非商用に関わらずご自由にお使いください。


## バグなどの報告
[https://twitter.com/murasaki_0606](https://twitter.com/murasaki_0606)
TwitterもしくはIssueにて報告してくれるとありがたいです。
