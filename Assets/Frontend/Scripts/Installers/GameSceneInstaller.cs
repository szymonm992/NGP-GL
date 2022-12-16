using UnityEngine;
using Zenject;
using Frontend.Scripts.Models;
using Frontend.Scripts.Components;
using Frontend.Scripts.Signals;
using Frontend.Scripts.Interfaces;
using GLShared.Networking;
using GLShared.General.ScriptableObjects;
using Frontend.Scripts.Components.Temporary;
using GLShared.General.Signals;
using GLShared.General.Models;
using GLShared.Networking.Components;
using UnityEngine.TextCore.Text;

namespace Frontend.Scripts
{
    public class GameSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            InstallMain();
        }

        private void InstallMain()
        {
            
        }


       
    }

 
}
