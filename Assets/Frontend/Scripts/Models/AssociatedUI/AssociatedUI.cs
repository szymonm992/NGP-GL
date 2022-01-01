using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UInc.Core.Utilities;
using System;

namespace Frontend.Scripts.Models
{
    public abstract class AssociatedUI
    {
        [Tooltip("UI elements assigned to the state/selectable elements to be turned off or turned on (derriving from Selectable class)")]
        [Header("Selectables")]
        [SerializeField] protected List<SelectableKVP> ui_elements = new List<SelectableKVP>();
        public virtual void ToggleUI(bool val) =>  ui_elements.ForEach(btn => btn.UI_element.interactable = val);
       
    }

   
}
