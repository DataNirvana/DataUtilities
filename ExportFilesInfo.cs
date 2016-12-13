using System;
using System.Collections.Generic;
using MGL.DomainModel;

//-------------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A global resource to monitor the progres with exporting Excel files from applications (e.g. IDPGrievances and User lists etc)
    ///     Essentially a collection of the ProcessingStateClass
    /// </summary>
    public class ExportFilesInfo {


        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The processing state of all the files being exported on the server (an integer from one to 100) using a hash of the session ID, time and type of export as a key
        /// </summary>
        public static Dictionary<string, ProcessingState> ProcessingStates = new Dictionary<string, ProcessingState>();


        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void AddProcessingState(string seshID) {

            // Add or reset our reference in the static dictionary ...
            lock (ExportFilesInfo.ProcessingStates) {
                // Automatically kill previous versions if they exist (The user should always check first if it is still running)
                if (ExportFilesInfo.ProcessingStates.ContainsKey(seshID) == true) {
                    ExportFilesInfo.ProcessingStates.Remove(seshID);
                }

                ExportFilesInfo.ProcessingStates.Add(seshID, new ProcessingState(seshID));
            }
        }




        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void UpdateIsRunning(string seshID, bool isRunning) {

            // Add or reset our reference in the static dictionary ...
            lock (ExportFilesInfo.ProcessingStates) {
                if (ExportFilesInfo.ProcessingStates.ContainsKey(seshID)) {
                    ExportFilesInfo.ProcessingStates[seshID].IsRunning = isRunning;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static bool GetIsRunning(string seshID) {
            bool isRunning = false;

            // Add or reset our reference in the static dictionary ...
            lock (ExportFilesInfo.ProcessingStates) {
                if (ExportFilesInfo.ProcessingStates.ContainsKey(seshID)) {
                    return ExportFilesInfo.ProcessingStates[seshID].IsRunning;
                }
            }

            return isRunning;
        }

        
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void UpdateProgress(string seshID, int progress) {

            // Add or reset our reference in the static dictionary ...
            lock (ExportFilesInfo.ProcessingStates) {
                if (ExportFilesInfo.ProcessingStates.ContainsKey(seshID)) {
                    ExportFilesInfo.ProcessingStates[seshID].Progress = progress;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static int GetProgress(string seshID) {
            int progress = 0;

            // Add or reset our reference in the static dictionary ...
            lock (ExportFilesInfo.ProcessingStates) {
                if (ExportFilesInfo.ProcessingStates.ContainsKey(seshID)) {
                    return ExportFilesInfo.ProcessingStates[seshID].Progress;
                }
            }

            return progress;
        }
        

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void UpdateMessage(string seshID, string message) {

            // Add or reset our reference in the static dictionary ...
            lock (ExportFilesInfo.ProcessingStates) {
                if (ExportFilesInfo.ProcessingStates.ContainsKey(seshID)) {
                    ExportFilesInfo.ProcessingStates[seshID].Message = message;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string GetMessage(string seshID) {
            string message = "";

            // Add or reset our reference in the static dictionary ...
            lock (ExportFilesInfo.ProcessingStates) {
                if (ExportFilesInfo.ProcessingStates.ContainsKey(seshID)) {
                    return ExportFilesInfo.ProcessingStates[seshID].Message;
                }
            }

            return message;
        }







        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void UpdateData(string seshID, byte[] data) {

            // Add or reset our reference in the static dictionary ...
            lock (ExportFilesInfo.ProcessingStates) {
                if (ExportFilesInfo.ProcessingStates.ContainsKey(seshID)) {
                    ExportFilesInfo.ProcessingStates[seshID].Data = data;
                }
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static byte[] GetData(string seshID) {
            byte[] data = null;

            // Add or reset our reference in the static dictionary ...
            lock (ExportFilesInfo.ProcessingStates) {
                if (ExportFilesInfo.ProcessingStates.ContainsKey(seshID)) {
                    return ExportFilesInfo.ProcessingStates[seshID].Data;
                }
            }

            return data;
        }




        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     1 = Success 2 = Warning 3 = Failed
        /// </summary>
        public static void UpdateFinalState(string seshID, int finalState) {

            // Add or reset our reference in the static dictionary ...
            lock (ExportFilesInfo.ProcessingStates) {
                if (ExportFilesInfo.ProcessingStates.ContainsKey(seshID)) {
                    ExportFilesInfo.ProcessingStates[seshID].FinalState = finalState;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static int GetFinalState(string seshID) {
            int finalState = 0;

            // Add or reset our reference in the static dictionary ...
            lock (ExportFilesInfo.ProcessingStates) {
                if (ExportFilesInfo.ProcessingStates.ContainsKey(seshID)) {
                    return ExportFilesInfo.ProcessingStates[seshID].FinalState;
                }
            }

            return finalState;
        }






        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void RemoveProcessingState(string seshID) {

            // Add or reset our reference in the static dictionary ...
            lock (ExportFilesInfo.ProcessingStates) {
                if (ExportFilesInfo.ProcessingStates.ContainsKey(seshID)) {
                    ExportFilesInfo.ProcessingStates.Remove(seshID);
                }
            }
        }



    }
}