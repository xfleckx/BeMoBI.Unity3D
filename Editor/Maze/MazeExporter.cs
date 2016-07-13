using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Assets.SNEED.EditorExtensions.Maze
{
    public interface IMazeExporter
    {
        string CreateTargetFileName(beMobileMaze mazeToExport);

        void Export(beMobileMaze maze, FileInfo targetFile);
    }


    /// <summary>
    /// This implementation of the IMazeExporter interface 
    /// produces a textfile containig a matlab compatible string which evals to a matrix
    /// containg zeros and ones for existing units
    /// </summary>
    public class SimpleTextFileMazeExporter : IMazeExporter
    {
        public const string FILE_EXTENSION = "maze";

        public const string FILE_NAME_PATTERN = "{0}.{1}";  

        public string CreateTargetFileName(beMobileMaze mazeToExport)
        {
            return string.Format(FILE_NAME_PATTERN, mazeToExport.name, FILE_EXTENSION);
        }


        /// <summary>
        /// Export as a textfield which could be read as a matlab expression and evals to an matrix containing zeros and ones on unit positions
        /// </summary>
        /// <param name="maze"></param>
        /// <param name="targetFile"></param>
        public void Export(beMobileMaze maze, FileInfo targetFile)
        {
            using(var fs = new FileStream(targetFile.FullName,FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                StringBuilder mazeAsTextMatrix = new StringBuilder();

                mazeAsTextMatrix.Append("matlab matrix:\t");

                for (int y_i = 0; y_i < maze.Rows; y_i++)
                {
                    StringBuilder rowAsLine = new StringBuilder();

                    for (int x_i = 0; x_i < maze.Columns; x_i++)
                    {
                        if (x_i == 0 && y_i == 0)
                        {
                            rowAsLine.Append('[');
                        }

                        if (maze[x_i, y_i] == null)
                        {
                            rowAsLine.Append("0");
                        }
                        else 
                        {
                            rowAsLine.Append("1");
                        }

                        if(x_i != maze.Columns -1)
                            rowAsLine.Append(", ");
                    }

                    if(y_i != maze.Rows - 1)
                    { 
                        rowAsLine.Append(';');
                    }

                    if (y_i == maze.Columns-1)
                    {
                        rowAsLine.Append(']');
                    }

                    mazeAsTextMatrix.Append(rowAsLine.ToString());
                }

                byte[] info = new UTF8Encoding(true).GetBytes(mazeAsTextMatrix.ToString());
                fs.Write(info, 0, info.Length);
            }
        }
    }
}
