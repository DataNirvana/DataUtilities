using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Configuration;
using System.Diagnostics;

namespace MGL.Data.DataUtilities
{
    public abstract class MGLDBConnectionMonitor
    {
        private static List<string> IgnoreMethods
        {
            get
            {
                List<string> ignoreMethods = HttpContext.Current.Application["IgnoreMethodStrings"] as List<string>;
                if (ignoreMethods == null)
                {
                    ignoreMethods = new List<string>();
                    ignoreMethods.Add("ValidateConnection");
                    ignoreMethods.Add("ExecuteReader");
                    ignoreMethods.Add("RunSqlReader");
                    ignoreMethods.Add("ExecuteNonQuery");
                    ignoreMethods.Add("Disconnect");
                    ignoreMethods.Add("RunSqlScalar");
                    ignoreMethods.Add("Connect");
                    ignoreMethods.Add("RunSqlNonQuery");
                    ignoreMethods.Add("RunSqlScalar");
                    ignoreMethods.Add("GetPseudoMethodName");
                    ignoreMethods.Add("Connected");

                    HttpContext.Current.Application.Lock();
                    HttpContext.Current.Application["IgnoreMethodStrings"] = ignoreMethods;
                    HttpContext.Current.Application.UnLock();
                }

                return ignoreMethods;
            }
        }

        [Conditional("DEBUG")]
        public static void Connected()
        {
            if (DBMonitoringOn)
            {

                OpenDBConnectionsCounter++;

                string methodName = GetPseudoMethodName();

                CalledMethodNames.Add(methodName);
            }
        }

        [Conditional("DEBUG")]
        public static void Disconnected()
        {
            if (DBMonitoringOn)
            {
                OpenDBConnectionsCounter--;

                string methodName = GetPseudoMethodName();

                if (CalledMethodNames.Contains(methodName))
                {
                    CalledMethodNames.Remove(methodName);
                }
            }
        }

        private static bool DBMonitoringOn
        {
            get
            {
                return Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["DBMonitoringOn"]);
            }
        }

        private static string GetPseudoMethodName()
        {
            StackTrace st = new StackTrace(false);

            StringBuilder methodName = new StringBuilder();

            string name = "";
            for (int i = 0; i < st.FrameCount - 1; i++)
            {
                name = st.GetFrame(i).GetMethod().Name;

                if (!IgnoreMethods.Contains(name))
                {
                    methodName.Append(name + "().");
                }
            }

            return methodName.ToString();
        }

        public static List<string> CalledMethodNames
        {
            get
            {
                List<string> calledMethodNames = HttpContext.Current.Application["DBMonCalledMethodNames"] as List<string>;
                if (calledMethodNames == null)
                {
                    calledMethodNames = new List<string>();
                    HttpContext.Current.Application.Lock();
                    HttpContext.Current.Application["DBMonCalledMethodNames"] = calledMethodNames;
                    HttpContext.Current.Application.UnLock();
                }

                return calledMethodNames;
            }
        }

        public static int OpenDBConnectionsCounter
        {
            get
            {
                int openDBConnectionsCounter = 0;
                object openDBConnectionsCounterObj = HttpContext.Current.Application["OpenDBConnectionsCounter"];
                if (openDBConnectionsCounterObj == null)
                {
                    HttpContext.Current.Application["OpenDBConnectionsCounter"] = 0;
                }
                else
                {
                    openDBConnectionsCounter = (int)openDBConnectionsCounterObj;
                }

                return openDBConnectionsCounter;
            }
            set
            {
                HttpContext.Current.Application.Lock();
                HttpContext.Current.Application["OpenDBConnectionsCounter"] = value;
                HttpContext.Current.Application.UnLock();
            }
        }
    }
}
