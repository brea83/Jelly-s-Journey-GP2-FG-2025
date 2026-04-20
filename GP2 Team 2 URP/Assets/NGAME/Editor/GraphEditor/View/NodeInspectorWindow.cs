using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace NGAME.Editor
{
    [EditorWindowTitle(title = "Node Inspector")]
    public class NodeInspectorWindow : GraphViewToolWindow
    {
        private NodeInspector m_NodeInspector;

        protected override string ToolName => "Node Inspector";

        private new void OnEnable()
        {
            base.OnEnable();
            OnGraphViewChanged();
        }

        private void OnDisable()
        {
            
        }
        public void UpdateSelection(NodeView nodeView)
        {
            if (m_NodeInspector != null)
                m_NodeInspector.UpdateSelection(nodeView);
        }

        public void Repaint(NodeView nodeView)
        {
           if(m_NodeInspector != null)
                m_NodeInspector.Repaint(nodeView);
        }

        protected override void OnGraphViewChanged()
        {
            if(m_SelectedGraphView == null || m_SelectedGraphView is not RoomGraphView)
            {
                m_NodeInspector = null;
                return;
            }
            RoomGraphView graphView = m_SelectedGraphView as RoomGraphView;
            m_NodeInspector = graphView.GetOrCreateNodeInspector();
            m_NodeInspector.IsWindowed = true;
            base.rootVisualElement.Add(m_NodeInspector);
        }

        protected override void OnGraphViewChanging()
        {
            if (m_NodeInspector != null)
            {
                if (m_SelectedGraphView != null && m_SelectedGraphView is RoomGraphView graph)
                {
                    graph.ReleaseNodeInspector(m_NodeInspector);
                }

                base.rootVisualElement.Remove(m_NodeInspector);
                m_NodeInspector = null;
            }
        }

        
    }
}
