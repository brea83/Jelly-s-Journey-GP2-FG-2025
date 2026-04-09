using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NGAME
{
    [System.Serializable]
    public class SceneBounds
    {
        public Vector2 MinPoint { get; private set; }
        public Vector2 MaxPoint { get; private set; }
        public float AspectRatio { get; private set; }

        public float Width()
        {
            return MaxPoint.x - MinPoint.x;
        }
        public float Height()
        {
            return MaxPoint.y - MinPoint.y;
        }
        public Vector2 GetWidthAndHeight()
        {
            return new Vector2(Width(), Height());
        }

        // TODO add centroid finding
        public SceneBounds (SceneConnectionsData connectionsData = null, SceneSpawnData spawnData = null)
        {
            connectionsData.UpdateBounds();

            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            if(connectionsData != null) 
            {
                Vector2 connectionMin = connectionsData.MinPoint;
                Vector2 connectionMax = connectionsData.MaxPoint;

                min = Vector2.Min(connectionMin, min);
                max = Vector2.Max(connectionMax, max);
            }

            if(spawnData != null)
            {
                List<Vector3> spawnPositions = spawnData.SpawnPoints.ConvertAll(x => x.Position);
            
                foreach(Vector3 position3d in spawnPositions)
                {
                    Vector2 position = new Vector2(position3d.x, position3d.z);

                    min = Vector2.Min(position, min);
                    max = Vector2.Max(position, max);
                }
            }

            MinPoint = min; 
            MaxPoint = max;

            float width = MaxPoint.x - MinPoint.x;
            float height = MaxPoint.y - MinPoint.y;
            AspectRatio = width / height;

        }

        public SceneBounds(List<RegionConnectionData> connectionsData, List<SpawnerData> spawnData)
        {
            UpdateBounds(connectionsData, spawnData);
        }

        public void UpdateBounds(List<RegionConnectionData> connectionsData = null, List<SpawnerData> spawnData = null)
        {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            if (connectionsData != null)
            {
                List<Vector3> connectionPositions = connectionsData.ConvertAll(x => x.Position);

                foreach (Vector3 position3d in connectionPositions)
                {
                    Vector2 position = new Vector2(position3d.x, position3d.z);

                    min = Vector2.Min(position, min);
                    max = Vector2.Max(position, max);
                }
            }

            if (spawnData != null)
            {
                List<Vector3> spawnPositions = spawnData.ConvertAll(x => x.Position);

                foreach (Vector3 position3d in spawnPositions)
                {
                    Vector2 position = new Vector2(position3d.x, position3d.z);

                    min = Vector2.Min(position, min);
                    max = Vector2.Max(position, max);
                }
            }

            MinPoint = min;
            MaxPoint = max;

            float width = MaxPoint.x - MinPoint.x;
            float height = MaxPoint.y - MinPoint.y;
            AspectRatio = width / height;
        }

        public void AddPointToBounds(Vector2 point) 
        {
            MinPoint = Vector2.Min(point, MinPoint);
            MaxPoint = Vector2.Max(point, MaxPoint);
            float width = MaxPoint.x - MinPoint.x;
            float height = MaxPoint.y - MinPoint.y;
            AspectRatio = width / height;
        }
    }
}
