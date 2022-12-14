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
                error = "Podany login jest zbyt d�ugi!";
            else if (login.Length < 4)
                error = "Podany login jest zbyt kr�tki!";
            else if (password.Length > 16)
                error = "Podane has�o jest zbyt d�ugie!";
            else if (password.Length < 4)
                error = "Podane has�o jest zbyt kr�tkie!";
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
                return new(false, "Podane has�o jest zbyt d�ugie!");
            }

            if(passwordLength < 4)
            {
                return new(false, "Podane has�o jest zbyt kr�tkie!");
            }

            return new(true, string.Empty);
        }

        public (bool, string) IsLoginValid(string inputLogin)
        {
            if(inputLogin.Length > 16)
            {
                return new(false, "Podany login jest zbyt d�ugi!");
            }

            if(inputLogin.Length < 4)
            {
                return new(false, "Podany login jest zbyt kr�tki!");
            }

            return new(true, string.Empty);
        }
    }
}
