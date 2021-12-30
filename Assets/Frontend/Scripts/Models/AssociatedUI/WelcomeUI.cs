using System.Collections;
using System.Collections.Generic;
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

        public void ClearError() => this.error_text.text = "";
        public void DisplayError(string content) => this.error_text.text = content;
    }
}
