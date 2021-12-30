using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Frontend.Scripts
{
    public static class SelectableExtensions
    {
        //converting selectable (ui element) to selected type
        public static T ReturnAs<T>(this Selectable selectable) where T : Selectable
        {
            return selectable as T;
        }
    }
        
}
