using System;
using System.Text;
using System.Globalization;
using MGL.DomainModel;

//-----------------------------------------------------------------------------------------------------------------------------------------------------------------
/**
	Author: Edgar Scrase
	Copyright: Manchester Geomatics Limited, 2004.  All Rights Reserved
	Date: September 2004

*/
namespace MGL.Data.DataUtilities {

	//--------------------------------------------------------------------------------------------------------------------------------------------------------------
	/// <summary>
	/// Summary description for Class2.
	/// </summary>
	public static class DateTimeInformation {

		private static string[] months = { "Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec" };
        private static string[] fullMonths = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        //public static readonly DateTime NullDate = new DateTime(1, 1, 1, 1, 1, 1);
        // 2-Nov-2016 - Updated this to get it from the Domain model for consistency
        // It is equivalent to this amount of ticks: 36610000000 or -2.2089522e+12 (-2208952200000) in JavaScript notation (which pivots on the 1 Jan 1970 ...)
        // see this link for more information http://www.codeproject.com/Questions/859061/How-do-I-convert-Csharp-DateTime-Ticks-to-javascri
        public static readonly DateTime NullDate = DateTimeNull.NullDate;
        // Javascript cannot handle well dates too far in arrears, therefore 1901-1-1 1:1:1 is as far back as we can go ...
        public static readonly DateTime NullDateJS = new DateTime(1901, 1, 1, 1, 1, 1);


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static bool IsNullDate(DateTime dtToTest) {
            if (dtToTest.CompareTo(NullDate) == 0 || dtToTest == DateTime.MaxValue || dtToTest == DateTime.MinValue) {
                return true;
            }
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static bool IsNullJavascriptDate(DateTime dtToTest) {
            if (dtToTest.CompareTo(NullDateJS) == 0 || dtToTest == DateTime.MaxValue || dtToTest == DateTime.MinValue) {
                return true;
            }
            return false;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        ///<summary>if format is "number", then 20040930, else 30Sep2004</summary>
        public static string GetCurrentDate( string format ) {
			string date = "";

            if ( format != null && format != "" && format.ToLower().Equals( "number" ) ) {            //        20040930 (the equivalent of 30Sep2004)

                date = System.DateTime.Today.Year.ToString();
                int monthInt = System.DateTime.Today.Month;
                date = (monthInt < 10) ? date+"0"+monthInt : date+monthInt;
                int day = System.DateTime.Today.Day;
                date = (day < 10) ? date+"0"+day : date+day;

            } else {                                                             //      The default
                int day = System.DateTime.Today.Day;
                date = (day < 10) ? date+"0"+day : date+day;
                int monthInt = System.DateTime.Today.Month;
                date= date+months[monthInt - 1];
                date = date+System.DateTime.Today.Year.ToString();
            }
			return date;
		}




		//-----------------------------------------------------------------------------------------------------------------------------------------------------------
        ///<summary>201504</summary>
		public static string GetCurrentTime() {

			string time = "";
			int hour = System.DateTime.Now.Hour;
			time = ( hour < 10 ) ? time+"0"+hour : time+hour;
			int minute = System.DateTime.Now.Minute;
			time = ( minute < 10 ) ? time+"0"+minute : time+minute;
			int second = System.DateTime.Now.Second;
			time = ( second < 10 ) ? time+"0"+second : time+second;

			return time;
		}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        ///<summary>201504</summary>
        public static string GetTime( DateTime aTime) {

            string time = "";
            int hour = aTime.Hour;
            time = (hour < 10) ? time + "0" + hour : time + hour;
            int minute = aTime.Minute;
            time = (minute < 10) ? time + "0" + minute : time + minute;
            int second = aTime.Second;
            time = (second < 10) ? time + "0" + second : time + second;

            return time;
        }


		//-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static int GetCurrentMilliseconds() {
            return DateTime.Now.Millisecond;
        }


		//-----------------------------------------------------------------------------------------------------------------------------------------------------------
        ///<summary>2004-12-31 15:59:59</summary>
		public static string GetUniversalDateTime(DateTime DT) {
			string dt = "";

			if ( DT.Equals( null ) == false ) {

				dt = DT.Year.ToString();
				int month = DT.Month;
				dt = ( month < 10 ) ? dt+"-0"+month : dt+"-"+month;
				int day = DT.Day;
				dt = ( day < 10 ) ? dt+"-0"+day : dt+"-"+day;

				dt = dt+" ";

				int hour = DT.Hour;
				dt = ( hour < 10 ) ? dt+"0"+hour : dt+hour;
				int minute = DT.Minute;
				dt = ( minute < 10 ) ? dt+":0"+minute : dt+":"+minute;
				int second = DT.Second;
				dt = ( second < 10 ) ? dt+":0"+second : dt+":"+second;

			}

			return dt;
		}


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Formats the given date time string (probably from the database) into 311299 format (ddmmyy)</summary>
        public static string FormatTableNameDate(string date) {
            date = date.Substring(0, date.IndexOf(" "));
            date = date.Replace("/", "");
            date = date.Substring(0, 4) + date.Substring(6);
            return date;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Formats the given date time string (probably from the database - dd-mm-yyyy hh:mm:ss)
        /// into the database neutral format yyyy-mm-dd</summary>
        public static string FormatDatabaseDate(string date) {
            if (date == null || date == "") {
                return "";
            } else {
                date = date.Substring(0, date.IndexOf(" "));
                date = date.Replace("/", "");
                string dbDate = date.Substring(4) + "-" + date.Substring(2, 2) + "-" + date.Substring(0, 2);
                return dbDate;
            }
        }
        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Formats the given date time string (probably from the database - dd-mm-yyyy hh:mm:ss)
        /// into the database neutral format yyyy-mm-dd</summary>
        public static string FormatDatabaseDate(DateTime date, bool includeDashes, bool includeTime) {

            string dateBit = FormatDatabaseDate(date, includeDashes);
            if ( dateBit != null && includeTime) {
                dateBit = dateBit + " " + date.Hour.ToString("00") + ":" + date.Minute.ToString("00") + ":" + date.Second.ToString("00");
            }
            return dateBit;
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Converts a  date time to yyyymmdd - this is the standard input into mysql
        /// If using the dashes you will need to use the following MySQL function STR_TO_DATE('0000-00-00', '%Y-%m-%d');</summary>
        public static string FormatDatabaseDate(DateTime date, bool includeDashes) {
            if (IsNullDate(date)) {
                return null;
            } else {
                StringBuilder dbDate = new StringBuilder();
                dbDate.Append(date.Year.ToString());

                int month = date.Month;
                int day = date.Day;

                if (includeDashes) {
                    dbDate.Append("-");
                }
                if (month < 10) {
                    dbDate.Append("0");
                }
                dbDate.Append(month);

                if (includeDashes) {
                    dbDate.Append("-");
                }
                if (day < 10) {
                    dbDate.Append("0");
                }
                dbDate.Append(day);

                return dbDate.ToString();
            }
        }


        //-------------------------------------------------------------------------------------------------------------------------
        ///<summary>
        ///     Formats the given date time string (probably from the database) into a DateTime object (JUST the Date)
        ///     The dbDate is in the format dd-mm-yyyy hh:mm:ss
        ///</summary>
        public static DateTime FormatDate(string dbDate) {
            if (dbDate.IndexOf(" ") > -1)
            {
                dbDate = dbDate.Substring(0, dbDate.IndexOf(" "));
            }
            DateTime tempDate = DateTime.Parse(dbDate);
            return tempDate;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        ///<summary>
        ///     Formats the given date time string (probably from the database) into a DateTime object (JUST the Date)
        ///     The dbDate is in the format dd-mm-yyyy hh:mm:ss
        ///     If return null date is false, this method will throw an exception if an empty string is provided
        ///     This overload does NOT return the time
        ///</summary>
        public static DateTime FormatDate(string dbDate, bool returnNullDate) {
            return FormatDate(dbDate, returnNullDate, false);
        }
        //-------------------------------------------------------------------------------------------------------------------------
        ///<summary>
        ///     Formats the given date time string (probably from the database) into a DateTime object (JUST the Date)
        ///     The dbDate is in the format dd-mm-yyyy hh:mm:ss
        ///     If return null date is false, this method will throw an exception if an empty string is provided
        ///</summary>
        public static DateTime FormatDate(string dbDate, bool returnNullDate, bool includeTime) {
            DateTime tempDate = DateTimeInformation.NullDate;

            if ((dbDate != null && dbDate != "") || returnNullDate == false) {
                if (includeTime == false) {
                    dbDate = dbDate.Substring(0, dbDate.IndexOf(" "));
                }
                tempDate = DateTime.Parse(dbDate);
            }

            return tempDate;
        }
        //-------------------------------------------------------------------------------------------------------------------------
        ///<summary>
        ///     Formats the given date time string (probably from the database) into a DateTime object
        ///     The dbDate is in the format dd-mm-yyyy hh:mm:ss
        ///     If return null date is false, this method will throw an exception if an empty string is provided
        ///</summary>
        public static DateTime FormatDateTime(string dbDateTime) {
            return FormatDate(dbDateTime, true, true);
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Formats the date to the following format Tue Mar 13 2012 12:05:14</summary>
        public static string FormatJavascriptDateTime(DateTime date) {
            string dateBit = null;
            if (DateTimeInformation.IsNullDate(date) == false) {
                dateBit = date.Day + " " + months[date.Month - 1] + " " + date.Year;
            }
            return dateBit;
        }



        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Formats the given date time string into the visuallly appealling 1 Jan 2007</summary>
        public static string PrettyDateFormat(DateTime date) {
            string dateBit = null;
            if (DateTimeInformation.IsNullDate(date) == false) {
                dateBit = date.Day + " " + months[date.Month - 1] + " " + date.Year;
            }
            return dateBit;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Formats the given date time string into the visuallly appealling 1 Jan 2007</summary>
        public static string PrettyDateFormat(DateTime date, bool includeDay, bool includeMonth, bool includeYear) {
            string dateBit = null;
            if (DateTimeInformation.IsNullDate(date) == false) {
                dateBit =
                    ((includeDay) ? date.Day + " " : "") +
                    ((includeMonth) ? months[date.Month - 1] + " " : "") +
                    ((includeYear) ? date.Year.ToString() : "");
            }
            return dateBit;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Formats the given date time string into the visuallly appealling 1 Jan 2007</summary>
        public static string PrettyDateFormat(DateTime date, bool includeDashes) {
            string dateBit = null;

            if (DateTimeInformation.IsNullDate(date) == false) {
                string filler = (includeDashes) ? "-" : " ";

                dateBit = date.Day + filler + months[date.Month - 1] + filler + date.Year;
            }
            return dateBit;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Extracts the DB Date String from the given visuallly appealling 1 Jan 2007 type date</summary>
        public static string PrettyDateReverseFormat(string prettyDate) {
            // split on spaces
            string dbDate = null;
            if (prettyDate != null && prettyDate != "") {

                // 29-Jan-2014 - if there are dashes in the date replace them first with spaces .... (dashes are now added for Excel compatibility)
                prettyDate = prettyDate.Replace("-", " ");

                dbDate = "STR_TO_DATE('"+prettyDate+"', '%e %b %Y')";
            }
            return dbDate;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Extracts the DB Date String from the given visuallly appealling 1 Jan 2007 type date</summary>
        public static DateTime PrettyDateParser(string prettyDate, bool includeTime) {
            // split on spaces
            DateTime dt = new DateTime();
            if (prettyDate != null && prettyDate != "") {

                // catch days less than 10 and add a leading zero ...
                if (prettyDate.IndexOf(" ") == 1) {
                    prettyDate = "0" + prettyDate;
                }

                // 29-Jan-2014 - if there are dashes in the date replace them first with spaces .... (dashes are now added for Excel compatibility)
                prettyDate = prettyDate.Replace("-", " ");

                string format = "dd MMM yyyy";

                if (includeTime) {
                    format = "dd MMM yyyy HH:mm:ss";
                }

                DateTime.TryParseExact(prettyDate, format, null, DateTimeStyles.None, out dt);
//                dbDate = "STR_TO_DATE('" + prettyDate + "', '%e %b %Y')";


            }
            return dt;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Formats the given date time string into the visuallly appealling 1 Jan 2007 23:01:01</summary>
//        public static string PrettyDateTimeFormat(DateTime date) {
            // this one will assume that the timezone offset is zero, but lets leave this blank for now ...
//        }
        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Formats the given date time string and also appends the UTC timezone differential (e.g. UTC +3.5).
        ///     The timezone offset is derived from the client machine via javascript in the login, passwordRequestReset or passwordReset pages
        ///     And renders the date and time in the visuallly appealling "1 Jan 2007 23:01:01 (UTC +1)" format
        /// </summary>
        public static string PrettyDateTimeFormat(DateTime date, int timezoneOffset) {
            string dateBit = null;

            if (DateTimeInformation.IsNullDate(date) == false) {

                // the default if the tz is zero
                string utcText = TimezoneText( timezoneOffset );

                string time = FormatDatabaseDate(date, true, true).Split(new string[] { " " }, StringSplitOptions.None)[1];
                dateBit = date.Day + " " + months[date.Month - 1] + " " + date.Year + " " + time + " (" +utcText +")";
            }
            return dateBit;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Formats the given timezone offset in minutes to the pretty "1 Jan 2007 23:01:01 (UTC +1)" format
        /// </summary>
        public static string TimezoneText(int timezoneOffset) {

            // the default if the tz is zero
            string utcText = "UTC";

            if (timezoneOffset != 0) {

                // lets calculate the value in hours - most timezones are multiples of hours, but some are on the half hour mark.
                // and lets make the hours absolute to make the rendering easier
                int digits = (timezoneOffset % 60 == 0) ? 0 : 1;
                double hours = Math.Abs(Math.Round((double)(timezoneOffset / 60), digits));

                // positive tz means it is actually behind UTC!
                if (timezoneOffset == 0) {
                    utcText = "UTC";
                } else if (timezoneOffset > 0) {
                    utcText = "UTC-" + hours;
                } else if (timezoneOffset < 0) {
                    utcText = "UTC+" + hours;
                }
            }

            return utcText;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>Formats the given date time string into the visuallly appealling 1 Jan 2007 23:01:01</summary>
        public static string PrettyTimeFormat(DateTime date) {
            string time = null;
            if (DateTimeInformation.IsNullDate(date) == false) {
                time = FormatDatabaseDate(date, true, true).Split(new string[] { " " }, StringSplitOptions.None)[1];
            }
            return time;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Parse a date from a string in the form '[MMM ]YYYY[ initials]'.
        /// </summary>
        /// <returns>The date with 1 as the month day.</returns>
        public static DateTime ParseDateFromMon_Year_IL(string strDate) {
            if (strDate == null || strDate == String.Empty) {
                throw new Exception("Expected non-null, non-empty date input");
            }

            char[] splitChars = { ' ' };
            string[] colDateParts = strDate.Split(splitChars);

            if (colDateParts.Length == 1) {
                int intYear = Convert.ToInt32(colDateParts[0]);

                return new DateTime(intYear, 1, 1);
            } else {
                int intYear = Convert.ToInt32(colDateParts[1]);
                int intMonth = ParseAbbreviatedMonth(colDateParts[0]);

                return new DateTime(intYear, intMonth, 1);
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Parse an abbreviated or partial month string, ignoring case,
        /// into a month number in the range [1-12].
        /// </summary>
        /// <param name="abbreviatedMonth">The abbreviated or partial month e.g. Jan</param>
        /// <returns>Integer in the range [1-12].</returns>
        public static int ParseAbbreviatedMonth(string abbreviatedMonth) {
            int i = -1;
            for (i = months.Length - 1; i >= 0; i--) {
                if (abbreviatedMonth.ToUpper().Contains(months[i].ToUpper())
                    || months[i].ToUpper().Contains(abbreviatedMonth.ToUpper())) {
                    break;
                }
            }

            return (i + 1);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string GetTimestampFileExt(DateTime dateTime) {
            return dateTime.ToString("dd-MM-yyyy_HH-mm-ss");
        }




        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>The dates should be in the pretty format 1 Jan 2006</summary>
        public static string DateQueryGetPseudoCode(DateTime fromDate, DateTime toDate) {

            string fromDateStr = null;
            if (IsNullDate(fromDate) == false) {
                fromDateStr = DateTimeInformation.PrettyDateFormat(fromDate);
            }

            string toDateStr = null;
            if (IsNullDate(toDate) == false) {
                toDateStr = DateTimeInformation.PrettyDateFormat(toDate);
            }

            return DateQueryGetPseudoCode(fromDateStr, toDateStr);
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string DateQueryGetPseudoCode(string fromDate, string toDate) {
            string clause = null;

            if (fromDate != null && fromDate != "" && toDate != null && toDate != "") {
                clause = "Between " + fromDate + " and " + toDate;
            } else if (fromDate != null && fromDate != "") {
                clause = "Greater than " + fromDate;
            } else if (toDate != null && toDate != "") {
                clause = "Less than " + toDate;
            } else {
                // Noone has used this new sexy tool.  Boo hoo
            }

            return clause;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>The dates should be in the pretty format 1 Jan 2006</summary>
        public static string DateQueryGetDatabaseFormat(DateTime fromDate, DateTime toDate) {
            string clause = null;

            string fromDateStr = null;
            if (IsNullDate(fromDate) == false) {
                fromDateStr = DateTimeInformation.FormatDatabaseDate(fromDate, true, true);
            }

            string toDateStr = null;
            if (IsNullDate(toDate) == false) {
                // add one to the toDate as MySQL BETWEEN is inclusive of the start and exclusive of the end ....
                toDate = toDate.AddDays(1);
                toDateStr = DateTimeInformation.FormatDatabaseDate(toDate, true, true);
            }

            if (fromDateStr != null && fromDateStr != "" && toDateStr != null && toDateStr != "") {
                clause = "Between " + QuoteDate(fromDateStr) + " and " + QuoteDate(toDateStr);
            } else if (fromDateStr != null && fromDateStr != "") {
                clause = ">= " + QuoteDate(fromDateStr);
            } else if (toDateStr != null && toDateStr != "") {
                clause = "<= " + QuoteDate(toDateStr);
            } else {
                // Noone has used this new sexy tool.  Boo hoo
            }

            return clause;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the database representation of the pseudo code.  Note that the range will always include both the
        ///     start date and the end date
        /// </summary>
        public static string DateQueryParsePseudoCode(string pseudoCode) {
            string sql = null;

            // remove between, Greater than, less than ...
            if (pseudoCode != null && pseudoCode != "") {

                string tempPC = pseudoCode.ToLower();

                int queryType = -2;
                if ( tempPC.Contains( "between" )) {
                    queryType = 0;
                } else if (tempPC.Contains( "greater than" )) {
                    queryType = 1;
                } else if (tempPC.Contains( "less than" )) {
                    queryType = -1;
                }

                if ( queryType != -2 ) {
                    tempPC = tempPC.Replace("between", "").Replace("greater than", "").Replace("less than", "").Replace( "and", "&" ).Trim();

                    if (queryType == 0) {   // split it and reverse each pretty date ....
                        string[] bits = tempPC.Split('&');
                        if (bits.Length == 2) {
                            // add one to the toDate ..... as MySQL BETWEEN is inclusive of the start and exclusive of the end
                            string toDateString = PrettyDateReverseFormat(bits[1].Trim()); // the default ...
                            try {
                                string tempDate = "";
                                string[] toDateBits = bits[1].Trim().Split(' ');
                                int monthIndex=0;
                                int index = 0;
                                foreach( string month in months ) {
                                    if ( toDateBits[ 1 ].ToLower().Equals( month.ToLower() )) {
                                        monthIndex = index;
                                        break;
                                    }
                                        index++;
                                }
                                tempDate = toDateBits[0] + "-" + (monthIndex+1) + "-" + toDateBits[2];
                                DateTime toDate = DateTime.Parse(tempDate);

                                toDate = toDate.AddDays( 1 );
                                toDateString = FormatDatabaseDate(toDate, true);
                            } catch { }

                            sql = "Between " + PrettyDateReverseFormat(bits[0].Trim()) + " and " + QuoteDate( toDateString );
                        } else {
                            // This isnt a real between ...
                        }
                    } else {                     // reverse the pretty date ...
                        tempPC = PrettyDateReverseFormat(tempPC.Trim());
                        if (queryType == -1) {
                            sql = "<= " + tempPC;
                        } else if (queryType == 1) {
                            sql = ">= " + tempPC;
                        }
                    }
                }
            }
            return sql;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the number of whole days between two dates. E.g 36 hours would be retuned as 1;
        /// </summary>
        public static int DateRangeDays(DateTime aFrom, DateTime aTo)
        {
            TimeSpan tSpan = new TimeSpan(aTo.Ticks - aFrom.Ticks);
            return tSpan.Days;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the number of whole months between two dates.
        /// </summary>
        public static int DateRangeMonths(DateTime aFrom, DateTime aTo)
        {
            TimeSpan tSpan = new TimeSpan(aTo.Ticks - aFrom.Ticks);
            double tDays = tSpan.TotalDays;

            double tRufMonths = tDays / 31.0;
            int tMonthsCalc = (int)Math.Floor(tRufMonths);

            DateTime tMatch = new DateTime(aFrom.Ticks);
            tMatch = tMatch.AddMonths(tMonthsCalc);

            while (tMatch < aTo)
            {
                tMatch = tMatch.AddMonths(1);
                tMonthsCalc += 1;
            }

            if (tMatch == aTo)
            {
                return tMonthsCalc;
            }
            else
            {
                return tMonthsCalc - 1;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the number of whole Years between two dates.
        /// </summary>
        public static int DateRangeYears(DateTime aFrom, DateTime aTo)
        {
            TimeSpan tSpan = new TimeSpan(aTo.Ticks - aFrom.Ticks);
            double tDays = tSpan.TotalDays;

            double tRufYears = tDays / 366.0;
            int tYearsCalc = (int)Math.Floor(tRufYears);

            DateTime tMatch = new DateTime(aFrom.Ticks);
            tMatch = tMatch.AddYears(tYearsCalc);

            while (tMatch < aTo)
            {
                tMatch = tMatch.AddYears(1);
                tYearsCalc += 1;
            }

            if (tMatch == aTo)
            {
                return tYearsCalc;
            }
            else
            {
                return tYearsCalc - 1;
            }
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Compares dates, but just down to the nearest second.  The built in Date Compare method
        ///     The threshold in seconds is a fudge factor for comparison.
        /// </summary>
        public static int CompareDates(DateTime dt1, DateTime dt2, int thresholdInSeconds) {
            int comparison = -1; // default to less than ...

            TimeSpan t = dt1.Subtract( dt2 );
            if (Math.Abs((double) t.TotalSeconds ) < (double) thresholdInSeconds) {
                comparison = 0;
            } else if (t.TotalSeconds > thresholdInSeconds) {
                comparison = 1;
            } else {
                comparison = -1;
            }

            return comparison;
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string QuoteDate(string s) {
            if (s != null) {
                return "'" + s.Replace("'", "\\'") + "'";
            }
            return null;
        }



	} // end of class
}
