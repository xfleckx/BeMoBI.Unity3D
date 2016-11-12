using Assets.SNEED.Mazes.Grid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.SNEED.EditorExtensions.Maze
{
    public interface IMazeExporter
    {
        string CreateTargetFileName(beMobileMaze mazeToExport);

        event Func<String, bool> UnexpectedValuesFound;

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

        bool abortExport = false;

        public event Func<String, bool> UnexpectedValuesFound;

        private void OnUnexpectedValuesFound(String message)
        {
            if (UnexpectedValuesFound != null)
                abortExport = UnexpectedValuesFound(message);
        }


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
            using (var fs = new FileStream(targetFile.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                var builders = new List<StringBuilder>();

                if (maze.Rows == 0 || maze.Columns == 0)
                {
                    OnUnexpectedValuesFound("Maze has no Grid to use during Export process");

                    if (abortExport)
                        return;
                }


                builders.Add(AppendMazeMatrix(maze));

                var pathController = maze.GetComponent<PathController>();

                if (pathController.Paths.Count == 0)
                {
                    OnUnexpectedValuesFound("No Paths found during Export process");

                    if (abortExport)
                        return;
                }

                foreach (var path in pathController.Paths)
                {
                    builders.Add(AppendPathWithMatrix(maze, path));
                }

                StringBuilder finalBuilder = new StringBuilder();
                int appedingIndex = 0;

                do
                {
                    finalBuilder.Append(builders[appedingIndex]).AppendLine();
                    appedingIndex++;
                } while (appedingIndex < builders.Count);

                byte[] info = new UTF8Encoding(true).GetBytes(finalBuilder.ToString());
                fs.Write(info, 0, info.Length);
            }
        }


        private StringBuilder AppendMazeMatrix(beMobileMaze maze)
        {
            var traversal = new MazeGridTraversal(maze);

            StringBuilder mazeAsTextMatrix = new StringBuilder();

            var mazeID = string.Format("Maze: {0} matlab matrix:\t", maze.name);

            mazeAsTextMatrix.Append(mazeID);

            mazeAsTextMatrix.Append(" [ ");

            traversal.travers(
                s => {

                    string content = "0";

                    mazeAsTextMatrix.Append(content);

                    if (s.ColumnsLeft > 0)
                        mazeAsTextMatrix.Append(", ");

                    if(s.ColumnsLeft == 0)
                        mazeAsTextMatrix.Append("; ");
                   },
                (u, s) => {

                    OpenDirections openDirectionsCode = u.WaysOpen;

                    OpenDirections directions = ValidateOpenDirections(u);

                    if (openDirectionsCode != directions)
                        Debug.Log(u.GridID + "Has wrong directions code - Correct during export!");

                    string content = ((int) openDirectionsCode).ToString();

                    mazeAsTextMatrix.Append(content);

                    if (s.ColumnsLeft > 0)
                        mazeAsTextMatrix.Append( ", ");

                    if (s.ColumnsLeft == 0)
                        mazeAsTextMatrix.Append("; ");
                });

            #region original
            //for (int y_i = maze.Rows - 1; y_i >= 0; y_i--)
            //{
            //    StringBuilder rowAsLine = new StringBuilder();

            //    for (int x_i = 0; x_i < maze.Columns; x_i++)
            //    {
            //        if (maze[x_i, y_i] == null)
            //        {
            //            rowAsLine.Append("0");
            //        }
            //        else
            //        {
            //            rowAsLine.Append("1");
            //        }

            //        if (x_i < maze.Columns - 1)
            //            rowAsLine.Append(", ");

            //    }

            //    if (y_i != 0)
            //    {
            //        rowAsLine.Append("; ");

            //    }


            //    mazeAsTextMatrix.Append(rowAsLine.ToString());
            //}
            #endregion

            mazeAsTextMatrix.Append(" ]");
            return mazeAsTextMatrix;
        }

        private OpenDirections ValidateOpenDirections(MazeUnit u)
        {
            var children = u.transform.AllChildren().Where(
                go => !(go.Equals(MazeUnit.TOP) || go.Equals(MazeUnit.FLOOR)));

            OpenDirections waysOpen = OpenDirections.None;

            foreach (var item in children)
            {
                if (item.name.Equals(MazeUnit.NORTH) && !item.activeSelf)
                    waysOpen |= OpenDirections.North;
                
                if (item.name.Equals(MazeUnit.SOUTH) && !item.activeSelf)
                    waysOpen |= OpenDirections.South;

                if (item.name.Equals(MazeUnit.WEST) && !item.activeSelf)
                    waysOpen |= OpenDirections.West;

                if (item.name.Equals(MazeUnit.EAST) && !item.activeSelf)
                    waysOpen |= OpenDirections.East;
            }

            return waysOpen;
        }

        private StringBuilder AppendPathWithMatrix(beMobileMaze maze, PathInMaze path)
        {
            StringBuilder pathAsTextMatrix = new StringBuilder();

            var pathID = string.Format("Path: {0} matlab path matrix:\t", path.ID);

            var pathElements = path.PathAsLinkedList.ToList();

            pathAsTextMatrix.Append(pathID);

            pathAsTextMatrix.Append(" [ ");

            var grid = new MazeGridTraversal(maze);

            grid.travers(
                s => {

                    string content = "0";

                    pathAsTextMatrix.Append(content);

                    if (s.ColumnsLeft > 0)
                        pathAsTextMatrix.Append(", ");

                    if (s.ColumnsLeft == 0)
                        pathAsTextMatrix.Append("; ");

                },
                (u,s) => {
                    var currentUnitIsPathElement = pathElements.Any(
                        e => e.Unit.GridID == new Vector2(s.Column, s.Row));

                    if (currentUnitIsPathElement)
                    {
                        pathAsTextMatrix.Append("2");
                    }
                    else
                    {
                        pathAsTextMatrix.Append("1");
                    }


                    if (s.ColumnsLeft > 0)
                        pathAsTextMatrix.Append(", ");

                    if (s.ColumnsLeft == 0)
                        pathAsTextMatrix.Append("; ");
                }
                );

            #region

            //for (int y_i = 0; y_i < maze.Rows; y_i++)
            //{
            //    StringBuilder rowAsLine = new StringBuilder();

            //    for (int x_i = 0; x_i < maze.Columns; x_i++)
            //    {

            //        var currentUnitIsPathElement = pathElements.Any(e => e.Unit.GridID == new UnityEngine.Vector2(x_i, y_i));

            //        if (maze[x_i, y_i] == null)
            //        {
            //            rowAsLine.Append("0");
            //        }
            //        else if (currentUnitIsPathElement)
            //        {
            //            rowAsLine.Append("2");
            //        }
            //        else
            //        {
            //            rowAsLine.Append("1");
            //        }

            //        if (x_i != maze.Columns - 1)
            //            rowAsLine.Append(", ");
            //    }

            //    if (y_i != maze.Rows - 1)
            //    {
            //        rowAsLine.Append("; ");
            //    }

            //    pathAsTextMatrix.Append(rowAsLine.ToString());
            //}

            #endregion original

            pathAsTextMatrix.Append(" ] ");

            return pathAsTextMatrix;
        }
        
    }
}
