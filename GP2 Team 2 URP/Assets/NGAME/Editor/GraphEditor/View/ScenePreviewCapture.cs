using UnityEditor;
using UnityEngine;

namespace NGAME.Editor
{

    public enum SupportedAspects
    {
        Aspect4by3 = 1,
        Aspect5by4 = 2,
        Aspect16by10 = 3,
        Aspect16by9 = 4
    };
    [System.Serializable, CreateAssetMenu(fileName = "NpcSpawnTest", menuName = "TEST/NpcSpawnTest")]
    public class ScenePreviewCapture : ScriptableObject
    {
        
        public string scenePath;

        public SupportedAspects AspectRatioEnum = SupportedAspects.Aspect16by9;
        public Vector2 AspectRatio = new Vector2(1.0f, 1.0f);
        public Vector3 CameraPosition = new Vector3(0.0f, 20.0f, 0.0f);
        public float OrothoHeight = 5.0f;
        public int RenderTextureHeight = 1080;
    }
}
