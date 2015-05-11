using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class MazeEditorUtil
{
    public static MazeUnit[,] ReconfigureGrid(beMobileMaze maze, int rows, int columns)
    {
        MazeUnit[,] newGrid = new MazeUnit[rows, columns];

        maze.Grid.CopyTo(newGrid, 0);

        return maze.Grid;
    }

    public static MazeUnit[,] ReconfigureGrid(beMobileMaze maze, float rows, float columns)
    {
        return ReconfigureGrid(maze, Mathf.FloorToInt(rows), Mathf.FloorToInt(columns));
    }
}
