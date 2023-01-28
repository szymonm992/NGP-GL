using GLShared.General.Models;
using GLShared.General.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ShellsDatabase", menuName = "UT/Databases/Frontend shells database")]
    public class FrontendShellsDatabase : ShellsDatabase
    {
        [SerializeField] private ShellEntryInfo[] allShells;

        public override IEnumerable<ShellEntryInfo> AllShells => allShells;
    }
}
