using Frontend.Scripts.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
namespace Frontend.Scripts.Components
{
    public enum GameState
    {
        Unknown,
        [PlaceholderFactory(typeof(CalibrationState.Factory))]
        [GameStateEntity(typeof(CalibrationState))]
        Calibration,

        [PlaceholderFactory(typeof(LobbyState.Factory))]
        [GameStateEntity(typeof(LobbyState))]
        Lobby,
    }

   

    public static class GameStateHelper
    {
        public static Type GetTypeOfState(this GameState state)
        {
            return GeneralAttributeHelper.GetEnumAttributeValue<GameState, PlaceholderFactoryAttribute, Type>(state, attr => attr.AttributeType);
        }

        public static Type GetTypeOfBaseClass(this GameState state)
        {
            return GeneralAttributeHelper.GetEnumAttributeValue<GameState, GameStateEntityAttribute, Type>(state, attr => attr.BaseClassType);
        }
    }

    internal class GameStateEntityAttribute : Attribute
    {
        public Type BaseClassType { get; private set; }
        public GameStateEntityAttribute(Type type)
        {
            BaseClassType = type;
        }
    }
    internal class PlaceholderFactoryAttribute : Attribute
    {
        public Type AttributeType { get; private set; }
        public PlaceholderFactoryAttribute(Type type) {
            AttributeType = type;
        }
    }

}
