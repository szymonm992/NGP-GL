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
        [SerializeField] public List<SelectableKVP> ui_elements = new List<SelectableKVP>();

        public virtual void ToggleUI(bool val) =>  ui_elements.ForEach(btn => btn.UI_element.interactable = val);

       
      

       
    }

   
}
