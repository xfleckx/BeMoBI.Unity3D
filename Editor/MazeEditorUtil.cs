using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class MazeEditorUtil
{
    public static MazeUnit[,] ReconfigureGrid(beMobileMaze maze,  int newColumns, int newRows)
    {
        if (maze.Grid != null) { 

            MazeUnit[,] newGrid = new MazeUnit[newColumns, newRows];

            for (int col = 0; col < newColumns; col++)
            {
                for (int row = 0; row < newRows; row++)
                {
                    var hostGameObject = GameObject.Find(string.Format(maze.UnitNamePattern, row, col));
                    
                    if(hostGameObject != null)
                        newGrid[col, row] = hostGameObject.GetComponent<MazeUnit>();
                }
            }

            maze.Grid = newGrid;

            var unitsOutsideOfGrid = maze.Units.Select(
                (u) => {

                    if (u.GridID.x > newColumns || u.GridID.y > newRows)
                        return u.gameObject;
                    else
                        return null;
                });

            unitsOutsideOfGrid.ImmediateDestroyObjects();

            return maze.Grid;

        } 
        


        maze.Grid = new MazeUnit[newColumns, newRows];
        
        return maze.Grid;
    }



    public static MazeUnit[,] ReconfigureGrid(beMobileMaze maze,  float columns, float rows)
    {
        return ReconfigureGrid(maze,Mathf.FloorToInt(columns), Mathf.FloorToInt(rows));
    }


    public static void ImmediateDestroyObjects(this IEnumerable<GameObject> set)
    {
        if (set.Any())
        {
            var first = set.First();
            var tail = set.Skip(1);
            GameObject.DestroyImmediate(first);
            tail.ImmediateDestroyObjects();
        }
    }
}
