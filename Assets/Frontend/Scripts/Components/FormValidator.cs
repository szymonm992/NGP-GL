using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class FormValidator
    {
        public delegate void DisplayErrorDelegate(string content);

        public bool IsLoginFormValid(string login, string password, DisplayErrorDelegate delegateError)
        {
            string error = "";

            if (login.Length > 16)
                error = "Podany login jest zbyt d³ugi!";
            else if (login.Length < 4)
                error = "Podany login jest zbyt krótki!";
            else if (password.Length > 16)
                error = "Podane has³o jest zbyt d³ugie!";
            else if (password.Length < 4)
                error = "Podane has³o jest zbyt krótkie!";
            else
                return true;

            delegateError?.Invoke(error);
            return false;
        }
    }
}
