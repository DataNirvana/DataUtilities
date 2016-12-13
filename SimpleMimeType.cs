using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//-----------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------
    public static class SimpleMimeType {

        //------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string GetMimeTypeFromFileSuffix(string Filename) {

            string mime = "application/octetstream";
            string ext = System.IO.Path.GetExtension(Filename).ToLower();
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (rk != null && rk.GetValue("Content Type") != null)
                mime = rk.GetValue("Content Type").ToString();
            return mime;
        }

    }
}
