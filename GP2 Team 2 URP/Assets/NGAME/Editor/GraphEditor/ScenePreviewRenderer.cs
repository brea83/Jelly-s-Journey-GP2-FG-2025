using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Windows;

namespace NGAME.Editor
{
    public class ScenePreviewRenderer
    {
        public static Texture2D WriteTexture(Scene aScene, SceneData sceneData, int renderTextureHeight)
        {
            SceneBounds bounds = sceneData.Bounds;

            Camera camera = InitPreviewCamera(aScene, bounds);

            Texture2D result = DrawScene(bounds, camera, renderTextureHeight);
            Color[] pixels = result.GetPixels();
            MarkSpawners(sceneData, result, new Vector2Int(result.width, result.height), camera);
            result.Apply(false);
            return result;
        }
        void DelayWritingSpawnerData()
        {

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

            //textureResult.Apply(false);
            //Graphics.CopyTexture(renderTexture, textureResult);

            RenderTexture oldActive = RenderTexture.active;
            RenderTexture.active = renderTexture;
            textureResult.ReadPixels(new Rect(0,0, renderTexture.width, renderTexture.height), 0, 0);

            RenderTexture.active = oldActive;

            return textureResult;
        }

        private static void MarkSpawners(SceneData sceneData, Texture2D texture, Vector2Int actualTextureSize, Camera camera)
        {
            SceneBounds bounds = sceneData.Bounds;

            Color[] pixels = texture.GetPixels();
            //texture.
            //List<Vector2Int> newSpawnPositions = new();
            Vector3 worldMin3D = camera.ScreenToWorldPoint(new Vector3(0, 0, 10));
            Vector3 worldMax3D = camera.ScreenToWorldPoint(new Vector3(actualTextureSize.x - 1, actualTextureSize.y - 1, 10));

            Vector2 worldMin = new Vector2(worldMin3D.x, worldMin3D.z);
            Vector2 worldMax = new Vector2(worldMax3D.x, worldMax3D.z);

            Vector2 worldMaxRemaped = RemapVector2(worldMax, worldMin/*bounds.MinPoint*/, worldMax/*bounds.MaxPoint*/, new Vector2(0.0f, 0.0f), new Vector2(actualTextureSize.x - 1, actualTextureSize.y - 1));
            int indexMax = Mathf.FloorToInt(worldMaxRemaped.y) * (actualTextureSize.x) + Mathf.FloorToInt(worldMaxRemaped.x);
            if (indexMax < pixels.Length || indexMax > 0)
            {
                pixels[indexMax] = Color.red;
            }
            pixels[0] = Color.blue;

            foreach (SpawnerData spawner in sceneData.SpawnPoints)
            {
                Vector2 position = new Vector2(spawner.Position.x, spawner.Position.z);

                position = RemapVector2(position, worldMin/*bounds.MinPoint*/, worldMax/*bounds.MaxPoint*/, new Vector2(0.0f, 0.0f), new Vector2(actualTextureSize.x - 1, actualTextureSize.y -1));

                int x = Mathf.FloorToInt(position.x);
                int y = Mathf.FloorToInt(position.y);
                int index = y * (actualTextureSize.x) + x;
                if(index >= pixels.Length || index < 0)
                {
                    Debug.LogWarning("INDEX MATH WRONG");
                    return;
                }
                pixels[index] = Color.magenta;
            }
            texture.SetPixels(pixels);
            //texture.Apply(false);
        }

        public static float Remap(float input, float inputRangeMin, float inputRangeMax, float outputRangeMin, float outputRangeMax)
        {
            float t = Mathf.InverseLerp(inputRangeMin, inputRangeMax, input);
            float output = Mathf.Lerp(outputRangeMin, outputRangeMax, t);
            return output;
        }

        public static Vector2 RemapVector2(Vector2 input, Vector2 inputRangeMin, Vector2 inputRangeMax, Vector2 outputRangeMin, Vector2 outputRangeMax)
        {
            float newX = Remap(input.x, inputRangeMin.x, inputRangeMax.x, outputRangeMin.x, outputRangeMax.x);
            float newY = Remap(input.y, inputRangeMin.y, inputRangeMax.y, outputRangeMin.y, outputRangeMax.y);
            return new Vector2(newX, newY);
        }
    }
}
