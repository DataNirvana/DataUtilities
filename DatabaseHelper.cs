using MGL.Data.DataUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MGL.Data.DataUtilities {
    public class DatabaseHelper {

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string SQL_INJECTION_CHECK_PARAMETER(bool doStrictTest, string paramText) {
            return SQL_INJECTION_CHECK_PARAMETER(doStrictTest, paramText, false);
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Added 12-10-2015 to support the secure string wrappers ....
        /// </summary>
        public static SecureString SQL_INJECTION_CHECK_PARAMETER(bool doStrictTest, SecureString paramText, bool ignoreQuotes) {

            return SecureStringWrapper.Encrypt(

                SQL_INJECTION_CHECK_PARAMETER(doStrictTest, SecureStringWrapper.Decrypt(paramText).ToString(), ignoreQuotes)

            );

        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Checks the given parameter for quotes and semicolons - removes if found</summary>
        public static string SQL_INJECTION_CHECK_PARAMETER(bool doStrictTest, string paramText, bool ignoreQuotes) {
            if (paramText != null && paramText != "") {

                if (!ignoreQuotes) {
                    // Steve, July 2010: Moved this to start to fix bug whereby this was replacing the
                    // escape chars written by the apostrophe replacements, this may have caused some strings
                    // to fail to be inserted into the database. (will also add this to DataUtility.DatabaseInformation
                    // class to make sure they are both consistent (except the semi colon replacement...does this need to be here?!?!).

                    paramText = paramText.Replace("\\", "\\\\");

                    paramText = paramText.Replace("'", "\\'");
                    paramText = paramText.Replace("’", "\\'");
                    paramText = paramText.Replace("‘", "\\'");
                    paramText = paramText.Replace("’", "\\'");

                    paramText = paramText.Replace("\"", "\\\"");
                    //                    paramText = paramText.Replace(";", "");
                }

                if (doStrictTest) {
                    // paramText = paramText.ToLower().Replace(" or ", "");
                    // split on a series of keyworks and identify if the keywords are present OUTSIDE of quotes (ie, the number of single or double quotes preceding the text is even
                    paramText = paramText.Replace("delete", "");
                    paramText = paramText.Replace("drop", "");
                    paramText = paramText.Replace("select", "");
                    paramText = paramText.Replace("insert", "");
                    paramText = paramText.Replace("update", "");
                    paramText = paramText.Replace("union", "");
                    paramText = paramText.Replace("truncate", "");
                    paramText = paramText.Replace("--", "");
                    paramText = paramText.Replace(";", "");
                }
            }
            return paramText;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Checks the given parameter for quotes and semicolons - removes if found</summary>
        public static string SQLInjectionCheckParameter(bool doStrictTest, string paramText, bool testOnlyIfInsideQuotes, bool escapeQuotes) {
            if (paramText != null && paramText != "") {
                if (testOnlyIfInsideQuotes == false) {
                    return (SQL_INJECTION_CHECK_PARAMETER(doStrictTest, paramText));
                } else {
                    if (escapeQuotes) {
                        paramText = paramText.Replace("'", "\\'");
                        paramText = paramText.Replace("\"", "\\\"");
                    }
                    //                    paramText = paramText.Replace(";", "");
                    if (doStrictTest) {
                        //                paramText = paramText.ToLower().Replace(" or ", "");
                        // split on a series of keyworks and identify if the keywords are present OUTSIDE of quotes (ie, the number of single or double quotes preceding the text is even
                        foreach (string keyword in SQLInjectionKeywords) {
                            int keywordIndex = paramText.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase);
                            if (keywordIndex != -1) {
                                string[] bits = paramText.ToLower().Split(new string[] { keyword }, StringSplitOptions.None);
                                if (bits.Length > 1) {
                                    foreach (string bit in bits) {
                                        string temp = bit.Replace("\"", "");
                                        int numDQ = bit.Length - temp.Length;
                                        temp = bit.Replace("'", "");
                                        int numSQ = bit.Length - temp.Length;

                                        if (numDQ % 2 == 0 || numSQ % 2 == 0) {
                                            paramText = paramText.Insert(keywordIndex, "***");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return paramText;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string[] SQLInjectionKeywords = new string[] { ";", "delete", "drop", "select", "insert", "update", "union", "alter", "truncate" };


    }
}
