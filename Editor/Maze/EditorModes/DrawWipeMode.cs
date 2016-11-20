using UnityEngine;
using UnityEditor;
using Assets.SNEED.EditorExtensions.Maze.UnitCreation;
using Assets.SNEED.EditorExtensions;
using Assets.SNEED.Mazes;
using System.Linq;

namespace Assets.SNEED.EditorExtension.Maze.EditorModes
{
    public class DrawingMode : EditorMode
    {
        private GameObject unitPrefab;

        private float unitFloorOffset = 0f;

        private Vector3 unitDimensions;

        private bool addMode = true;
        private bool removeMode = false;

        public override string Name
        {
            get
            {
                return "Draw or Wipe Units";
            }
        }

        public override Color GetPrimaryColor()
        {
            return Color.magenta;
        }

        public override void OnGUI(MazeCreationWorkflowBackEnd backend)
        {
            unitPrefab = EditorGUILayout.ObjectField("Unit Prefab:", unitPrefab, typeof(GameObject), false) as GameObject;
            
            if (unitPrefab != null)
            {
                var unit = unitPrefab.GetComponent<MazeUnit>();

                if (unit != null) { 
                    if(unit.Dimension != backend.selectedMaze.RoomDimension)
                    {
                        EditorUtility.DisplayDialog("Wrong Size!", "The selected Prefab has not the correct dimensions!", "Ok");
                        unitPrefab = null;
                    }
                    else
                    {
                        unitDimensions = unit.Dimension;
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Wrong Selection!", "The Selected Prefab is not a MazeUnit!", "Reset");
                    unitPrefab = null;
                }
            } 
            
            unitDimensions = EditorGUILayout.Vector3Field("Cell Size: ", unitDimensions);

            if (unitPrefab == null && GUILayout.Button("Create new Unit"))
            {
                UnitCreator.OpenUnitCreator(backend.selectedMaze.RoomDimension, c => {
                    // get the created Unit prefab automaticaly back to the Editor Window
                    c.onUnitPrefabCreated += prefab =>
                    {
                        var mazeUnit = prefab.GetComponent<MazeUnit>();

                        if (mazeUnit != null)
                        {
                            unitPrefab = prefab;
                        }
                    };

                });
            }

            EditorGUILayout.Space();

            if (unitPrefab == null)
            {
                removeMode = GUILayout.Toggle(removeMode || Event.current.shift, "Remove", "Button");
                return;
            }

            EditorGUILayout.BeginHorizontal();

            addMode = GUILayout.Toggle(!removeMode, "Add", "Button");
            removeMode = GUILayout.Toggle(!addMode || Event.current.shift, "Remove", "Button");

            EditorGUILayout.EndHorizontal();

        }

        protected override void Click(Event evt, int button)
        {
            var maze = backend.selectedMaze;
            var pos = backend.visual.currentTilePosition;
            var mode = removeMode ? "Remove" : "Add";

            Debug.Log("Try clicking on " + backend.getMazeName() + "@ " + pos + "Mode: " + mode);

            bool hasAUnitOnThisPosition = maze.Units.Any((u) => u.GridID.x == pos.x && u.GridID.y == pos.y);

            if (unitPrefab != null && !hasAUnitOnThisPosition && addMode) { 
                AddANewUnit(maze, pos, backend);
                backend.indicateChange();
            }

            if (hasAUnitOnThisPosition && removeMode) { 
                RemoveAUnit(maze, pos, backend);
                backend.indicateChange();
            }

            Consume(evt);
        }

        private void AddANewUnit(beMobileMaze maze, Vector2 pos, MazeCreationWorkflowBackEnd backend)
        {
            var unitHost = PrefabUtility.InstantiatePrefab(unitPrefab) as GameObject;

            PrefabUtility.DisconnectPrefabInstance(unitHost);

            var unit = MazeEditorUtil.InitializeUnit(maze, pos, unitFloorOffset, unitHost);

            maze.Grid[(int)pos.x, (int)pos.y] = unit;

            maze.Units.Add(unit);
        }

        /// <summary>
        /// Erases a block at the pre-calculated mouse hit position
        /// </summary> 
        private void RemoveAUnit(beMobileMaze maze, Vector2 pos, MazeCreationWorkflowBackEnd backend)
        {
            Debug.Log("Remove call...");

            var allÚnits = maze.GetComponentsInChildren<MazeUnit>();

            var selected = allÚnits.Where(u => u.GridID == pos);

            var unitHost = selected.First();

            if (!unitHost)
            {
                Debug.Log("Nothing to erase!");
                return;
            }

            // if a game object was found with the same assetName and it is a child we just destroy it immediately
            if (unitHost != null && unitHost.transform.parent == maze.transform)
            {
                maze.Units.Remove(unitHost);
                maze.Grid[(int)pos.x, (int)pos.y] = null;
                Editor.DestroyImmediate(unitHost.gameObject);
            }

        }

        protected override void Drag(Event evt, int button)
        {
        }

        public override void Reset()
        {

        }
    }
}
