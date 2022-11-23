using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components.Temporary
{
    public class TempGameManager : MonoBehaviour, IInitializable
    {
        [Inject] private readonly SignalBus signalBus;

        [SerializeField] private GameObjectContext playerContext;

        public void Initialize()
        {
            signalBus.Fire(new BattleSignals.CameraSignals.OnCameraBound()
            {
                context = playerContext,
                startingEulerAngles = playerContext.transform.eulerAngles,
                inputProvider = playerContext.Container.Resolve<IPlayerInputProvider>()
            });
        }
    }
}
