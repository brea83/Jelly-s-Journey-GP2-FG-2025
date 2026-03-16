using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneReadingTest : EditorWindow
{
    [SerializeField] private int m_SelectedIndex = -1;
    private VisualElement m_Root;
    private VisualElement m_GraphPanel;
    private VisualElement m_ScenesRightPane;

    [MenuItem("NGAME/SceneReadingTest")]
    public static void ShowMyEditor()
    {
        // This method is called when the user selects the menu item in the Editor.
        EditorWindow wnd = GetWindow<SceneReadingTest>();
        wnd.titleContent = new GUIContent("Scene Reading Test");

        // Limit size of the window.
        wnd.minSize = new Vector2(450, 200);
        wnd.maxSize = new Vector2(1920, 720);
    }

    protected class SceneData
    {
        public string Name = "default name";
        public string FilePath = "";
        public bool IncludeInGraphTool = false;
        public string Description = "";
    }
    public void CreateGUI()
    {
        CreateGeneralSettingsPanel();
        CreateScenesPanel();
    }

    private void CreateGeneralSettingsPanel()
    {
        VisualElement panel = new VisualElement();
        panel.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI Toolkit/Styles/Editor/NGAMESettingsWindow.uss"));

        Label title = new Label();
        title.text = "NGAME Settings";
        title.AddToClassList("title1");
        panel.Add(title);

        TextElement subtitle = new TextElement();
        subtitle.text = "Node Graph Adventure Map Editor";
        subtitle.AddToClassList("subtitle1");
        panel.Add(subtitle);

        rootVisualElement.Add(panel);
    }

    private void CreateScenesPanel()
    {
        // Get a list of all sprites in the project.
        List<SceneData> allSceneData = FindSceneData();

        // Create a two-pane view with the left pane being fixed.
        var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

        // Add the panel to the visual tree by adding it as a child to the root element.
        rootVisualElement.Add(splitView);

        // A TwoPaneSplitView always needs two child elements.
        var leftPane = new ListView();
        splitView.Add(leftPane);
        m_ScenesRightPane = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
        splitView.Add(m_ScenesRightPane);

        // Initialize the list view with all sprites' names.
        leftPane.makeItem = () => new Label();
        leftPane.bindItem = (item, index) => { (item as Label).text = allSceneData[index].Name; };
        leftPane.itemsSource = allSceneData;

        // React to the user's selection.
        leftPane.selectionChanged += OnSpriteSelectionChange;

        // Restore the selection index from before the hot reload.
        leftPane.selectedIndex = m_SelectedIndex;

        // Store the selection index when the selection changes.
        leftPane.selectionChanged += (items) => { m_SelectedIndex = leftPane.selectedIndex; };
    }

    private void OnSpriteSelectionChange(IEnumerable<object> selectedItems)
    {
        // Clear all previous content from the pane.
        m_ScenesRightPane.Clear();

        var enumerator = selectedItems.GetEnumerator();
        if (enumerator.MoveNext())
        {
            var selectedScene = enumerator.Current as SceneData;
            if (selectedScene != null)
            {
                DisplaySceneSettings(selectedScene);
            }
        }
    }

    // does not include a null check because intended use is after a null check
    private void DisplaySceneSettings(SceneData sceneData)
    {

        VisualElement settingsPane = new VisualElement();
        settingsPane.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI Toolkit/Styles/Editor/NGAMESettingsWindow.uss"));

        Label title = new Label();
        title.text = sceneData.Name;
        title.AddToClassList("header1");
        settingsPane.Add(title);

        // toggle inclusion in graph tool
        Toggle toggleIncludeInGraph = new Toggle();
        toggleIncludeInGraph.name = "bIncludeInGraph";
        toggleIncludeInGraph.label = "Include Scene in Graph Tool";
        toggleIncludeInGraph.value = sceneData.IncludeInGraphTool;
        settingsPane.Add(toggleIncludeInGraph);

        // add description of found elements.
        TextElement descriptionElement = new TextElement();
        descriptionElement.text = sceneData.Description;
        descriptionElement.visible = sceneData.IncludeInGraphTool;
        settingsPane.Add(descriptionElement);

        toggleIncludeInGraph.RegisterValueChangedCallback(evt => 
        { 
            sceneData.IncludeInGraphTool = evt.newValue;
            Debug.Log(sceneData.Name + ", has had its bool to include in graph set to: " + sceneData.IncludeInGraphTool.ToString());
            descriptionElement.visible = evt.newValue;
        });

        // Add the settings panel to the right-hand pane.
        m_ScenesRightPane.Add(settingsPane);
    }



    private List<SceneData> FindSceneData()
    {
        List<SceneData> results = new List<SceneData>();
        string[] allObjectGuids = AssetDatabase.FindAssets("t:Scene");
        //List<string> scenePaths = new List<string>();
        StringBuilder description;

        foreach (string guid in allObjectGuids)
        {
            //scenePaths.Add(AssetDatabase.GUIDToAssetPath(guid));

            string filePath = AssetDatabase.GUIDToAssetPath(guid);


            Scene aScene = EditorSceneManager.OpenPreviewScene(filePath);
            if (!aScene.IsValid())
            {
                Debug.Log("Invalid scene found");
                continue;
            }

            SceneData currentSceneData = new SceneData();
            currentSceneData.Name = aScene.name;
            currentSceneData.FilePath = filePath;

            description = new StringBuilder();
            bool bComponentsFound = false;

            GameObject[] rootObjects = aScene.GetRootGameObjects();


            foreach (GameObject obj in rootObjects)
            {
                DataCollectionTest[] components = obj.GetComponentsInChildren<DataCollectionTest>();

                if (components.Length > 0)
                {
                    bComponentsFound = true;
                    foreach (DataCollectionTest component in components)
                    {
                        description.Append("Found a Data collection test component \n");
                        description.Append("Name: " + component.GetName() + "\n");
                        description.Append("Number: " + component.GetNumber() + "\n");

                        Vector3 position = component.transform.position;
                        description.Append("Position: " + position.ToString() + "\n");
                        description.Append("-------------\n");
                    }
                }
            }

            if (bComponentsFound)
            {
                Debug.Log("Scene: " + aScene.name + " contains target data types \n" + description.ToString());

                currentSceneData.Description = description.ToString();
            }
            else
            {
                currentSceneData.Description = "No Data Collection Test components found in scene.";
            }

            results.Add(currentSceneData);
            EditorSceneManager.ClosePreviewScene(aScene);
        }

        return results;
    }
}

