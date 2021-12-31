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
        [GameStateEntity(typeof(UnknownState))]
        Unknown,

        [GameStateEntity(typeof(WelcomeState))]
        Welcome,

        [GameStateEntity(typeof(OnLoginState))]
        OnLogin,

        [GameStateEntity(typeof(LobbyState))]
        Lobby,
    }

    public static class GameStateHelper
    {
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

}
