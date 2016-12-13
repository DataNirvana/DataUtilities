using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

//------------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Image Processing using the GDI set of classes...
    /// </summary>
    public class ImageProcessorGDI {

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public ImageProcessorGDI() {

        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static bool ResizeImage(string imagePath, int maxWidth, int maxHeight, out MemoryStream memoryStream) {
            bool success = false;

            memoryStream = null;

            try {

                Bitmap exifThumbnail = GetExifThumbnail(imagePath);

                FileStream fs = new FileStream( imagePath, FileMode.Open );
                Image im = Image.FromStream(fs, true, false);

                //_____ Get the correctly scaled image, using the newWidth and newHeight as the maximum allowed ...
                int newWidth, newHeight;
                newWidth = newHeight = 0;

                success = ImageProcessor.GetScaledWidthHeight(maxWidth, maxHeight, im.Width, im.Height, out newWidth, out newHeight);

                if (exifThumbnail != null) {
                    im.RotateFlip(RotateFlipType.Rotate180FlipY);
                    im.RotateFlip(RotateFlipType.Rotate180FlipY);
                }

                Image thumbnail = im.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero);

                ImageConverter ic = new ImageConverter();
                byte[] byteArray = (byte[]) ic.ConvertTo( thumbnail, typeof(byte[]));

                memoryStream = new MemoryStream(byteArray);

                fs.Flush();
                fs.Close();

            } catch (Exception ex) {

                Logger.LogError(5, "Problem resulted in crash in ImageProcessorGDI.ResizeImage(): " + ex.ToString());

            }

            return success;
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Bitmap GetExifThumbnail(string filename) {

            Bitmap b = null;

            FileStream fs = new FileStream(filename, FileMode.Open);

            Image img = Image.FromStream(fs, true, false);


            foreach (PropertyItem pi in img.PropertyItems) {

                if (pi.Id == 20507) {

                    b = (Bitmap)Image.FromStream(new MemoryStream(pi.Value));
                }
            }

            fs.Flush();
            fs.Close();

            return b;

        }




                //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static bool Test(bool writeToFile) {
            bool success = false;

            try {
                ImageProcessorGDI ipGDI = new ImageProcessorGDI();
                MemoryStream ms = null;

                string dir = "C:\\Backups\\Photos_Testing_ImageResizing\\";
                string photoPath = dir + "ImageResizeTest1.jpg";

                success = ImageProcessorGDI.ResizeImage(photoPath, 1999, 1999, out ms);

                //_____ Convert this thumbnail image to a memory stream ....
                if (writeToFile) {

                    Image im = Image.FromStream(ms);
//                    byte[] targetBytes = ms.ToArray();

                    im.Save( dir + "OutputGDI_" + DateTimeInformation.GetCurrentDate("number")
                        + "_" + DateTimeInformation.GetCurrentTime() + ".jpg", ImageFormat.Jpeg );

                    // Finally - write this file back out ....
//                    FileStream fs = new FileStream(, FileMode.Create);

//                    fs.Write(targetBytes, 0, targetBytes.Length);
                    //fs.Flush();
//                    fs.Close();

//                    if (targetBytes.Length > 0) {
                        success = true;
//                    }
                } else {
                    if (ms != null && ms.Length > 0) {
                        success = true;
                    }
                }


            } catch (Exception ex) {
                Logger.LogError(5, "Problem resulted in crash in ImageProcessorGDI.Test(): " + ex.ToString());
//                string temp = "";
            }

            return success;
        }



    }
}
