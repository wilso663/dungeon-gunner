using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code
#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;
    /// <summary>
    /// Initialize the room node type and load into the list
    /// </summary>
    public void Initialize(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        // Load room node type list
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Populate a string array with the room node types to display that can be selected
    /// </summary>
    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];

        for(int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }

        return roomArray;
    }

    /// <summary>
    /// Draw node with the node style
    /// </summary>
    public void Draw(GUIStyle nodeStyle)
    {
        // Draw node box using begin area
        GUILayout.BeginArea(rect, nodeStyle);
        
        // Start region to detect popup selection changes
        EditorGUI.BeginChangeCheck();

        // Display a popup using the RoomNodeType name values that can be selected from (default to current set roomNodeType)
        int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

        int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());
        roomNodeType = roomNodeTypeList.list[selection];

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);

        GUILayout.EndArea();
    }

    public void ProcessEvents(Event currentEvent)
    {
        switch(currentEvent.type) 
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if(currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        } else if(currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    /// <summary>
    /// Make the currently moused over object the active object in the editor and set it to selected
    /// </summary>
    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;

        isSelected = !isSelected;
    }

    /// <summary>
    /// Makes the currently moused over node the object to set to draw from if being dragged by right mouse
    /// </summary>
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    /// <summary>
    /// Process releasing left mouse to select a node
    /// </summary>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
            ProcessLeftClickUpEvent();
    }

    /// <summary>
    /// Set current mouse over object to not be left click dragging
    /// </summary>
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    /// <summary>
    /// Process event to drag node and update gui
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    /// <summary>
    /// Drag node method to move nodes in the editor
    /// </summary>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Add childID to the node
    /// </summary>
    public bool AddChildRoomIDToRoomNode(string childID)
    {
        childRoomNodeIDList.Add(childID);
        return true;
    }

    public bool AddParentRoomIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

#endif
    #endregion
}
