using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    [CreateAssetMenu(fileName = "FrontVisualSettings", menuName = "UT/Frontend/Front visual settings")]
    public class FrontVisualSettings : ScriptableObject
    {
        [Header("Outline")]
        [Range(3f, 5f)]
        [SerializeField] private float outlineWidth = 3f;
        [SerializeField] private Color outlineColor = Color.black;

        public float OutlineWidth => outlineWidth;
        public Color OutlineColor => outlineColor;
    }
}
