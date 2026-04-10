using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NGAME.Editor
{
    public class RegionPreview
    {
        public VisualElement Container { get => m_Container; }

        private VisualElement m_Container = new();
        private VisualElement m_RegionBG = new();

        //private Vector2 m_RegionMinPoint = Vector2.zero;
        //private Vector2 m_RegionMaxPoint = Vector2.one;
        private Vector2 m_RegionSize = Vector2.one;
        private float m_RegionAspectRatio = 1.0f;

        private Vector2 m_ContainerSize = Vector2.one;
        private float m_ContainerAspectRatio = 1.0f;

        private List<VisualElement> m_DoorMarkers = new();

        private NodeView m_ParentNode;
        private Sprite m_PreviewImage;

        public RegionPreview(NodeView parentNode)
        {
            m_ParentNode = parentNode;
            CreateUiElements();
        }
        public RegionPreview(Vector2 newRegionSize)
        {
            m_RegionSize = newRegionSize;
            m_RegionAspectRatio = newRegionSize.x / newRegionSize.y;
            CreateUiElements();
        }

        public RegionPreview()
        {
            CreateUiElements();
        }

        private void CreateUiElements()
        {
            m_Container = new VisualElement();
            m_Container.style.flexShrink = 0;
            m_Container.AddToClassList("PreviewBackground");
            //m_Container.style.backgroundColor = Color.navyBlue;
            m_Container.style.alignItems = Align.Center;
            m_Container.style.justifyContent = Justify.Center;

            
            SetPreviewBG();

            m_Container.RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
            
        }

        private void SetPreviewBG()
        {
            if (m_RegionBG != null)
                m_RegionBG.RemoveFromHierarchy();
            m_RegionBG = new VisualElement();
            m_RegionBG.style.flexShrink = 0;
            m_RegionBG.style.flexGrow = 0;

            if (m_ParentNode == null || m_ParentNode.Node == null)
            {
                m_RegionBG.style.backgroundColor = Color.magenta;
            }
            else if (m_ParentNode.CurrentSceneConnections == null)
            {
                m_RegionBG.style.backgroundColor = Color.blue;
            }
            else
            {
                string guid = m_ParentNode.CurrentSceneConnections.SceneGuid;

                Texture2D texture = null;
                m_ParentNode.m_RoomGraphView.ScenePreviewLookup.TryGetValue(guid, out texture);
                if (texture != null)
                {
                    //Rect rect = new Rect(0, 0, 100, 100);
                    //Vector2 pivot = Vector2.zero;

                    //m_PreviewImage = Sprite.Create(texture, rect, pivot);
                    m_RegionBG.style.backgroundImage = new StyleBackground(texture);
                    SceneData data = m_ParentNode.m_RoomGraphView.SceneLookup[guid];
                    m_RegionSize = data.Bounds.GetWidthAndHeight();
                    m_RegionAspectRatio = data.Bounds.AspectRatio;
                }
            }
            m_Container.Add(m_RegionBG);
        }

        public void UpdateBounds()
        {
            if(m_ParentNode == null || m_ParentNode.Node == null || m_ParentNode.CurrentSceneConnections == null)
            {
                return;
            }

            string guid = m_ParentNode.CurrentSceneConnections.SceneGuid;

            if (!m_ParentNode.m_RoomGraphView.SceneLookup.ContainsKey(guid))
            {
                return;
            }

            SceneData data = m_ParentNode.m_RoomGraphView.SceneLookup[guid];
            m_RegionSize = data.Bounds.GetWidthAndHeight();
            m_RegionAspectRatio = data.Bounds.AspectRatio;
            SetPreviewBG();
            EditorApplication.delayCall += DelayedSetHeight;
        }

        private void GeometryChangedCallback(GeometryChangedEvent evt)
        {
            m_Container.UnregisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
            // Do what you need to do here, as geometry should be calculated.
            SetHeight(300.0f);
        }

        private void DelayedSetHeight()
        {
            EditorApplication.delayCall -= DelayedSetHeight;
            SetHeight(300.0f);
        }

        public void SetHeight(float height)
        {
            m_Container.style.height = height;
            m_Container.style.width = height;
            Vector2 newSize = new Vector2(/*m_Container.resolvedStyle.width*/ height, height);
            m_ContainerSize = newSize;
            m_ContainerAspectRatio = newSize.x / newSize.y;

            ResizeToFit();
        }
            
        private void ResizeToFit()
        {
            //95 % of actual container size
            Vector2 paddedContainerSize = m_ContainerSize;// * 0.95f;

            if (m_ContainerAspectRatio > m_RegionAspectRatio)
            {
                m_RegionBG.style.width = m_RegionSize.x * (paddedContainerSize.y / m_RegionSize.y);
                m_RegionBG.style.height = paddedContainerSize.y;
            }
            else
            {
                m_RegionBG.style.width = paddedContainerSize.x;
                m_RegionBG.style.height = m_RegionSize.y * (paddedContainerSize.x / m_RegionSize.x);
            }
        }

        public void SetRegionSize(Vector2 newSize)
        {
            m_RegionSize = newSize;
            m_RegionAspectRatio = newSize.x / newSize.y;

            ResizeToFit();
        }

    }
}
