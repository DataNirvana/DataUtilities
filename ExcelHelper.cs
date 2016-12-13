using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------
    public class ExcelHelper {

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string[] alphabet = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string ConvertColumnNumberToLetter(int number) {
            string letters = "";

            // note that the cell is One based, not zero based ...
            if (number >= 0) {

                int bigNumber = (int)Math.Floor((double)number / 26);
                int smallNumber = number % 26;

                if (bigNumber > 0) {
                    letters = alphabet[bigNumber - 1];
                }

                letters = letters + alphabet[smallNumber];

            }

            return letters;
        }



    }
}
