using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
namespace NGAME.Editor
{
    
    public class NodeInspector : GraphElement
    {
        private UnityEditor.Editor _editor;
        private NodeView m_CachedNode = null;

        private VisualElement m_MainContainer;

        private VisualElement m_Root;

        private Label m_TitleLabel;

        private Label m_SubTitleLabel;

        private ScrollView m_ScrollView;

        private VisualElement m_ContentContainer;

        private VisualElement m_HeaderItem;

        private Button m_AddButton;

        private bool m_Scrollable = true;

        private Dragger m_Dragger;

        private GraphView m_GraphView;

        internal static readonly string StyleSheetPath = "StyleSheets/GraphView/Blackboard.uss";

        private bool m_Windowed;

        public NodeInspector(GraphView associatedGraphView = null) : base()
        {
            m_Root = GetFirstAncestorOfType<VisualElement>();
            m_MainContainer = new VisualElement();

            m_HeaderItem = new VisualElement();
            m_HeaderItem.name = "header";
            m_HeaderItem.AddToClassList("blaockboardHeader");

            m_ContentContainer = new VisualElement();
            m_ContentContainer.name = "contentContainer";

            m_TitleLabel = new Label();
            m_TitleLabel.name = "titleLabel";
            m_SubTitleLabel = new Label();
            m_SubTitleLabel.name = "subTitleLabel";
            m_HeaderItem.Add(m_TitleLabel);
            m_HeaderItem.Add(m_SubTitleLabel);

            m_MainContainer.Add(m_HeaderItem);
            m_MainContainer.Add(m_ContentContainer);

            base.hierarchy.Add(m_MainContainer);

            base.capabilities |= Capabilities.Resizable | Capabilities.Movable;
            base.style.overflow = Overflow.Hidden;
            ClearClassList();
            AddToClassList("blackboard");

            m_Dragger = new Dragger()
            {
                clampToParentEdges = true
            };

            this.AddManipulator(m_Dragger);
            Scrollable = false;

            base.hierarchy.Add(new Resizer());
            
            RegisterCallback(delegate (DragUpdatedEvent e)
            {
                e.StopPropagation();
            });

            RegisterCallback(delegate (WheelEvent e)
            {
                e.StopPropagation();
            });
            RegisterCallback(delegate (MouseDownEvent e)
            {
                if (e.button == 0)
                {
                    ClearSelection();
                }

                e.StopPropagation();
            });
            
            m_GraphView = associatedGraphView;
            focusable = true;


        }

        //
        // Summary:
        //     The GraphView that the Inspector is attached to. Based on GraphView.Blackboard
        public GraphView Graph
        {
            get
            {
                if (!IsWindowed && m_GraphView == null)
                {
                    m_GraphView = GetFirstAncestorOfType<GraphView>();
                }

                return m_GraphView;
            }
            set
            {
                if (IsWindowed)
                {
                    m_GraphView = value;
                }
            }
        }

        //
        // Summary:
        //     Set to true when the Blackboard displays in a separate window. Set to false when
        //     the Blackboard displays in the GraphView.
        public bool IsWindowed
        {
            get
            {
                return m_Windowed;
            }
            set
            {
                if (m_Windowed != value)
                {
                    if (value)
                    {
                        base.capabilities &= ~Capabilities.Movable;
                        AddToClassList("windowed");
                        this.RemoveManipulator(m_Dragger);
                    }
                    else
                    {
                        base.capabilities |= Capabilities.Movable;
                        RemoveFromClassList("windowed");
                        this.AddManipulator(m_Dragger);
                    }

                    m_Windowed = value;
                }
            }
        }

        //
        // Summary:
        //     All selected elements in the GraphView that the Blackboard is attached to.
        public List<ISelectable> selection => Graph?.selection;

        //
        // Summary:
        //     The title of this window.
        public override string title
        {
            get
            {
                return m_TitleLabel.text;
            }
            set
            {
                m_TitleLabel.text = value;
            }
        }

        //
        // Summary:
        //     The subtitle of this window.
        public string subTitle
        {
            get
            {
                return m_SubTitleLabel.text;
            }
            set
            {
                m_SubTitleLabel.text = value;
            }
        }

        public override VisualElement contentContainer => m_ContentContainer;

