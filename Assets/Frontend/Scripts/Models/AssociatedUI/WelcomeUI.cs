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
        
        /// <summary>
        /// Return selectable by its name
        /// </summary>
        /// <param name="name">name of the element</param>
        /// <returns></returns>
        public Selectable GetElement(string name)
        {
            return ui_elements.Where(selectable => selectable.Name == name).FirstOrDefault().UI_element;
        }
    
        public void ClearError() => error_text.text = "";
        public void DisplayError(string content) => error_text.text = content;
       
    }
}
