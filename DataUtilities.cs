using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using MGL.DomainModel;
using System.Globalization;
//using MGL.DomainModel.Database;


//------------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {

    //----------------------------------------------------------------------------------------------------------------------------
    public struct MGLMaxMin {
        public double Min;
        public double Max;
    }


    //-------------------------------------------------------------------------------------------------------------------------------------------------------------
    /**
        Description:	Utilities
        Type:				Action (Low level)
        Author:			Edgar Scrase & Desmond Fitzgerald
        Date:				March 2005
        Version:			0.1

        Notes:


    */
    public static class DataUtilities {

        private static string thisClassName = "MGL_Utility.Utilities";


        //------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void Test() {

            Console.WriteLine("Test method for " + thisClassName);

            string doubleList = "3,4.5,4.9,-2323.32323232, -0.00005656";
            Console.WriteLine("Converting this list of doubles ("+doubleList+") to this array ("+DataUtilities.GetCSVList( DataUtilities.GetDoubleArray( doubleList ))+")");

            int significantDigit = 3;
            double tempNum = 33.5;
            Console.WriteLine("Number Rounding (significant digit is " + significantDigit + "):" + tempNum + " up (" + RoundNumber(tempNum, true, significantDigit) + ") and down (" + RoundNumber(tempNum, false, significantDigit) + ")");
            Console.WriteLine("Number Rounding (significant digit is " + --significantDigit + "):" + tempNum + " up (" + RoundNumber(tempNum, true, significantDigit) + ") and down (" + RoundNumber(tempNum, false, significantDigit) + ")");
            Console.WriteLine("Number Rounding (significant digit is " + --significantDigit + "):" + tempNum + " up (" + RoundNumber(tempNum, true, significantDigit) + ") and down (" + RoundNumber(tempNum, false, significantDigit ) + ")");
            tempNum = -550.5;
            Console.WriteLine("Number Rounding (significant digit is "+significantDigit+"):"+ tempNum + " up (" + RoundNumber(tempNum, true) + ") and down (" + RoundNumber(tempNum, false) + ")");
            significantDigit = 5;
            tempNum = -0.005505;
            Console.WriteLine("Number Rounding (significant digit is " + significantDigit + "):" + tempNum + " up (" + RoundNumber(tempNum, true, significantDigit) + ") and down (" + RoundNumber(tempNum, false, significantDigit) + ")");
            Console.WriteLine("Number Rounding (significant digit is " + --significantDigit + "):" + tempNum + " up (" + RoundNumber(tempNum, true, significantDigit) + ") and down (" + RoundNumber(tempNum, false, significantDigit) + ")");
            Console.WriteLine("Number Rounding (significant digit is " + --significantDigit + "):" + tempNum + " up (" + RoundNumber(tempNum, true, significantDigit) + ") and down (" + RoundNumber(tempNum, false, significantDigit) + ")");
            Console.WriteLine("Number Rounding (significant digit is " + --significantDigit + "):" + tempNum + " up (" + RoundNumber(tempNum, true, significantDigit) + ") and down (" + RoundNumber(tempNum, false, significantDigit) + ")");
            Console.WriteLine("Number Rounding (significant digit is " + --significantDigit + "):" + tempNum + " up (" + RoundNumber(tempNum, true, significantDigit) + ") and down (" + RoundNumber(tempNum, false, significantDigit) + ")");
            tempNum = 9.2345;
            Console.WriteLine("Number Rounding (significant digit is "+significantDigit+"):" + tempNum + " up (" + RoundNumber(tempNum, true) + ") and down (" + RoundNumber(tempNum, false) + ")");
            tempNum = 0.92345;
            Console.WriteLine("Number Rounding (significant digit is "+significantDigit+"):" + tempNum + " up (" + RoundNumber(tempNum, true) + ") and down (" + RoundNumber(tempNum, false) + ")");


        }

        public static bool IsStringNullOrEmpty(string stringValue)
        {
            if (stringValue == null || stringValue == String.Empty)
                return true;
            else
                return false;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string[] ConvertArrayListToStringArray( ArrayList al ) {
            string[] sa = new string[al.Count];
            int count = 0;
            foreach ( string temp in al ) {
                sa[count] = temp;
                count++;
            }
            return sa;
        }


        //------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static ArrayList ConvertArrayToArrayList( string[] sa ) {
            ArrayList al = new ArrayList();
            foreach ( string temp in sa ) {
                al.Add( temp );
            }
            return al;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static ArrayList ConvertArrayToArrayList(double[] sa) {
            ArrayList al = new ArrayList();
            foreach (double temp in sa) {
                al.Add(temp);
            }
            return al;
        }


        //------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static List<int> ConvertStringListToIntList(List<string> sList) {
            List<int> iList = new List<int>();
            if (sList != null) {
                try {
                    foreach (string s in sList) {
                        iList.Add(int.Parse(s));
                    }
                } catch {
                    iList = null;
                }
            }
            return iList;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Takes a csv string of doubles, no spaces</summary>
        public static double[] GetDoubleArray(string csvStringOfDoubles) {
            double[] ds = null;

            if (csvStringOfDoubles != null) {
                try {
                    string[] strList = csvStringOfDoubles.Split(',');
                    ds = new double[strList.Length];

                    int counter = 0;
                    foreach (string str in strList) {
                        ds[counter] = double.Parse(str);
                        counter++;
                    }
                } catch (Exception ex) {
                    Logger.Log(thisClassName + " - GetDoubleArray - " + ex.ToString(), LoggerHighlightType.Error, 5);
//                    MGLErrorLog.LogError(thisClassName, "GetDoubleArray", ex, LoggerErrorTypes.Parsing);
                }
            }
            return ds;
        }
        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Takes an ArrayList of doubles</summary>
        public static double[] GetDoubleArray(ArrayList listOfDoubles) {
            double[] ds = null;

            if (listOfDoubles != null) {
                try {
                    ds = new double[listOfDoubles.Count];

                    int counter = 0;
                    foreach (double d in listOfDoubles) {
                        ds[counter] = d;
                        counter++;
                    }
                } catch (Exception ex) {
                    Logger.Log(thisClassName + " - GetDoubleArray - " + ex.ToString(), LoggerHighlightType.Error, 5);
//                    MGLErrorLog.LogError(thisClassName, "GetDoubleArray", ex, LoggerErrorTypes.Parsing);
                }
            }
            return ds;
        }
        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Takes an ArrayList of doubles</summary>
        public static double[] GetDoubleArray(List<double> listOfDoubles) {
            double[] ds = null;

            if (listOfDoubles != null) {
                try {
                    ds = new double[listOfDoubles.Count];

                    int counter = 0;
                    foreach (double d in listOfDoubles) {
                        ds[counter] = d;
                        counter++;
                    }
                } catch (Exception ex) {
                    Logger.Log(thisClassName + " - GetDoubleArray - " + ex.ToString(), LoggerHighlightType.Error, 5);
                    //MGLErrorLog.LogError(thisClassName, "GetDoubleArray", ex, LoggerErrorTypes.Parsing);
                }
            }
            return ds;
        }




        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Takes an ArrayList of doubles</summary>
        public static int[] GetIntegerArray(ArrayList listOfIntegers) {
            int[] ints = null;

            if (listOfIntegers != null) {
                try {
                    ints = new int[listOfIntegers.Count];

                    int counter = 0;
                    foreach (int i in listOfIntegers) {
                        ints[counter] = i;
                        counter++;
                    }
                } catch (Exception ex) {
                    Logger.Log(thisClassName + " - GetIntegerArray - " + ex.ToString(), LoggerHighlightType.Error, 5);
                    //MGLErrorLog.LogError(thisClassName, "GetIntegerArray", ex, LoggerErrorTypes.Parsing);
                }
            }
            return ints;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Takes a csv list of strings, no spaces</summary>
        public static string[] GetStringArray( bool useQuotes, bool singleQuoted, string csvStringList) {
            string[] ss = null;

            if (csvStringList != null) {
                try {

                    if (useQuotes) {
                        csvStringList = GetQuotedList(singleQuoted, csvStringList);
                    }
                    ss = csvStringList.Split(',');
                } catch (Exception ex) {
                    Logger.Log(thisClassName + " - GetStringArray - " + ex.ToString(), LoggerHighlightType.Error, 5);
                    //MGLErrorLog.LogError(thisClassName, "GetStringArray", ex, LoggerErrorTypes.Parsing);
                }
            }
            return ss;
        }
        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Convert a double array to a string list</summary>
        public static string[] GetStringArray(double[] vals) {
            string[] stringList = null;

            if (vals != null) {
                try {
                    stringList = new string[vals.Length];
                    int count = 0;
                    foreach (double temp in vals) {
                        stringList[count++] = Math.Round(temp, 3).ToString();
                    }

                } catch (Exception ex) {
                    Logger.Log(thisClassName + " - GetStringList - " + ex.ToString(), LoggerHighlightType.Error, 5);
                    //MGLErrorLog.LogError(thisClassName, "GetStringList", ex, LoggerErrorTypes.Parsing);
                }
            }
            return stringList;
        }
        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Takes a csv list of strings, no spaces</summary>
        public static List<string> GetStringList(bool useQuotes, bool singleQuoted, string[] stringArray) {
            List<string> stringList = null;

            if (stringArray != null) {
                try {
                    stringList = new List<string>();
                    foreach (string temp in stringArray) {
                        if (useQuotes == true) {
                            if (singleQuoted) {
                                stringList.Add(Quote(temp));
                            } else {
                                stringList.Add(QuoteDouble(temp));
                            }
                        } else {
                            stringList.Add(temp);
                        }
                    }
                } catch (Exception ex) {
                    Logger.Log(thisClassName + " - GetStringList - " + ex.ToString(), LoggerHighlightType.Error, 5);
//                    MGLErrorLog.LogError(thisClassName, "GetStringList", ex, LoggerErrorTypes.Parsing);
                }
            }
            return stringList;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Takes a csv list of strings, no spaces</summary>
        public static string GetQuotedList(bool singleQuoted, string csvStringList) {
            if (csvStringList != null) {
                try {

                    csvStringList = csvStringList.Replace(", ", ",");
                    csvStringList = csvStringList.Replace(" ,", ",");

                    string quote = "\"";
                    if (singleQuoted == true) {
                        quote = "'";
                    }
                    csvStringList = csvStringList.Replace(",", quote + "," + quote);
                    csvStringList = quote + csvStringList + quote;

                } catch (Exception ex) {
                    Logger.Log(thisClassName + " - GetQuotedList - " + ex.ToString(), LoggerHighlightType.Error, 5);
//                    MGLErrorLog.LogError(thisClassName, "GetQuotedList", ex, LoggerErrorTypes.Parsing);
                }
            }
            return csvStringList;
        }
        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Takes an ArrayList of strings, no spaces</summary>
        public static string GetQuotedList(bool singleQuoted, ArrayList stringList) {
            StringBuilder csvStringList = new StringBuilder();

            if (stringList != null && stringList.Count > 0) {
                try {

                    string quote = "\"";
                    if (singleQuoted == true) {
                        quote = "'";
                    }

                    foreach (string temp in stringList) {
                        csvStringList.Append(quote + temp + quote + ",");
                    }
                    csvStringList.Remove(csvStringList.Length - 1, 1);
                } catch (Exception ex) {
                    Logger.Log(thisClassName + " - GetQuotedList - " + ex.ToString(), LoggerHighlightType.Error, 5);
//                    MGLErrorLog.LogError(thisClassName, "GetQuotedList", ex, LoggerErrorTypes.Parsing);
                }
            }
            return csvStringList.ToString();
        }
        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Takes an ArrayList of strings, no spaces</summary>
        public static string GetQuotedList(bool singleQuoted, string[] stringList) {
            StringBuilder csvStringList = new StringBuilder();

            if (stringList != null && stringList.Length > 0) {
                try {

                    string quote = "\"";
                    if (singleQuoted == true) {
                        quote = "'";
                    }

                    foreach (string temp in stringList) {
                        csvStringList.Append(quote + temp + quote + ",");
                    }
                    csvStringList.Remove(csvStringList.Length - 1, 1);
                } catch (Exception ex) {
                    Logger.Log(thisClassName + " - GetQuotedList - " + ex.ToString(), LoggerHighlightType.Error, 5);
                    //MGLErrorLog.LogError(thisClassName, "GetQuotedList", ex, LoggerErrorTypes.Parsing);
                }
            }
            return csvStringList.ToString();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Aaaaargh - Do not use this - the obfuscator gets it confused with other methods
        ///     List long ==List string in the obfuscation world ....
        /// </summary>
        ////////////public static string GetCSVList(List<string> listOfStrings)
        ////////////{
        ////////////    StringBuilder stringList = new StringBuilder();

        ////////////    if (listOfStrings != null && listOfStrings.Count > 0)
        ////////////    {
        ////////////        foreach (string tempStr in listOfStrings)
        ////////////        {
        ////////////            stringList.Append(tempStr + ",");
        ////////////        }
        ////////////        stringList.Remove(stringList.Length - 1, 1);
        ////////////    }
        ////////////    return stringList.ToString();
        ////////////}

        //-------------------------------------------------------------------------------------------------------------------------
        public static string GetCSVList(ArrayList listOfStrings) {

            StringBuilder stringList = new StringBuilder();

            if (listOfStrings != null && listOfStrings.Count > 0) {
                foreach( string tempStr in listOfStrings ) {
                    stringList.Append(tempStr + ",");
                }
                stringList.Remove(stringList.Length - 1, 1);
            }
            return stringList.ToString();
        }

        public static string GetValueList(Dictionary<string, string> dictionary)
        {

            StringBuilder stringList = new StringBuilder();

            stringList.Append("(");

            if (dictionary != null && dictionary.Count > 0)
            {
                foreach (string key in dictionary.Keys)
                {
                    stringList.Append(key + "),(");
                }
                stringList.Remove(stringList.Length - 2, 2);
            }

            return stringList.ToString();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        public static string GetCSVList(string[] listOfStrings) {

            StringBuilder stringList = new StringBuilder();

            if (listOfStrings != null && listOfStrings.Length > 0) {
                foreach (string tempStr in listOfStrings) {
                    stringList.Append(tempStr + ",");
                }
                stringList.Remove(stringList.Length - 1, 1);
            }
            return stringList.ToString();
        }
        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Currently just copes with ints and doubles</summary>
        public static string GetCSVList(ArrayList listOfNumbers, bool isInt) {
            StringBuilder stringList = new StringBuilder();
            if (listOfNumbers != null && listOfNumbers.Count > 0) {
                if (isInt) {
                    foreach (int tempInt in listOfNumbers) {
                        stringList.Append(tempInt + ",");
                    }
                } else {
                    foreach (double tempDouble in listOfNumbers) {
                        stringList.Append(tempDouble + ",");
                    }
                }
                stringList.Remove(stringList.Length - 1, 1);
            }
            return stringList.ToString();
        }
        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Currently just copes with ints, longs and doubles</summary>
        public static string GetCSVList(ArrayList listOfNumbers, string objType) {
            StringBuilder stringList = new StringBuilder();
            if (listOfNumbers != null && listOfNumbers.Count > 0) {
                if ( objType.Equals( "int" )) {
                    foreach (int tempInt in listOfNumbers) {
                        stringList.Append(tempInt + ",");
                    }
                } else if ( objType.Equals( "long" )) {
                    foreach (long tempLong in listOfNumbers) {
                        stringList.Append(tempLong + ",");
                    }
                } else {
                    foreach (double tempDouble in listOfNumbers) {
                        stringList.Append(tempDouble + ",");
                    }
                }
                stringList.Remove(stringList.Length - 1, 1);
            }
            return stringList.ToString();
        }
        //-------------------------------------------------------------------------------------------------------------------------
        public static string GetCSVList(double[] listOfDoubles) {

            StringBuilder stringList = new StringBuilder();

            if (listOfDoubles != null && listOfDoubles.Length > 0) {
                foreach (double temp in listOfDoubles) {
                    stringList.Append(temp + ",");
                }
                stringList.Remove(stringList.Length - 1, 1);
            }
            return stringList.ToString();
        }
        //-------------------------------------------------------------------------------------------------------------------------
        public static string GetCSVList(int[] listOfIntegers) {

            StringBuilder stringList = new StringBuilder();

            if (listOfIntegers != null && listOfIntegers.Length > 0) {
                foreach (int temp in listOfIntegers) {
                    stringList.Append(temp + ",");
                }
                stringList.Remove(stringList.Length - 1, 1);
            }
            return stringList.ToString();
        }
        //-------------------------------------------------------------------------------------------------------------------------
        public static string GetCSVList(List<int> listOfIntegers) {

            StringBuilder stringList = new StringBuilder();

            if (listOfIntegers != null && listOfIntegers.Count > 0) {
                foreach (int temp in listOfIntegers) {
                    stringList.Append(temp + ",");
                }
                stringList.Remove(stringList.Length - 1, 1);
            }
            return stringList.ToString();
        }
        //-------------------------------------------------------------------------------------------------------------------------
        public static string GetCSVList(List<long> listOfLongs) {

            StringBuilder stringList = new StringBuilder();

            if (listOfLongs != null && listOfLongs.Count > 0) {
                foreach (long temp in listOfLongs) {
                    stringList.Append(temp + ",");
                }
                stringList.Remove(stringList.Length - 1, 1);
            }
            return stringList.ToString();
        }
        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// This does not work when obfuscated - this needs investigation.  Due to there being other method calls with just a list of generics
        /// The obfuscator merges these into one call, so the list of strings appears as a list of numbers!!!
        /// </summary>
        //public static string GetCSVList(List<string> listOfStrings) {

        //    StringBuilder stringList = new StringBuilder();

        //    if (listOfStrings != null && listOfStrings.Count > 0) {
        //        foreach (string temp in listOfStrings) {
        //            stringList.Append(temp + ",");
        //        }
        //        stringList.Remove(stringList.Length - 1, 1);
        //    }
        //    return stringList.ToString();
        //}
        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     This one works ok as the method call is overridden ....
        /// </summary>
        public static string GetCSVList(List<string> listOfStrings, bool useQuotes) {

            StringBuilder stringList = new StringBuilder();

            if (listOfStrings != null && listOfStrings.Count > 0) {
                foreach (string temp in listOfStrings) {
                    if (useQuotes) {
                        stringList.Append(QuoteDouble(temp) + ",");
                    } else {
                        stringList.Append(temp + ",");
                    }
                }
                stringList.Remove(stringList.Length - 1, 1);
            }
            return stringList.ToString();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     11-Feb-2016 - a variation using a custom separator
        /// </summary>
        public static StringBuilder GetList(string[] listOfStuff, string separator) {

            StringBuilder stringList = new StringBuilder();

            if (listOfStuff != null && listOfStuff.Length > 0) {
                foreach (string temp in listOfStuff) {
                        stringList.Append(temp + separator);
                }
                stringList.Remove(stringList.Length - 1, 1);
            }
            return stringList;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary> Gets the min and max in the range </summary>
        public static MGLMaxMin GetMaxMin(double[] values) {
            double tempMin = double.MaxValue;
            double tempMax = double.MinValue;

            foreach (double d in values) {
                if (d < tempMin) {
                    tempMin = d;
                }
                if (d > tempMax) {
                    tempMax = d;
                }
            }

            MGLMaxMin maxMin = new MGLMaxMin();
            maxMin.Max = tempMax;
            maxMin.Min = tempMin;

            return maxMin;
        }
        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary> Gets the min and max in the range </summary>
        public static MGLMaxMin GetMaxMin(List<double> values) {
            double tempMin = double.MaxValue;
            double tempMax = double.MinValue;

            foreach (double d in values) {
                if (d < tempMin) {
                    tempMin = d;
                }
                if (d > tempMax) {
                    tempMax = d;
                }
            }

            MGLMaxMin maxMin = new MGLMaxMin();
            maxMin.Max = tempMax;
            maxMin.Min = tempMin;

            return maxMin;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary> Round a maximum and maxiumum of some data to graph or map to a more suitable range for graphing or mapping. </summary>
        public static MGLMaxMin RoundMaxMin(ref double minimum, ref double maximum) {
            MGLMaxMin maxMin = new MGLMaxMin();
            maxMin.Min = minimum;
            maxMin.Max = maximum;
            return DataUtilities.RoundMaxMin(maxMin);
        }
        //-------------------------------------------------------------------------------------------------------------------------
        public static MGLMaxMin RoundMaxMin(MGLMaxMin maxMin) {
//            double range = maxMin.Max - maxMin.Min;
//            double magnitude = Math.Round(Math.Log10(range));
//            double magMinusOne = magnitude - 1;

//            double maxDec = (maxMin.Max / Math.Pow(10, magMinusOne));
//            double minDec = (maxMin.Min / Math.Pow(10, magMinusOne));

            // If equal, then round up/down so that we have available
            // space above and below the minimum and maximum.
            // Nice - but need to avoid for percents and others that it makes no sense to have negative values for
            //            if (minDec == Math.Floor(minDec)) {
            //                minDec--;
            //            }
            //            if (maxDec == Math.Ceiling(maxDec)) {
            //                maxDec++;
            //            }

//            maxMin.Min = Math.Floor(minDec) * Math.Pow(10, magMinusOne);
  //          maxMin.Max = Math.Ceiling(maxDec) * Math.Pow(10, magMinusOne);

            // round to the second significant digit - standardised this!!!
            maxMin.Min = Math.Floor(maxMin.Min * 100) / 100; //RoundNumber( maxMin.Min, false, 2);
            maxMin.Max = Math.Ceiling(maxMin.Max * 100) / 100; // RoundNumber(maxMin.Max, true, 2);

            return maxMin;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        public static double RoundNumber(double number, bool roundUp) {
            return RoundNumber(number, roundUp, 3);
        }
        //-------------------------------------------------------------------------------------------------------------------------
        public static double RoundNumber(double number, bool roundUp, int significantDigit) {

            int roundAmount = (int) Math.Round( Math.Pow(10, significantDigit), 0);

            if (roundUp) {
                return Math.Ceiling(number * roundAmount) / roundAmount;
            } else {
                return Math.Floor(number * roundAmount) / roundAmount;
            }
//            return DataUtilities.RoundNumber(number, roundUp, 3);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Reasonably straightforwardly generates the nearest rounded number.  The significant digit
        /// is the digit that will be rounded, so (23.36, up, 3) results in 23.4 while (23.36, up, 2) results in 23.
        /// Also note that 0 always remains 0
        /// </summary>
        //public static double RoundNumber(double number, bool roundUp, int significantDigit ) {
        //    double roundedNum = 0;

        //    // if the sig digit is less than 0, default to 2
        //    if (significantDigit < 0) {
        //        significantDigit = 2;
        //    }

        //    if (number > 0) {
        //        try {
        //            // use Floor instead of round to be less dramatic
        //            double magnitude = Math.Floor(Math.Log10(number));
        //            double magMinusOne = magnitude - (significantDigit - 1);

        //            // decrease the given number by a factor of 10 and then round this num up or down
        //            double tempNum = (number / Math.Pow(10, magMinusOne));
        //            if (roundUp == true) {
        //                tempNum = Math.Ceiling(tempNum);
        //            } else {
        //                tempNum = Math.Floor(tempNum);
        //            }
        //            roundedNum = tempNum * Math.Pow(10, magMinusOne);
        //        } catch (Exception ex) {
        //            Logger.Log(thisClassName + " - RoundNumber - " + ex.ToString(), LoggerHighlightType.Error);
        //            //MGLErrorLog.LogError(thisClassName, "RoundNumber", ex, LoggerErrorTypes.Parsing);
        //        }
        //    } else if (number < 0) {
        //        // Negative number, so make positive as cannot log neg nums and then call this method again, but with the opposite roundUp
        //        roundedNum = DataUtilities.RoundNumber(number * -1, !roundUp, significantDigit);
        //        roundedNum *= -1;
        //    }

        //    return roundedNum;
        //}

        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Reasonably straightforwardly generates the nearest rounded number.  The significant digit
        /// is the digit that will be rounded, so (23.36, 3) results in 23.4 while (23.4999, 2) results in 23
        /// Also note that 0 always remains 0
        /// </summary>
        //public static double RoundNumber(double number, int significantDigit) {
        //    double roundedNum = 0;

        //    // if the sig digit is less than 0, default to 2
        //    if (significantDigit < 0) {
        //        significantDigit = 2;
        //    }

        //    if (number > 0) {
        //        try {
        //            // use Floor instead of round to be less dramatic
        //            double magnitude = Math.Floor(Math.Log10(number));
        //            double magMinusOne = magnitude - (significantDigit - 1);

        //            // decrease the given number by a factor of 10 and then round this num up or down
        //            double tempNum = (number / Math.Pow(10, magMinusOne));
        //            tempNum = Math.Round(tempNum);
        //            roundedNum = tempNum * Math.Pow(10, magMinusOne);
        //        } catch (Exception ex) {
        //            Logger.Log(thisClassName + " - RoundNumber - " + ex.ToString(), LoggerHighlightType.Error);
        //            //MGLErrorLog.LogError(thisClassName, "RoundNumber", ex, LoggerErrorTypes.Parsing);
        //        }
        //    } else if (number < 0) {
        //        // Negative number, so make positive as cannot log neg nums and then call this method again, but with the opposite roundUp
        //        roundedNum = DataUtilities.RoundNumber(number * -1, significantDigit);
        //        roundedNum *= -1;
        //    }

        //    return roundedNum;
        //}


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Rounds numbers to the third significant digit</summary>
        //public static double[] RoundArray(double[] numbers) {
        //    for (int i = 0; i < numbers.Length; i++) {
        //        numbers[i] = RoundNumber(numbers[i], 3);
        //    }
        //    return numbers;
        //}




        //-------------------------------------------------------------------------------------------------------------------------
        public static string RemoveHTMLLineBreaks(string s) {
            if (s != null) {
                s = s.Replace("<BR>", " ");
                s = s.Replace("<br>", " ");
                s = s.Replace("<br/>", " ");
                s = s.Replace("<BR/>", " ");
            }
            return s;
        }

                //-------------------------------------------------------------------------------------------------------------------------
        public static string RemoveLineBreaks(string s)
        {
            if (s != null) {
                s = s.Replace("\r", "");
                s = s.Replace("\n", "");
            }
            return s;
        }



        //-------------------------------------------------------------------------------------------------------------------------
        public static string RemoveQuotes(string s) {
            if (s != null) {
                s = s.Replace("'", "");
                s = s.Replace("\"", "");
            }
            return s;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        public static string EscapeQuotes(string s) {
            if (s != null) {
                s = s.Replace("'", "\\'");
                s = s.Replace("\"", "\\\"");
            }
            return s;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary> - _ ' " Dissapear </summary>
        public static string RemoveSpacesQuotesCommasDashes(string s) {
            if (s != null) {
                s = s.Replace(" ", "").Replace("-", "").Replace("'", "").Replace(",", "").Replace("\"", "");
            }
            return s;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary> _ Dissapear as if by magic</summary>
        public static string RemoveUnderscores(string s) {
            if (s != null) {
                s = s.Replace("_", "");
            }
            return s;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Returns true if the given string contains LOWER CASE alphanumberic characters.
        ///     Much faster than the equivalent RegEx</summary>
/*        public static string[] AlphaChars = {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z"};
        public static bool StringContainsAlpha(string s) {
            foreach( string alpha in AlphaChars ) {
                if (s.Contains(alpha)) {
                    return true;
                }
            }
            return false;
        }
*/
        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Returns true if the given string contains LOWER CASE alphanumberic characters.
        ///     Much faster than the equivalent RegEx</summary>
/*        public static string[] NumericStrings = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        public static bool StringContainsNumeric(string s) {
            foreach (string number in NumericStrings) {
                if (s.Contains(number)) {
                    return true;
                }
            }
            return false;
        }
*/

        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///
        ///         Returns true if the given string contains LOWER CASE alphanumberic characters.
        ///         Much faster than the equivalent RegEx
        ///
        /// </summary>
        private static char[] NumericChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        public static bool StringStartsWithNumber(string s) {
            if (s.Length > 0) {
                char firstChar = s.ToCharArray()[0];
                foreach (char tempNum in NumericChars) {
                    if (tempNum.Equals(firstChar)) {
                        return true;
                    }
                }
            }
            return false;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///
        ///         Returns true if the given string contains LOWER CASE alphanumberic characters.
        ///         Much faster than the equivalent RegEx
        ///
        /// </summary>
        ///
        private static char[] AlphaNumericChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        public static bool StringStartsWithAlphaNumbericCharacter(string s) {
            if (s.Length > 0) {
                char firstChar = s.ToLower().ToCharArray()[0];
                foreach (char tempChar in AlphaNumericChars) {
                    if (tempChar.Equals(firstChar)) {
                        return true;
                    }
                }
            }
            return false;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///
        ///         Returns true if the given string contains LOWER CASE alpha characters.
        ///         Much faster than the equivalent RegEx
        ///
        /// </summary>
        ///
        private static char[] AlphaChars = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        public static bool StringStartsWithAlphaCharacter(string s) {
            if (s.Length > 0) {
                char firstChar = s.ToLower().ToCharArray()[0];
                foreach (char tempChar in AlphaChars) {
                    if (tempChar.Equals(firstChar)) {
                        return true;
                    }
                }
            }
            return false;
        }
        //-------------------------------------------------------------------------------------------------------------------------
        public static bool StringEndsWithAlphaCharacter(string s) {
            if (s.Length > 0) {
                char lastChar = s.ToLower().ToCharArray()[s.Length - 1];
                foreach (char tempChar in AlphaChars) {
                    if (tempChar.Equals(lastChar)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string RemoveSlashes(string s)
        {
            string newString = null;

            if (s != null)
            {
                newString = s.Replace("\\", String.Empty).Replace("/", String.Empty);
            }

            return newString;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Wraps the given string in single quotes; escapes any existing quotes</summary>
        public static string Quote(string s) {
            return Quote(ref s);
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string Quote(ref string s) {
            if (s != null) {
                return "'" + s.Replace("'", "\\'") + "'";
            }
            return null;
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string QuoteDouble(string s) {
            return QuoteDouble(ref s);
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string QuoteDouble(ref string s) {
            if (s != null) {
                return "\"" + s.Replace("\"", "\\\"") + "\"";
            }
            return null;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string QuoteNum(string s) {
            if (s != null && s != "") {
                return s;
            }
            return "null";
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Formats the given date time string (probably from the database) into the database neutral format yyyy-mm-dd</summary>
        public static string FormatDatabaseDate(string date) {
            return DateTimeInformation.FormatDatabaseDate(date);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        ///<summary>
        ///     Formats the given date time string (probably from the database) into a DateTime object (JUST the Date)
        ///     The dbDate is in the format dd-mm-yyyy hh:mm:ss
        ///</summary>
        public static DateTime FormatDate(string dbDate) {
            return DateTimeInformation.FormatDate(dbDate);
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Formats the given date time string (probably from the database) into 311299 format (ddmmyy)</summary>
        public static string FormatTableNameDate(string date) {
            return DateTimeInformation.FormatTableNameDate(date);
        }



        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Trims the field, if null sets to an empty string.  This defaults to _strings_</summary>
        public static string ParseContent(string content) {
            return ParseContent(content, false);
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Trims the field, if null sets to an empty string</summary>
        public static string ParseContent(string content, bool isNum) {
            if (content == null) {
                return (isNum) ? "0" : "";
            } else {
                string tempContent = content.Trim();
                if (isNum && tempContent == "") {
                    tempContent = "0";
                }
                return tempContent;
            }
        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///
        ///     Parses a CSV line, with the separators currently assumed to be commas (,).  This approach is necessary
        ///     as there may be commas within string fields.  This approach ignores commas within string fields
        ///
        /// </summary>
        public static string[] ParseCSVLine(string line) {
            char separator = ',';
            char quote = '"';
            string[] csvBits = null;

            if (line != null && line != "") {
                bool inTextField = false;

                // if there are text quote, we need to parse through the data character by character
                char[] characters = line.ToCharArray();
                StringBuilder cleanedLine = new StringBuilder();

                char previousChar = ' ';
                foreach (char currentChar in characters) {
                    // looking for the opening quote or the delimiter
                    if (currentChar == quote && previousChar !=  '\\') {
                        inTextField = (inTextField == true) ? false : true;
                    } else if (inTextField == false && currentChar == separator) {
                        cleanedLine.Append( "¬" );
                    } else {
                        cleanedLine.Append( currentChar );
                    }
                    previousChar = currentChar;
                }
                csvBits = cleanedLine.ToString().Split('¬');
            }
            return csvBits;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Removes quotes from the start and end of the string only</summary>
        public static string DeQuote(string quotedField) {
            if (quotedField != null && quotedField != "") {
//                quotedField = quotedField.Trim();
                if (quotedField.StartsWith("\"")) {
                    quotedField.Substring(1);
                }
                if (quotedField.EndsWith("\"")) {
                    quotedField.Substring(0, quotedField.Length - 2);
                }
            }
            return quotedField;
        }

        private static string something = "J1ngl3_D0bbl3_1432";

        public static string Something {
            get { return something; }
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///
        ///         Gets a Long from the leading edge of a string.
        ///         Note doesn't yet deal with negative numbers or scientifics
        ///         Designed for strimming numbers from E.g. Building Text fields (13, Lurvely Cottage)
        ///
        ///
        /// </summary>
        public static long GetNumberFromLeadingEdgeOfString(string s) {
            long myNum = -1;

            if (s.Length > 0) {
                string tempString = "";
                foreach (char tempS in s.ToCharArray()) {

                    if (long.TryParse(tempS.ToString(), out myNum)) {
                        tempString = tempString + tempS.ToString();
                    } else {
                        break;
                    }
                }

                if (long.TryParse(tempString, out myNum) == false) {
                    myNum = -1;
                }
            }
            return myNum;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///
        ///         Gets a Long from the trailing edge of a string.
        ///         Note doesn't yet deal with negative numbers or scientifics
        ///         Designed for strimming numbers from E.g. SubBuildingInfomation fields (Flat 13)
        ///
        /// </summary>
        public static long GetNumberFromTrailingEdgeOfString(string s) {
            long myNum = -1;

            if (s.Length > 0) {
                string tempString = "";
                char[] tempChars = s.ToCharArray();
                for (int i = tempChars.Length -1; i >= 0; i--) {
                    char tempS = tempChars[i];

                    if (long.TryParse(tempS.ToString(), out myNum)) {
                        tempString = tempS.ToString() + tempString;
                    } else {
                        break;
                    }
                }

                if (long.TryParse(tempString, out myNum) == false) {
                    myNum = -1;
                }
            }
            return myNum;
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static bool ListContains(List<MGLSimpleStringContent> sscList, string s, bool isName) {
            bool found = false;

            if (sscList != null && sscList.Count > 0 && s != null) {

                Dictionary<string, int> myDict = new Dictionary<string, int>();

                foreach (MGLSimpleStringContent ssc in sscList) {
                    string temp = "";
                    if (isName) {
                        temp = ssc.Name.ToLower();
                    } else {
                        temp = ssc.Value.ToLower();
                    }
                    if (myDict.ContainsKey(temp) == false) {
                        myDict.Add(temp, 0);
                    }

                }

                if (myDict.ContainsKey(s.ToLower())) {
                    found = true;
                }
            }

            return found;
        }



        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Dictionary<string, int> ConvertListToDictionary(List<string> data) {
            Dictionary<string, int> myDict = null;

            if ( data != null ){
                myDict = new Dictionary<string, int>();

                foreach (string temp in data) {
                    if (myDict.ContainsKey(temp) == false) {
                        myDict.Add(temp, 1);
                    } else {
                        int myVal = 0;
                        myDict.TryGetValue(temp, out myVal);
                        myDict.Remove(temp);
                        myDict.Add(temp, ++myVal);
                    }
                }
            }

            return myDict;
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Dictionary<string, int> ConvertListToDictionary(List<MGLSimpleStringContent> data) {
            Dictionary<string, int> myDict = null;

            if (data != null) {
                myDict = new Dictionary<string, int>();

                foreach (MGLSimpleStringContent ssc in data) {
                    if (myDict.ContainsKey(ssc.Name) == false) {
                        myDict.Add(ssc.Name, 1);
                    } else {
                        int myVal = 0;
                        myDict.TryGetValue(ssc.Name, out myVal);
                        myDict.Remove(ssc.Name);
                        myDict.Add(ssc.Name, ++myVal);
                    }
                }
            }

            return myDict;
        }


        ////--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //public static KeyValuePair<int, double> ConvertStringtoKeyValuePair(string data) {



        //}

        ////--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //public static List<KeyValuePair<int, double>> ConvertStringtoKeyValuePairList(string data) {
        //    List<KeyValuePair<int, double>> list = new List<KeyValuePair<int, double>>();

        //    if (data != null && data != "") {

        //    }

        //    return list;
        //}

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Great new method to remove diacritics .....
        /// </summary>
        public static string RemoveDiacritics(string text) {
            StringBuilder str = new StringBuilder();

            if (text != null && text != "") {

                string formD = text.Normalize(NormalizationForm.FormD);

                foreach (char ch in formD) {
                    UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                    if (uc != UnicodeCategory.NonSpacingMark) {
                        str.Append(ch);
                    }
                }
            }

            return str.ToString().Normalize(NormalizationForm.FormC);
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets a series of indices randomly .....
        /// </summary>
        public static bool GetRandomNumbers(int numInArray, int numRandomIndicesToGenerate, out List<int> randomIndices) {
            bool success = false;

            randomIndices = new List<int>();

            if (numInArray < numRandomIndicesToGenerate) {
                Logger.LogError(5, "The number of indices is less than what you are requiring us to generate.  That is just fucking stupid!");
            } else if (numInArray == numRandomIndicesToGenerate) {

                for (int i = 0; i < numInArray; i++ ) {
                    randomIndices.Add( i );
                }

            } else {

                while (randomIndices.Count < numRandomIndicesToGenerate) {

                    Random random = new Random();
                    int tempRandomNum = random.Next(0, numInArray);
                    if (randomIndices.Contains(tempRandomNum) == false) {
                        randomIndices.Add(tempRandomNum);
                    }

                }
            }

            success = randomIndices.Count == numRandomIndicesToGenerate;


            return success;
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///      Converts empty strings to NULL and quotes and escapes all quotes in strings
        /// </summary>
        public static string DatabaseifyString(string str) {
            return DatabaseifyString(str, false);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///      Converts empty strings to NULL and quotes and escapes all quotes in strings
        ///      set useEmptyNullValue to true if you want an empty string!!!
        /// </summary>
        public static string DatabaseifyString(string str, bool useBlankNullValue) {
            return DatabaseifyString(str, useBlankNullValue, false);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///      Converts empty strings to NULL and quotes and escapes all quotes in strings
        ///      set useBlankNullValue to true if you want an empty string!!!
        ///      And set emptyString to true if you want empty strings instead of nulls ...
        /// </summary>
        public static string DatabaseifyString(string str, bool useBlankNullValue, bool useEmptyString) {

            if (str == null || str == "") {

                if (useEmptyString == false) {
                    str = (useBlankNullValue == true) ? "" : "NULL";
                } else {
                    str = "''";
                }

            } else {

                str = "'" + DatabaseHelper.SQL_INJECTION_CHECK_PARAMETER(false, str, false) + "'";

            }

            return str;
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     converts \r\n and \n into <br /> for html display
        /// </summary>
        public static string HTMLifyNewLines(string str) {

            if (str != null) {

                // 11-Oct-15 - ignore strings that end with li, ul or ol as these will already produce a new line by default ...
                if (str.Contains("</li>") == true || str.Contains("</ul>") == true || str.Contains("</ol>") == true) {

                    str = str.Replace("</li>\r\n", "</li>");
                    str = str.Replace("</li>\n", "</li>");

                    str = str.Replace("</ul>\r\n", "</ul>");
                    str = str.Replace("</ul>\n", "</ul>");

                    str = str.Replace("</ol>\r\n", "</ol>");
                    str = str.Replace("</ol>\n", "</ol>");

                }

                // the default
                str = str.Replace("\r\n", "<br />");
                str = str.Replace("\n", "<br />");

            }
            return str;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string TitleCaseString(string str, bool justFirstChar) {

            if (str != null && str != "") {
                if (justFirstChar == true) {

                    if (str.Length == 1) {
                        return str.ToUpper();
                    } else {
                        return char.ToUpper(str[0]) + str.Substring(1).ToLower();
                    }

                } else {

                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());

                }
            }

            return str;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Counts the line breaks in the given string and returns the line break index of the nth line break where n is specified by
        ///     the lineBreakToGetIndexOf.  This parameter is one based, so 1 would be the first index, etc
        ///     Added 4-Nov-2015
        /// </summary>
        public static int CountLineBreaks(string s, int lineBreakToGetIndexOf, out int lineBreakIndex) {
            int count = 0;
            lineBreakIndex = -1;

            char newLine = '\n';

            if (string.IsNullOrEmpty(s) == false) {

                // optimise by checking for line breaks at the outset
                if (s.Contains(newLine.ToString()) == true) {

                    // now iterate through the characters.  Count the line breaks and if we find the nth line break,
                    // lets get the index of this location in the string
                    int stringCounter = 0;
                    foreach (char c in s.ToCharArray()) {
                        if (c == newLine) {
                            count++;
                            if (lineBreakToGetIndexOf == count) {
                                lineBreakIndex = stringCounter;
                            }
                        }
                        stringCounter++;
                    }
                }
            }

            return count;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     http://stackoverflow.com/questions/4135317/make-first-letter-of-a-string-upper-case-for-maximum-performance
        /// </summary>
        public static string FirstLetterToUpper(string str) {
            if (str == null)
                return null;

            if (str.Length > 1) {
                return char.ToUpper(str[0]) + str.Substring(1);
            } else {
                return str.ToUpper();
            }            
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     http://stackoverflow.com/questions/4135317/make-first-letter-of-a-string-upper-case-for-maximum-performance
        /// </summary>
        public static string ToTitleCase(string str) {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }


    } // End of Class
}
