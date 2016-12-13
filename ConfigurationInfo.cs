using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Collections.Generic;
using System.Collections;

using System.Configuration;
using System.Text;
using System.Security;
using DataNirvana.DomainModel.Database;

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
/**
    Description:	LoadConfigurationFile
    Type:				Extraction - Special Case
    Author:			Edgar Scrase
    Date:				March 2005
    Version:			2.0

    Notes:			Uses the new and improved version 2 Configuration File Structure

                        Extracts default application level parameters from a config file (probably in C:/AddressGazetteer_Config) or enables
                        Users to Get and Set specific parameters

                        The majority of the parameters are used for web or addressing applications, but the general parameters in this config
                        class should be suitable for most general purpose applications as well

                       29-09-2015 - A decade on and we are finally getting a bit more security conscious!
                       For the passwords etc, we have added secure strings where applicable

*/
namespace MGL.Data.DataUtilities {

    


    //-------------------------------------------------------------------------------------------------------------------------------------------------------------
    public class ConfigurationInfo {

        // This directory needs to be there or else we need to specifically recompile
        private string configFileName = null;
        private string configPublicName = null;
        private DatabaseConnectionInfo dbConInfo = new DatabaseConnectionInfo();  // Probably the LIS or Address Database

        private string physicalTempFileDir = null;
        private string physicalLogDir = null;
        private string physicalWebRootDir = null;
        private string physicalSVGFileDir = null;
        private string physicalRasterPath = null;

        // NOT in the web root typically
        private string physicalDownloadDir = null;

        private string webHost = null;
        private string webRoot = null;

        private string webDownloadDir = null;

