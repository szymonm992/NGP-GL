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

        public (bool, string) IsPasswordValid(string inputPassword)
        {
            int passwordLength = inputPassword.Length;

            if (passwordLength > 16)
            {
                return new(false, "Podane has³o jest zbyt d³ugie!");
            }

            if(passwordLength < 4)
            {
                return new(false, "Podane has³o jest zbyt krótkie!");
            }

            return new(true, string.Empty);
        }

        public (bool, string) IsLoginValid(string inputLogin)
        {
            if(inputLogin.Length > 16)
            {
                return new(false, "Podany login jest zbyt d³ugi!");
            }

            if(inputLogin.Length < 4)
            {
                return new(false, "Podany login jest zbyt krótki!");
            }

            return new(true, string.Empty);
        }
    }
}
