using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Frontend.Scripts.Models
{

    [Serializable]
    public struct SelectableKVP
    {
        public string Name;
        public Selectable UI_element;
    }

}
