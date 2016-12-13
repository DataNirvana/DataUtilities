using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MGL.Data.DataUtilities;

//---------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {
    //------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Created May 2011, Edgar Scrase in Islamabad
    ///     Well its a point init - in the simplest kind of form ....
    ///     If we need to get sexier then will need to import the SVG Library ...
    /// </summary>
    public struct Point {
        public double x;
        public double y;

        public Point(double x, double y) {
            this.x = x;
            this.y = y;
        }
    }


    //------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Created May 2011, Edgar Scrase in Islamabad
    ///     Attempts to parse various fairly crappily formatted Latitude and Longitudes and return the info in decimal degrees
    ///     Oh the Joys ...
    /// </summary>
    public class ConvertCoordinates {

        /*
         * To convert from decimal to degrees minutes and seconds ...
            1. The whole units of degrees will remain the same (i.e. in 121.135° longitude, start with 121°).
            2. Multiply the decimal by 60 (i.e. .135 * 60 = 8.1).
            3. The whole number becomes the minutes (8').
            4. Take the remaining decimal and multiply by 60. (i.e. .1 * 60 = 6).
            5. The resulting number becomes the seconds (6"). Seconds can remain as a decimal.
            6. Take your three sets of numbers and put them together, using the symbols for degrees (°), minutes (‘), and seconds (") (i.e. 121°8'6" longitude)
        */


        /*
         * To convert from Degrees Minutes Seconds to Decimal Degrees
         * Math.round(absdlat + (absmlat/60.) + (absslat/3600.)) * latsign/1000000
         */

        public static double minLong = 60;
        public static double maxLong = 80;

        public static double minLat = 23;
        public static double maxLat = 38;


        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        public ConvertCoordinates() {

        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///    Longitude = Eastings
        ///    Latitude = Northings
        /// </summary>
        public bool ConvertDegreesMinutesSecondsToDecimalDegrees(string dmsLongitude, string dmsLatitude, out Point decimalDegrees,
            out string warning, out LoggerHighlightType lht) {

            bool success = false;

            decimalDegrees = new Point();
            warning = "";
            lht = LoggerHighlightType.Normal;

            LoggerHighlightType lht2 = LoggerHighlightType.Normal;
            string warningX = "";
            string warningY = "";

            // Parse the longitude ...
            success = ParseCoordinate(dmsLongitude, out decimalDegrees.x, out warningX, out lht);

            // Parse the Latitude ...
            success = success & ParseCoordinate(dmsLatitude, out decimalDegrees.y, out warningY, out lht2);

            if (warningX != "") {
                warning = warningX;
            } else if (warningY != "") {
                warning = warningY;
                lht = lht2;
            }

            // final check - that it is in our bounding box ...
            if (success) {
                if (decimalDegrees.x < minLong || decimalDegrees.x > maxLong) {
                    success = false;
                    warning = "The Longitude is not correct for the area of interest - it should be between "
                        +minLong+" and "+maxLong+" but is actually "+decimalDegrees.x+".  Sort it out!";
                    lht = LoggerHighlightType.Error;
                }
                if (decimalDegrees.y < minLat || decimalDegrees.y > maxLat) {
                    success = false;
                    warning = "The Latitude is not correct for the area of interest - it should be between "
                        +minLat+" and "+maxLat+" but is actually "+decimalDegrees.y+".  Sort it out!";
                    lht = LoggerHighlightType.Error;
                }

            }

            return success;
        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool ParseCoordinate(string coordinateString, out double decimalDegreeCoordinate, out string warning, out LoggerHighlightType lht) {
            bool success = false;

            decimalDegreeCoordinate = 0;
            warning = "";
            lht = LoggerHighlightType.Normal;

            try {

                if (coordinateString != null && coordinateString != "") {

                    coordinateString = coordinateString.Trim();

                    //______01______ remove leading N and E
                    if (Regex.IsMatch(coordinateString, "^[A-Za-z]")) {
                        coordinateString = coordinateString.Substring(1);
                        coordinateString = coordinateString.Trim();
                    }

                    //______02______ remove trailing N and E (Added 24th May 2011) - Two cases 45.5555E and 45.454545-E
                    if (Regex.IsMatch(coordinateString, "[0-9]+ ?[-]? ?[A-Za-z]$")) {
                        coordinateString = coordinateString.Substring(0, coordinateString.Length - 1);
                        coordinateString = coordinateString.Trim();

                        if (Regex.IsMatch(coordinateString, "[0-9]+ ?[-]$")) {
                            coordinateString = coordinateString.Substring(0, coordinateString.Length - 1);
                            coordinateString = coordinateString.Trim();
                        }
                    }


                    // These are all not quite good enough to parse ...
                    // 30.27622* - ok
                    // 070*59523 - ok
                    // 41 52.762' - ok
                    //24.4785 0 - ok
                    // `29. 13470 - ambiguoys
                    // 68 07` 25" - ok
                    // 27 14' 50" - ok
                    // 28⁰03857 - ambiguous
                    // 28⁰03.817 - ambiguous

                    // Use these switches for debugging ...
                    //if (coordinateString.Equals("30.27622*")) {
                    //    string temp = "";
                    //}

                    //if (coordinateString.Equals("24.4785 0")) {
                    //    string temp = "";
                    //}

                    //if (coordinateString.Equals("41 52.762")) {
                    //    string temp = "";
                    //}

                    //if (coordinateString.Equals("070*59523")) {
                    //    string temp = "";
                    //}


                    // 3-May-2011 - Added the * at the end ...
                    //28.44545*
                    if (Regex.IsMatch(coordinateString, "^[0-9]+ ?[.] ?[0-9]+[*]?$")) {                    //______1______ Start with the GOOD options!

                        // remove any spaces in funky places ...
                        string tempCoordinateString = coordinateString.Replace(" ", "");
                        tempCoordinateString = tempCoordinateString.Replace("*", "");

                        success = double.TryParse(tempCoordinateString, out decimalDegreeCoordinate);
                        if (success == false) {
                            warning = "Could not parse the given coordinate to a decimal degree - the given value was " + coordinateString + ".";
                            lht = LoggerHighlightType.Error;
                        } else if (coordinateString.Length - coordinateString.IndexOf(".") < 5) {
                            warning = "Not enough decimal places - only " + (coordinateString.Length - coordinateString.IndexOf(".")) + " were found.";
                            lht = LoggerHighlightType.Warning;
                        }


                    } else if (Regex.IsMatch(coordinateString, "^[0-9]+$")) {                      //______2______ No decimal place ....

                        // hmmm - make an assumption that the first two digits are the real number and the rest is the decimal ....
                        // Flag as a warning
                        if (coordinateString.Length > 2) {
                            string firstBit = coordinateString.Substring(0, 2);
                            string secondBit = coordinateString.Substring(2);

                            if (secondBit.Length < 5) {
                                warning = "Not enough decimal places found after splitting - only " + secondBit.Length + " were found.";
                                lht = LoggerHighlightType.Warning;
                            }

                            success = double.TryParse(firstBit + "." + secondBit, out decimalDegreeCoordinate);
                            if (success == false) {
                                warning = "Could not parse the given coordinate to a decimal degree - the given value was " + coordinateString + ".";
                                lht = LoggerHighlightType.Error;
                            }

                        } else {
                            warning = "The given coordinate was too short! - the given value was " + coordinateString + ".";
                            lht = LoggerHighlightType.Error;
                        }

                    } else if (Regex.IsMatch(coordinateString, "^[0-9]+[** .]+[0-9]+$")) {                      //______3______ No decimal place ....
                        // 30*16309   º
                        //070*95506
                        //30*16309
                        string tempCoordinateString = coordinateString.Replace(" ", ".");
                        tempCoordinateString = tempCoordinateString.Replace("*", ".");
                        tempCoordinateString = tempCoordinateString.Replace("..", ".");
                        tempCoordinateString = tempCoordinateString.Replace("..", ".");

                        success = double.TryParse(tempCoordinateString, out decimalDegreeCoordinate);
                        if (success == false) {
                            warning = "Could not parse the given coordinate to a decimal degree - the given value was " + coordinateString + ".";
                            lht = LoggerHighlightType.Error;
                        }

                    } else if (Regex.IsMatch(coordinateString, "^[0-9]+[.]+[0-9 ]+$")) {                      //______4______ decimal place and random spaces ....
                        // 30.1630 9
                        string tempCoordinateString = coordinateString.Replace(" ", "");
                        tempCoordinateString = tempCoordinateString.Replace("..", ".");

                        success = double.TryParse(tempCoordinateString, out decimalDegreeCoordinate);
                        if (success == false) {
                            warning = "Could not parse the given coordinate to a decimal degree - the given value was " + coordinateString + ".";
                            lht = LoggerHighlightType.Error;
                        }

                    } else if (Regex.IsMatch(coordinateString, "^[0-9]+[ ]?[⁰º]+[ ]?[0-9]+[.][0-9]+$")) {                      //______5______ Decimal Minutes ....
                        //28⁰03.244
                        // º
                        string tempCoordString = coordinateString.Replace(" ", "");
                        string[] bits = tempCoordString.Split(new string[] { "º", "⁰" }, StringSplitOptions.RemoveEmptyEntries);

                        if (bits != null && bits.Length >= 2) {

                            double degrees, minutes, seconds = 0;
                            success = double.TryParse(bits[ 0 ], out degrees);
                            success = success & double.TryParse(bits[ 1 ], out minutes);

                            // seconds will always be zero in this instance ...
                            decimalDegreeCoordinate = ConvertCoordinate(degrees, minutes, seconds);

                            success = (decimalDegreeCoordinate > 0);

                        } else {
                            warning = "Couldn't parse the given coordinate into Degrees, and Decimal Minutes - the given value was " + coordinateString + ".";
                            lht = LoggerHighlightType.Error;
                        }


                    } else if (Regex.IsMatch(coordinateString, "^[0-9]+[ ]?[˚]+[ ]?[0-9]+[ ]?[’][ ]?[0-9]+[ ]?[”][ ]?$")) {                      //______6______ Decimal Minutes Seconds ....

                        // 70˚ 43’ 758”
                        // These are actually Decimal minutes ...  Seconds should only have up to 60!!!!!

                        string tempCoordString = coordinateString.Replace(" ", "");
                        string[] bits = tempCoordString.Split(new string[] { "˚", "’", "”" }, StringSplitOptions.RemoveEmptyEntries);

                        if (bits != null && bits.Length >= 3) {

                            double degrees, minutes, seconds = 0;
                            success = double.TryParse(bits[0], out degrees);
                            // make the minutes look pretty ...
                            string tempMinutes = bits[1].Trim() + "." + bits[2].Trim();
                            success = success & double.TryParse(tempMinutes, out minutes);

                            // seconds will always be zero in this instance ...
                            decimalDegreeCoordinate = ConvertCoordinate(degrees, minutes, seconds);

                            success = (decimalDegreeCoordinate > 0);

                        } else {
                            warning = "Couldn't parse the given coordinate into Degrees, and Decimal Minutes - the given value was " + coordinateString + ".";
                            lht = LoggerHighlightType.Error;
                        }

                    } else if (Regex.IsMatch(coordinateString, "^[0-9]+[ .]+[0-9]+[,. ]+[0-9]+$")) {                      //______7______ Decimal Minutes ....
                        //28 03.244
                        // 28.23.455
                        // 27 45,454
                        string[] bits = coordinateString.Split(new string[] { " ", ".", "," }, StringSplitOptions.RemoveEmptyEntries);

                        if (bits != null && bits.Length >= 2) {

                            double degrees, minutes, seconds = 0;
                            success = double.TryParse(bits[0], out degrees);

                            string tempMinutes = bits[1].Trim() + "." + bits[2].Trim();
                            success = success & double.TryParse(tempMinutes, out minutes);

                            // seconds will always be zero in this instance ...
                            decimalDegreeCoordinate = ConvertCoordinate(degrees, minutes, seconds);

                            success = (decimalDegreeCoordinate > 0);

                        } else {
                            warning = "Couldn't parse the given coordinate into Degrees, and Decimal Minutes - the given value was " + coordinateString + ".";
                            lht = LoggerHighlightType.Error;
                        }





                    } else {                                                                                                  //______8______ Some sort of Decimals, Minutes and Seconds ....

                        // Ok - got to here - so we need to split it out as an Decimal Minutes Seconds activity
                        //Logger.Log( "******************Splitting Decimals, Minutes and Seconds ..." );
                        // Try parsing on spaces and all the other natty characters ...
                        // Strim non numeric characters ...
                        //30,2',13"

                        string[] bits = coordinateString.Split(new string[] { " ", "°", "*", ".", "'", "\"", "-", ",", "`" }, StringSplitOptions.RemoveEmptyEntries);

                        if (bits != null) {
                            if (bits.Length >= 3) {

                                if (bits.Length > 3) {
                                    warning = "Couldn't parse the given coordinate into Decimals, Minutes and Seconds - too many numeric parts - the given value was " + coordinateString + ".";
                                    lht = LoggerHighlightType.Warning;
                                    //Logger.Log( "Too Many Bits MAN - found "+bits.Length+" ..." );
                                }

                                // So how do we differentiate between decimal minutes and degrees, minutes and seconds?

                                string tempDegrees = "";
                                string tempMinutes = "";
                                string tempSeconds = "";

                                foreach (string bit in bits) {
                                    if (bit != null && bit != "" && Regex.IsMatch(bit, "^[0-9]+$")) {
                                        if (tempDegrees == "") {
                                            tempDegrees = bit;
                                        } else if (tempMinutes == "") {
                                            tempMinutes = bit;
                                        } else if (tempSeconds == "") {
                                            tempSeconds = bit;
                                        } else {
                                            // oh fuck - there are four numeric parts to this string!!!
                                            // if decimal seconds, then append appropriately ....
                                            if (bits.Length == 4) {
                                                //Logger.Log("Appending Bit "+bit+" to the seconds "+tempSeconds+" ...");
                                                tempSeconds = tempSeconds + bit;
                                            } else {
                                                warning = "Couldn't parse the given coordinate into Decimals, Minutes and Seconds - too many numeric parts - the given value was " + coordinateString + ".";
                                                lht = LoggerHighlightType.Error;
                                            }
                                        }
                                    }
                                }

                                if (tempDegrees != "" && tempMinutes != "" && tempSeconds != "") {
                                    double degrees, minutes, seconds = 0;
                                    success = double.TryParse(tempDegrees, out degrees);
                                    success = success & double.TryParse(tempMinutes, out minutes);
                                    success = success & double.TryParse(tempSeconds, out seconds);

                                    if (seconds > 60) {
                                        if (seconds >= 100) {
                                            if (seconds >= 1000) {
                                                seconds = seconds / 10000 * 60;
                                            } else {
                                                seconds = seconds / 1000 * 60;
                                            }
                                        }
                                    }

                                    decimalDegreeCoordinate = ConvertCoordinate(degrees, minutes, seconds);



                                } else {
                                    // fuckity fuck - there are not enough decimal parts!!
                                    warning = "Couldn't parse the given coordinate into Decimals, Minutes and Seconds - not enough numeric parts - the given value was " + coordinateString + ".";
                                    lht = LoggerHighlightType.Error;
                                }

                            } else {

                                // Too complicated for now !!!
                                warning = "Couldn't parse the given coordinate into Decimals, Minutes and Seconds - the given value was " + coordinateString + ".";
                                lht = LoggerHighlightType.Error;

                            }
                        } else {
                            // Too complicated for now !!!
                            warning = "Couldn't parse the given coordinated into Decimals, Minutes and Seconds - the given value was " + coordinateString + ".";
                            lht = LoggerHighlightType.Error;
                        }
                    }


                } else {
                    warning = "No Coordinate Information Provided!!!";
                    lht = LoggerHighlightType.Error;
                }

            } catch (Exception ex) {
                Logger.Log("Error extracting the coordinate - the given value was " + coordinateString + " . " + ex.ToString());
                lht = LoggerHighlightType.Error;
            }

            return success;
        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        public double ConvertCoordinate(double degrees, double minutes, double seconds) {
            double decimalCoordinate = 0;

            int sign = (degrees > 0) ? 1 : -1;

            decimalCoordinate = (Math.Abs(degrees) + Math.Abs(minutes / 60) + Math.Abs(seconds / 3600)) * sign;

            // Tidy it up .....
            decimalCoordinate = Math.Round(decimalCoordinate, 5);

            return decimalCoordinate;
        }


    }
}
