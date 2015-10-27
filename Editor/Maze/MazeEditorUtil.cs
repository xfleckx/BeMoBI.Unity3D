
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

using Assets.BeMoBI.Unity3D.Editor.Maze.UnitCreation;

public static class MazeEditorUtil
{
    public static void RebuildGrid(beMobileMaze maze)
    {
        var GridDim = CalcGridSize(maze);

        if (!maze.Units.Any())
        {
            var existingUnits = maze.gameObject.GetComponentsInChildren<MazeUnit>();

            foreach (var unit in existingUnits)
            {
                maze.Units.Add(unit);
            }
        }

        maze.Grid = FillGridWith(maze.Units, (int)GridDim.x, (int)GridDim.y);
    }


    public static MazeUnit InitializeUnit(beMobileMaze maze, Vector2 tilePos, float unitFloorOffset, GameObject unit)
    {
        var tilePositionInLocalSpace = new Vector3(
            (tilePos.x * maze.RoomDimension.x) + (maze.RoomDimension.x / 2f),
            unitFloorOffset,
            (tilePos.y * maze.RoomDimension.z) + (maze.RoomDimension.z / 2f));

        // set the cubes parent to the game object for organizational purposes
        unit.transform.parent = maze.transform;

        //unit.transform.localPosition = maze.transform.position + maze.transform.TransformPoint(tilePositionInLocalSpace);
        unit.transform.localPosition = tilePositionInLocalSpace;
        // we scale the u to the tile size defined by the TileMap.TileWidth and TileMap.TileHeight fields 
        unit.transform.localScale = new Vector3(maze.RoomDimension.x, maze.RoomDimension.y * maze.RoomHigthInMeter, maze.RoomDimension.z);

        // give the u a assetName that represents it's location within the tile maze
        unit.name = string.Format(maze.UnitNamePattern, tilePos.x, tilePos.y);

        MazeUnit mazeUnit = unit.GetComponent<MazeUnit>();

        mazeUnit.Initialize(tilePos, maze.RoomDimension);

        return mazeUnit;
    }

    public static void Rescale(beMobileMaze maze, Vector3 newUnitScale, float floorOffset)
    {
        foreach (var item in maze.Units)
        {
            item.transform.localScale = newUnitScale;
            InitializeUnit(maze, item.GridID, floorOffset, item.gameObject);
        }
    }
  
    public static MazeUnit[,] FillGridWith(IEnumerable<MazeUnit> existingUnits, int columns, int rows)
    {
        var grid = new MazeUnit[columns, rows];

        foreach (var unit in existingUnits)
        {
            var x = Mathf.FloorToInt(unit.GridID.x);
            var y = Mathf.FloorToInt(unit.GridID.y);
            grid[x, y] = unit;
        }

        return grid;
    }

    public static Vector2 CalcGridSize(beMobileMaze maze)
    {
        int rows = Mathf.FloorToInt(maze.MazeLengthInMeter / maze.RoomDimension.z);
        int columns = Mathf.FloorToInt(maze.MazeWidthInMeter / maze.RoomDimension.x);

        return new Vector2(columns, rows);
    }

    /// <summary>
    /// Check if there are already unitsFound existing and put them into the new Grid
    /// </summary>
    /// <param name="maze"></param>
    /// <param name="newColumns"></param>
    /// <param name="newRows"></param>
    /// <returns></returns>
    public static MazeUnit[,] ReconfigureGrid(beMobileMaze maze, int newColumns, int newRows)
    {
        MazeUnit[,] newGrid = new MazeUnit[newColumns, newRows];

        for (int col = 0; col < newColumns; col++)
        {
            for (int row = 0; row < newRows; row++)
            {
                var hostGameObject = GameObject.Find(string.Format(maze.UnitNamePattern, col, row));

                if (hostGameObject != null)
                    newGrid[col, row] = hostGameObject.GetComponent<MazeUnit>();
            }
        }

        maze.Grid = newGrid;

        var unitsOutsideOfGrid = maze.Units.Select(
            (u) =>
            {

                if (u.GridID.x > newColumns || u.GridID.y > newRows)
                    return u.gameObject;
                else
                    return null;
            });

        unitsOutsideOfGrid.ImmediateDestroyObjects();

        return maze.Grid;
    }

    public static MazeUnit[,] ReconfigureGrid(beMobileMaze maze, float columns, float rows)
    {
        return ReconfigureGrid(maze, Mathf.FloorToInt(columns), Mathf.FloorToInt(rows));
    }

