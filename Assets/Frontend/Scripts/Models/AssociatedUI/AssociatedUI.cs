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

        public SelectableKVP GetElement(string name)
        {
            return ui_elements.Where(element => element.Name == name).FirstOrDefault();
        }
        public virtual void ToggleUI(bool val) => ui_elements.ForEach(btn => btn.UI_element.interactable = val);
       
    }

   
}
