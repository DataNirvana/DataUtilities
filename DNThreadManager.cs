using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//------------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///         10-Jun-2016 - Encapsulate the ThreadManagement in a static class (it has to be static so that it is visible to all the threads!)
    ///         4-Aug-2016 - Adapted all integer calculations to use ulongs where possible recognizing the increase in the size of the chunks to be processed
    ///         Note that we have to use longs rather than ulongs here as the built in atomic addition does not contain references for the unsigned equivalents ...        
    ///         NO NO NO - http://stackoverflow.com/questions/1391672/what-is-the-maximum-size-that-an-array-can-hold
    ///         The max size that an array can hold anyway is 2billion.  Imagine an array of even 2billion integers - this would be 8billion bytes just on its own, so we would be reaching
    ///         the theoretical maximum for most normal systems today .... Stick with ints for now
    /// 
    ///         This should make the processing of data more robust.
    ///         The ThreadManager ensures that while there are iterations still to complete, n-1 of the total number of logical processors are in action
    ///         to complete the iterations.One remains for management and not to totally kill the system!!
    /// </summary>
    public class DNThreadManager {

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The number of iterations to run 
        /// </summary>
        private static int IterationsToRun = 1000;

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The number of iterations that have been scheduled in one or more threads to run
        ///     Has to be a long as the built in atomic incrementer only deals in these
        /// </summary>
        private static int IterationsScheduled = 0;

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The number of iterations completed - publically accessible in a threadsafe manner so that external processes can poll the progress
        /// </summary>
        public static int IterationsCompleted {
            get {
                int tempC = 0;
                lock (C) {
                    if (C != null && C.Count > 0) {
                        tempC = C[0];
                    }
                }
                return tempC;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     In order to be threadsafe and to enable the integers to be locked when editing them, the number of iterations completed
        ///     is actually stored in a list with one value
        /// </summary>
        private static List<int> C = new List<int>();

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The number of iterations to hive off into a task for a specific thread running in a specific core ... 
        ///     This is now optimised at runtime by this process, but could be overridden by an external program
        /// </summary>
        public static int ChunkSize {
            get { return ThreadChunkSize; }
            set { ThreadChunkSize = value; }
        }
        private static int ThreadChunkSize = 1000;


        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The number of logical processors (physical processors * cores * logical processors) within which this processing will run
        /// </summary>
        private static int NumLogicalProcessors = 0;

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A list of the current processing threads that are being managed
        /// </summary>
        private static List<Thread> ProcessingThreads = new List<Thread>();

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The logging interval - publically configurable so that it can be set to be larger on fast machines, so that it does not interfere with the processing
        ///     (It is possible for the graphics of printing text to the console to slow the application down)
        /// </summary>
        private static int loggingInterval = 10;
        public static int LoggingInterval {
            get { return loggingInterval; }
            set { loggingInterval = value; }
        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Runs and manages a multithreaded task to be run.  As with all multithreaded applications, it is critical that these tasks do 
        ///     not rely on a preset order.
        ///     http://stackoverflow.com/questions/2082615/pass-method-as-parameter-using-c-sharp
        ///     Note that the ThreadManager runs in the currently active "main" thread
        /// </summary>
        /// <param name="processName">The name of the process that is being run.  This is inserted into logging to help make this as clear as possible.</param>
        /// <param name="numIterations">The total number of iterations to process</param>
        /// <param name="TaskToRun">
        ///     TaskToRun is the method in the calling code that will actually do the data crunching.
        ///     It has to accept a two integer parameters which are the size of the chunk to be executed, followed by the startIndex ...
        ///     e.g. if this was the third chunk to run, and the chunk size was 1000 the startIndex would be 3000
        /// </param>
        /// <returns>True if the process completed all iterations as expected</returns>
        public static bool Run(string processName, int numIterations, Action<int, int> TaskToRun) {

            /*
             * -----1----- 
             * In this overloaded version, we optimise the ThreadChunkSize for the calling code based on a simple division of the number of 
             * the total number of iterations to perform by the number of logical processors, reserving one processor for the management tasks.
             * In memory intensive tasks, this might produce suboptimal performance, so good to give the user the option of overriding this.
             * This methodology means that we will probably actually process a few more iterations than are requested as the total is rounded up to the next chunked block!
             * 15-Jun-16 - Lets try utilising all the processors but setting the threads to run at a lower priority
            */
            //ThreadChunkSize = ((int)Math.Ceiling((double)numIterations / (double)(Environment.ProcessorCount - 1)));
            ThreadChunkSize = ((int)Math.Ceiling((double)numIterations / (double)(Environment.ProcessorCount)));

            return Run(processName, numIterations, ThreadChunkSize, TaskToRun);
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Runs and manages a multithreaded task to be run.  As with all multithreaded applications, it is critical that these tasks do 
        ///     not rely on a preset order.
        ///     http://stackoverflow.com/questions/2082615/pass-method-as-parameter-using-c-sharp
        ///     Note that the ThreadManager runs in the currently active "main" thread
        /// </summary>
        /// <param name="processName">The name of the process that is being run.  This is inserted into logging to help make this as clear as possible.</param>
        /// <param name="numIterations">The total number of iterations to process</param>
        /// <param name="chunkSize">
        ///     The number of iterations to process in each threaded chunk.  
        ///     Set this to zero if you want the application to optimise this by splitting the total number of iterations equally between all but one logical processor.</param>
        /// <param name="TaskToRun">
        ///     TaskToRun is the method in the calling code that will actually do the data crunching.
        ///     It has to accept a two integer parameters which are the size of the chunk to be executed, followed by the startIndex ...
        ///     e.g. if this was the third chunk to run, and the chunk size was 1000 the startIndex would be 3000
        /// </param>
        /// <returns>True if the process completed all iterations as expected</returns>
        public static bool Run(string processName, int numIterations, int chunkSize, Action<int, int> TaskToRun) {
            bool success = false;

            //-----1----- Reset all the global variables
            IterationsToRun = numIterations;
            ResetGlobalVariables(chunkSize);

            //-----2----- Start the threaded processing ...
            Logger.LogSubHeading("Starting "+processName+" threaded processing....");                       
            InitialiseThreads(TaskToRun);
            
            //-----3----- Now we need to keep feeding threads until the process is complete.  This is the heavy lifting!
            success = ManageThreads(TaskToRun);
            
            Logger.Log(processName + " in the DNThreadManager processing completed " + IterationsCompleted + " iterations in total.");
            
            return success;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static int CalculateOptimalChunkSize(int numIterations) {
            return ((int)Math.Ceiling((double)numIterations / (double)Environment.ProcessorCount));
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Resets all the global variables to enable the processing to start again ....
        /// </summary>
        protected static void ResetGlobalVariables(int overriddenChunkSize) {

            //-----1a----- Reset the global progress variables
            IterationsScheduled = 0;
            C = new List<int>();
            ProcessingThreads = new List<Thread>();

            //-----1b----- Get the number of cores on this machine (http://stackoverflow.com/questions/1542213/how-to-find-the-number-of-cpu-cores-via-net-c)
            NumLogicalProcessors = Environment.ProcessorCount;
            // Keep one processor spare for sanity if there is more than one!!
            // 15-Jun-16 - Lets try utilising all the processors but setting the threads to run at a lower priority
            //NumLogicalProcessors = (NumLogicalProcessors > 1) ? NumLogicalProcessors - 1 : NumLogicalProcessors;
            Logger.Log("Utilising processing on " + NumLogicalProcessors + " processors...");

            /*
             * -----1c-----
             *  If the provided chunkSize is zero, we optimise the ThreadChunkSize for the calling code based on a simple division of the number of 
             * the total number of iterations to perform by the number of logical processors, reserving one processor for the management tasks.
             * In memory intensive tasks, this might produce suboptimal performance, so good to give the user the option of overriding this.
             * This methodology means that we will probably actually process a few more iterations than are requested as the total is rounded up to the next chunked block!
            */
            if ( overriddenChunkSize > 0) {
                ThreadChunkSize = overriddenChunkSize;
            } else {
                ThreadChunkSize = ((int)Math.Ceiling((double)IterationsToRun / (double)NumLogicalProcessors));
            }



            //-----1d----- And lets give the user some feedback now about how "chunky" this threaded management will be...
            int NumChunksToRun = (int)Math.Ceiling(((double)IterationsToRun) / ((double)ThreadChunkSize));
            Logger.Log("Number of iterations to perform is: " + IterationsToRun + ", which will require " + NumChunksToRun + " threaded chunks.");

        }



        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Simply spawns a thread for each of the logical processors (excluding the management dedicated processor)
        /// </summary>
        protected static void InitialiseThreads(Action<int, int> TaskToRun) {

            for (int i = 0; i < NumLogicalProcessors; i++) {

                SpawnThread(TaskToRun);

            }
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Checks whether or not the current processing threads initialised in InitialiseThreads have completed and keeps feeding
        ///     new threaded chunks to available logical processors until enough iterations have been scheduled to process all of the 
        ///     required iterations.  Returns true if the processing completed successfully.
        /// </summary>
        protected static bool ManageThreads(Action<int, int> TaskToRun) {

            // Now we need to keep removing threads that have finished and feeding threads to the spare logical processors until enough iterations have been scheduled
            // and the process is complete ...
            bool processComplete = false;
            while (processComplete == false) {

                //-----1----- Loop through the processing threads list and remove any that have completed
                for (int i = 0; i < ProcessingThreads.Count; i++) {
                    // if one thread has died, remove it and spawn another
                    if (ProcessingThreads[i].IsAlive == false) {
                        // Ensure the removal is threadsafe ...
                        lock (ProcessingThreads) {
                            ProcessingThreads.RemoveAt(i);
                            i--;
                        }
                    }
                }

                //-----2----- Check if the process has already completed
                processComplete = (IterationsCompleted == IterationsToRun);

                //-----3----- If the process is not complete, spawn more threads if there are spare ones AND if enough iterations have not already been scheduled ...
                if (processComplete == false && (IterationsScheduled < IterationsToRun)) {
                    
                    // So if one or more processes have completed in the threads, lets fire off a few more ...
                    int numSpareThreads = NumLogicalProcessors - ProcessingThreads.Count;

                    // Spawn the required number of threads (note that the check on iterations scheduled versus iterations required is also checked in spawn thread ...
                    if (numSpareThreads > 0) {
                        for (int i = 0; i < numSpareThreads; i++) {
                            SpawnThread(TaskToRun);
                        }
                    }
                }

                //-----4-----  Lets pause for a cup of PC tea and then lets log the progress and check again if we have completed all our iterations.
                Thread.Sleep(10);
                Logger.Log(IterationsCompleted, loggingInterval, IterationsToRun);
                processComplete = (IterationsCompleted == IterationsToRun);

                /*
                 * -----5----- Rather than an infinite loop if some of the iterations fail, lets fail out of the while loop gracefully
                 *      A graceful fail is defined as:
                 *          a. The process is not complete
                 *          b. Even though more than enough iterations have been scheduled to complete this task
                 *          c. And none of the processing threads are still alive so no actual processing is occurring.
                */
                if (processComplete == false 
                    && IterationsScheduled >= IterationsToRun
                    && ProcessingThreads.Count == 0) {

                    // Ok - lets just pause for a long cup of PC tea and lets check again, just in case some of the iterations had not previously been counted.
                    Thread.Sleep(100);
                    processComplete = (IterationsCompleted == IterationsToRun);
                    if (processComplete == false
                        && IterationsScheduled >= IterationsToRun
                        && ProcessingThreads.Count == 0) {

                        // Oh bugger.  Lets spit out a slightly grumpy error message ....
                        Logger.LogError(7, "ThreadManager did not complete successfully.  "
                        + IterationsToRun + " iterations were required and "
                        + IterationsScheduled + " have been scheduled, but only "
                        + IterationsCompleted + " iterations have been completed successfully.  Exiting.");
                        break;
                    }
                }
            }
            
            //-----6----- And lets spit out a final iteration cycles message if this process ended successfully
            if ( processComplete == true) {
                Logger.Log(IterationsCompleted, 1, IterationsToRun);
                Logger.Log("\n");
            }

            return processComplete;
        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Creates a new thread - this method is always called from the "main" Thread
        /// </summary>
        protected static Thread SpawnThread(Action<int, int> TaskToRun) {

            Thread t = null;

            //-----1a----- Get the chunk size to spawn in this thread
            int tempThreadChunkSize = ThreadChunkSize;
            //-----1b----- Special case exists that if this is the "last" threaded chunk to process, we might want to make it smaller and ensure that it just covers the remainder
            // from the previous full chunk
            if (IterationsScheduled > IterationsToRun) {
                // Should we use the % modulus here to get the difference??
                tempThreadChunkSize = ThreadChunkSize - (IterationsScheduled - IterationsToRun);
            }

           
            //-----2----- Only actually spawn a new thread if there is work to do!!
            // This stops us spawning totally irrelevant threads...
            if (tempThreadChunkSize > 0) {

                // Increment the total number of iterations that have been scheduled.  And ensure this is threadsafe.  For this particular variable this is probably overkill
                // as it is only used in the "main" thread management logic, but lets keep it for now.  If in a multithreaded context, we could still have it changed in another
                // thread immediately before or after this call!!
                Interlocked.Add(ref IterationsScheduled, tempThreadChunkSize);

                // Calculate the ABSOLUTE start index using the updated number of iterations scheduled minus current chunk size
                int tempStartIndex = IterationsScheduled - tempThreadChunkSize;

                // Initialise the thread call
                ThreadStart ts = new ThreadStart(delegate () { TaskToRun(tempThreadChunkSize, tempStartIndex); });
                // Create and start the thread!
                t = new Thread(ts);
                // 15-Jun-16 - Lets see if we can run one more thread (i.e. all cores are working), but at a lower priority, so that if there are other activities
                // on the PC, these take priority.
                t.Priority = ThreadPriority.BelowNormal;
                t.Start();

                // Add this thread to our global list of processing threads in a threadsafe manner
                lock (ProcessingThreads) {
                    ProcessingThreads.Add(t);
                }
            }

            // Currently redundant to return the thread itself, but maybe useful in the future
            return t;
        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Called from within the TaskToRun method once one or more specific iterations have completed.  
        ///     Increments the total number completed (stored in the C list) in a threadsafe manner using locking
        /// </summary>
        public static void IncrementNumIterationsCompleted() {

            // Easiest and safest just call the other overridden version of this method to do the incrementing ...
            IncrementNumIterationsCompleted(1);
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Called from within the TaskToRun method once one or more specific iterations have completed.  
        ///     Increments the total number completed (stored in the C list) in a threadsafe manner using locking
        /// </summary>
        public static void IncrementNumIterationsCompleted(int addition) {

            lock (C) {
                if (C.Count == 0) {
                    C.Add(addition);
                } else {
                    int tempC = C[0];
                    C[0] = Interlocked.Add(ref tempC, addition);
                }
            }
        }
  

    }
}
