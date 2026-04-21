using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NGAME.Editor
{ 
    [CustomEditor(typeof(RoomNode))]
    public class RoomNodeInspector : UnityEditor.Editor
    {
        private UnityEditor.Editor _cachedEditor;

        public void OnEnable()
        {
            //reset cache
            _cachedEditor = null;
        }
        public override void OnInspectorGUI()
        {
            RoomNode currentRoomNode = (RoomNode)target;

            if(currentRoomNode != null && _cachedEditor == null)
            {
                //no saved editor for the scriptable object
                //_cachedEditor = Editor.CreateEditor(currentRoomNode.Room);
            }

            // include the main class's inspector
            base.OnInspectorGUI();

            // add the scriptable object

            //_cachedEditor.DrawDefaultInspector();
        }
    }
    
}