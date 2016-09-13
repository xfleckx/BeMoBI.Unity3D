using UnityEngine;
using System.Collections;
using System;

namespace Assets.SNEED.Mazes.Grid
{
    /// <summary>
    /// Reusable simple traversion algorithm to travers a grid based maze from 0,0 to n,n
    /// Important for exporting mazes build with the maze editor.
    /// </summary>
    public class MazeGridTraversal
    {
        private beMobileMaze maze;

        public MazeGridTraversal(beMobileMaze maze)
        {
            this.maze = maze;
        }

        public void travers(
            Action<TraversalState> onEmptyFieldFound,
            Action<MazeUnit, TraversalState> onUnitFound)
        {
            for (int y_i = maze.Rows - 1; y_i >= 0; y_i--)
            {
                var rowsLeft = y_i - 1;

                for (int x_i = 0; x_i < maze.Columns; x_i++)
                {
                    var colLeft = maze.Columns - 1 - x_i;

                    if (maze[x_i, y_i] == null)
                    {
                        if (onEmptyFieldFound != null)
                            onEmptyFieldFound(new TraversalState(y_i, x_i, colLeft, rowsLeft));
                    }
                    else
                    {
                        if (onUnitFound != null)
                            onUnitFound(maze[x_i, y_i], new TraversalState(x_i, y_i, colLeft, rowsLeft));
                    }
                }
            }
        }
        
        public struct TraversalState
        {
            public int ColumnsLeft;
            public int RowsLeft;
            public int Column;
            public int Row;

            public TraversalState(int col, int row, int colLeft, int rowLeft)
            {
                ColumnsLeft = colLeft;
                RowsLeft = rowLeft;
                Column = col;
                Row = row;
            }
        }
    }

}