using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGAME
{
    public interface ISpawnPoint
    {
        public List<SO_SpawnTypeTag> AllowedSpawnableTypes { get; set; }

        public Vector3 GetPosition();
    }
}
