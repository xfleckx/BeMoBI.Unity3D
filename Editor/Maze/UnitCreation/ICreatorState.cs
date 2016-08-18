using System;
using UnityEngine;

namespace Assets.SNEED.EditorExtensions.Maze.UnitCreation
{
    interface ICreatorState
    {
        Vector3 RoomDimension { get; set; }
        string CreatorName { get; }
        void Initialize();
        
        event Action<GameObject> onUnitPrefabCreated;

        UnityEngine.Rect OnGUI();
    }
}
