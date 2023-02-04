using Frontend.Scripts.Models;
using GLShared.General.Utilities;
using Sfs2X.Entities.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Extensions
{
    public static class GeneralExtensions 
    {
        public static PlayerSpawnData ToPlayerSpawnData(this ISFSObject data)
        {
            return new PlayerSpawnData()
            {
                Username = data.GetUtfString("username"),
                SpawnPosition = new Vector3(data.GetFloat("spawnPositionX"),data.GetFloat("spawnPositionY"),
                data.GetFloat("spawnPositionZ")),

                SpawnEulerAngles = new Vector3(data.GetFloat("spawnRotationX"), data.GetFloat("spawnRotationY"),
                    data.GetFloat("spawnRotationZ")),
            };
        }

        public static ShellSpawnData ToShellSpawnData(this ISFSObject data)
        {
            return new ShellSpawnData()
            {
                OwnerUsername = data.GetUtfString("owner"),
                DatabaseIdentifier = data.GetUtfString("dbId"),
                SceneId = data.GetInt("id"),

                SpawnPosition = new Vector3(data.GetFloat("spawnPositionX"), data.GetFloat("spawnPositionY"),
                data.GetFloat("spawnPositionZ")),

                SpawnEulerAngles = new Vector3(data.GetFloat("spawnRotationX"), data.GetFloat("spawnRotationY"),
                    data.GetFloat("spawnRotationZ")),

                TargetingPosition = new Vector3(data.GetFloat("targetPosX"), data.GetFloat("targetPosY"),
                    data.GetFloat("targetPosZ")),
            };
        }
    }
}
