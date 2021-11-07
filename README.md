# Unity_TimelineCameraSwitcher
[![Unity 2020.3+](https://img.shields.io/badge/Unity-2020.3+-DCD0FF.svg?logo=unity&style=for-the-badge)](https://unity3d.com/jp/unity/qa/lts-releases?version=2020.3)

> ## v0.1.3 (08/11/2021)
> 
> - [UnityPackage](https://github.com/murasaqi/Unity_TimelineCameraSwitcher/releases/tag/v0.1.3)   
<br>

![demoImage](https://github.com/murasaqi/Unity_TimelineCameraSwitcher/blob/main/Docs/bandicam%202021-06-28%2002-02-49-927_1.gif)  

UnityのTimeline上でカメラスイッチングを簡易的に行えるアーティスト向けの拡張です。    
映像やアニメーション制作において煩雑だったカメラワーク作成を大幅に簡略化することが可能です。  
Cinemasceneとの併用も可能です。

## インストールガイド  
### UnityPackage版
[UnityPackage](https://github.com/murasaqi/Unity_TimelineCameraSwitcher/releases/tag/v0.1.3)

### PackageManager版
![demoImage](https://github.com/murasaqi/Unity_TimelineCameraSwitcher/blob/main/Docs/packageInstall.png)  

UnityのPackageManagerの＋タブから
```
Add package from git URL
```
を選択し
```
https://github.com/murasaqi/Unity_TimelineCameraSwitcher.git?path=/Assets/TimelineCameraSwitcher#v0.1.3
```
を入力することで追加できます

もしくは
UnityProjectのディレクトリにあるPackages/manifest.jsonを開き  
dependenciesに一行追加してください。
```
{
"dependencies": {
    "com.murasaqi.camera_switcher_control": "https://github.com/murasaqi/Unity_TimelineCameraSwitcher.git?path=/Assets/TimelineCameraSwitcher#v0.1.3",
    ...
}
```

## 設定
<!-- ### Canvas -->
<!-- ![demoImage](https://github.com/murasaqi/Unity_TimelineCameraSwitcher/blob/main/Docs/AddTrack.gif)  
Canvasを作成しRawImage要素を作成します。 
MaterialにはShader Graph/Composite Shader をアタッチしてください。  
Example/PrefabディレクトリにPrefab化されたものを用意してあるのでSceneに配置して利用することが可能です。 -->
　　
### Timeline
![demoImage](https://github.com/murasaqi/Unity_TimelineCameraSwitcher/blob/main/Docs/AddTrack.gif)  
CameraSwitcherControlTrackを追加して、レンダリング結果を表示したいRawImage要素をドロップします。  

![demoImage](https://github.com/murasaqi/Unity_TimelineCameraSwitcher/blob/main/Docs/trackInspector.jpg)  
レンダリング時の解像度やカラーフォーマットの変更はTrackのインスペクターで可能です。

![demoImage](https://github.com/murasaqi/Unity_TimelineCameraSwitcher/blob/main/Docs/cameraDragToTrack.gif)  
あとはカメラをHierarchyからドラッグしていけばClipが作成されます。  
クリップのブレンディングでクロスフェードが可能です。  
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
