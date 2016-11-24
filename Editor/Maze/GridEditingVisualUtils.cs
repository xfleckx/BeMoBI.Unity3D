using UnityEngine;
using UnityEditor;
using Assets.SNEED.Mazes;
using System;

namespace Assets.SNEED.EditorExtensions
{
    public static class GridEditingVisualUtils
    {

        public static bool UpdateHitPosition(Transform targetLocalSpace, ref Vector3 lastHitPosition)
        {
            // build a plane object that 
            var p = new Plane(targetLocalSpace.TransformDirection(targetLocalSpace.up), targetLocalSpace.position);

            // build a ray type from the current mouse position
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            // stores the hit location
            var hit = new Vector3();

            // stores the distance to the hit location
            float dist;

            // cast a ray to determine what location it intersects with the plane
            if (p.Raycast(ray, out dist))
            {
                // the ray hits the plane so we calculate the hit location in world space
                hit = ray.origin + (ray.direction.normalized * dist);
            }

            // convert the hit location from world space to local space
            var value = targetLocalSpace.InverseTransformPoint(hit);

            // if the value is different then the current mouse hit location set the 
            // new mouse hit location and return true indicating a successful hit test
            if (value != lastHitPosition)
            {
                lastHitPosition = value;
                return true;
            }

            // return false if the hit test failed
            return false;
        }

        internal static Vector2 ConstrainTilePosition(Vector2 pos, int rows, int columns)
        {
            // do a check to ensure that the row and column are with the bounds of the tile maze
            var col = pos.x;
            var row = pos.y;

            if (row < 0)
            {
                row = 0;
            }

            if (row > rows - 1)
            {
                row = rows - 1;
            }

            if (col < 0)
            {
                col = 0;
            }

            if (col > columns - 1)
            {
                col = columns - 1;
            }
            
            // return the column and row values
            return new Vector2(col, row);
        }

        public static Vector2 GetTilePositionFromMouseLocation(Transform origin, Vector3 tileSize, Vector3 mouseHit, Vector2 tiles)
        {
            // calculate column and row location from mouse hit location
            var pos = new Vector3(
            calculateIndex(  mouseHit.x, tileSize.x, (int)tiles.x),
              mouseHit.y / origin.position.y,
            calculateIndex(  mouseHit.z, tileSize.z, (int)tiles.y));
            
            return new Vector2(pos.x, pos.z );
        }
        public static Vector3 RecalculateMarkerPosition(Vector2 tilePosition, Vector3 tileSize)
        {
            return new Vector3(
                tilePosition.x * tileSize.x + (tileSize.x / 2),
                0,
                tilePosition.y * tileSize.z + (tileSize.z / 2));
        }

        public static bool CameraIsATopDownView(Camera cam, Transform target)
        {
            var currentCamForward = cam.transform.TransformVector(cam.transform.forward);

            var currentMazeDown = target.transform.TransformVector(-target.transform.forward);

            return currentCamForward == currentMazeDown;
        }

       
        /// <summary>
        /// A custom floor function
        /// </summary>
        /// <param name="position"></param>
        /// <param name="cellWidth"></param>
        /// <param name="numCells"></param>
        /// <returns></returns>
        private static int calculateIndex(float position, float cellWidth, int numCells) {

            if (position > cellWidth * numCells)
                position = cellWidth * numCells;

            for (int i = 1; i <= numCells; i++) {
                float delta = position / i;
                if (delta > cellWidth) { continue; }
                else{
                    return (i - 1);
                }
            }

            return 0;
        }
    }
}