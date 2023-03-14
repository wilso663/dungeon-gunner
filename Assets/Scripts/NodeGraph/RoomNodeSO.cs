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

        // if the room node has a parent or is of type entrance display a label else display a popup
        if(parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance) 
        {
            //Display a label that can't be changed
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else { 
            // Display a popup using the RoomNodeType name values that can be selected from (default to current set roomNodeType)
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());
            roomNodeType = roomNodeTypeList.list[selection];
        }
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
        if (IsChildRoomValid(childID)) { 
        childRoomNodeIDList.Add(childID);
        return true;
        }
        return false;
    }

    /// <summary>
    /// Check if the child node can be validly added to the parent node
    /// </summary>
    public bool IsChildRoomValid(string childID)
    {
        bool isConnectedToBossNode = false;
        // set true connected boss room in node graph
        foreach(RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
                isConnectedToBossNode = true; break;
        }
        // if child node has boss room and is already connected
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedToBossNode)
            return false;

        // if child node has a type of none
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
            return false;

        // if node already has a child node with this id
        if (childRoomNodeIDList.Contains(childID))
            return false;

        // if child node is trying to connect to itself
        if (id == childID)
            return false;

        // if child node already has a parent
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
            return false;

        // if 2 corridors are trying to connect to each other
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
            return false;

        // if 2 rooms/non-corridors are trying to connect to each other
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
            return false;

        // if adding a corridor, check that the selected node is < the max permitted corridors (default 3)
        if(roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.amxChildCorridors)
            return false;

        // if adding a room to a corridor check the corridor doesn't already have a room
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
            return false;

        // if the child node is an entrance (these must always be the top level node)
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
            return false;

        return true;
    }

    /// <summary>
    ///  Add parentID to the node
    /// </summary>
    public bool AddParentRoomIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

#endif
    #endregion
}
