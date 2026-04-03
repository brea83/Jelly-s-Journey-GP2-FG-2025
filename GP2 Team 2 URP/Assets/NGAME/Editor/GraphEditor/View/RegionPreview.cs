using System.Collections.Generic;
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

        public RegionPreview()
        {
            m_Container = new VisualElement();
            m_Container.style.flexShrink = 0;
            
            m_Container.style.backgroundColor = Color.white;
            m_Container.style.alignItems = Align.Center;
            m_Container.style.justifyContent = Justify.Center;

            m_RegionBG = new VisualElement();
            m_RegionBG.style.flexShrink = 0;
            m_RegionBG.style.flexGrow = 0;
            //m_RegionBG.text = "Test";
            m_RegionBG.style.backgroundColor = Color.blue;

            m_Container.Add(m_RegionBG);
            m_Container.RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
            //m_RegionBG.style.position = Position.Absolute;
            //SetHeight(50.0f);

        }

        public RegionPreview(Vector2 newRegionSize)
        {
            m_RegionSize = newRegionSize;
            m_RegionAspectRatio = newRegionSize.x / newRegionSize.y;

            m_Container = new VisualElement();
            m_Container.style.flexShrink = 0;

            m_Container.style.backgroundColor = Color.white;
            m_Container.style.alignItems = Align.Center;
            m_Container.style.justifyContent = Justify.Center;

            m_RegionBG = new VisualElement();
            m_RegionBG.style.flexShrink = 0;
            m_RegionBG.style.flexGrow = 0;
            //m_RegionBG.text = "Test";
            m_RegionBG.style.backgroundColor = Color.blue;

            m_Container.Add(m_RegionBG);
            m_Container.RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
            //m_RegionBG.style.position = Position.Absolute;
            //SetHeight(50.0f);

        }
        private void GeometryChangedCallback(GeometryChangedEvent evt)
        {
            m_Container.UnregisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
            // Do what you need to do here, as geometry should be calculated.
            SetHeight(100.0f);
        }
        public void SetHeight(float height)
        {
            m_Container.style.height = height;
            Vector2 newSize = new Vector2(m_Container.resolvedStyle.width, height);
            m_ContainerSize = newSize;
            m_ContainerAspectRatio = newSize.x / newSize.y;

            ResizeToFit();
        }
            
        private void ResizeToFit()
        {
            //80 % of actual container size
            Vector2 paddedContainerSize = m_ContainerSize * 0.8f;

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
