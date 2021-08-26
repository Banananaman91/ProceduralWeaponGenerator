using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Editor;
using UnityEditor;
using UnityEngine;
using WeaponGenerator.WeaponAssetRarity;

public class ThumbnailCreator : EditorWindow
{
    [SerializeField] private GameObject[] _assets = new GameObject[0];
    private int _size = 64;
    private int _assetToView;

    [MenuItem("Assets/Create/Weapon Generator/Thumbnail Creator", false, 0)]
    private static void Init()
    {
        // Get existing open window or if none, make a new one:
        ThumbnailCreator window = (ThumbnailCreator)GetWindow(typeof(ThumbnailCreator));
        window.Show();
    }

    EditorGUISplitView verticalSplitView = new EditorGUISplitView (EditorGUISplitView.Direction.Vertical);
    private void OnGUI()
    {
        verticalSplitView.Resizable = false;
        verticalSplitView.BeginSplitView();
        if(GUILayout.Button("Generate Thumbnails")) CreateThumbnails();
        DisplayAssetsGui();
        verticalSplitView.Split();
        DisplayTexture();
        verticalSplitView.EndSplitView();
        Repaint();
    }

    private void DisplayTexture()
    {
        if (_assets.Length == 0) return;
        var texture = RuntimePreviewGenerator.GenerateModelPreview(_assets[_assetToView].transform, _size, _size);
        GUI.DrawTexture(new Rect(0, position.height / 2, _size, _size), texture);
    }

    private void DisplayAssetsGui()
    {
        if (GUILayout.Button("Next Item")) _assetToView = _assetToView == _assets.Length - 1 ? 0 : _assetToView + 1;
        else if (GUILayout.Button("Preview Item")) _assetToView = _assetToView == 0 ? _assets.Length - 1 : _assetToView - 1;
        
        _size = EditorGUILayout.IntField("Image Size", _size, GUILayout.Width(300f));
        RuntimePreviewGenerator.OrthographicMode =
            EditorGUILayout.Toggle("Orthographic Mode", RuntimePreviewGenerator.OrthographicMode);
        RuntimePreviewGenerator.Padding = EditorGUILayout.FloatField("Padding", RuntimePreviewGenerator.Padding);
        RuntimePreviewGenerator.PreviewDirection =
            EditorGUILayout.Vector3Field("Preview Direction", RuntimePreviewGenerator.PreviewDirection);
        RuntimePreviewGenerator.BackgroundColor =
            EditorGUILayout.ColorField("Background Colour", RuntimePreviewGenerator.BackgroundColor);
        RuntimePreviewGenerator.MarkTextureNonReadable =
            EditorGUILayout.Toggle("Mark non-readable", RuntimePreviewGenerator.MarkTextureNonReadable);
        var target = this;
        var so = new SerializedObject(target);

        var assets = so.FindProperty("_assets");
        EditorGUILayout.PropertyField(assets, true);
        //ensure modifications to variables by user are applied
        so.ApplyModifiedProperties();
    }

    private void CreateThumbnails()
    {
        foreach (var asset in _assets)
        {
            if (!asset) continue;
            var assetPath = AssetDatabase.GetAssetPath(asset);
            assetPath = assetPath.Replace(".prefab", "_thumbnail.asset");
            var boundaries = new Rect(0, 0, _size, _size);
            var sprite = Sprite.Create(RuntimePreviewGenerator.GenerateModelPreview(asset.transform, _size,_size), boundaries, Vector2.zero);
            AssetDatabase.CreateAsset(sprite, assetPath);
            WeaponCreatorMethods.AddThumbnailComponent(asset, sprite);
        }
    }
}
