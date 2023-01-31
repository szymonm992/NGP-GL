using Frontend.Scripts.Models;
using Sfs2X.Entities.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Extensions
{
    public static class GeneralExtensions 
    {
        public static SpawnData ToSpawnData(this ISFSObject data)
        {
            return new SpawnData()
            {
                Username = data.GetUtfString("username"),

                Identifier = data.GetUtfString("id"),

                SpawnPosition = new Vector3(data.GetFloat("spawnPositionX"),data.GetFloat("spawnPositionY"),
                data.GetFloat("spawnPositionZ")),

                SpawnEulerAngles = new Vector3(data.GetFloat("spawnRotationX"), data.GetFloat("spawnRotationY"),
                    data.GetFloat("spawnRotationZ")),
            };
        }
    }
}
