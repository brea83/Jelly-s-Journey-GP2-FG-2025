using UnityEngine;
namespace EncounterSystem
{
    public class SpawnTags : MonoBehaviour
    {
        // THIS IS A TEMP SOLUTION UNTIL I HAVE TIME TO INTEGRATE SPAWNING WITH EXISTING ENEMY DATA ARCHITECTURE
        // NEED TO SYNC UP AND FIGURE OUT IF WE'RE USING UNITY'S TAG SYSTEM OR STORING TYPES ON SCRIPT SIDE.
        public AllowedEnemyType SpawnLocationType;

    }
}