        //
        // Summary:
        //     Indicates whether the content of this Blackboard can be vertically scrolled by
        //     user. It is false by default.
        public bool Scrollable
        {
            get
            {
                return m_Scrollable;
            }
            set
            {
                if (m_Scrollable == value)
                {
                    return;
                }

                m_Scrollable = value;
                if (m_Scrollable)
                {
                    if (m_ScrollView == null)
                    {
                        m_ScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
                    }

                    m_ContentContainer.RemoveFromHierarchy();
                    m_Root.Add(m_ScrollView);
                    m_ScrollView.Add(m_ContentContainer);
                    //base.resizeRestriction = ResizeRestriction.None;
                    AddToClassList("scrollable");
                }
                else
                {
                    if (m_ScrollView != null)
                    {
                        //base.resizeRestriction = ResizeRestriction.FlexDirection;
                        m_ScrollView.RemoveFromHierarchy();
                        m_ContentContainer.RemoveFromHierarchy();
                        m_Root.Add(m_ContentContainer);
                    }

                    RemoveFromClassList("scrollable");
                }
            }
        }

        //
        // Summary:
        //     Adds an element to the selection in the GraphView that the Blackboard is attached
        //     to.
        //
        // Parameters:
        //   selectable:
        //     Element to add to selection.
        public virtual void AddToSelection(ISelectable selectable)
        {
            Graph?.AddToSelection(selectable);
        }

        //
        // Summary:
        //     Removes an element from the selection in the GraphView that the Blackboard is
        //     attached to.
        //
        // Parameters:
        //   selectable:
        //     Element to remove from selection.
        public virtual void RemoveFromSelection(ISelectable selectable)
        {
            Graph?.RemoveFromSelection(selectable);
        }

        //
        // Summary:
        //     Clears the selection in the GraphView that the Blackboard is attached to.
        public virtual void ClearSelection()
        {
            Graph?.ClearSelection();
        }


        // CUSTOM BEHAVIOR
        public void UpdateSelection(NodeView nodeView)
        {
            Clear();
            Object.DestroyImmediate(_editor);
             _editor = UnityEditor.Editor.CreateEditor(nodeView.Node);
            
            var container = _editor.CreateInspectorGUI();

            Add(container);


            //EditorApplication.delayCall += BindWaveObjectFieldChanges;

        }

        public void Repaint(NodeView nodeView)
        {
            m_CachedNode = nodeView;
            EditorApplication.delayCall += DelayedRepaint;
        }

        private void DelayedRepaint()
        {
            EditorApplication.delayCall -= DelayedRepaint;
            if (m_CachedNode == null)
            {
                return;
            }
            UpdateSelection(m_CachedNode);

        }

        //private void BindWaveObjectFieldChanges()
        //{
        //    EditorApplication.delayCall -= BindWaveObjectFieldChanges;
        //    PropertyField wavesField = this.Q<PropertyField>("WavesField");
        //    if (wavesField == null)
        //        return;
        //    ListView wavesList = wavesField.Q<ListView>();

        //    if (wavesList == null)
        //        return;
        //    wavesList.itemsSourceChanged += DelayWavesPropertyUpdate;
            

        //    //foreach (VisualElement child in )
        //    //{
        //    //    PropertyField waveDataObjectInputField = child.Q<PropertyField>("WaveDataObjectField");
        //    //    if (waveDataObjectInputField != null)
        //    //    {
        //    //        waveDataObjectInputField.RegisterValueChangeCallback(OnWavesPropertyChanged);
        //    //    }
        //    //}
        //}

        //private void DelayWavesPropertyUpdate()
        //{
        //    EditorApplication.delayCall += OnWavesPropertyChanged;
        //}

        //private void OnWavesPropertyChanged()
        //{
        //    EditorApplication.delayCall -= OnWavesPropertyChanged;
        //    SerializedProperty wavesList = _editor.serializedObject.FindProperty("Waves");
        //    PropertyField oldField = this.Q<PropertyField>("WavesField");
        //    VisualElement parentPanel = oldField.parent;
        //    oldField.RemoveFromHierarchy();

        //    PropertyField waves = new PropertyField(wavesList);
        //    waves.Bind(_editor.serializedObject);
        //    waves.name = "WavesField";

        //    parentPanel.Add(waves);

        //    BindWaveObjectFieldChanges();
        //}
    }
}