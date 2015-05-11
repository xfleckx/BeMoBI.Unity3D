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
            
            var existingCols = maze.Grid.GetUpperBound(0);
            var existingRows = maze.Grid.GetUpperBound(1);

            for (int col = 0; col <= existingCols; col++)
            {
                for (int row = 0; row <= existingRows; row++)
                {
                    if (row >= newRows || col >= newColumns) {

                        if (maze.Grid[col, row] != null)
                            GameObject.DestroyImmediate(maze.Grid[col, row].gameObject);
   
                        continue;
                    }

                    newGrid[col, row] = maze.Grid[col, row];
                }
            }

            maze.Grid = newGrid;
            
            return maze.Grid;

        } 
        
        maze.Grid = new MazeUnit[newColumns, newRows];
        
        return maze.Grid;
    }

    public static MazeUnit[,] ReconfigureGrid(beMobileMaze maze,  float columns, float rows)
    {
        return ReconfigureGrid(maze,Mathf.FloorToInt(columns), Mathf.FloorToInt(rows));
    }
}
