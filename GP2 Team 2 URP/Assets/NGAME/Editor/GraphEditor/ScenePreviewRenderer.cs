using UnityEngine;
using UnityEngine.SceneManagement;

namespace NGAME.Editor
{
    public class ScenePreviewRenderer
    {
        public static Texture2D WriteTexture(Scene aScene, SceneData sceneData, int renderTextureHeight)
        {
            SceneBounds bounds = sceneData.Bounds;

            Camera camera = InitPreviewCamera(aScene, bounds);

            return DrawScene(bounds, camera, renderTextureHeight);
        }

        private static Camera InitPreviewCamera(Scene aScene, SceneBounds bounds)
        {
            
            Camera camera = aScene.GetRootGameObjects()[0].GetComponentInChildren<Camera>();

            camera.cameraType = CameraType.Preview;
            camera.orthographic = true;

            Vector2 position2d = bounds.CenterPoint;
            Vector3 camPosition = new Vector3(position2d.x, 25.0f, position2d.y);
            Quaternion camRotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);

            camera.scene = aScene;
            camera.transform.SetPositionAndRotation(camPosition, camRotation);
            camera.aspect = bounds.AspectRatio;

            camera.orthographicSize = bounds.Height();

            return camera;
        }

        private static Texture2D DrawScene(SceneBounds bounds, Camera camera, int renderTextureHeight)
        {
            float aspectRatio = bounds.AspectRatio;
            RenderTexture renderTexture = new RenderTexture(Mathf.RoundToInt(aspectRatio * renderTextureHeight), renderTextureHeight, 16);
            camera.targetTexture = renderTexture;
            camera.Render();
            Texture2D textureResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            textureResult.Apply(false);
            Graphics.CopyTexture(renderTexture, textureResult);

            return textureResult;
        }
    }
}
