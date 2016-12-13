using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Security;

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {

    //----------------------------------------------------------------------------------------------------------------------------------------------------------
    public static class ReflectionHelper {


        //-------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the object value of a specific _property_ in the PNACase or Individual objects
        ///     This method is being used to e.g. retrieve the string,string or int,string lists of lookup values in the KeyInfo class in the Humanitarian project
        /// </summary>
        public static object GetPropertyValue(Object obj, string propertyName) {

            object objVal = null;

            try {

                if (obj != null) {
                    // iterate through the properties of the object and if we find the specific property name, lets get the value and then go ...
                    Type t = obj.GetType();
                    PropertyInfo[] typeProperties = t.GetProperties();

                    foreach (PropertyInfo propInfo in typeProperties) {
                        string tempPropName = propInfo.Name;

                        if (propertyName.Equals(tempPropName) == true) {

                            objVal = propInfo.GetValue(obj, null);
                            break;
                        }
                    }
                }

            } catch (Exception ex) {
                //
                Logger.LogError(7, "Couldn't get the property value '" + propertyName + "' from the given object in GetPropertyValue: " + ex.ToString());

            }

            return objVal;
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Retrieves a list field with the given fieldName from the object provided and then extracts the value from the
        ///     string,string or int,string list of KeyValuePairs based in the given specificString or specificInt values
        /// </summary>
        public static string GetFieldValue(Object keyInfoObj, string fieldName, string specificStringItemVal, int specificIntItemVal) {

            string val1 = null;
            bool success = false;

            try {

                if (keyInfoObj != null) {
                    // iterate through the properties of each grievance and compare each ...
                    Type t = keyInfoObj.GetType();
                    FieldInfo typeField = t.GetField(fieldName);

                    object obj1 = typeField.GetValue(keyInfoObj);

                    // Parse the object extracted as a string,string or int,string based on the input parameters
                    if (specificStringItemVal != null && specificStringItemVal != "") {
                        success = ReflectionHelper.LookupGenericListItemList(specificStringItemVal, out val1, obj1 as List<KeyValuePair<string, string>>, true);
                    } else if (specificIntItemVal != 0) {
                        success = ReflectionHelper.LookupGenericListItemList(specificIntItemVal, out val1, obj1 as List<KeyValuePair<int, string>>, true);
                    }
                }

            } catch (Exception ex) {
                //
                Logger.LogError(7, "Couldn't get the field value '" + fieldName + "' with specific values a:'"
                    + specificStringItemVal + "' and b:'" + specificIntItemVal + "' from the given object in GetFieldValue: " + ex.ToString());

            }

            return val1;
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Parses the string value from the given object - particularly useful for dates and uses the pretty date format used elsewhere
        /// </summary>
        public static string ParseRawVal(object rawObj) {
            string val = "";

            try {
                if (rawObj == null) {
                    val = "";
                } else {
                    // check to see if it is a date ....
                    if (rawObj.GetType() == (new DateTime().GetType())) {
                        DateTime tempDT = new DateTime();
                        DateTime.TryParse(rawObj.ToString(), out tempDT);
                        val = DateTimeInformation.PrettyDateFormat(tempDT);
                    } else {
                        // integer or string ...
                        val = rawObj.ToString();
                    }
                }
            } catch (Exception ex) {
                //
                Logger.LogError(7, "Couldn't parse the raw value from the given object in GetFieldValue: " + ex.ToString());

            }

            return val;
        }



        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Looks up the specific value from the static lists in KeyInfo ...
        /// </summary>
        public static bool LookupGenericListItemList(int id, out string name, List<KeyValuePair<int, string>> genericIntStringList) {

            name = "";
            // Default case is to always log the errors if they are misssing ...
            return LookupGenericListItemList(id, out name, genericIntStringList, true);

        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Looks up the specific value from the static lists in KeyInfo ...
        /// </summary>
        public static bool LookupGenericListItemList(int id, out string name, List<KeyValuePair<int, string>> genericIntStringList, bool logErrorOnMissingValues) {
            bool success = false;
            name = "";

            try {

                if (id > 0) {

                    foreach (KeyValuePair<int, string> item in genericIntStringList) {

                        if (item.Key == id) {

                            name = item.Value;
                            success = true;
                            break;
                        }
                    }

                    if (success == false && logErrorOnMissingValues == true) {
                        Logger.LogError(2, "Could not lookup the Name for the given code (" + id + ")." + System.Environment.StackTrace);
                    }
                } else {
                    success = true;
                }
            } catch (Exception ex) {
                Logger.LogError(2, "Could not lookup the Name for the given code (" + id + ").  See the detailed error: " + ex.ToString());
            }

            return success;
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Looks up the specific value from the static lists in KeyInfo ...
        /// </summary>
        public static bool LookupGenericListItemList(string idVal, out string name, List<KeyValuePair<string, string>> genericStringStringList, bool logErrorOnMissingValues) {
            bool success = false;
            name = "";

            try {

                if (idVal != null && idVal != "") {

                    foreach (KeyValuePair<string, string> item in genericStringStringList) {

                        if (item.Key != null && item.Key.Equals(idVal, StringComparison.CurrentCultureIgnoreCase)) {

                            name = item.Value;
                            success = true;
                            break;
                        }
                    }

                    if (success == false && logErrorOnMissingValues == true) {
                        Logger.LogError(2, "Could not lookup the Name for the given code (" + idVal + ")." + System.Environment.StackTrace);
                    }
                } else {
                    success = true;
                }
            } catch (Exception ex) {
                Logger.LogError(2, "Could not lookup the Name for the given code (" + idVal + ").  See the detailed error: " + ex.ToString());
            }

            return success;
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Looks up the specific value from the static lists in KeyInfo ...
        ///     firstValIsNOTID - this is a bit hacky - but need a way of reversing the inputs so that the first value is the name rather than the ID!!!
        /// </summary>
        public static bool LookupGenericListItemList(string name, out string idVal, List<KeyValuePair<string, string>> genericStringStringList,
            bool logErrorOnMissingValues, bool firstValIsNOTID) {

            bool success = false;
            idVal = "";

            try {

                if (name != null && name != "" && name.Equals("Please Choose", StringComparison.CurrentCultureIgnoreCase) == false) {

                    foreach (KeyValuePair<string, string> item in genericStringStringList) {

                        if (item.Value != null && item.Value.Equals(name, StringComparison.CurrentCultureIgnoreCase)) {

                            idVal = item.Key;
                            success = true;
                            break;
                        }
                    }

                    if (success == false && logErrorOnMissingValues == true) {
                        Logger.LogError(2, "Could not lookup the ID for the given name (" + name + ")." + System.Environment.StackTrace);
                    }
                } else {
                    success = true;
                }
            } catch (Exception ex) {
                Logger.LogError(2, "Could not lookup the ID for the given name (" + name + ").  See the detailed error: " + ex.ToString());
            }

            return success;
        }



        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Looks up the specific value from the static lists in KeyInfo ...
        /// </summary>
        public static bool LookupGenericListItemList(string name, out int id, List<KeyValuePair<int, string>> genericIntStringList) {

            id = 0;
            return LookupGenericListItemList(name, out id, genericIntStringList, true);

        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Looks up the specific value from the static lists in KeyInfo ...
        /// </summary>
        public static bool LookupGenericListItemList(SecureString name, out int id, List<KeyValuePair<int, string>> genericIntStringList, bool logErrorOnMissingValues) {
            id = 0;

            return( LookupGenericListItemList( SecureStringWrapper.Decrypt( name ).ToString(), out id, genericIntStringList, logErrorOnMissingValues ));
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Looks up the specific value from the static lists in KeyInfo ...
        /// </summary>
        public static bool LookupGenericListItemList(string name, out int id, List<KeyValuePair<int, string>> genericIntStringList, bool logErrorOnMissingValues) {
            bool success = false;
            id = 0;

            try {

                // 11-Jun-2015 - Added a check to ensure that the default option is fast tracked ....
                if (name != null && name != "" && name.Equals("Please Choose", StringComparison.CurrentCultureIgnoreCase) == false) {

                    foreach (KeyValuePair<int, string> item in genericIntStringList) {

                        if (item.Value.Equals(name, StringComparison.CurrentCultureIgnoreCase)) {

                            id = item.Key;
                            success = true;
                            break;
                        }
                    }

                    if (success == false && logErrorOnMissingValues == true) {
                        Logger.LogError(2, "Could not lookup the Code for the given name (" + name + ")." + System.Environment.StackTrace);
                    }
                } else {
                    success = true;
                }
            } catch (Exception ex) {
                Logger.LogError(2, "Could not lookup the Code for the given name (" + name + ").  See the detailed error: " + ex.ToString());
            }

            return success;
        }



    }
}
