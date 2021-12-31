using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Frontend.Scripts.Models
{
    [System.Serializable]
    public class WelcomeUI : AssociatedUI
    {
        
        [SerializeField] private Text error_text;
        public override void ToggleUI(bool val)
        {
            base.ToggleUI(val);
            ClearError();
        }
        
        public Selectable GetElement(string name)
        {
            foreach (SelectableKVP btn in ui_elements)
            {
                if (btn.Name == name) return btn.UI_element;
            }
            return null;
        }
   
        
        public void ClearError() => error_text.text = "";
        public void DisplayError(string content)
        {

            error_text.text = content;
        }
       
    }
}
