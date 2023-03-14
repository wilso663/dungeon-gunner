using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    #region ROM SETTINGS

    public const int amxChildCorridors = 3; 
    // Max number of child corridors leading from a room
    // max should be < 4 as it can cause dungeon building to fail and rooms to not fit together
    #endregion
}
