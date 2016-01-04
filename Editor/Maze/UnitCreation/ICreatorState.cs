using System;
using UnityEngine;

namespace Assets.Daedalus.Unity3D.Editor.Maze.UnitCreation
{
    interface ICreatorState
    {
        Vector3 RoomDimension { get; set; }
        string CreatorName { get; }
        void Initialize();
        UnityEngine.Rect OnGUI();
    }
}
