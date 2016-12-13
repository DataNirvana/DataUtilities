using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Xml;

//---------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {

    //------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Reads a configuration file for websites which contains all the sensitive keys, certificate files, user names and passwords ...
    ///     XML is of the form where there is a list of MGLSecurityItems and the auth is optional if the security item is e.g. a key
    ///     and not a username and password combo
    ///
    /// <MGLSecurityItemList>
    /// 	<MGLSecurityItem id="DatabaseLogin">
    /// 	    <key>My_User_Name</key>
    /// 	    <auth>AxMTd3d3LmRhdGFuaXJ2cdw2tECHgCi8QJeX+5bD</auth>
    /// 	</MGLSecurityItem>
    /// </MGLSecurityItemList>
    /// </summary>
    public static class MGLXMLReader {

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static string fileName = "Q:/Docs/AppConfig/MG_Web_Application_Keys_"; // PNA.xml ....


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Essentially extracts a key value pair where the key is mandatory in the XML and MUST be non zero in length;
        ///     the auth is optional and can be an empty string as it is context specific.
        /// </summary>
        public static bool GetInfo(string appName, string id, out SecureString key, out SecureString auth) {

            bool success = false;
            key = new SecureString();
            auth = new SecureString();

            XmlDocument xmlDoc = new XmlDocument();

            try {
                StringBuilder tempfileName = new StringBuilder(fileName + appName + ".xml");
                xmlDoc.Load( fileName + appName + ".xml");

                // loop through the security items and look for the one specified by ID ...
                // the document element is the root element - in this case - MGLSecurityItemList
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes) {

                    string tempID = node.Attributes["id"].Value;

                    if (node != null && node.Attributes["id"] != null && node.Attributes["id"].Value != null
                        && node.Attributes["id"].Value.Equals(id, StringComparison.CurrentCultureIgnoreCase) == true) {

                        // get the key - success is derived by the encrypted key being the same length as the text in the xml file
                        // the key must ALWAYS be present
                        XmlNode keyNode = node.SelectSingleNode("key");
                        if (keyNode != null && keyNode.InnerText != null) {

                            key = SecureStringWrapper.Encrypt(keyNode.InnerText.ToCharArray());
                            success = key.Length > 0 && (key.Length == keyNode.InnerText.Length);

                        }

                        // get the auth if it exists - success is derived by the encrypted key being the same length as the text in the xml file
                        // Note that it is valid for the auth to be an empty string ...
                        XmlNode authNode = node.SelectSingleNode("auth");
                        if (authNode != null && authNode.InnerText != null) {

                            auth = SecureStringWrapper.Encrypt(authNode.InnerText.ToCharArray());
                            success = success && (auth.Length == authNode.InnerText.Length);

                        }

                        break;

                    }

                }

            } catch (Exception ex) {

                Logger.LogError(7, "Error getting the info for the given app "+appName+" and specific token(s): "+id+".  The specific error message provided was: "+ex.ToString());

            } finally {
                // kill the xmlDoc!
                xmlDoc = null;

            }

            return success;
        }


    }
}
