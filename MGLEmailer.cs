using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using System.IO;
using System.Net.Mime;

//-------------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     DEPRECTATED - Use MGL.Security.Email.MGLSecureEmailer instead as this enables emails to be signed and encrypted ...
    /// </summary>
    public class MGLEmailer {

        private static string defaultFromMailAddress = "info@manchestergeomatics.com";
        public static string FromMailAddress {
            get { return defaultFromMailAddress; }
            set { defaultFromMailAddress = value; }
        }

        private static string defaultSMTPHost = "post.wizards.co.uk";
        public static string SMTPHost {
            get { return defaultSMTPHost; }
            set { defaultSMTPHost = value; }
        }

        private static string defaultSMTPUsername = "manchestergeomatics@scootmail.com";
        public static string SMTPUsername {
            get { return defaultSMTPUsername; }
            set { defaultSMTPUsername = value; }
        }

        private static string defaultSMTPPassword = "ju1c13r";
        public static string SMTPPassword {
            get { return defaultSMTPPassword; }
            set { defaultSMTPPassword = value; }
        }

        private static int defaultSMTPPort = 25;
        public static int SMTPPort {
            get { return defaultSMTPPort; }
            set { defaultSMTPPort = value; }
        }

        private static bool enableSSL = false;
        public static bool EnableSSL {
            get { return enableSSL; }
            set { enableSSL = value; }
        }

        private static bool bodyIsHTML = false;
        public static bool BodyIsHTML {
            get { return bodyIsHTML; }
            set { bodyIsHTML = value; }
        }



        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///
        /// </summary>
        /// <param name="mailAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="SMTPHost">Leave blank "" for default</param>
        /// <param name="SMTPUsername">Leave blank "" for default</param>
        /// <param name="SMTPPassword">Leave blank "" for default</param>
        public static bool SendEmail(string mailAddress, string subject, string body, string SMTPHost, string SMTPUsername, string SMTPPassword) {
            return SendEmail(mailAddress, subject, body, SMTPHost, SMTPUsername, SMTPPassword, defaultFromMailAddress, -1, false);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static bool SendEmail(string mailAddress, string subject, string body, string SMTPHost, string SMTPUsername, string SMTPPassword, string fromMailAddress) {
            return SendEmail(mailAddress, subject, body, SMTPHost, SMTPUsername, SMTPPassword, fromMailAddress, -1, false);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///
        /// </summary>
        /// <param name="mailAddress">The email to send this message to</param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="SMTPHost">Leave blank "" for default</param>
        /// <param name="SMTPUsername">Leave blank "" for default</param>
        /// <param name="SMTPPassword">Leave blank "" for default</param>
        public static bool SendEmail(string mailAddress, string subject, string body, string SMTPHost, string SMTPUsername, string SMTPPassword, string fromMailAddress, int portID, bool EnableSSL) {
            bool isSuccess = true;
            try {
                CheckSMTPDetails(ref fromMailAddress, ref SMTPHost, ref SMTPUsername, ref SMTPPassword, ref portID);

                //_____1_____ Lets start the message - note we just want to add the from, to and subject for now
                MailMessage msgMail = new MailMessage(fromMailAddress, mailAddress);
                msgMail.Subject = subject;
                msgMail.Body = body;

                // Select whether the body should be identified as HTML or not using the global paramaters ....
                msgMail.IsBodyHtml = bodyIsHTML;

                //_____2_____ Setup the SMTP client
                Logger.LogInfo("Creating smtp client: " + SMTPHost);
                System.Net.Mail.SmtpClient smtpClient = new SmtpClient(SMTPHost);

                if (portID > 0) {
                    Logger.LogInfo("Using port id: " + portID);
                    smtpClient = new SmtpClient(SMTPHost, portID);
                } else {
                    smtpClient = new SmtpClient(SMTPHost);
                    Logger.LogInfo("Using default port id: " + smtpClient.Port);
                }

                // Sort the credentials to be used for the SMTP connection out
                System.Net.NetworkCredential ncAuth = new System.Net.NetworkCredential(
                    SMTPUsername, SMTPPassword);

                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = ncAuth;

                // to use ssl or not that is the question - with the SMTP connection
                if (EnableSSL || SMTPHost.Contains("gmail")) {
                    Logger.LogInfo("Using SSL.");
                    smtpClient.EnableSsl = true;
                }


                Logger.LogInfo("Sending email.");
                smtpClient.Send(msgMail);

                msgMail.Dispose();
            } catch (Exception ex) {
                isSuccess = false;
                string msg = "Error whilst trying to send MGL email using the following settings: " +
                "SMTPHost: " + SMTPHost + ", " +
                "SMTPUsername: " + SMTPUsername + ", " +
                "portID: " + portID + ", " +
                "EnableSSL: " + EnableSSL.ToString() + ". ";

                //MGLErrorLog.LogError(msg + ":" + ex);
                //Logger.Log(msg, ex.StackTrace);

                Logger.LogWarning("Failed to send the mgl email: " + msg + "\n" + ex.ToString());
            }

            return isSuccess;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static void CheckSMTPDetails(ref string fromMailAddress, ref string SMTPHost, ref string SMTPUsername, ref string SMTPPassword, ref int SMTPPort) {
            if (string.IsNullOrEmpty(fromMailAddress)) {
                fromMailAddress = defaultFromMailAddress;
            }

            if (string.IsNullOrEmpty(SMTPHost)) {
                SMTPHost = defaultSMTPHost;
            }
            if (string.IsNullOrEmpty(SMTPUsername)) {
                SMTPUsername = defaultSMTPUsername;
            }
            if (string.IsNullOrEmpty(SMTPPassword)) {
                SMTPPassword = defaultSMTPPassword;
            }
            if (SMTPPort < 0) {
                SMTPPort = defaultSMTPPort;
            }
        }



        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static bool Test() {
            bool success = false;

            MGLEmailer.BodyIsHTML = true;
            int port = 587;

            success = MGLEmailer.SendEmail("ed_scrase@hotmail.com", "Testing Sending an email",
                "Eddie - testing again<br/><br/>And another line - HTML is awesome! <br /><b>E</b>",
                "smtp.gmail.com", "unhcr.pakimteam@gmail.com", "xxx_Password_HERE_xxx", "unhcr.pakimteam@gmail.com",
                port, true);

            return success;
        }


    }
}

