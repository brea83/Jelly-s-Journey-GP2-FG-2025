using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NGAME.Editor
{
    [CustomEditor(typeof(RoomNode))]
    public class NodeViewInspector : UnityEditor.Editor
    {
        //public override void OnInspectorGUI()
        //{
        //    serializedObject.Update();
        //    serializedObject.ApplyModifiedProperties();
        //}
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement inspector = new();

            inspector.Add(new Label("Room Node"));

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

            Foldout graphData = new();
            graphData.text = "Graph Debug Info";

            PropertyField position = new PropertyField(serializedObject.FindProperty("m_Position"));
            position.Bind(serializedObject);
            position.SetEnabled(false);

            PropertyField outgoingEdges = new PropertyField(serializedObject.FindProperty("OutgoingEdges"));
            outgoingEdges.Bind(serializedObject);
            outgoingEdges.SetEnabled(false);

            PropertyField incomingEdges = new PropertyField(serializedObject.FindProperty("IncomingEdges"));
            incomingEdges.Bind(serializedObject);
            incomingEdges.SetEnabled(false);

            graphData.Add(position);
            graphData.Add(outgoingEdges);
            graphData.Add(incomingEdges);


            inspector.Add(guidField);
            inspector.Add(isStartNode);
            inspector.Add(sceneData);
            inspector.Add(waves);
            inspector.Add(graphData);
            return inspector;
        }


    }
}
