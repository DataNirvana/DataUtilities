using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

//-----------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {

    #region Logging helper Enums and classifiers

    //------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A simple enum used to classify the type of each individual log message to write
    /// </summary>
    public enum LoggerHighlightType {
        Normal = 0,
        Error = 1,
        Warning = 2,
        Processing = 3,
        Info = 4
    }

    //-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A classification for the types of each individual error message produced
    /// </summary>
    public static class LoggerErrorTypes {
        public static string Database = "Database";
        public static string Parsing = "Parsing";
        public static string Logic = "Logic";
        public static string NoData = "No Data";
        public static string DataQuality = "Poor Data Quality";
        public static string FileIO = "File Input or Output";
    }

    #endregion

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///         Description:	    Logger class - very simple and uses a static list approach to append messages to.  Designed to be require
    ///                               minimal resources and when benchmarked, should consume less than 2-3% of the a processes performance.
    ///         Type:				Logger
    ///         Author:			Edgar Scrase
    ///         Date:				v1.0 - March 2005
    ///                               v2.0 - April 2011
    ///                               v2.1 - November 2015
    ///                               v2.2 - May 2016
    ///         Version:		    2.0
    ///         Notes:			    Wow - the fact that this was started in 2005 makes me feel old ....
    ///                               In 2011 - updated to version 2.0 and added the nice command line highlighting from the log4net library and streamlined the error processing
    ///                               So there is now no need to separately report a list of errors ...
    ///                               2015 - v2.1 - included the severity level, to make it easier to filter web logs with 100's of error messages.
    ///                               2016 - v2.2 - using locking on the static list to make it as threadsafe as possible...
    /// </summary>
    public static class Logger {

        #region Static Properties
       
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// A list of logging messages buffered for writing.
        /// </summary>
        public static List<string> LogList {
            get { return logList; }
            set { logList = value; }
        }
        private static List<string> logList = new List<string>();

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The file name to log to
        /// </summary>
        private static string fileName = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The log directory
        /// </summary>
        private static string logDirectory = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Used for some of the processing logs, which use backspace to give the impression of interating sexily ...
        /// </summary>
        private static int prevStringLength = 0;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Records the start time of the process ....
        /// </summary>
        private static TimeSpan start;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The action that this log is related to ...
        /// </summary>
        private static string action = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Whether or not to write to the console
        /// </summary>
        private static bool writeToConsole = true;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Whether or not to record the log info in the log file
        /// </summary>
        private static bool storeLogList = true;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Whether or not to record the date and time against almost every log item
        /// </summary>
        private static bool logDateAndTime = true;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Whether or not to write each individual error message as a log text file (true) or to keep adding them to the static list
        /// </summary>
        private static bool writeSingleErrorMessage = false;


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Called when the log is first initialised, probably right at the start of console applications or in the global.asax of web projects
        /// </summary>
        /// <param name="logAction">The name that the log file will be prefixed with</param>
        /// <param name="logDir">The directory containing the log files e.g. "C:/Temp" (without the quotes!)</param>
        /// <param name="writeToTheConsole">Whether or not to write the message to the console</param>
        /// <param name="writeLogListToFile">Whether or not to write the message to the log file</param>
        /// <param name="writeASingleErrorMessage">Whether or not to write each individual error message as a log text file (true) or to keep adding them to the static list</param>
        public static void StartLog(string logAction, string logDir, bool writeToTheConsole, bool writeLogListToFile, bool writeASingleErrorMessage) {

            //-----1----- Check that the log directory exists; if it doesn't then create it
            if (logDir != null && logDir != "" && Directory.Exists(logDir) == false) {
                Directory.CreateDirectory(logDir);
            }

            //-----2a----- Set the log action, whether or not to write to the console, store the log list in a static list variable and if we should write single error messages.
            action = logAction;
            writeToConsole = writeToTheConsole;
            storeLogList = writeLogListToFile;
            writeSingleErrorMessage = writeASingleErrorMessage;

            //-----2b----- Set the log directory and generate this log's file name
            logDirectory = logDir;
            // 19-Nov-2015 - modified so that the directory is not passed in
            fileName = GenerateLogFileName(logAction);

            //-----3----- Start timing
            start = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);

            //-----4----- Reset the list in a threadsafe manner
            lock ( logList) {                
                logList.Clear(); 
            }          

            //-----5----- Set the header
            string header = "*************************************\nAction: " + action + "\nStart Time: " + DateTime.Now.ToString() + "\n*************************************\n\n";
            Logger.Log(header);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the name of the action that the log has been initialised to log
        /// </summary>
        public static string GetLogName() {
            return action;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the name of the log file
        /// </summary>
        public static string GetLogFileName() {
            return fileName;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Generates the log file name.  Normally a concatenation of the action and the date and time.
        ///     For error messages, the severity is also prefixed to the log file for easier reading of web related issues
        ///     19-Nov-2015 - modified so that the directory is not passed in
        /// </summary>
        /// <param name="logAction">A description of the action that this log is performing</param>
        /// <returns>The name of the log file</returns>
        public static string GenerateLogFileName(string logAction) {

            // Generate the fileName here based on the action and the current date and time
            string date = DateTimeInformation.GetCurrentDate(null);
            string time = DateTimeInformation.GetCurrentTime();

            // 8-Oct-2015 - Added the milliseconds so that we can better see what is happening in more complex situations
            string tempFileName = logAction + "_" + date + "_" + time + "_" + DateTimeInformation.GetCurrentMilliseconds() + ".txt";

            // check to see if this fileName exists already (i.e. there is another log file with the same action for exactly the same second!!!
            // If this has happened, add the number of milliseconds too
            if (File.Exists(fileName)) { // SimpleIO.FileExists(fileName)) {
                //tempFileName = logDirectory + "/" + logAction + "_" + date + "_" + time + "_" + DateTimeInformation.GetCurrentMilliseconds() + ".txt";
                tempFileName = logAction + "_" + date + "_" + time + "_" + DateTimeInformation.GetCurrentMilliseconds() + ".txt";
            }

            return tempFileName;
        }



        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Logs the given text as a sub heading
        /// </summary>
        public static void LogSubHeading(string heading) {
            // Pretty and breaks up the log files nicely ...
            string s = "\n\n------------------------------------------------------------------------------------------\n" + heading + "\n\n";

            Logger.Log(s);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Logs the given text as a sub sub heading
        /// </summary>
        public static void LogSubSubHeading(string heading) {
            // Pretty and breaks up the log files nicely ...
            Logger.Log("\n\n" + heading);
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Logs the given text and the key phrase
        /// </summary>
        public static void Log(string keyPhrase, string info) {
            // add a new line for extra style points
            string s = "\n" + keyPhrase + ": " + info;

            Logger.Log(s);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     This version of the log method is useful if you only want to print the given information to the console
        /// </summary>
        private static void Log(string info, bool justPrintToConsole) {

            string s = info;
            if (justPrintToConsole == false) {
                Logger.Log(s);
            } else if (writeToConsole == true) {

                DoConsoleOutput(s, false, LoggerHighlightType.Processing, 0);

            }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Logs the given information
        /// </summary>
        /// <param name="info">The information to be logged.</param>
        /// <param name="lht">The type of formatting to use for the message in the console window.</param>
        /// <param name="severity">
        ///     The criticality of the information to be logged.  Normally in a range from 0 to 10, with 10 being the most critical.
        ///     As the logs have become more complicated, custom three digit severity codes are also now used for known issues.
        /// </param>
        public static void Log(string info, LoggerHighlightType lht, int severity) {

            if (LoggerHighlightType.Error == lht) {

                info = "ERROR - " + severity + " - " + (logDateAndTime ? DateTimeInformation.PrettyDateTimeFormat(DateTime.Now, 0) : "") + info;

            } else if (LoggerHighlightType.Warning == lht) {

                info = "WARNING - " + severity + " - " + (logDateAndTime ? DateTimeInformation.PrettyDateTimeFormat(DateTime.Now, 0) : "") + info;

            } else if (LoggerHighlightType.Processing == lht) {

                // Nothing to add for the processing messages

            } else if (LoggerHighlightType.Info == lht) {

                info = "INFO - " + severity + " - " + (logDateAndTime ? DateTimeInformation.PrettyDateTimeFormat(DateTime.Now, 0) : "") + info;

            } else {

                // NADA

            }

            // Store the info in the log list, if required
            if (storeLogList == true) {
                // Add the information in a threadsafe manner
                lock (logList) {
                    logList.Add(info);
                }
            }

            // write to the console if its a console application
            if (writeToConsole == true) {

                DoConsoleOutput(info, true, lht, severity);

            }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The generic case - all info recorded as normal highlight type ...
        /// </summary>
        public static void Log(string info) {

            if (storeLogList == true) {
                // Add the information in a threadsafe manner
                lock (logList) {
                    logList.Add(info);
                }
            }

            // write to the console if its a console application
            if (writeToConsole == true) {

                DoConsoleOutput(info, true, LoggerHighlightType.Normal, 0);

            }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Give the user some feedback only when the counter matches the feedback interval
        ///     Uses backspaces, to scrub the previous message, so that the log does not scroll and scroll and gives the impression of updating itself
        ///     The feedback interval will vary depending on how quickly each iteration takes - you want the impression of quick visual change,
        ///     without the log file being the processing crux point!
        ///     E.g. on a very fast process, logging each iteration will become the processing bottle neck.
        ///     The message takes the form "Processed 123"....
        /// </summary>
        /// <param name="counter">The number of iterations that a task has completed</param>
        /// <param name="feedbackInterval">How often to provide the console output (e.g. 10 or 100 or 1000)</param>
        public static void Log(int counter, int feedbackInterval) {

            if (counter % feedbackInterval == 0) {
                StringBuilder currentString = new StringBuilder();

                for (int i = 0; i < prevStringLength; i++) {
                    currentString.Append("\b");
                }

                currentString.Append("Processed " + counter + "                 ");

                // 25 May 2011 - Always take off the previous string length - as these are just the backspaces ...
                prevStringLength = currentString.Length - prevStringLength;

                Logger.Log(currentString.ToString(), true);
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Give the user some feedback only when the counter matches the feedback interval
        ///     Uses backspaces, to scrub the previous message, so that the log does not scroll and scroll and gives the impression of updating itself
        ///     The feedback interval will vary depending on how quickly each iteration takes - you want the impression of quick visual change,
        ///     without the log file being the processing crux point!
        ///     E.g. on a very fast process, logging each iteration will become the processing bottle neck.
        ///     The message takes the form "Processed 123"....
        /// </summary>
        /// <param name="counter">The number of iterations that a task has completed</param>
        /// <param name="feedbackInterval">How often to provide the console output (e.g. 10 or 100 or 1000)</param>
        public static void Log(uint counter, uint feedbackInterval) {
            Log((int)counter, (int)feedbackInterval);
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Give the user some feedback only when the counter matches the feedback interval.
        ///     Uses backspaces, to scrub the previous message, so that the log does not scroll and scroll and gives the impression of updating itself
        ///     The feedback interval will vary depending on how quickly each iteration takes - you want the impression of quick visual change,
        ///     without the log file being the processing crux point!
        ///     E.g. on a very fast process, logging each iteration will become the processing bottle neck.
        ///     The message takes the form "Processed 123 of 1000 (12% complete)"....
        /// </summary>
        /// <param name="counter">The number of iterations that a task has completed</param>
        /// <param name="feedbackInterval">How often to provide the console output (e.g. 10 or 100 or 1000)</param>
        /// <param name="totalRecordsToProcess">The total number of iterations that a task will have to process.  Used to calculate the percentage.</param>
        public static void Log(int counter, int feedbackInterval, long totalRecordsToProcess) {
            Log((long)counter, (long)feedbackInterval, totalRecordsToProcess);
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Give the user some feedback only when the counter matches the feedback interval
        ///     Uses backspaces, to scrub the previous message, so that the log does not scroll and scroll and gives the impression of updating itself
        ///     The feedback interval will vary depending on how quickly each iteration takes - you want the impression of quick visual change,
        ///     without the log file being the processing crux point!
        ///     E.g. on a very fast process, logging each iteration will become the processing bottle neck.
        ///     The message takes the form "Processed 123"....
        /// </summary>
        /// <param name="counter">The number of iterations that a task has completed</param>
        /// <param name="feedbackInterval">How often to provide the console output (e.g. 10 or 100 or 1000)</param>
        public static void Log(long counter, long feedbackInterval, long totalRecordsToProcess) {

            try {
                if (totalRecordsToProcess > 0) {
                    if (counter % feedbackInterval == 0) {
                        StringBuilder currentString = new StringBuilder();

                        for (int i = 0; i < prevStringLength; i++) {
                            currentString.Append("\b");
                        }

                        int percentComplete = (int)Math.Round(((double)counter / totalRecordsToProcess) * 100);

                        currentString.Append("Processed " + counter + " of " + totalRecordsToProcess + " (" + percentComplete + "% complete)" + "                 ");

                        // 25 May 2011 - Always take of the previous string length - as these are just the backspaces ...
                        prevStringLength = currentString.Length - prevStringLength;

                        Logger.Log(currentString.ToString(), true);
                    }
                }
            } catch (Exception ex) {
                Logger.LogError(5, "Crazy error with the logging of the process feedback ..." + ex.ToString());
            }

        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Custom logging for error messages.
        /// </summary>
        /// <param name="className">The class in which the error occurred</param>
        /// <param name="methodName">The method in which the error occurred</param>
        /// <param name="info">The description of the error message</param>
        /// <param name="ex">The exception itself that was generated.</param>
        /// <param name="sqlString">If this is a SQL error, the SQL that caused this error to occur.</param>
        /// <param name="loggerErrorType">The classification of this error message (see the generic types in the LoggerErrorTypes class above).</param>
        public static void LogError(string className, string methodName, string info, Exception ex, string sqlString, string loggerErrorType) {
            StringBuilder s = new StringBuilder();

            s.Append(className + ":" + methodName + ":\n\n");
            s.Append(info + "\n\n");

            // 1-Jun-2015 - catch exceptions caused by there being, erm, no exception provided!
            if (ex == null) {
                s.Append("No exception data provided\n\n");
            } else {
                s.Append(ex.ToString() + "\n\n");
            }

            s.Append("SQL Used: " + sqlString + "\n\n");
            s.Append("Error Type: " + loggerErrorType);

            // go for the default with these to make life easy
            LogError(5, s.ToString());
        }



        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Custom logging for error messages.
        /// </summary>
        /// <param name="severity">
        ///     The criticality of the information to be logged.  Normally in a range from 0 to 10, with 10 being the most critical.
        ///     2-4 are very low level, non critical errors.  5-7 are normal application errors (e.g. SQL errors).  8-10 are critical.
        ///     0 is normally reserved for information messages, and 1 for warnings.
        ///     As the logs have become more complicated, custom three digit severity codes are also now used for known issues.
        /// </param>
        /// <param name="info">The information to be logged.</param> 
        public static void LogError(int severity, string info) {
            Log(info, LoggerHighlightType.Error, severity);

            if (writeSingleErrorMessage) {
                Logger.Write(severity);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Custom logging for warnings
        /// </summary>
        /// <param name="info">The information to be logged.</param> 
        public static void LogWarning(string info) {
            Log(info, LoggerHighlightType.Warning, 1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Custom logging for information
        /// </summary>
        /// <param name="info">The information to be logged.</param> 
        public static void LogInfo(string info) {
            Log(info, LoggerHighlightType.Info, 0);
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Clears the log - drastic, so use sparingly!
        /// </summary>
        public static void Clear() {

            // Clear the log in a threadsafe manner
            lock (logList) {
                logList.Clear();
            }

            start = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);

        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     And FINALLY, what the logging is all about, lets write the log file!
        /// </summary>
        /// <param name="severity">
        ///     The criticality of the information to be logged.  Normally in a range from 0 to 10, with 10 being the most critical.
        ///     2-4 are very low level, non critical errors.  5-7 are normal application errors (e.g. SQL errors).  8-10 are critical.
        ///     0 is normally reserved for information messages, and 1 for warnings.
        ///     As the logs have become more complicated, custom three digit severity codes are also now used for known issues.
        /// </param>
        public static void Write(int severity) {

            if (logList.Count > 1) { // ie more than just the header!!

                Logger.Log("\n\n*************************************");
                Logger.Log("End Time: " + DateTime.Now.ToString());

                // Stop timing
                TimeSpan end = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
                TimeSpan difference = end.Subtract(start);
                Logger.Log("Process took " + Math.Round(difference.TotalMinutes, 2) + " minutes or " + Math.Round(difference.TotalSeconds, 2) + " seconds to complete");
                Logger.Log("*************************************");


                // 19-Nov-2015 - prepend all logged files with the severity...
                // Lets make sure to do this in a threadsafe manner
                lock (logList) {
                    File.WriteAllLines(logDirectory + "/" + severity + "_" + fileName, logList.ToArray());
                }                              

                // Finally clear the log
                Clear();

                // 8-Oct-2015 - And really finally, if this is a single log error message scenario, then lets regenerate the Log file name so that we dont overwrite it
                if (writeSingleErrorMessage == true) {
                    fileName = GenerateLogFileName(action);
                }
            }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Prints out the given information to the console using the thematic colouring
        /// </summary>
        /// <param name="info">The information to be logged.</param> 
        /// <param name="useNewLine">
        ///     Whether or not to log the information on a new line.  
        ///     Currently, this will always be true, apart from for processing information, that uses backspaces on the same line to overwrite the messages.
        /// </param>
        /// <param name="lht">The thematic classification to use to make the messages visually appealing.</param>
        /// <param name="severity">
        ///     The criticality of the information to be logged.  Normally in a range from 0 to 10, with 10 being the most critical.
        ///     2-4 are very low level, non critical errors.  5-7 are normal application errors (e.g. SQL errors).  8-10 are critical.
        ///     0 is normally reserved for information messages, and 1 for warnings.
        ///     As the logs have become more complicated, custom three digit severity codes are also now used for known issues.
        /// </param>
        public static void DoConsoleOutput(string info, bool useNewLine, LoggerHighlightType lht, int severity) {

            //-----1----- Set the thematic colouring to use for this message, based on the specified type of highlight
            if (LoggerHighlightType.Error == lht) {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;

            } else if (LoggerHighlightType.Warning == lht) {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.Black;

            } else if (LoggerHighlightType.Processing == lht) {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.DarkGray;

            } else if (LoggerHighlightType.Info == lht) {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;

            } else {
                // the defaults ...
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }

            //-----2----- And lets write the information, incorporating the date and time and message severity if required.
            if (info != null) {
                if (info != "" && LoggerHighlightType.Processing != lht && logDateAndTime == true) {
                    info = DateTimeInformation.PrettyDateTimeFormat(DateTime.Now, 0) + ":"+severity+": " + info;
                }
                if (useNewLine) {
                    Console.WriteLine(info);
                } else {
                    Console.Write(info);
                }
            }

            //-----3----- Reset the colours to the defaults ...
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

        }



    }  // End of Class
}
