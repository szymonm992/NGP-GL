using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public static class GeneralHelper 
    {
        public static void QuickSortList<T>(this List<T> inputList, int left, int right)
            where T : IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T> //we only want number types so we add some constraints
        {
            if(left<right)
            {
                var pivot = inputList.PartitionInternalList(left, right);
                inputList.QuickSortList(0, pivot - 1);
                inputList.QuickSortList(pivot+1,right);
            }
        }

       
        public static void SwapElementsInList<T>(this List<T> inputList, int i, int j)
            where T : IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T> 
        {
            var temp = inputList[i];
            inputList[i] = inputList[j];
            inputList[j] = temp;
        }

        private static int PartitionInternalList<T>(this List<T> inputList, int left, int right)
             where T : IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
        {
            int index = left;
            T pivot = inputList[left];
            for(int i = left+1; i <= right; i++)
            {
                if(inputList[i].CompareTo(pivot) <= 0)
                {
                    index++;
                    inputList.SwapElementsInList(index, i);
                }
            }
            inputList.SwapElementsInList(index, left);
            return index;
        }

    }
}