        private int maxNumDBRows = 100;


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public ConfigurationInfo() {
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public ConfigurationInfo(string configFileName) {

            this.configFileName = configFileName;
            this.loadConfigFile();
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Overloaded constructor that provides the database parameters WITHOUT the LCF having to
        /// read them from a MGL.config file. Not pretty but the alternative would require changing tonnes
        /// of existing code that uses LCF's
        /// </summary>
        public ConfigurationInfo(string dbType, string dbHost, string dbName, SecureString dbUser, SecureString dbPass, int dbPort) {
            dbConInfo.HOST = dbHost;
            dbConInfo.NAME = dbName;
            dbConInfo.PASSWORD = dbPass;
            dbConInfo.TYPE = dbType;
            dbConInfo.USER = dbUser;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Uses reflection to clone all of the exposed attributes of the load configuration class</summary>
        public ConfigurationInfo Clone() {
            try {
                Type type = this.GetType();
                object clone = Activator.CreateInstance(type); // Throws an exception if a global that shouldnt be null is null (e.g. a struct)

                PropertyInfo[] typeProperties = type.GetProperties();

                object sourceVal = null;
                object targetVal = null;

                foreach (PropertyInfo propInfo in typeProperties) {
                    string propName = propInfo.Name;
                    sourceVal = propInfo.GetValue(this, null);
                    if (sourceVal != null) {
                        targetVal = sourceVal;
                        propInfo.SetValue(clone, targetVal, null);
                    }
                }

                // do we need to do something specific on the DB Con Info?????

                return (ConfigurationInfo)clone;
            } catch (Exception ex) {
                Logger.LogError(8, "LoadConfigurationFile: Clone: Error cloning the Config File (" + LoggerErrorTypes.Parsing + "):" + ex.ToString());
                Logger.LogError(8, "Error cloning lcf at: " + ex);
            }
            return null;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string ConfigFileName {
            get { return configFileName; }
            set { configFileName = value; }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The config file name without the file suffix and the path e.g. DNFDemo from c:/DNFDemo.mgl
        /// </summary>
        public string ConfigKeyName {
            get {
                string configKeyName = "";

                if (ConfigFileName != null) {
                    configKeyName = ConfigFileName.Split('.')[0];
                    string[] bits = configKeyName.Split('/');
                    configKeyName = bits[bits.Length - 1];
                    bits = configKeyName.Split('\\');
                    configKeyName = bits[bits.Length - 1];
                    configKeyName = configKeyName.ToLower();
                }

                return configKeyName;
            }
            // This needs to be here for the clone stuff to work, but nothing needs to be set
            set {
            }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string ConfigPublicName {
            get { return configPublicName; }
            set { configPublicName = value; }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public DatabaseConnectionInfo DbConInfo {
            get { return dbConInfo; }
            set { dbConInfo = value; }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string PhysicalTemporaryFileDirectory {
            get { return physicalTempFileDir; }
            set { physicalTempFileDir = value; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string PhysicalLogDirectory {
            get { return physicalLogDir; }
            set { physicalLogDir = value; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string PhysicalWebRootDirectory {
            get { return physicalWebRootDir; }
            set { physicalWebRootDir = value; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string PhysicalSVGFileDirectory {
            get { return physicalSVGFileDir; }
            set { physicalSVGFileDir = value; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string PhysicalRasterFileDirectory {
            get { return physicalRasterPath; }
            set { physicalRasterPath = value; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string PhysicalDownloadDirectoryName {
            get { return physicalDownloadDir; }
            set { physicalDownloadDir = value; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string WebHost {
            get { return webHost; }
            set { webHost = value; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string WebRoot {
            get { return webRoot; }
            set { webRoot = value; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets the web host and the web root</summary>
        public string WebProjectPath() {

            string tempWebRoot = webHost;
            if (webRoot != null && webRoot != "") {
                if (webRoot.StartsWith("/") == false && webRoot.StartsWith("\\") == false) {
                    tempWebRoot = tempWebRoot + "/";
                }

                tempWebRoot = tempWebRoot + webRoot;

                if (webRoot.EndsWith("/") == false && webRoot.EndsWith("\\") == false) {
                    tempWebRoot = tempWebRoot + "/";
                }

            } else {
                tempWebRoot = tempWebRoot + "/";
            }
            return tempWebRoot;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     By default the WebProjectPath will end with a /.  This overridden method enables this slash to be retained or removed.
        ///     Used in e.g. Global.aspx.Begin_Request
        /// </summary>
        public string WebProjectPath(bool includeTrailingSlash) {
            string wpp = WebProjectPath();
            return wpp.Substring(0, wpp.Length-1);
        }



        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        private string HackOutOldSession(string inputValue) {
            //If the input value has the appBasePath and a session ID added
            // already (like when we are cloning a loadConfig file) then we
            // need to hack out the webSVGCodeVirtualRoot
            string appBasePath = GetApplicationBasePath();
            if (inputValue.Contains(appBasePath)) {
                try {
                    int pos = inputValue.IndexOf("))/");
                    inputValue = inputValue.Remove(0, pos + 3);
                    inputValue = inputValue.Remove(inputValue.Length - 1);
                } catch {
                }
            }

            return inputValue;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        protected string GetApplicationBasePath() {
            //////////////////////////////////////////////////////////////////////
            // Note sure if this is right - be careful!!!
            string basePath = WebHost + "/" + WebRoot;
            try {
                string port = System.Web.HttpContext.Current.Request.ServerVariables["SERVER_PORT"];
                if (port == null || port == "80" || port == "443")
                    port = "";
                else
                    port = ":" + port;

                basePath = System.Web.HttpContext.Current.Request.ServerVariables["SERVER_NAME"] +
                                            port + System.Web.HttpContext.Current.Request.ApplicationPath;
            } catch {
            }
            return basePath;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string WebDownloadDirectory {
            //Apply path modifier so that we dont lose the session variable from
            // the url when we are in cookieless session mode
            get {
                string appBasePath = GetApplicationBasePath();

                string path = webDownloadDir;

                if (!webDownloadDir.ToLower().Contains(appBasePath.ToLower())) {
                    path = appBasePath + "/" + webDownloadDir;
                }

                //If we are using Cookieless mode - put all in a try catch as this can also be used outside of a web context altogether ...
                try {
                    if (System.Web.HttpContext.Current.Session != null && System.Web.HttpContext.Current.Session.IsCookieless) {
                        //                        if (System.Web.HttpContext.Current.Session.IsCookieless) {
                        path = HackOutOldSession(webDownloadDir);
                        //Uri loginUri = new Uri(webSVGCodeVirtualRoot);
                        //string requestedPage = loginUri.MakeRelativeUri(webSVGCodeVirtualRoot).ToString();
                        string sessID = System.Web.HttpContext.Current.Request.UrlReferrer.AbsolutePath;
                        sessID = System.Web.HttpContext.Current.Session.SessionID;

                        //Need to make sure the "s" is always added to session variables
                        path = appBasePath + "/(S(" + sessID + "))/" + webDownloadDir + "/";
                    }
                } catch {
                }

                return path;
            }

            set {

                string inputValue = value;

                try {
                    //If we are using Cookieless mode
                    if (System.Web.HttpContext.Current.Session.IsCookieless) {
                        inputValue = HackOutOldSession(value);
                    }
                } catch {
                }

                webDownloadDir = inputValue;
            }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public int MaxNumDBQueryRows {
            get { return maxNumDBRows; }
            set { maxNumDBRows = value; }
        }



        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /*
                Database Host|				<localhost>
                Database Type|				<MySQL>
                Database Name|				<oldham>
                Database UserName|			<root>
                Database Password|			<topology>

                SVG Database Host|			<localhost>
                SVG Database Type|			<MySQL>
                SVG Database Name|			<oldham_svg>
                SVG Database UserName|		<root>
                SVG Database Password|		<topology>

                Physical Web Root Directory|	<c:/inetpub/wwwroot/SVGMappingTest/>
                Physical Temporary File Directory|	<c:/temp/DTF>
                Physical Log Directory|			<c:/temp/logs>

                Web Host|					<localhost>
                Web Root|					<SVGMappingTest>

                SVG Maps File Path|			<SVGMaps>
                Raster Image Path|			<RasterImages>
                SVG Code Path|				<Code/SVG>
                Download Path|				<DownloadFiles>

                Max Number Of DB Query Rows|	<100>

                LLPG Organisation Name|		<OLDHAM>
                LLPG Data Provider Code|		<1055>
                LLPG County|				<Lancashire>
        */
        public bool loadConfigFile() {
            bool fileLoaded = false;

            StreamReader sr = null;

            // load a config file specifying upload directory and other key parameters like db Name and type
            try {
                Console.WriteLine("Trying to load config file " + configFileName);

                sr = File.OpenText(configFileName);

                string s = "";
                while ((s = sr.ReadLine()) != null) {

                    // check each line
                    if (s != null && s != "") {

                        char[] separators = { '|' };
                        string[] bits = s.Split(separators);
                        string variable = bits[1].Substring(bits[1].IndexOf("<") + 1, (bits[1].IndexOf(">") - 1 - bits[1].IndexOf("<")));
                        string key = bits[0].ToLower().Trim();

                        if (key.Equals("database host")) {
                            dbConInfo.HOST = variable;
                        } else if (key.Equals("database type")) {
                            dbConInfo.TYPE = variable;
                        } else if (key.Equals("database name")) {
                            dbConInfo.NAME = variable;
                        } else if (key.Equals("database username")) {
                            dbConInfo.USER = SecureStringWrapper.Encrypt( variable );
                        } else if (key.Equals("database password")) {
                            dbConInfo.PASSWORD = SecureStringWrapper.Encrypt( variable );

                        } else if (key.Equals("physical temporary files directory")) {
                            physicalTempFileDir = variable;
                        } else if (key.Equals("physical log directory")) {
                            physicalLogDir = variable;
                        } else if (key.Equals("physical web root directory")) {
                            physicalWebRootDir = variable;
                        } else if (key.Equals("physical download files directory")) {
                            physicalDownloadDir = variable;

                        } else if (key.Equals("web host")) {
                            webHost = variable;
                        } else if (key.Equals("web root")) {
                            webRoot = variable;


                        } else if (key.Equals("download files path")) {
                            webDownloadDir = variable;
                        } else if (key.Equals("max number of db query rows")) {
                            try { this.maxNumDBRows = int.Parse(variable); } catch { }
                        } else {																				// Untrapped parameter
                            //MGLErrorLog.LogWarning("LoadConfigurationFile", "LoadConfigurationFile", true, "Untrapped configuration line:" + s, LoggerErrorTypes.DataQuality);
                        }
                    }
                }

                // now build the web and physical paths properly
                BuildPaths();

                fileLoaded = true;
                sr.Close();
            } catch (Exception ex) {
                Logger.LogError(9, ex.Message);
                Logger.LogError(9, ex.StackTrace);

                // error opening file
                Logger.LogError(9, "LoadConfigurationFile: Error opening file or reading configuration parameters (" + LoggerErrorTypes.FileIO + "): " + ex.ToString());
                if (sr != null) {
                    sr.Close();
                }
            }
            return fileLoaded;
        }

        /// <summary>
        /// Builds the full web and physical paths using the
        /// information from the web.config
        /// This method should only be called ONCE after the
        /// web.config file has been loaded.
        /// </summary>
        public void BuildPaths() {
            // now build the web and physical paths properly
            physicalWebRootDir = physicalWebRootDir + "/";
            physicalSVGFileDir = physicalWebRootDir + physicalSVGFileDir + "/";
            physicalRasterPath = physicalWebRootDir + physicalRasterPath + "/";
            physicalDownloadDir = physicalDownloadDir + "/";

            string tempWebRoot = webHost;
            if (webRoot != null && webRoot != "") {
                webRoot = "/" + webRoot + "/";
            } else {
                webRoot = "/";
            }
            tempWebRoot = tempWebRoot + webRoot;

            webDownloadDir = tempWebRoot + webDownloadDir + "/";
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Fix me. This should only be used for debug.
        /// </summary>
        /// <returns>A human readable string for the contents of this instance.</returns>
        public override string ToString() {
            StringBuilder tValues = new StringBuilder();
            Type tType = this.GetType();
            PropertyInfo[] typeProperties = tType.GetProperties();

            int count = 1;
            foreach (PropertyInfo propInfo in typeProperties) {
                string propName = propInfo.Name;
                tValues.Append(count + ":Name:" + propName + ":Value__:" + propInfo.GetValue(this, null));
                count++;
            }

            return tValues.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Only use this in a WEB context or your code will explode!!!
        ///     30-Sep-2015
        /// </summary>
        public bool LoadConfigurationInfoFromWebConfig() {
            bool success = false;

            try {

                MglWebConfigurationInfo mwci = MglWebConfigurationInfo.GetConfig();
                MglWebConfigurationInfoParamsCollection mwcipc = mwci.ConfigInfoList;

                string temp1 = mwcipc[0].Name;
                string temp2 = mwcipc[0].Value;

                Type type = this.GetType();
                PropertyInfo[] typeProperties = type.GetProperties();

                for (int i = 0; i < typeProperties.Length; i++) {
                    PropertyInfo propInfo = typeProperties[i];
                    string propName = propInfo.Name;

                    foreach (MglWebConfigurationInfoParam mwcip in mwcipc) {

                        if (propName.Equals(mwcip.Name, StringComparison.CurrentCultureIgnoreCase)
                            || propName.Equals("__" + mwcip.Name, StringComparison.CurrentCultureIgnoreCase)) {

                            propInfo.SetValue(this, mwcip.Value, null);
                        }
                    }
                }

                // Now check for the Database parameters as we need to load them into the dbConInfo struct .....
                foreach (MglWebConfigurationInfoParam mwcip in mwcipc) {
                    if (mwcip.Name.Equals("DbConInfo.Host", StringComparison.CurrentCultureIgnoreCase)) {
                        dbConInfo.HOST = mwcip.Value;
                    } else if (mwcip.Name.Equals("DbConInfo.Name", StringComparison.CurrentCultureIgnoreCase)) {
                        dbConInfo.NAME = mwcip.Value;
                    } else if (mwcip.Name.Equals("DbConInfo.Type", StringComparison.CurrentCultureIgnoreCase)) {
                        dbConInfo.TYPE = mwcip.Value;
                    } else if (mwcip.Name.Equals("DbConInfo.User", StringComparison.CurrentCultureIgnoreCase)) {
                        dbConInfo.USER = SecureStringWrapper.Encrypt( mwcip.Value );
                    } else if (mwcip.Name.Equals("DbConInfo.Password", StringComparison.CurrentCultureIgnoreCase)) {
                        dbConInfo.PASSWORD = SecureStringWrapper.Encrypt( mwcip.Value );
                    } else if (mwcip.Name.Equals("DbConInfo.Port", StringComparison.CurrentCultureIgnoreCase)) {
                        int port = 3306;
                        int.TryParse(mwcip.Value, out port);
                        dbConInfo.PORT = port;

                        // 25-Aug-2015 - add the additional SSL specific parameters
                    } else if (mwcip.Name.Equals("DbConInfo.SSLRequired", StringComparison.CurrentCultureIgnoreCase)) {
                        dbConInfo.SSLRequired = (mwcip.Value != null && mwcip.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase));
                    } else if (mwcip.Name.Equals("DbConInfo.SSLCertificatePath", StringComparison.CurrentCultureIgnoreCase)) {
                        dbConInfo.SSLCertificatePath = SecureStringWrapper.Encrypt( mwcip.Value );
                    } else if (mwcip.Name.Equals("DbConInfo.SSLCertificatePassword", StringComparison.CurrentCultureIgnoreCase)) {
                        dbConInfo.SSLCertificatePassword = SecureStringWrapper.Encrypt( mwcip.Value );
                    }
                }

                // get to here then looking fine ....
                success = true;

            } catch (Exception ex) {
                Logger.LogError(9, "Problem getting the configuration object from the Web.Config: " + ex.ToString());
            }


            return success;
        }




        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void Test() {

            // this assumes that the following configuration file is available
            Logger.LogSubHeading("Testing the load configuration file");
            ConfigurationInfo lcf = new ConfigurationInfo("c:/Address_Gazetteer_Config/OldhamConfig.mgl");

            // now print out all of the public properties
            Type type = lcf.GetType();
            PropertyInfo[] typeProperties = type.GetProperties();
            int count = 0;
            foreach (PropertyInfo propInfo in typeProperties) {
                string propName = propInfo.Name;
                Console.WriteLine(count + ":Name:" + propName + ":Value__:" + propInfo.GetValue(lcf, null));
                count++;
            }

            Logger.LogSubHeading("Cloning the object - neccessary as the custom object are not immutable");
            ConfigurationInfo lcf2 = lcf.Clone();
            Logger.Log("Proving cloning works - Changing the database name");
        }



    }  // end of class
}
