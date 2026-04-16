using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NGAME.Editor
{
    public class ConnectionPort : UnityEditor.Experimental.GraphView.Port
    {
        public Action<ConnectionPort> OnConnectionPortConnect;
        public Action<ConnectionPort> OnConnectionPortDisconnect;

        public NodeView nodeView => GetFirstAncestorOfType<NodeView>();
        //public ConnectionContainer GetConnectionContainer => GetFirstAncestorOfType<ConnectionContainer>();

        // TODO update this with better constraints once custom edge class is made?
        public static ConnectionPort Create<TEdge>(Orientation orientation, Direction direction, Capacity capacity, Type type) where TEdge : Edge, new()
        {
            CustomEdgeConnectorListener listener = new();
            ConnectionPort port = new ConnectionPort(orientation, direction, capacity, type)
            {
                m_EdgeConnector = new EdgeConnector<TEdge>(listener)
            };

            port.AddManipulator(port.m_EdgeConnector);
            port.portName = direction == Direction.Input ? "In" : "Out";
            return port;
        }

        protected ConnectionPort(Orientation portOrientation, Direction portDirection, 
            Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        { }

        protected class CustomEdgeConnectorListener : IEdgeConnectorListener
        {
            protected GraphViewChange m_GraphViewChange;
            protected List<Edge> m_EdgesToCreate;
            protected List<GraphElement> m_EdgesToDelete;

            public CustomEdgeConnectorListener()
            {
                m_EdgesToCreate = new();
                m_EdgesToDelete = new();
                m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                m_EdgesToCreate.Clear();
                m_EdgesToCreate.Add(edge);
                m_EdgesToDelete.Clear();
                if (edge.input.capacity == Capacity.Single)
                {
                    foreach (Edge connection in edge.input.connections)
                    {
                        if (connection != edge)
                        {
                            m_EdgesToDelete.Add(connection);
                        }
                    }
                }

                if (edge.output.capacity == Capacity.Single)
                {
                    foreach (Edge connection2 in edge.output.connections)
                    {
                        if (connection2 != edge)
                        {
                            m_EdgesToDelete.Add(connection2);
                        }
                    }
                }

                if (m_EdgesToDelete.Count > 0)
                {
                    graphView.DeleteElements(m_EdgesToDelete);
                }

                List<Edge> edgesToCreate = m_EdgesToCreate;
                if (graphView.graphViewChanged != null)
                {
                    edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
                }

                foreach (Edge item in edgesToCreate)
                {
                    Debug.Log("CustomEdgeConnectorListener: Edge created between " + edge.input.portName + ", and " + edge.output.portName);
                    graphView.AddElement(item);
                    edge.input.Connect(item);
                    edge.output.Connect(item);
                }
            }

            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
                
            }
        }

        public override void OnStartEdgeDragging()
        {
            base.OnStartEdgeDragging();
        }

        public override void OnStopEdgeDragging()
        {
            base.OnStopEdgeDragging();
        }

        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            Debug.Log("Port.Connect on" + this.portName + ": Edge created between " + edge.input.portName + ", and " + edge.output.portName);
        }

        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
        }

        public override void DisconnectAll()
        {
            base.DisconnectAll();
        }

        public void OrientCap(bool bInRow, bool bOnLeftOfNode)
        {
            if (bInRow)
            {
                SetLabelFront();
            }
            else if (bOnLeftOfNode)
            {
                if (direction == Direction.Input)
                    SetLabelFront();
                else
                    SetCapFront();
            }
            else
            {
                if (direction == Direction.Input)
                    SetCapFront();
                else
                    SetLabelFront();
            }
        }

        public void SetCapFront()
        {
            m_ConnectorBox.BringToFront();
        }

        public void SetLabelFront()
        {
            m_ConnectorText.BringToFront();
        }
    }
}
