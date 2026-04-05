using NGAME.Editor;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Source - https://stackoverflow.com/q/71498153
// Posted by dw218192
// Retrieved 2026-04-05, License - CC BY-SA 4.0

[CustomEditor(typeof(ScenePreviewCapture))]
public class ScenePreviewCaptureInspector : Editor
{
    

    Camera _cam = null;
    RenderTexture _renderTexture;
    Texture2D _tex2d;
    Scene _scene;

    // preview variables
    SupportedAspects _aspectChoiceIdx;//= SupportedAspects.Aspect16by10;
    Vector2 _aspectRatio;
    float _curAspect;// = 1.0f;
    // world space (orthographicSize)
    float _worldScreenHeight;// = 5;
    int _renderTextureHeight;// = 1080;

    float ToFloat(SupportedAspects aspects)
    {
        switch (aspects)
        {
            case SupportedAspects.Aspect16by10:
                return 16 / 10f;
            case SupportedAspects.Aspect16by9:
                return 16 / 9f;
            case SupportedAspects.Aspect4by3:
                return 4 / 3f;
            case SupportedAspects.Aspect5by4:
                return 5 / 4f;
            default:
                throw new ArgumentException();
        }
    }

    void DrawRefScene()
    {
        _renderTexture = new RenderTexture(Mathf.RoundToInt(_curAspect * _renderTextureHeight), _renderTextureHeight, 16);
        _cam.targetTexture = _renderTexture;
        _cam.Render();
        _tex2d = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGBA32, false);
        _tex2d.Apply(false);
        Graphics.CopyTexture(_renderTexture, _tex2d);
    }

    Vector2 GetGUIPreviewSize()
    {
        Vector2 camSizeWorld = new Vector2(_worldScreenHeight * _curAspect, _worldScreenHeight);
        float scaleFactor = EditorGUIUtility.currentViewWidth / camSizeWorld.x;
        return new Vector2(EditorGUIUtility.currentViewWidth, scaleFactor * camSizeWorld.y);
    }

    #region Init
    void OnEnable()
    {
        void OpenSceneDelay()
        {
            EditorApplication.delayCall -= OpenSceneDelay;
            DrawRefScene();
        }
        ScenePreviewCapture scenePreviewTest = target as ScenePreviewCapture;

        //SerializedProperty aspectRatioProperty = serializedObject.FindProperty("AspectRatio");
        //_aspectChoiceIdx = (SupportedAspects)aspectRatioProperty.intValue;

        _scene = EditorSceneManager.OpenPreviewScene(scenePreviewTest.scenePath);
        InitPreviewCamera();

        SerializedProperty orthoProperty = serializedObject.FindProperty("OrothoHeight");
        _worldScreenHeight = orthoProperty.floatValue;

        SerializedProperty renderProperty = serializedObject.FindProperty("RenderTextureHeight");
        _renderTextureHeight = renderProperty.intValue;

        EditorApplication.delayCall += OpenSceneDelay;
    }

    void InitPreviewCamera()
    {
        _cam = _scene.GetRootGameObjects()[0].GetComponentInChildren<Camera>();

        _cam.cameraType = CameraType.Preview;
        _cam.orthographic = true;

        SerializedProperty positionProperty = serializedObject.FindProperty("CameraPosition");
        Vector3 previewPoint = positionProperty.vector3Value;//new Vector3(0f, 20f, 0f);
        Quaternion previewRotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
        _cam.transform.SetPositionAndRotation(previewPoint, previewRotation);
        _cam.scene = _scene;

        SerializedProperty aspectProperty = serializedObject.FindProperty("AspectRatio");
        _aspectRatio = aspectProperty.vector2Value;
        _curAspect = _aspectRatio.x / _aspectRatio.y; //ToFloat(_aspectChoiceIdx);
        _cam.aspect = _curAspect;

        SerializedProperty orthoProperty = serializedObject.FindProperty("OrothoHeight");
        _cam.orthographicSize = orthoProperty.floatValue;//_worldScreenHeight;
    }

    void OnDisable()
    {
        EditorSceneManager.ClosePreviewScene(_scene);
    }
    #endregion

    void OnCamSettingChange(ScenePreviewCapture sceneToPreview)
    {
        SerializedProperty aspectProperty = serializedObject.FindProperty("AspectRatio");
        aspectProperty.vector2Value = _aspectRatio;
        _curAspect = _aspectRatio.x / _aspectRatio.y;//ToFloat(_aspectChoiceIdx);
        _cam.aspect = _curAspect;
        _cam.orthographicSize = _worldScreenHeight;

        SerializedProperty positionProperty = serializedObject.FindProperty("CameraPosition");
        positionProperty.vector3Value = _cam.transform.position;
        SerializedProperty orthoProperty = serializedObject.FindProperty("OrothoHeight");
        orthoProperty.floatValue = _worldScreenHeight;
        //SerializedProperty renderProperty = serializedObject.FindProperty("RenderTextureHeight");
        //renderProperty.intValue = _renderTextureHeight;
        //SerializedProperty aspectRatioProperty = serializedObject.FindProperty("AspectRatio");
        //aspectRatioProperty.intValue = (int)_aspectChoiceIdx;

        EditorUtility.SetDirty(sceneToPreview);
        AssetDatabase.SaveAssetIfDirty(sceneToPreview);
        DrawRefScene();
    }

    // GUI states
    class GUIControlStates
    {
        public bool foldout = false;
    };
    GUIControlStates _guiStates = new GUIControlStates();
    public override void OnInspectorGUI()
    {
        // draw serializedObject fields
        // ....

        ScenePreviewCapture sceneToPreview = target as ScenePreviewCapture;
        SceneAsset oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneToPreview.scenePath);

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        SceneAsset newScene = EditorGUILayout.ObjectField("scene", oldScene, typeof(SceneAsset), false) as SceneAsset;

        if (EditorGUI.EndChangeCheck())
        {
            string newPath = AssetDatabase.GetAssetPath(newScene);
            SerializedProperty scenePathProperty = serializedObject.FindProperty("scenePath");
            scenePathProperty.stringValue = newPath;
            EditorSceneManager.ClosePreviewScene(_scene);

            _scene = EditorSceneManager.OpenPreviewScene(newPath);
            InitPreviewCamera();
            OnCamSettingChange(sceneToPreview);
        }

        // display options
        using (var scope = new EditorGUI.ChangeCheckScope())
        {
            SerializedProperty aspectProperty = serializedObject.FindProperty("AspectRatio");
            _aspectRatio = EditorGUILayout.Vector2Field("Aspect Ratio", _aspectRatio);

            if (scope.changed)
            {
                OnCamSettingChange(sceneToPreview);
            }
        }
        //_guiStates.foldout = EditorGUILayout.Foldout(_guiStates.foldout, "Projection Settings", true);
        //if (_guiStates.foldout)
        //{
        using (var scope = new EditorGUI.ChangeCheckScope())
        {
            Vector3 previewPoint = EditorGUILayout.Vector3Field("Camera Position", sceneToPreview.CameraPosition);

            Quaternion previewRotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            _cam.transform.SetPositionAndRotation(previewPoint, previewRotation);

            _worldScreenHeight = EditorGUILayout.FloatField("Orthographic Height", sceneToPreview.OrothoHeight);
            //_renderTextureHeight = EditorGUILayout.IntField("Render Texture Height", sceneToPreview.RenderTextureHeight);

            if (scope.changed)
            {
                OnCamSettingChange(sceneToPreview);
            }
        }
        //}

        if (_tex2d != null)
        {
            _tex2d.filterMode = FilterMode.Point;
            Vector2 sz = GetGUIPreviewSize();
            Rect r = EditorGUILayout.GetControlRect(false,
                GUILayout.Height(sz.y),
                GUILayout.ExpandHeight(false));
            EditorGUI.DrawPreviewTexture(r, _tex2d);
        }

        serializedObject.ApplyModifiedProperties();

    }
}

