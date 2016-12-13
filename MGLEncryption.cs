using System;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Security;
using System.Text;


//--------------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     5-Jul-2015 - Updated version of the MGLEncryption class, using longer application specific private encryption keys
    ///     That can be set during the application start
    ///     And using the Rfc2898DeriveBytes instead of the obsolete PasswordDeriveBytes class
    ///     And we are now also adding additional padding to the start of each password, so even the same text in different passwords is unique we also need to do something about storing a password specific salt each time!
    ///     Use a hash to ensure this is a one time conversion ...
    ///
    ///     Good references:
    ///         http://blog.mking.io/password-security-best-practices-with-examples-in-csharp/ - excellent overview of what to DO and NOT to do
    ///         https://crackstation.net/hashing-security.htm - salts have to be created each time a password is changed ...
    ///         https://www.fishnetsecurity.com/6labs/resource-library/white-paper/best-practices-secure-forgot-password-feature - recommends to use security questions on
    ///         the web page and an "out-of-band" input through e.g. provision of a temporary key through sms or email.  Unfortunately, we are not storing any good
    ///         security question stuff so good enough to do the password reset via email option
    ///         http://www.codeproject.com/Articles/704865/Salted-Password-Hashing-Doing-it-Right
    ///         http://codereview.stackexchange.com/questions/20608/is-this-rijndael-secure-enough-for-use-in-production-systems
    ///         http://security.stackexchange.com/questions/52041/is-using-sha-512-for-storing-passwords-tolerable
    ///
    ///
    /// </summary>
    public class MGLEncryption {

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The encryption constructor.  Probably never used as almost all the methods in this class are static!
        /// </summary>
        public MGLEncryption() {
            // Eaaaaasssssssyyy
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The crytographic key (private key) to use when running the encryptions.
        ///     CHANGING THIS KEY WILL MEAN PREVIOUSLY ENCRYPTED PASSWORDS WILL NOT BE DECIPHERABLE     
        /// </summary>
        private static SecureString CRYPT_KEY = null; // "***** STORE IN WEB OR CMD LINE CONFIG FILE *****";

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Sets the crytographic key (private key) to use when running the encryptions.
        ///     CHANGING THIS KEY WILL MEAN PREVIOUSLY ENCRYPTED PASSWORDS WILL NOT BE DECIPHERABLE     
        /// </summary>
        public static SecureString SetCryptKey {
            set {
                CRYPT_KEY = value;
            }
            // No get method as we dont want to allow other application components to get the crypt key (possible security issue)
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The number of times to hash the salt
        ///     13-Aug-15 - increased the default number of iterations
        /// </summary>
        private static int SaltIterations = 1300;

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Sets the number of salt hash iterations
        /// </summary>
        public static int SetSaltIterations {
            set {
                SaltIterations = value;
            }
            // No get method as we dont want to allow other application components to get this number (possible security issue)
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The length of the salt to prefix to the information
        /// </summary>
        private static int SaltLength = 8;
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The length of the IV padding to suffix to the information
        /// </summary>
        private static int SaltPaddingLength = 4;


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Encrypts the given string, and returns the encrypted version
        /// </summary>
        public static StringBuilder Encrypt(StringBuilder str) {
            StringBuilder encryptedPassword = null;
            try {

                encryptedPassword = Encrypt(str, CRYPT_KEY);

            } catch (Exception ex) {
                Logger.LogError(9, "Error trying to encrypt a string. " + ex.StackTrace);
            }

            return encryptedPassword;
        }
        

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Encrypts the given string - using the specific cryptKey
        /// </summary>
        public static StringBuilder Encrypt(StringBuilder str, SecureString cryptKey) {
            StringBuilder encryptedPassword = null;
            try {

                // First we need to turn the input string into a byte array.
                // 5-Jul-15 - by adding a random padding of 4 chars at the start, we ensure that a password of "Hello World" will not be
                // the same twice when encrypted
                StringBuilder randomPaddingSalt = GetSalt(SaltPaddingLength);

                // Turn the password into Key and IV.  We are using salt to make it harder to guess our key
                // using a dictionary attack - trying to guess a password by enumerating all possible words.
                // and generate a password specific salt that we will append to the end of the string ...
                StringBuilder randomAlgSaltStr = GetSalt(SaltLength);

                encryptedPassword = Encrypt(str, randomPaddingSalt.ToString(), randomAlgSaltStr.ToString(), SaltIterations, cryptKey);

            } catch (Exception ex) {
                Logger.LogError(9, "Error trying to encrypt a string. " + ex.StackTrace);
            }

            return encryptedPassword;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Encrypt a string into a string using a password
        ///     Uses Encrypt(byte[], byte[], byte[])
        /// </summary>
        protected static StringBuilder Encrypt(StringBuilder clearText, string randomPaddingSalt, string randomAlgSaltStr, int numIterations, SecureString cryptKey) {

            // First we need to turn the input string into a byte array.
            // 5-Jul-15 - by adding a random padding of 4 chars at the start, we ensure that a password of "Hello World" will not be
            // the same twice when encrypted
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(randomPaddingSalt + clearText);

            // Turn the password into Key and IV.  We are using salt to make it harder to guess our key
            // using a dictionary attack - trying to guess a password by enumerating all possible words.
            // and generate a password specific salt that we will append to the end of the string ...
            byte[] randomAlgSalt = System.Text.Encoding.Unicode.GetBytes(randomAlgSaltStr);

            // 5-Jul-15 - use the Rfc2898DeriveBytes instead of the obsolete PasswordDeriveBytes
            Rfc2898DeriveBytes db = new Rfc2898DeriveBytes(SecureStringWrapper.Decrypt(cryptKey).ToString(), randomAlgSalt, numIterations);

            // Now do the two way encryption of this data ...
            byte[] encryptedData = Encrypt(clearBytes, db.GetBytes(32), db.GetBytes(16));

            // Now we need to turn the resulting byte array into a string.  A common mistake would be to use an Encoding class for that.
            // It does not work because not all byte values can be represented by characters.
            // We are going to be using Base64 encoding that is designed exactly for what we are trying to do.
            StringBuilder base64Str = new StringBuilder(Convert.ToBase64String(encryptedData));

            // And when we return the string we will add on our salts and the number of hash iterations to the end of the string ...
            base64Str.Append(":");
            base64Str.Append(randomPaddingSalt);
            base64Str.Append(":");
            base64Str.Append(randomAlgSaltStr);
            base64Str.Append(":");
            base64Str.Append(numIterations);
            //base64Str = base64Str + ":" + randomPaddingSalt + ":" + randomAlgSaltStr + ":" + numIterations;

            return base64Str;

        }



        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Encrypt a byte array into a byte array using a key and an IV
        protected static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV) {
            // Create a MemoryStream to accept the encrypted bytes
            MemoryStream ms = new MemoryStream();

            // Create a symmetric algorithm - We are going to use Rijndael because it is strong and available on all platforms.
            // You can use other algorithms, to do so substitute the next line with something like: TripleDES alg = TripleDES.Create();
            //Aes alg = Aes.Create();
            Rijndael alg = Rijndael.Create();
            alg.Padding = PaddingMode.PKCS7;
            alg.KeySize = 256;
            alg.Mode = CipherMode.CBC;
            alg.BlockSize = 128;

            /*
            int keySize = alg.KeySize; // 256
            CipherMode m = alg.Mode; // CBC
            int blockSize = alg.BlockSize; // 128
            */

            // Now set the key and the IV.  We need the IV (Initialization Vector) because the algorithm is operating in its default
            // mode called CBC (Cipher Block Chaining).  The IV is XORed with the first block (8 byte) of the data before it is encrypted, and then each
            // encrypted block is XORed with the following block of plaintext.  This is done to make encryption more secure.
            // There is also a mode called ECB which does not need an IV, but it is much less secure.  So we don't want to use that one!
            alg.Key = Key;
            alg.IV = IV;

            // Create a CryptoStream through which we are going to be pumping our data.  CryptoStreamMode.Write means that we are going to be
            // writing data to the stream and the output will be written in the MemoryStream we have provided.
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);

            // Write the data and make it do the encryption
            cs.Write(clearData, 0, clearData.Length);

            // Close the crypto stream (or do FlushFinalBlock).  This will tell it that we have done our encryption and there is no more data coming in,
            // and it is now a good time to apply the padding and finalize the encryption process.
            cs.Close();

            // Now get the encrypted data from the MemoryStream.  Some people make a mistake of using GetBuffer() here, which is not the right way.
            byte[] encryptedData = ms.ToArray();

            return encryptedData;
        }



        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Decrypts the given string and returns the clear text.
        /// </summary>
        public static StringBuilder Decrypt(StringBuilder encryptedStr) {
            StringBuilder decryptedStr = null;
            try {
                decryptedStr = Decrypt(encryptedStr, CRYPT_KEY);
            } catch (Exception ex) {
                Logger.LogError(9, "Error trying to decrypt a string. " + ex.StackTrace);
            }

            return decryptedStr;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Decrypt a string into a string using the given cryptographic key
        ///     Uses Decrypt(byte[], byte[], byte[])   
        /// </summary>
        public static StringBuilder Decrypt(StringBuilder encryptedStr, SecureString cryptKey) {

            StringBuilder decryptedStr = null;

            try {

                // get the attributes from the encrypted string
                // 12-10-2015 - unfortunately, at some point we do have to convert this back to a string - and this is where!
                string[] bits = encryptedStr.ToString().Split(new string[] { ":" }, StringSplitOptions.None);

                // parse the numberOfIterations
                int numIterations = 0;
                int.TryParse(bits[3], out numIterations);
                // and convert our random alg salt to binary ...
                byte[] randomAlgSalt = System.Text.Encoding.Unicode.GetBytes(bits[2]);

                // Get the hash of the cryptoKey
                Rfc2898DeriveBytes db = new Rfc2898DeriveBytes(SecureStringWrapper.Decrypt(cryptKey).ToString(), randomAlgSalt, numIterations);

                // convert the base64 string to binary
                byte[] encryptedData = Convert.FromBase64String(bits[0]);

                // Now do the two way decryption of this data ...
                byte[] decryptedData = Decrypt(encryptedData, db.GetBytes(32), db.GetBytes(16));

                // convert the decrypted data to a string
                decryptedStr = new StringBuilder(System.Text.Encoding.Unicode.GetString(decryptedData));

                // And finally, lets chop our padding of the front ....  originally SubString( bits[ 1 ].length )
                decryptedStr = decryptedStr.Remove(0, bits[1].Length);

            } catch (Exception ex) {
                // Lets catch the null encrypted string provided!
                StringBuilder temp = new StringBuilder();
                if (encryptedStr != null && encryptedStr.Length > 0) {
                    temp.Append(encryptedStr);
                } else {
                    temp.Append("Null or empty encrypted string provided");
                }

                Logger.LogError(9, "MGLEncryption - error occurred while decrypting the given string - stacktrace: " + ex.StackTrace
                    + " and the encrypted string provided was: " + temp + " and specific exception: " + ex.ToString());

            }
            
            return decryptedStr;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Decrypt a byte array into a byte array using a key and an IV 
        /// </summary>
        protected static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV) {

            // Create a MemoryStream that is going to accept the decrypted bytes
            MemoryStream ms = new MemoryStream();

            // Create a symmetric algorithm. We are going to use Rijndael because it is strong and available on all platforms.
            // You can use other algorithms, to do so substitute the next line with something like TripleDES alg = TripleDES.Create();
            Rijndael alg = Rijndael.Create();
            alg.Padding = PaddingMode.PKCS7;
            alg.KeySize = 256;
            alg.Mode = CipherMode.CBC;
            alg.BlockSize = 128;

            // Now set the key and the IV. We need the IV (Initialization Vector) because the algorithm is operating in its default
            // mode called CBC (Cipher Block Chaining). The IV is XORed with the first block (8 byte)
            // of the data after it is decrypted, and then each decrypted block is XORed with the previous
            // cipher block. This is done to make encryption more secure.
            // There is also a mode called ECB which does not need an IV, but it is much less secure.
            alg.Key = Key;
            alg.IV = IV;

            // Create a CryptoStream through which we are going to be pumping our data. CryptoStreamMode.Write means that we are going to be
            // writing data to the stream and the output will be written in the MemoryStream we have provided.
            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);

            // Write the data and make it do the decryption
            cs.Write(cipherData, 0, cipherData.Length);

            // Close the crypto stream (or do FlushFinalBlock). This will tell it that we have done our decryption
            // and there is no more data coming in, and it is now a good time to remove the padding
            // and finalize the decryption process.
            cs.Close();

            // Now get the decrypted data from the MemoryStream. Some people make a mistake of using GetBuffer() here, which is not the right way.
            byte[] decryptedData = ms.ToArray();

            return decryptedData;
        }


        
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Compares two strings - note that this method includes a Threading element so that hackers cannot tell how similar strings are
        ///     by using code to time the comparison - e.g. a match on the first and second chars will be marginally slower than one that does not
        ///     match on the first character
        ///
        ///     The test str is UNencrypted, while the current str is fully encrypted ...
        /// </summary>
        public static bool Compare(StringBuilder testStr, StringBuilder encryptedStr) {

            return Compare(testStr, encryptedStr, CRYPT_KEY);

        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Compares two strings - note that this method includes a Threading element so that hackers cannot tell how similar strings are
        ///     by using code to time the comparison - e.g. a match on the first and second chars will be marginally slower than one that does not
        ///     match on the first character
        ///
        ///     The test str is UNencrypted, while the current str is fully encrypted ...
        /// </summary>
        public static bool Compare(StringBuilder testStr, StringBuilder encryptedStr, SecureString cryptKey) {

            bool success = false;
            string[] bits = null;

            try {

                // get the attributes from the current Password
                // unfortunately we still need to use a string here to split our info
                bits = encryptedStr.ToString().Split(new string[] { ":" }, StringSplitOptions.None);

                int numIterations = 0;
                int.TryParse(bits[3], out numIterations);

                // Encrypt the test password
                StringBuilder encryptedTestPassword = Encrypt(testStr, bits[1], bits[2], numIterations, cryptKey);

                // now do basic string comparison to compare the two strings!
                if (AreEqual(encryptedTestPassword, encryptedStr) == true) {
                    success = true;
                }

            } catch (Exception ex) {

                Logger.LogError(9, "MGLEncryption - error occurred while comparing two strings: " + ex.ToString());

            } finally {
                // kill the bits array ...
                bits = null;
            }

            // Now introduce a random response element, delaying by up to 100 ms. (0.1 seconds)
            Thread.Sleep(new Random().Next(0, 100));

            return success;
        }


        //----------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Compares two char arrays and returns true if they are equal (textually equivalent, both null or both empty)
        /// </summary>
        public static bool AreEqual(char[] chars1, char[] chars2) {
            bool equalovski = false;

            if (chars1 != null && chars2 != null && chars1.Length > 0 && chars2.Length > 0) {
                if (chars1.Length == chars2.Length) {

                    bool tempEq = true;

                    int i = 0;
                    foreach( char c in chars1) {

                        if (c.CompareTo(chars2[i]) != 0) {
                            tempEq = false;
                            break;
                        }
                        i++;
                    }

                    equalovski = tempEq;
                }
            } else if (chars1 == null && chars2 == null) {
                equalovski = true;
            } else if (chars1.Length == 0 && chars2.Length == 0) {
                equalovski = true;
            }

            return equalovski;
        }
        //----------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Compares two StringBuilders and returns true if they are equal (textually equivalent, both null or both empty)
        /// </summary>
        public static bool AreEqual(StringBuilder sb1, StringBuilder sb2) {
            bool equalovski = false;

            if (sb1 != null && sb2 != null && sb1.Length > 0 && sb2.Length > 0) {
                if (sb1.Length == sb2.Length) {

                    bool tempEq = true;

                    // There is no otherway to get at the characters in a StringBuilder so this is the best approach (although for is slower than foreach) ...
                    for (int i = 0; i < sb1.Length; i++) {

                        if (sb1[i].CompareTo(sb2[i]) != 0) {
                            tempEq = false;
                            break;
                        }
                    }

                    equalovski = tempEq;
                }
            } else if (sb1 == null && sb2 == null) {
                equalovski = true;
            } else if (sb1.Length == 0 && sb2.Length == 0) {
                equalovski = true;
            }

            return equalovski;
        }
        //----------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Compares two StringBuilders and returns true if they are equal (textually equivalent with ability to specify case sensitivity, both null or both empty)
        /// </summary>
        public static bool AreEqual(StringBuilder sb1, StringBuilder sb2, bool isCaseSensitive) {

            bool success = false;

            if (isCaseSensitive == true) {
                success = AreEqual(sb1, sb2);
            } else {
                if ((sb1 == null && sb2 != null) || (sb1 != null && sb2 == null)) {
                    success = false;
                } else {
                    success =
                        (sb1 == null && sb2 == null)
                        || (sb1.Length == 0 && sb2.Length == 0)
                        || (sb1 != null && sb2 != null && sb1.ToString().Equals(sb2.ToString(), StringComparison.CurrentCultureIgnoreCase));
                }
            }

            return success;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Chops out a bit of a StringBuilder and returns it as as separate StringBuilder ...
        /// </summary>
        public static StringBuilder SubstringStringBuilder(StringBuilder input, int startIndex, int length) {
            StringBuilder output = new StringBuilder();

            if (input != null && input.Length > 0) {
                // 13-Oct-2015 - instantiate the char array to be just the length of the output that we want
                char[] tempArray = new char[length];
                input.CopyTo(startIndex, tempArray, 0, length);

                foreach (char c in tempArray) {
                    output.Append(c);
                }

            }

            return output;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Generates a long key from a set of input characters
        /// </summary>
        public static SecureString GenerateKey() {

            return SecureStringWrapper.Encrypt(GetSalt(50));

        }
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Generates a long key from a set of input characters
        /// </summary>
        public static StringBuilder GetSalt(int saltLength) {

            /*
                Old simple approach to generating a random string - the Random class in C# is fundamentally flawed and will always 
                produce the same set of "random" numbers in the same sequence!!!
                Here are our seed characters:
                string chars = "abcdefghijklmnopqrstuvwxyz_ABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789_";
                Random random = new Random();
                string result = new string(
                    Enumerable.Repeat(chars, saltLength)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
            */

            // New approach using dedicated random cryptographic class - Build a random list of characters to the specified length ...
            // use a Cryptographically Secure Pseudo-Random Number Generator (CSPRNG)
            RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider();
            byte[] salt = new byte[saltLength];
            csprng.GetBytes(salt);

            StringBuilder base64Str = new StringBuilder(Convert.ToBase64String(salt));

            if (base64Str.Length > saltLength) {
                base64Str = MGLEncryption.SubstringStringBuilder( base64Str, 0, saltLength);
            }

            return base64Str;
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     HTML of course tries to do clever stuff by replacing certain characters with their computer representation (e.g. a space is %20)
        ///     Therefore, we want to replace these characters with known equivalents before HTML gets a chance to do this.    
        ///     This method replaces / + = and : with _S, _P, _E and _C respectively
        /// </summary>
        public static StringBuilder HTMLifyString(StringBuilder rawStr) {
            StringBuilder htmlStr = null;

            if (rawStr != null) {
                htmlStr = new StringBuilder(rawStr.ToString().Replace("/", "_S").Replace("+", "_P").Replace("=", "_E").Replace(":", "_C"));
            }

            return htmlStr;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     HTML of course tries to do clever stuff by replacing certain characters with their computer representation (e.g. a space is %20)
        ///     Therefore, we want to replace these characters with known equivalents before HTML gets a chance to do this.    
        ///     This method puts the / + = and : back by converting with _S, _P, _E and _C respectively
        /// </summary>
        public static StringBuilder DeHTMLifyString(StringBuilder htmlStr) {
            StringBuilder rawStr = null;

            if (htmlStr != null) {
                // Note that the order is important - these need to be completed in reverse of the HTMLify method ...
                rawStr = new StringBuilder(htmlStr.ToString().Replace("_C", ":").Replace("_E", "=").Replace("_P", "+").Replace("_S", "/"));
            }

            return rawStr;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns a truly random boolean
        /// </summary>
        public static bool GenerateRandomBool() {
            RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider();
            byte[] salt = new byte[1];
            csprng.GetBytes(salt);

            // This could have shown a marginal preference for even numbers, but actually as zero is counted as an even number,
            // then there will be 128 odd and 128 even numbers
            if ((int)salt[0] % 2 == 0) {
                return true;
            } else {
                return false;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns a truly random integer betwen two indexes (including both the start and end indexes)
        /// </summary>
        public static int GenerateRandomInt(int startIndex, int endIndex) {
            return Convert.ToInt32(Math.Round(GenerateRandomDouble(startIndex, endIndex)));
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns a truly random long betwen two indexes (including both the start and end indexes)
        /// </summary>
        public static long GenerateRandomLong(long startIndex, long endIndex) {
            return Convert.ToInt64( Math.Round(GenerateRandomDouble( startIndex, endIndex)));
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns a truly random integer betwen two indexes (including both the start and end indexes)
        /// </summary>
        public static DateTime GenerateRandomDateTime(DateTime start, DateTime end) {
            return new DateTime( Convert.ToInt64(Math.Round(GenerateRandomDouble(start.Ticks, end.Ticks))));
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns a truly random double betwen two indexes (including both the start and end indexes). The number can be one of
        ///      9,223,372,036,854,775,807 permutations (the number of values in a long).
        ///     
        ///     21-Jul-2016 - Disaster!! This method was fundamentally flawed and was only capable of producing a range of 256 random numbers.
        ///         This matters much less when the ranges are small, but when looking at doubles or longs the ranges are so large that it's really obvious and clunky.
        ///         Luckily, saved by this article - http://stackoverflow.com/questions/6299197/rngcryptoserviceprovider-generate-number-in-a-range-faster-and-retain-distribu
        ///         It uses a randomly generated 4 bytes to convert to an Int32.  We've taken that and extended to a long (Int64) or 8 bytes to try to squeeze out even more 
        ///         randomness for really large ranges.
        ///         
        ///         It is though still computer generated ... for true randomness we need to sample natural events like ambient noise or temperatures.
        ///         
        /// 
        /// </summary>
        public static double GenerateRandomDouble(double startIndex, double endIndex) {
            // our randomly generated double - default to the startIndex ...
            double random = startIndex;

            /* 
             *  -----1-----
             *  Use the RNG crypto service provider to generate a more truely random long (Int64).
             *  The Random class (without a seed) will always produce numbers in the same order!  Not entirely helpful if we are currently restarting the application often.
             *  One of the uses of this method is to select e.g. projects and photos randomly, so always having the same order looks a bit funny!
            */
            RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider();
            byte[] salt = new byte[8];
            csprng.GetBytes(salt);

            //-----2------ Catch silly ranges and zero ranges ..
            if (startIndex > endIndex) {
                Logger.LogError(6, "What a muppet - in GenerateRandomDouble, the start index " + startIndex + " was larger than the end index " + endIndex + "!  Sort it out");

            } else if (startIndex == endIndex) {           // 9-Feb-2016 - Added this case to make 100% sure that the integer generated is gucci if there is no range
                random = startIndex;
            } else {

                //-----3----- Calculate the range of possible values
                double difference = Convert.ToDouble(endIndex - startIndex);
                
                //-----4----- and convert the max long (our denominator) to a double
                //double denominator = ulong.MaxValue;  
                //-----5----- The hard graft - convert the byte array to an unsigned long (negative values would screw with the calculations
                //double enumerator = BitConverter.ToUInt64(salt, 0);
                //-----6----- Create a double between 0 and 1 by dividing our random long by the biggest possible long
                //double ratio = enumerator / denominator;  
                double ratio = (double) BitConverter.ToUInt64(salt, 0) / (double) ulong.MaxValue;

                //-----7----- And then lastly, lets start with our min value and add a proportion of the range between the min and the max that is specified by our random ratio.
                random = startIndex + (difference * ratio);
            }

            return random;
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Tests the encryption methods 
        /// </summary>
        public static bool Test() {
            bool success = false;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------------
            // Test the general encryption stuff ...
            SecureString tempKey = MGLEncryption.GenerateKey();
            Console.WriteLine(tempKey);
            MGLEncryption.SetCryptKey = tempKey;

            StringBuilder testStr = new StringBuilder("What a lovely __ time of it!");
            StringBuilder encryptedStr = MGLEncryption.Encrypt(testStr);
            StringBuilder decryptedStr = MGLEncryption.Decrypt(encryptedStr);
            bool theSame = MGLEncryption.Compare(testStr, encryptedStr);
            bool theSameStr = AreEqual(testStr, decryptedStr);
            bool notTheSame = MGLEncryption.Compare(new StringBuilder("Oh dear"), encryptedStr);

            StringBuilder htmledStr = MGLEncryption.HTMLifyString(encryptedStr);
            StringBuilder deHtmledStr = MGLEncryption.DeHTMLifyString(htmledStr);
            bool htmlWorking = MGLEncryption.AreEqual(deHtmledStr, encryptedStr);

            // Now test the password stuff ...
            SecureString tempKey2 = MGLEncryption.GenerateKey();
            Console.WriteLine(tempKey2);

            success = (theSame == true) && (theSameStr == true) && (notTheSame == false) && (htmlWorking == true);

            return success;
        }



    }
}





