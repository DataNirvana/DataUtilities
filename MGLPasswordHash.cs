using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Text;
using System.Security;


//--------------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     5-Jul-2015 - Updated version of the MGLEncryption class, focused on ONE WAY encryption of password strings
    ///     And using the Rfc2898DeriveBytes instead of the obsolete PasswordDeriveBytes class
    ///     And we are now also adding additional padding to the start of each password, so even the same text in different passwords is unique we also need
    ///     to do something about storing a password specific salt each time!
    ///     Use a hash to ensure this is a one time conversion ...
    ///
    ///     Good references:
    ///         https://crackstation.net/hashing-security.htm - salts have to be created each time a password is changed ...
    ///         https://www.fishnetsecurity.com/6labs/resource-library/white-paper/best-practices-secure-forgot-password-feature - recommends to use security questions on
    ///         the web page and an "out-of-band" input through e.g. provision of a temporary key through sms or email.  Unfortunately, we are not storing any good
    ///         security question stuff so good enough to do the password reset via email option
    ///         http://www.codeproject.com/Articles/704865/Salted-Password-Hashing-Doing-it-Right
    ///
    /// </summary>
    public class MGLPasswordHash {

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public MGLPasswordHash() {
            // Eaaaaasssssssyyy
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        // 13-Aug-15 - increased the number of iterations
        private static int SaltIterations = 1300;
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static int SaltLength = 8;
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Encrypts the given string
        /// </summary>
        public static StringBuilder EncryptPassword(SecureString password) {
            return EncryptPassword(SecureStringWrapper.Decrypt(password));
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Encrypts the given string
        /// </summary>
        public static StringBuilder EncryptPassword(StringBuilder password) {
            StringBuilder encryptedPassword = null;

            try {

                // First we need to turn the input string into a byte array.
                // 5-Jul-15 - by adding a random padding of 8 chars at the start, we ensure that a password of "Hello World" will not be
                // the same twice when encrypted
                StringBuilder randomPaddingSalt = MGLEncryption.GetSalt(SaltLength);

                // Turn the password into Key and IV.  We are using salt to make it harder to guess our key
                // using a dictionary attack - trying to guess a password by enumerating all possible words.
                // and generate a password specific salt that we will append to the end of the string ...
                StringBuilder randomAlgSaltStr = MGLEncryption.GetSalt(SaltLength);

                encryptedPassword = Encrypt(password, randomPaddingSalt.ToString(), randomAlgSaltStr.ToString(), SaltIterations);

            } catch (Exception ex) {
                Logger.LogError(9, "Error trying to encrypt a password. " + ex.StackTrace);
            }

            return encryptedPassword;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Encrypt a string into a string using a password
        ///     Uses Encrypt(byte[], byte[], byte[])
        /// </summary>
        protected static StringBuilder Encrypt(StringBuilder clearText, string randomPaddingSalt, string randomAlgSaltStr, int numIterations) {

            // First we need to turn the input string into a byte array.
            // 5-Jul-15 - by adding a random padding of 8 chars at the start, we ensure that a password of "Hello World" will not be
            // the same twice when encrypted
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(randomPaddingSalt + clearText);

            // Turn the password into Key and IV.  We are using salt to make it harder to guess our key
            // using a dictionary attack - trying to guess a password by enumerating all possible words.
            // and generate a password specific salt that we will append to the end of the string ...
            byte[] randomAlgSalt = System.Text.Encoding.Unicode.GetBytes(randomAlgSaltStr);

            // 5-Jul-15 - use the Rfc2898DeriveBytes instead of the obsolete PasswordDeriveBytes
            Rfc2898DeriveBytes db = new Rfc2898DeriveBytes(clearBytes, randomAlgSalt, numIterations);

            byte[] encryptedHashedData = db.GetBytes(128);

            // Now we need to turn the resulting byte array into a string.  A common mistake would be to use an Encoding class for that.
            // It does not work because not all byte values can be represented by characters.
            // We are going to be using Base64 encoding that is designed exactly for what we are trying to do.
            StringBuilder base64Str = new StringBuilder();
            base64Str.Append(Convert.ToBase64String(encryptedHashedData));

            // And when we return the string we will add on our salts and the number of iterations to the end of the string ...
            //base64Str = base64Str + ":" + randomPaddingSalt + ":" + randomAlgSaltStr + ":" + numIterations;
            base64Str.Append(":");
            base64Str.Append(randomPaddingSalt);
            base64Str.Append(":");
            base64Str.Append(randomAlgSaltStr);
            base64Str.Append(":");
            base64Str.Append(numIterations);

            return base64Str;

        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Compares two passwords - note that this method includes a Threading element so that hackers cannot tell how similar strings are
        ///     by using code to time the comparison - e.g. a match on the first and second chars will be marginally slower than one that does not
        ///     match on the first character
        ///
        ///     The test password is UNencrypted, while the current password is fully encrypted ...
        /// </summary>
        public static bool Compare(StringBuilder testPassword, StringBuilder currentPassword) {

            bool success = false;
            string[] bits = null;

            try {

                // get the attributes from the current Password
                // have to conver it to a string unfortunately to split on the colons ....
                bits = currentPassword.ToString().Split(new string[] { ":" }, StringSplitOptions.None);

                int numIterations = 0;
                int.TryParse(bits[3], out numIterations);

                // Encrypt the test password
                StringBuilder encryptedTestPassword = Encrypt(testPassword, bits[1], bits[2], numIterations);

                // now do basic string comparison to compare the two strings!
                if (MGLEncryption.AreEqual(encryptedTestPassword, currentPassword) == true) {
                    success = true;
                }

            } catch (Exception ex) {

                Logger.LogError(9, "MGLPasswordHash - error occurred while comparing two passwords: " + ex.ToString());

            } finally {

                // always kill the array of bits
                bits = null;
            }

            // Now introduce a random response element, delaying by up to 100 ms. (0.1 seconds)
            Thread.Sleep(new Random().Next(0, 100));

            return success;
        }


        ////---------------------------------------------------------------------------------------------------------------------------------------------------------------
        ///// <summary>
        /////     Generates a long key from a set of input characters
        /////     13-Oct-2015 - Use the MGLEncryption method instead ...
        ///// </summary>
        //public static string GetSalt(int saltLength) {

        //    // Use a dedicated random cryptographic class to build a random list of characters to the specified length ...
        //    // use a Cryptographically Secure Pseudo-Random Number Generator (CSPRNG)
        //    RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider();
        //    byte[] salt = new byte[saltLength];
        //    csprng.GetBytes(salt);

        //    string base64Str = Convert.ToBase64String(salt);

        //    return base64Str;
        //}


        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static bool TestEncryption() {
            bool success = false;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------------
            // Test the general encryption stuff ...
            StringBuilder tempKey = MGLEncryption.GetSalt(30);
            StringBuilder tempKey2 = MGLEncryption.GetSalt(30);

            // Test the mgl encryption 2 ...
            StringBuilder testPword2 = MGLPasswordHash.EncryptPassword(tempKey);
            StringBuilder testPword3 = MGLPasswordHash.EncryptPassword(tempKey2);

            bool theSame3 = MGLPasswordHash.Compare(tempKey, testPword2);
            bool theSame4 = MGLPasswordHash.Compare(tempKey, testPword3);

            success = theSame3 == true && theSame4 == false;

            return success;
        }


    }
}





