using NGAME;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace NGAME.Editor
{
    public class NGAMESettings : EditorWindow
    {
        private SO_Settings m_Settings;

        [SerializeField] private int m_SelectedIndex = -1;
        private VisualElement m_RootScrollElement;
        private VisualElement m_GraphPanel;
        private ListView m_ScenesListView;
        private VisualElement m_ScenesRightPane;

        private Dictionary<string, SceneInclusionData> m_GuidToSceneData;

        private StyleSheet m_Styles;

        //private bool m_SettingsAreLoaded = false;

        [MenuItem("NGAME/Settings")]
        public static void ShowMyEditor()
        {
            // This method is called when the user selects the menu item in the Editor.
            NGAMESettings wnd = GetWindow<NGAMESettings>();
            wnd.titleContent = new GUIContent("NGAME Settings");

            // Limit size of the window.
            wnd.minSize = new Vector2(450, 200);
            wnd.maxSize = new Vector2(1920, 720);
        }

        [OnOpenAssetAttribute(1)]
        public static bool OpenEditorFromSO(UnityEngine.EntityId entityID, int line)
        {
            //SO_Settings settings = EditorUtility.EntityIdToObject(entityID) as SO_Settings;
            string filepath = AssetDatabase.GetAssetPath(entityID);
            System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(filepath);
            if (assetType == typeof(SO_Settings))
            {
                ShowMyEditor();
                return false;
            }

            return false;
        }

        [OnOpenAssetAttribute(2)]
        public static bool InitEditorFromSO(UnityEngine.EntityId entityID, int line)
        {
            string filepath = AssetDatabase.GetAssetPath(entityID);
            System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(filepath);
            if (assetType == typeof(SO_Settings))
            {
                SO_Settings settings = AssetDatabase.LoadAssetAtPath<SO_Settings>(filepath);
                if(settings != null)
                {
                    Debug.Log("NGAME SETTINGS is recieving initial settings values");
                    NGAMESettings wnd = GetWindow<NGAMESettings>();
                    wnd.m_Settings = settings;

                    wnd.LoadSettingsObject();
                    wnd.PopulateSceneList();
                    return true;
                }
            }
            return false;
        }

        private void OnDestroy()
        {
            if(m_Settings != null)
            {
                Debug.Log("SAVING SETTINGS ASSET");
                AssetDatabase.SaveAssetIfDirty(m_Settings);
            }
            Debug.Log("NGAME SETTINGS WINDOW ON DESTROY");
        }

        public void CreateGUI()
        {
            Debug.Log("NGAME SETTINGS . CreateGUI() called");
            m_RootScrollElement = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            rootVisualElement.Add(m_RootScrollElement);

            string[] guids = AssetDatabase.FindAssets("NGAMESettingsStyle  t:StyleSheet");
            if (guids.Length > 0)
            {
                m_Styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

                CreateGeneralSettingsPanel();
            CreateScenesPanel();
        }

        private void OpenSettingsObject(VisualElement parent)
        {
            var objectField = new ObjectField();
            objectField.objectType = typeof(SO_Settings);
            objectField.label = "Select a Settings File";
            objectField.value = m_Settings;

            objectField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue is SO_Settings)
                {
                    m_Settings = evt.newValue as SO_Settings;
                    LoadSettingsObject();
                    PopulateSceneList();
                }

            });

            parent.Add(objectField);
        }

        private void LoadSettingsObject()
        {
            if(m_Settings == null)
            {
                return;
            }

            if(m_GuidToSceneData == null)
            {
                m_GuidToSceneData = new Dictionary<string, SceneInclusionData>();
                Debug.Log("Settings Window's Guid To Scene Data dictionary was null so making a new one");
            }

            if(m_Settings.Scenes == null)
            {
                Debug.Log("Settings Object's SceneData list was null so making a new one");
                m_Settings.Scenes = new List<SceneInclusionData>();
            }
            if (m_Settings.Guids == null)
            {
                Debug.Log("Settings Object's scene GUIDs list was null so making a new one");
                m_Settings.Guids = new List<string>();
            }

            if(m_Settings.Guids.Count() != m_Settings.Scenes.Count())
            {
                Debug.LogError("Somehow the list of guids and their associated scene data arent the same length, this shouldn't be possible");
                return;
            }
            // check for deleted or changed scenes
            for(int i = 0; i < m_Settings.Guids.Count(); i++)
            {
                string guidKey = m_Settings.Guids[i];
                SceneInclusionData data = m_Settings.Scenes[i];
                Debug.Log("Checking if " + data.Name + " is a valid scene");

                string filePath = AssetDatabase.GUIDToAssetPath(guidKey);
                Scene aScene;
                if (filePath != data.FilePath)
                {
                    aScene = EditorSceneManager.OpenPreviewScene(filePath);
                }
                else
                {
                    aScene = EditorSceneManager.OpenPreviewScene(data.FilePath);
                }

                if (!aScene.IsValid())
                {
                    Debug.Log("Invalid scene found");
                    data.Description = "Invalid Scene Marked for Deletion from Settings";
                }

                m_GuidToSceneData.Add(guidKey, data);
                EditorSceneManager.ClosePreviewScene(aScene);
            }

            // check if any new scenes need to be added
            // Get a list of all sprites in the project.
           /* List<SceneData> allSceneData =*/ FindSceneData();

        }

        

        private void CreateGeneralSettingsPanel()
        {
            VisualElement panel = new VisualElement();
            // Find all Texture2Ds that have 'co' in their filename, that are labelled with 'architecture' and are placed in 'MyAwesomeProps' folder
            
            if(m_Styles != null)
            {
                panel.styleSheets.Add(m_Styles);
            }
            //panel.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("../UIElements/Styles/NGAMESettingsWindow.uss"));
            m_RootScrollElement.Add(panel);

            Label title = new Label();
            title.text = "NGAME Settings";
            title.AddToClassList("title1");
            panel.Add(title);

            TextElement subtitle = new TextElement();
            subtitle.text = "Node Graph Adventure Map Editor";
            subtitle.AddToClassList("subtitle1");
            panel.Add(subtitle);

            Label header = new Label();
            header.text = "General Settings";
            header.AddToClassList("header1");
            panel.Add(header);

            
            OpenSettingsObject(panel);

            TextElement tempSetting = new TextElement();
            tempSetting.text = "This is where settings about what classes to look for in scenes will go. Current Default is to look for Door and Spawner interfaces.";
            panel.Add(tempSetting);
        }

        private void CreateScenesPanel()
        {

            VisualElement panel = new VisualElement();
            if (m_Styles != null)
            {
                panel.styleSheets.Add(m_Styles);
            }
            //panel.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI Toolkit/Styles/Editor/NGAMESettingsWindow.uss"));
            m_RootScrollElement.Add(panel);

            Label header = new Label();
            header.text = "Scene Selection";
            header.AddToClassList("header1");
            panel.Add(header);

            // Create a two-pane view with the left pane being fixed.
            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

            // Add the panel to the visual tree by adding it as a child to the root element.
            panel.Add(splitView);

            VisualElement leftPanel = new VisualElement();
            splitView.Add(leftPanel);

            Label leftPanelLabel = new Label();
            leftPanelLabel.text = "Scenes";
            leftPanelLabel.AddToClassList("header2");
            leftPanel.Add(leftPanelLabel);

            // A TwoPaneSplitView always needs two child elements.
            m_ScenesListView = new ListView();
            leftPanel.Add(m_ScenesListView);

            VisualElement rightPanel = new VisualElement();
            splitView.Add(rightPanel);

            Label rightPanelLabel = new Label();
            rightPanelLabel.text = "Options";
            rightPanelLabel.AddToClassList("header2");
            rightPanel.Add(rightPanelLabel);

            m_ScenesRightPane = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            rightPanel.Add(m_ScenesRightPane);

            PopulateSceneList();
            // React to the user's selection.
            m_ScenesListView.selectionChanged += OnSceneSelectionChanged;

            // Restore the selection index from before the hot reload.
            m_ScenesListView.selectedIndex = m_SelectedIndex;

            // Store the selection index when the selection changes.
            m_ScenesListView.selectionChanged += (items) => { m_SelectedIndex = m_ScenesListView.selectedIndex; };
        }

        private void PopulateSceneList()
        {
            if( m_Settings == null)
            {
                return;
            }

            if(m_ScenesListView.childCount != 0)
            {
                m_ScenesListView.Clear();
            }
            // Get a list of all sprites in the project.
            List<SceneInclusionData> allSceneData = m_GuidToSceneData.Values.ToList();//FindSceneData();

            // Initialize the list view with all sprites' names.
            m_ScenesListView.makeItem = () => new Label();
            m_ScenesListView.bindItem = (item, index) => { (item as Label).text = allSceneData[index].Name; };
            m_ScenesListView.itemsSource = allSceneData;
        }

        private void OnSceneSelectionChanged(IEnumerable<object> selectedItems)
        {
            // Clear all previous content from the pane.
            m_ScenesRightPane.Clear();

            var enumerator = selectedItems.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var selectedScene = enumerator.Current as SceneInclusionData;
                if (selectedScene != null)
                {
                    DisplaySceneSettings(selectedScene);
                }
            }
        }

        // does not include a null check because intended use is after a null check
        private void DisplaySceneSettings(SceneInclusionData sceneData)
        {

            VisualElement settingsPane = new VisualElement();
            if (m_Styles != null)
            {
                settingsPane.styleSheets.Add(m_Styles);
            }
            //settingsPane.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI Toolkit/Styles/Editor/NGAMESettingsWindow.uss"));

            // toggle inclusion in graph tool
            Toggle toggleIncludeInGraph = new Toggle();
            toggleIncludeInGraph.name = "bIncludeInGraph";
            toggleIncludeInGraph.label = "Include Scene in Graph Tool";
            toggleIncludeInGraph.value = sceneData.IncludeInGraphTool;
            settingsPane.Add(toggleIncludeInGraph);

            // add description of found elements.
            TextElement descriptionElement = new TextElement();
            //descriptionElement.text = sceneData.Description;
            descriptionElement.visible = sceneData.IncludeInGraphTool;
            settingsPane.Add(descriptionElement);

            toggleIncludeInGraph.RegisterValueChangedCallback(evt =>
            {
                sceneData.IncludeInGraphTool = evt.newValue;
                Debug.Log(sceneData.Name + ", has had its bool to include in graph set to: " + sceneData.IncludeInGraphTool.ToString());
                if (sceneData.IncludeInGraphTool)
                {
                    descriptionElement.text = GetComponentDescription(sceneData.FilePath);
                }
                else
                {
                    descriptionElement.text = "";
                }
                    descriptionElement.visible = evt.newValue;
                EditorUtility.SetDirty(m_Settings);
                Debug.Log("EditorUtility.SetDirty(m_Settings), for toggling inclusion in graph tool for sceneData: " + sceneData.Name);
                //AssetDatabase.SaveAssets();
            });

            if (sceneData.IncludeInGraphTool)
            {
                descriptionElement.text = GetComponentDescription(sceneData.FilePath);
            }
            else
            {
                descriptionElement.text = "";
            }
                // Add the settings panel to the right-hand pane.
                m_ScenesRightPane.Add(settingsPane);
        }



        private void /* List<SceneData>*/ FindSceneData()
        {
            //List<SceneData> results = new List<SceneData>();
            string[] allObjectGuids = AssetDatabase.FindAssets("t:Scene");
            

            foreach (string guid in allObjectGuids)
            {
                string filePath = AssetDatabase.GUIDToAssetPath(guid);

                Scene aScene = EditorSceneManager.OpenPreviewScene(filePath);
                if (!aScene.IsValid())
                {
                    Debug.Log("Invalid scene found");
                    continue;
                }

                SceneInclusionData currentSceneData = new SceneInclusionData();
                currentSceneData.Name = aScene.name;
                currentSceneData.Guid = guid;
                currentSceneData.FilePath = filePath;

                //results.Add(currentSceneData);
                if (!m_GuidToSceneData.ContainsKey(guid)) 
                {
                    m_GuidToSceneData.Add(guid, currentSceneData);
                    m_Settings.Guids.Add(guid);
                    m_Settings.Scenes.Add(currentSceneData);
                    EditorUtility.SetDirty(m_Settings);
                    Debug.Log("EditorUtility.SetDirty(m_Settings), for sceneData: " + currentSceneData.Name);
                }

                EditorSceneManager.ClosePreviewScene(aScene);
            }

            //AssetDatabase.SaveAssets();
            //return results;
        }

        private string GetComponentDescription(string filePath)
        {
            Scene aScene = EditorSceneManager.OpenPreviewScene(filePath);
            if (!aScene.IsValid())
            {
                Debug.Log("Invalid scene found");
                EditorSceneManager.ClosePreviewScene(aScene);
                return "Invalid Scene at path: " + filePath;
            }
            StringBuilder description = new StringBuilder();
            bool bComponentsFound = false;

            GameObject[] rootObjects = aScene.GetRootGameObjects();


            foreach (GameObject obj in rootObjects)
            {
                IEncounterRegionConnector[] components = obj.GetComponentsInChildren<IEncounterRegionConnector>();

                if (components.Length > 0)
                {
                    bComponentsFound = true;
                    foreach (IEncounterRegionConnector component in components)
                    {
                        RegionConnectionData data = component.GetRegionConnectionData();
                        description.Append("Found " + data.TypeName + "\n");
                        description.Append("Connection Type: " + data.ConnectionType.ToString() + "\n");
                        description.Append("Is Lockable: " + data.IsLockable.ToString() + "\n");

                        description.Append("Position: " + data.Position.ToString() + "\n");
                        description.Append("-------------\n");
                    }
                }
            }

            if (bComponentsFound)
            {
                Debug.Log("Scene: " + aScene.name + " contains target data types \n" + description.ToString());
                EditorSceneManager.ClosePreviewScene(aScene);
                return description.ToString();
            }
            else
            {
                EditorSceneManager.ClosePreviewScene(aScene);
                return "No IEncounterRegionConnector components found in scene.";
            }
        }
    }

}