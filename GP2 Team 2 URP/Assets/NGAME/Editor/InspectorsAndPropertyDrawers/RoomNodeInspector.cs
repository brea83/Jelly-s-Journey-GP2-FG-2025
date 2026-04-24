using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NGAME.Editor
{
    [CustomEditor(typeof(RoomNode))]
    public class RoomNodeInspector : UnityEditor.Editor
    {
        //public override void OnInspectorGUI()
        //{
        //    serializedObject.Update();
        //    serializedObject.ApplyModifiedProperties();
        //}
        public override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();
            VisualElement inspector = new();

            PropertyField script = new(serializedObject.FindProperty("m_Script"));
            script.SetEnabled(false);
            inspector.Add(script);

            Label title = new();
            title.name = "Title";
            title.Bind(serializedObject);
            title.bindingPath = serializedObject.FindProperty("m_Name").propertyPath;
            inspector.Add(title);

            //if (m_InspectorUXML != null)
            //{
            //    VisualElement uxmlContent = m_InspectorUXML.CloneTree();
            //    inspector.Add(uxmlContent);
            //}

            PropertyField guidField = new PropertyField(serializedObject.FindProperty("m_Guid"), "Node Guid");
            guidField.Bind(serializedObject);
            guidField.SetEnabled(false);

            PropertyField isStartNode = new PropertyField(serializedObject.FindProperty("_isStartNode"));
            isStartNode.Bind(serializedObject);
            isStartNode.SetEnabled(false);

            PropertyField sceneData = new PropertyField(serializedObject.FindProperty("SceneData"));
            sceneData.Bind(serializedObject);
            sceneData.SetEnabled(false);

            PropertyField waves = new PropertyField(serializedObject.FindProperty("Waves"));
            waves.Bind(serializedObject);
            waves.name = "WavesField";

            Foldout debugInfo = new();
            debugInfo.text = "Debug Info";

            PropertyField position = new PropertyField(serializedObject.FindProperty("m_Position"));
            position.Bind(serializedObject);
            position.SetEnabled(false);

            PropertyField outgoingEdges = new PropertyField(serializedObject.FindProperty("OutgoingEdges"));
            outgoingEdges.Bind(serializedObject);
            outgoingEdges.SetEnabled(false);

            PropertyField incomingEdges = new PropertyField(serializedObject.FindProperty("IncomingEdges"));
            incomingEdges.Bind(serializedObject);
            incomingEdges.SetEnabled(false);

            debugInfo.Add(guidField);
            debugInfo.Add(isStartNode);
            debugInfo.Add(position);
            debugInfo.Add(sceneData);
            debugInfo.Add(outgoingEdges);
            debugInfo.Add(incomingEdges);
            debugInfo.value = false;

            inspector.Add(waves);
            inspector.Add(debugInfo);

            serializedObject.ApplyModifiedProperties();
            return inspector;
        }

        
    }
}
