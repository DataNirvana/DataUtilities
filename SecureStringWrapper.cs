using System;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     21-Sep-2015 - Simple Encrypt and Decrypt wrappers for Secure String ...
    ///     http://www.codeproject.com/Tips/549109/Working-with-SecureString
    /// </summary>
    public static class SecureStringWrapper {

        //---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     This method clears the given string value once the string has been secured.
        /// </summary>
        public static SecureString Encrypt(string str) {

            if (str != null && str.Length > 0) {
                char[] chars = str.ToCharArray();
                str = "";
                return Encrypt( chars);

            }
            return new SecureString();
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     This method clears the given string value once the string has been secured.
        /// </summary>
        public static SecureString Encrypt(StringBuilder str) {

            if (str != null && str.Length > 0) {
                char[] chars = str.ToString().ToCharArray();
                str = null;
                return Encrypt(chars);

            }
            return new SecureString();
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     This method clears the given char array once the SecureString has been created.
        /// </summary>
        public static SecureString Encrypt(char[] chars) {
            SecureString secureStr = new SecureString();
            if (chars != null && chars.Length > 0 ) {
                foreach (char c in chars) {
                    secureStr.AppendChar(c);
                }
                chars = null;
            }
            return secureStr;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returning a StringBuilder means the information CAN be cleared in VM before being returned
        /// </summary>
        public static StringBuilder Decrypt(SecureString secureStr) {
            IntPtr str = IntPtr.Zero;
            try {
                str = Marshal.SecureStringToGlobalAllocUnicode(secureStr);
                StringBuilder s = new StringBuilder(Marshal.PtrToStringUni(str));
                return s;
            } finally {
                // it is critical that this is in a finally block so that it always runs and removes the pointer to the string
                Marshal.ZeroFreeGlobalAllocUnicode(str);
            }
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Compares the two secure strings and returns true if they are equivalent textually, both null or both empty
        ///     if isCaseSensitive == true, we also compare the case sensitivity of the information - but this needs a string comparison
        ///     so will result in strings popping into memory and is probably a little bit slower ...
        /// </summary>
        public static bool AreEqual(SecureString ss1, SecureString ss2, bool isCaseSensitive) {

            return MGLEncryption.AreEqual(SecureStringWrapper.Decrypt(ss1), SecureStringWrapper.Decrypt(ss2), isCaseSensitive);

        }



    }
}
