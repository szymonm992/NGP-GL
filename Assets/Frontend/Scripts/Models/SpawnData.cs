using Sfs2X.Entities.Data;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Frontend.Scripts.Models
{
    public class SpawnData
    {
        public string Username { get; set; }
        public Vector3 SpawnPosition { get; set; }
        public Quaternion SpawnRotation { get; set; }
        
    }
}