    public static void ImmediateDestroyObjects(this IEnumerable<GameObject> set)
    {
        if (!set.Any())
            return;

        var first = set.First();
        var tail = set.Skip(1);
        GameObject.DestroyImmediate(first);
        tail.ImmediateDestroyObjects();
    }

    public static void ReplaceUnits(beMobileMaze targetMaze, GameObject replacementPrefab)
    {
        foreach (var existingUnit in targetMaze.Units)
        {
            var newUnitFromPrefab = PrefabUtility.InstantiatePrefab(replacementPrefab) as GameObject;
            
            PrefabUtility.DisconnectPrefabInstance(newUnitFromPrefab);

            //newUnitFromPrefab.hideFlags = HideFlags.HideAndDontSave;

            var newUnit = newUnitFromPrefab.GetComponent<MazeUnit>();

            ReplaceUnit(targetMaze, existingUnit, newUnit);

            UnityEngine.Object.DestroyImmediate(newUnitFromPrefab);
        }
    }

    public static void ReplaceUnit(beMobileMaze hostMaze, MazeUnit oldUnit, MazeUnit newUnit, bool ignoreOldScaling = true, bool forceRecalculationOfComponentPosition = true)
    {
        #region prepare children lists

        int new_ChildCount = newUnit.transform.childCount;

        List<GameObject> new_Children = new List<GameObject>();

        int old_ChildCount = oldUnit.transform.childCount;

        List<GameObject> old_children = new List<GameObject>();

        for (int i = 0; i < old_ChildCount; i++)
        {
            var old_child = oldUnit.transform.GetChild(i);

            old_children.Add(old_child.gameObject);
        }

        for (int i = 0; i < new_ChildCount; i++)
        {
            var new_child = newUnit.transform.GetChild(i);

            new_Children.Add(new_child.gameObject);
        }

        #endregion
        
        if (!ignoreOldScaling)
            oldUnit.transform.localScale = hostMaze.RoomDimension;
        else
            oldUnit.transform.localScale = Vector3.one;

        var dimension = hostMaze.RoomDimension;

        foreach (var newChild in new_Children)
        {
            if (old_children.Any((go) => go.name.Equals(newChild.name)))
            {
                var old_equivalent = old_children.Single((go) => go.name.Equals(newChild.name));

                newChild.transform.parent = old_equivalent.transform.parent;

                if (!forceRecalculationOfComponentPosition)
                {
                    //obtain old wall position
                    newChild.transform.localPosition = newChild.transform.position;
                }
                else
                {
                    if (old_equivalent.name.Equals(MazeUnit.TOP))
                    {
                        newChild.transform.localPosition = new Vector3(0, dimension.y, 0);
                    }

                    if (old_equivalent.name.Equals(MazeUnit.NORTH))
                    {
                        newChild.transform.localPosition = new Vector3(0, dimension.y / 2, dimension.z / 2);
                    }

                    if (old_equivalent.name.Equals(MazeUnit.SOUTH))
                    {
                        newChild.transform.localPosition = new Vector3(0, dimension.y / 2, -dimension.z / 2);
                    }

                    if (old_equivalent.name.Equals(MazeUnit.WEST))
                    {
                        newChild.transform.localPosition = new Vector3(-dimension.x / 2, dimension.y / 2, 0);
                    }

                    if (old_equivalent.name.Equals(MazeUnit.EAST))
                    {
                        newChild.transform.localPosition = new Vector3(dimension.x / 2, dimension.y / 2, 0);
                    }

                    if (old_equivalent.name.Equals(MazeUnit.FLOOR))
                    {
                    }
                }
                
                newChild.transform.localScale = Vector3.one;

                newChild.SetActive(old_equivalent.activeSelf);
                
                old_equivalent.transform.parent = null;

                // TODO Bug here -> localScaling remains in a strange way!
            }
        }

        foreach (var item in old_children)
        {
            UnityEngine.Object.DestroyImmediate(item.gameObject);
        }
    }
    public static bool IsSet(OpenDirections value, OpenDirections flag)
    {
        return (value & flag) == flag;
    }

