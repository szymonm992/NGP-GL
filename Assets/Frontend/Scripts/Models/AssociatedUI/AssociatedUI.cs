using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UInc.Core.Utilities;
using System;

namespace Frontend.Scripts.Models
{
    [System.Serializable]
    public abstract class AssociatedUI
    {

        [SerializeField] protected List<SelectableKVP> ui_elements = new List<SelectableKVP>();
      
        public AssociatedUI()
        {
        }

        public virtual void ToggleUI(bool val) => ui_elements.ForEach(btn => btn.UI_element.interactable = val);
       
    }

   
}
