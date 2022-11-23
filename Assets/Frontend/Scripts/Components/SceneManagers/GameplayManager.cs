using Frontend.Scripts.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class GameplayManager : MonoBehaviour
    {
        private void Start()
        {
            List<int> test = new List<int>() {  2, 4, 1, -1, 0,-100, 6, 13, -22, 74, 1, 23, 78, 45, 24, 0 };
            test.QuickSortList(0, test.Count-1);

           for(int i=0;i<test.Count;i++)
            {
                //Debug.Log("element at " + i + " is equal to" + test[i]);
            }
        }

    }
}
