using GLShared.General.Interfaces;
using GLShared.General.Models;
using GLShared.Networking.Components;
using GLShared.Networking.Interfaces;
using Sfs2X.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototyping.Scripts.Components
{
    public class PrototypingSyncManager : MonoBehaviour, ISyncManager
    {
        public double CurrentServerTime => 0;

        public int SpawnedPlayersAmount => 0;

        public void Initialize()
        {
        }

        public void SyncInputs(PlayerInput _)
        {
        }

        public void SyncPosition(PlayerEntity _)
        {
        }

        public void SyncShell(ShellEntity _)
        {
            
        }

        public void SyncTurretTransform(ITurretController _)
        {
        }

        public void TryCreatePlayer(string _, Vector3 spawnPosition, Vector3 spawnEulerAngles)
        {
        }

        public void TryCreateShell(string _, string databaseId, int sceneId, Vector3 spawnPosition, Vector3 spawnEulerAngles)
        {
        }

        public virtual void TryDestroyingShell(int sceneIdentifier)
        {

        }
    }
}
