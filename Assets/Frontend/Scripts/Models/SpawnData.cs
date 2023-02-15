using UnityEngine;

namespace Frontend.Scripts.Models
{
    public class PlayerSpawnData
    {
        public string Username { get; set; }
        public string Team { get; set; }
        public Vector3 SpawnPosition { get; set; }
        public Vector3 SpawnEulerAngles { get; set; }
    }

    public class ShellSpawnData
    { 
        public string OwnerUsername { get; set; }
        public string DatabaseIdentifier { get; set; }
        public int SceneId { get; set; }
        public Vector3 SpawnPosition { get; set; }
        public Vector3 SpawnEulerAngles { get; set; }
        public Vector3 TargetingPosition { get; set; }
    }
}