    public static void ResizeUnitByMeshModification(GameObject unit, UnitMeshModificationModel modificationModel)
    {
        var m = modificationModel;

        var boxCollider = unit.GetComponent<BoxCollider>();

        if (boxCollider != null) { 
            boxCollider.size = m.RoomDimensions;
            boxCollider.center = new Vector3(0, m.RoomDimensions.y / 2, 0);
        }

        for (int i = 0; i < unit.transform.childCount; i++)
        {
            var wall = unit.transform.GetChild(i);

            Mesh mesh = GetMeshFrom(wall);

            List<Vector3> vertices = null;

            if (wall.name == MazeUnit.FLOOR)
            {
                vertices = new List<Vector3>()
                {
                    new Vector3(m.meshOrigin.x,      0, m.meshOrigin.z), 
                    new Vector3(m.WidthEndPoint, 0, m.meshOrigin.z),
                    new Vector3(m.meshOrigin.x,      0, m.DepthEndPoint),
                    new Vector3(m.WidthEndPoint, 0, m.DepthEndPoint)
                };
               
            }

            if (wall.name == MazeUnit.TOP)
            {
                wall.transform.localPosition = new Vector3(0, m.RoomDimensions.y, 0);

                vertices = new List<Vector3>()
                {
                    new Vector3(m.meshOrigin.x,         0, m.meshOrigin.z),
                    new Vector3(m.WidthEndPoint,    0, m.meshOrigin.z),
                    new Vector3(m.meshOrigin.x,         0, m.DepthEndPoint),
                    new Vector3(m.WidthEndPoint,    0, m.DepthEndPoint)
                };
            }

            if (wall.name == MazeUnit.NORTH)
            {
                wall.transform.localPosition = new Vector3(0, m.RoomDimensions.y / 2, m.RoomDimensions.z / 2);

                vertices = new List<Vector3>()
                {
                    new Vector3(m.meshOrigin.x,         m.meshOrigin.y,           0),
                    new Vector3(m.WidthEndPoint,    m.meshOrigin.y,           0),
                    new Vector3(m.meshOrigin.x,         m.HeightEndPoint,      0),
                    new Vector3(m.WidthEndPoint,    m.HeightEndPoint,     0)
                };

            }

            if (wall.name == MazeUnit.SOUTH)
            {
                wall.transform.localPosition = new Vector3(0, m.RoomDimensions.y / 2, -m.RoomDimensions.z / 2);

                vertices = new List<Vector3>()
                {
                    new Vector3(m.meshOrigin.x,         m.meshOrigin.y,        0),
                    new Vector3(m.WidthEndPoint,    m.meshOrigin.y,        0),
                    new Vector3(m.meshOrigin.x,         m.HeightEndPoint,  0),
                    new Vector3(m.WidthEndPoint,    m.HeightEndPoint,  0)
                };

            }

            if (wall.name == MazeUnit.WEST)
            {
                wall.transform.localPosition = new Vector3(-m.RoomDimensions.x / 2, m.RoomDimensions.y / 2, 0);

                vertices = new List<Vector3>()
                {
                    new Vector3(0,   m.meshOrigin.y,         m.meshOrigin.z),
                    new Vector3(0,   m.HeightEndPoint,   m.meshOrigin.z),
                    new Vector3(0,   m.meshOrigin.y,         m.DepthEndPoint),
                    new Vector3(0,   m.HeightEndPoint,   m.DepthEndPoint)
                };
            }

            if (wall.name == MazeUnit.EAST)
            {
                wall.transform.localPosition = new Vector3(m.RoomDimensions.x / 2, m.RoomDimensions.y / 2, 0);

                vertices = new List<Vector3>()
                {
                    new Vector3(0,   m.meshOrigin.y,         m.meshOrigin.z),
                    new Vector3(0,   m.HeightEndPoint,   m.meshOrigin.z),
                    new Vector3(0,   m.meshOrigin.y,         m.DepthEndPoint),
                    new Vector3(0,   m.HeightEndPoint,   m.DepthEndPoint)
                };
            }

            if (vertices != null)
                mesh.SetVertices(vertices);

            mesh.UploadMeshData(true);
        }
    }

    const string MeshAssertionMessageFormat = "The unit you are trying to modify doesn't have the expected structure! Missing Mesh on {0}";
    const string MeshFilterAssertionMessageFormat = "The unit you are trying to modify doesn't have the expected structure! Missing MeshFilter component on {0}";
    private static Mesh GetMeshFrom(Transform wall)
    {
        var meshFilter = wall.gameObject.GetComponent<MeshFilter>();

        Debug.Assert(meshFilter != null, string.Format(MeshFilterAssertionMessageFormat, wall.name));


        Debug.Assert(meshFilter.sharedMesh != null, string.Format(MeshAssertionMessageFormat, wall.name));

        return meshFilter.sharedMesh;
    }
}
