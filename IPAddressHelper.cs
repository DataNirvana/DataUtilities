using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;

//-------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {
    //-----------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Thanks to the four guys from rolla for the advice on always getting the IP4 address and filtering out any IPV6 addresses.
    ///     http://www.4guysfromrolla.com/articles/071807-1.aspx
    ///     And test whether IPV6 is even possible - most ISPs still don't support it:
    ///     http://ds.testmyipv6.com/
    /// </summary>
    public static class IPAddressHelper {

        //--------------------------------------------------------------------------------------------------------------------------------------------
        public static string GetIP4AddressFromHTTPRequest() {
            string ip4Address = "";

            try {

                foreach (IPAddress IPA in Dns.GetHostAddresses(HttpContext.Current.Request.UserHostAddress)) {
                    if (IPA.AddressFamily.ToString() == "InterNetwork") {
                        ip4Address = IPA.ToString();
                        break;
                    }
                }

            } catch (Exception ex) {
                Logger.LogError( 3, "Could not find the IP4 address from the HTTP request.  The specific error generated was: "+ex.ToString());
            }

            return ip4Address;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Return the IP Address provided by the request - 99.9% of the time this will be an IP4
        /// </summary>
        /// <returns></returns>
        public static string GetIPAddressFromHTTPRequest() {

            try {
                return HttpContext.Current.Request.UserHostAddress;
            } catch (Exception ex) {
                Logger.LogError(3, "Could not find the IP address from the HTTP request.  The specific error generated was: " + ex.ToString());
            }

            return "";
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Return the IP Address provided by the request - 99.9% of the time this will be an IP4, but this degrades to an IP6, if no IP4 is provided
        /// </summary>
        /// <returns></returns>
        public static string GetIP4OrAnyAddressFromHTTPRequest() {

            string tempIPAddress = GetIP4AddressFromHTTPRequest();

            if (string.IsNullOrEmpty(tempIPAddress)) {
                tempIPAddress = GetIPAddressFromHTTPRequest();
            }

            return tempIPAddress;

        }

        //--------------------------------------------------------------------------------------------------------------------------------------------
        public static string GetIP4AddressFromLocalMachine() {
            string ip4Address = String.Empty;

            foreach (IPAddress IPA in Dns.GetHostAddresses(Dns.GetHostName())) {
                if (IPA.AddressFamily.ToString() == "InterNetwork") {
                    ip4Address = IPA.ToString();
                    break;
                }
            }

            return ip4Address;
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Converts an IP 4 address of the form 192.168.1.1 to an integer in big endian fashion
        ///     (max is equivalent to an unsigned integer in a database ie 4 billion or so) - which in c# is a long ...
        /// </summary>
        public static long ParseIP4(string ipAddress) {
            long ip = 0;

            if (string.IsNullOrEmpty(ipAddress) == false) {

                string[] bits = ipAddress.Split(new string[] { "." }, StringSplitOptions.None);

                if (bits != null && bits.Length == 4) {
                    /*
                     * MySQL has this super useful query called INET_ATON
                        SELECT * FROM IP_City_Lookup WHERE
                            (IP_From BETWEEN 3310027880 AND 3310327880
                            OR IP_To BETWEEN 3310027880 AND 3310327880)
                        AND (INET_ATON("197.76.139.8") BETWEEN IP_From AND IP_To);
                     *
                     * OR we convert in c# for speed ...
                     * For example, my local google.com is at 64.233.187.99. That's equivalent to:
                    64*2^24 + 233*2^16 + 187*2^8 + 99 = 1089059683
                    */
                    double[] multipliers = new double[] { Math.Pow(2, 24), Math.Pow(2, 16), Math.Pow(2, 8), 1.0 };

                    for( int i=0; i < bits.Length; i++) {
                        long bit = 0;
                        long.TryParse( bits[ i ], out bit );
                        ip += bit * (long) multipliers[ i ];
                    }
                }
            }

            return ip;
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------
        public static void Test() {

            bool test1 = ParseIP4("64.233.187.99") == 1089059683;
            bool test2 = ParseIP4("197.76.139.8") == 3310127880;
            bool test3 = ParseIP4("1.1.1.1") == 16843009;
            bool test4 = ParseIP4("1.2.1.1") == 16908545;
            bool test5 = ParseIP4("1.1.2.1") == 16843265;

            bool success = test1 && test2;
        }

    }
}
