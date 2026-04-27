using Codice.Client.Common.TreeGrouper;
using UnityEditor;
using UnityEngine;

public class WaveSpawnerEditorWindow : EditorWindow {
    private WaveSpawnerGraph graph;

    private Vector2 drag;
    private WaveSpawnerNode selectedNodeForConnection;

    [MenuItem("Tools/WaveSpawner")]
    public static void Open() {
        GetWindow<WaveSpawnerEditorWindow>("WaveSpawner");
    }

    private void OnGUI() {
        DrawGrid(20f, 0.2f);
        DrawGrid(100f, 0.4f);
        DrawToolbar();

        if (graph == null) {
            EditorGUILayout.HelpBox(
                "Wähle oben ein Dialogue Graph Asset aus.",
                MessageType.Info
            );

            return;
        }
        DrawConnections();
        DrawNodes();
        ProcessEvents(Event.current);

        if (GUI.changed) {
            Repaint();
        }
    }

    private void DrawToolbar() {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        graph = (WaveSpawnerGraph)EditorGUILayout.ObjectField(
            graph,
            typeof(WaveSpawnerGraph),
            false,
            GUILayout.Width(300)
        );

        if (GUILayout.Button("Add Node", EditorStyles.toolbarButton)) {
            AddNode(new Vector2(100, 100));
        }

        if (GUILayout.Button("Save", EditorStyles.toolbarButton)) {
            SaveGraph();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void AddNode(Vector2 position) {
        if (graph == null) return;

        Undo.RecordObject(graph, "Add Wave Spawn Node");
        WaveSpawnerNode node = new WaveSpawnerNode(position);
        graph.nodes.Add(node);

        if (string.IsNullOrEmpty(graph.startNodeGuid)) {
            graph.startNodeGuid = node.guid;
        }
        
        EditorUtility.SetDirty(graph);
    }

    private void DrawNodes() {
        BeginWindows();

        for (int i = 0; i < graph.nodes.Count; i++)
        {
            WaveSpawnerNode node = graph.nodes[i];

            string title = node.guid == graph.startNodeGuid ? "StartNode" : "Wave Spawner Node";
            node.rect = GUI.Window(i, node.rect, id => DrawNodeWindow(id, node), title);
        }
        
        EndWindows();
    }

    private void DrawNodeWindow(int id, WaveSpawnerNode node)
    {
        EditorGUI.BeginChangeCheck();
        node.waveName = EditorGUILayout.TextField("Wave Name", node.waveName);
        node.enemyCount = EditorGUILayout.IntField("Enemies", node.enemyCount);
        node.waveTimer = EditorGUILayout.FloatField("Wave Timer", node.waveTimer);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Set as Start Wave")) {
            Undo.RecordObject(graph, "Set Start Node");
            graph.startNodeGuid = node.guid;
            EditorUtility.SetDirty(graph);
        }
        
        if (GUILayout.Button("Connect From This")) {
            selectedNodeForConnection = node;
        }
        
        if (selectedNodeForConnection != null && selectedNodeForConnection != node) {
            if (GUILayout.Button("Connect To This")) {
                Undo.RecordObject(graph, "Connect Waves");
                selectedNodeForConnection.nextNodeGuid = node.guid;
                selectedNodeForConnection = null;
                EditorUtility.SetDirty(graph);
            }
        }
        
        if (!string.IsNullOrEmpty(node.nextNodeGuid)) {
            if (GUILayout.Button("Clear Connection")) {
                Undo.RecordObject(graph, "Clear Connection");
                node.nextNodeGuid = "";
                EditorUtility.SetDirty(graph);
            }
        }

        GUI.backgroundColor = Color.red;

        if (GUILayout.Button("Delete")) {
            DeleteNode(node);
            return;
        }
        if (EditorGUI.EndChangeCheck()) {
            EditorUtility.SetDirty(graph);
        }
        GUI.DragWindow();
    }
    
    private void DrawArrow(Vector3 tip, Vector3 from) {
        Vector3 direction = (tip - from).normalized;

        Vector3 right = Quaternion.Euler(0, 0, 25) * -direction;
        Vector3 left = Quaternion.Euler(0, 0, -25) * -direction;

        Handles.DrawLine(tip, tip + right * 15);
        Handles.DrawLine(tip, tip + left * 15);
    }

    private void DrawGrid(float gridSpacing, float gridOpacity) {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();

        Handles.color = new Color(1f, 1f, 1f, gridOpacity);

        Vector3 offset = new Vector3(
            drag.x % gridSpacing,
            drag.y % gridSpacing,
            0
        );

        for (int i = 0; i < widthDivs; i++) {
            Handles.DrawLine(
                new Vector3(gridSpacing * i, 0, 0) + offset,
                new Vector3(gridSpacing * i, position.height, 0) + offset
            );
        }

        for (int j = 0; j < heightDivs; j++) {
            Handles.DrawLine(
                new Vector3(0, gridSpacing * j, 0) + offset,
                new Vector3(position.width, gridSpacing * j, 0) + offset
            );
        }

        Handles.color = Color.white;

        Handles.EndGUI();
    }

    private void DrawConnections() {
        if (graph == null) return;

        Handles.BeginGUI();

        foreach (WaveSpawnerNode node in graph.nodes) {
            if (string.IsNullOrEmpty(node.nextNodeGuid)) {
                continue;
            }

            WaveSpawnerNode targetNode = graph.nodes.Find(
                n => n.guid == node.nextNodeGuid
            );

            if (targetNode == null) {
                continue;
            }

            Vector3 startPos = new Vector3(
                node.rect.xMax,
                node.rect.center.y,
                0
            );

            Vector3 endPos = new Vector3(
                targetNode.rect.xMin,
                targetNode.rect.center.y,
                0
            );

            Vector3 startTangent = startPos + Vector3.right * 60;
            Vector3 endTangent = endPos + Vector3.left * 60;

            Handles.DrawBezier(
                startPos,
                endPos,
                startTangent,
                endTangent,
                Color.white,
                null,
                3f
            );

            DrawArrow(endPos, startPos);
        }

        Handles.EndGUI();
    }
    
    private void DeleteNode(WaveSpawnerNode node) {
        if (graph == null) return;

        Undo.RecordObject(graph, "Delete Dialogue Node");

        graph.nodes.Remove(node);

        foreach (WaveSpawnerNode otherNode in graph.nodes) {
            if (otherNode.nextNodeGuid == node.guid) {
                otherNode.nextNodeGuid = "";
            }
        }

        if (graph.startNodeGuid == node.guid) {
            graph.startNodeGuid = graph.nodes.Count > 0
                ? graph.nodes[0].guid
                : "";
        }

        EditorUtility.SetDirty(graph);
        GUI.changed = true;
    }
    
    private void ProcessEvents(Event e) {
        drag = Vector2.zero;

        if (e.type == EventType.MouseDrag && e.button == 2) {
            OnDrag(e.delta);
        }

        if (e.type == EventType.ContextClick) {
            Vector2 mousePosition = e.mousePosition;

            GenericMenu menu = new GenericMenu();

            menu.AddItem(
                new GUIContent("Add Wave Spawn Node"),
                false,
                () => AddNode(mousePosition)
            );

            menu.ShowAsContext();
            e.Use();
        }
    }

    private void OnDrag(Vector2 delta) {
        drag = delta;

        if (graph == null) return;

        Undo.RecordObject(graph, "Move Dialogue Graph");

        GUI.changed = true;
        EditorUtility.SetDirty(graph);
    }

    private void SaveGraph() {
        if (graph == null) return;

        EditorUtility.SetDirty(graph);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Dialogue Graph saved.");
    }
}