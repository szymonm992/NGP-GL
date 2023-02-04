using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class FormValidator
    {
        public delegate void DisplayErrorDelegate(string content);

        public (bool, string) IsPasswordValid(string inputPassword)
        {
            int passwordLength = inputPassword.Length;

            if (passwordLength > 24)
            {
                return new (false, "Given passsword is too long!");
            }

            if (passwordLength < 4)
            {
                return new (false, "Given password is too short!");
            }

            return new (true, string.Empty);
        }

        public (bool, string) IsLoginValid(string inputLogin)
        {
            if (inputLogin.Length > 24)
            {
                return new (false, "Given login is too long!");
            }

            if (inputLogin.Length < 4)
            {
                return new (false, "Given login is too short!");
            }

            return new (true, string.Empty);
        }
    }
}
