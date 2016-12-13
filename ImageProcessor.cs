using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
//using System.Drawing;

//------------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Image Processing using the Windows Presentation Foundation set of classes...
    /// </summary>
    public class ImageProcessor {

//        public static int myDefaultJpegCompressionQualityLevel = 85; // 85% ...
        public static int myDefaultJpegCompressionQualityLevel = 90; // 90% ...

        public static string outputMimeType = "image/jpeg";
        public static string outputImageSuffix = "jpg";
        public static List<string> validFileSuffixes = new List<string>(new string[] { "jpg", "gif", "png", "tif" });
        public static List<string> invalidFileSuffixes = new List<string>(new string[] { "db", "mdb" });

        /// <summary>
        ///     The prefix to enable the base 64 jpegs to be displayed correctly as the SRC attribute of images in HTML 5 web pages ...
        /// </summary>
        public static string Base64JPEGMetadata = "data:image/jpeg;base64,";

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public ImageProcessor() {

        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Converts the given bitmap to a base 64 jpeg image string
        /// </summary>
        public static string Generate64BitString(Bitmap b) {
            string base64String = null;

            try {
                if (b == null) {
                    Logger.LogError(7, "Loading the base 64 string from a bitmap failed as the bitmap was null");
                } else {
                    Byte[] data;

                    // save the bitmap to a memory stream and extract the byte array
                    using (var memoryStream = new MemoryStream()) {
                        b.Save(memoryStream, ImageFormat.Jpeg);

                        data = memoryStream.ToArray();
                    }

                    // convert this byte array to a base 64 string ...
                    base64String = Convert.ToBase64String(data);
                }
            } catch (Exception ex) {
                Logger.LogError(7, "Loading the base64String from an image failed: " + ex.ToString());
            } finally {
                // clean up!
                b = null;
            }

            return base64String;
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Converts the given byte array to a base 64 jpeg image string
        /// </summary>
        public static string Generate64BitString(Byte[] ba) {
            string base64String = null;

            try {
                if (ba == null) {
                    Logger.LogError(5, "Loading the base 64 string from a byte array failed as the array was null");
                } else {
                    // convert this byte array to a base 64 string ...
                    base64String = Convert.ToBase64String(ba);
                }
            } catch (Exception ex) {
                Logger.LogError(5, "Loading the base64String from an image failed: " + ex.ToString());
            } finally {
                // clean up
                ba = null;
            }

            return base64String;
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Converts the given bitmap to a jpeg byte array
        /// </summary>
        public static byte[] GenerateByteArray(Bitmap b) {
            byte[] data = null;

            try {
                if (b == null) {
                    Logger.LogError(5, "Loading the bytearray from a bitmap failed as the bitmap was null");
                } else {

                    // save the bitmap to a memory stream and extract the byte array
                    using (var memoryStream = new MemoryStream()) {
                        b.Save(memoryStream, ImageFormat.Jpeg);

                        data = memoryStream.ToArray();
                    }
                }
            } catch (Exception ex) {
                Logger.LogError(5, "Loading the bytearray from an image failed: " + ex.ToString());
            } finally {
                // clean up!
                b = null;
            }

            return data;
        }




        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Converts the given bitmap to a hex string
        /// </summary>
        public static string GenerateHexString(Bitmap b) {
            string hexString = null;

            try {
                if (b == null) {
                    Logger.LogError(5, "Loading the hexString from a bitmap failed as the bitmap was null");
                } else {
                    byte[] data;

                    // save the bitmap to a memory stream and extract the byte array
                    using (var memoryStream = new MemoryStream()) {
                        b.Save(memoryStream, ImageFormat.Jpeg);

                        data = memoryStream.ToArray();
                    }

                    // convert this byte array to a hex string ...
                    hexString = BitConverter.ToString(data);

                    // and remove all the dashes!
                    hexString = hexString.Replace("-", "");

                }
            } catch (Exception ex) {
                Logger.LogError(5, "Loading the hexString from an image failed: " + ex.ToString());
            } finally {
                // clean up!
                b = null;
            }

            return hexString;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Converts the given bitmap to a blob string
        /// </summary>
        public static string GenerateBlobString(Bitmap b) {
            string blobString = null;

            try {
                if (b == null) {
                    Logger.LogError(5, "Loading the blobString from a bitmap failed as the bitmap was null");
                } else {
                    Byte[] data;

                    // save the bitmap to a memory stream and extract the byte array
                    using (var memoryStream = new MemoryStream()) {
                        b.Save(memoryStream, ImageFormat.Jpeg);

                        data = memoryStream.ToArray();
                    }

                    // convert this byte array to a hex string ...
                    blobString = Encoding.UTF8.GetString(data);
                }
            } catch (Exception ex) {
                Logger.LogError(5, "Loading the blobString from an image failed: " + ex.ToString());
            } finally {
                // clean up!
                b = null;
            }

            return blobString;
        }



        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Converts a base64 string to an image.
        ///     Will strip the "data:image/jpeg;base64," header string if it exists
        /// </summary>
        public static Image LoadImage(string base64String) {
            Image image = null;

            try {
                if (base64String == null || base64String.Length == 0) {
                    Logger.LogError(5, "ImageProcessor.LoadImage failed - the given base 64 string is null");
                } else {

                    // the photo *may* have the header on the front of it : data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQ
                    // so lets check and split if needed
                    // 22-Feb-2016 - retain the original string so that we can double check that it is a jpg file being provided when we debug it ...
                    string base64StringData = base64String;
                    if (base64String.Contains(",")) {
                        base64StringData = base64String.Split(new string[] { "," }, StringSplitOptions.None)[1];
                    }

                    // 22-Feb-2016 - this method is occasionally throwing this error message:
                    // Loading the image from the base 64 string failed: System.FormatException: Invalid length for a Base-64 char array or string.
                    // a Base64 string has to be a multiple of 4 as each text character comprises one quarter of a byte
                    // See this useful resource for more info: http://stackoverflow.com/questions/2925729/invalid-length-for-a-base-64-char-array
                    // The workaround as you can see here, is to simply reappend the requisite number of = signs to the end of the base 64 string ...
                    int mod4 = base64StringData.Length % 4;
                    if (mod4 > 0) {

                        int lengthBefore = base64StringData.Length;

                        // 2-Mar-2016 - Tweak this code so that a max of two == are included
                        if (base64StringData.EndsWith("==") == true) {
                            // No nothing!! - this is legitimate usage - a max of two padding characters are allowed
                            // System.FormatException: The input is not a valid Base-64 string as it contains a non-base 64 character,
                            // more than two padding characters, or an illegal character among the padding characters.
                        } else if (base64StringData.EndsWith("=") == true) {
                            // in here so we need to do something - so lets add just one padder
                            base64StringData += "=";
                        } else {

                            // https://en.wikipedia.org/wiki/Base64
                            // Max two == This still doesnt work for some images, but looking at them they do appear as though they have been truncated
                            // so the issue may well be truncation due to the internet connection rather than any padding issues
                            string tempPadding = ((4-mod4) > 1 ) ? "==" : "=";
                            //string tempPadding = ((4 - mod4) % 2 == 0) ? "==" : "=";
                            //string tempPadding = "=";
                            //base64StringData += new string('=', 4 - mod4);
                            base64StringData += tempPadding;

                        }

                        int lengthAfter = base64StringData.Length;

                        Logger.LogWarning("ImageProcessor.LoadImage - The length of the given base 64 string was not a multiple of four characters as is required in this format.  Attempting to fix this by appending "
                            + (4 - mod4) + " special characters ('=') to the end of the string which signifies no data.  Sometimes these characters are removed from the end of URL strings."
                            + "  Length before: "+lengthBefore+" and after: "+lengthAfter+".");

                        // 11-Mar-2015 - add another message to make it clearer as to why this image issue might have been occurring...
                        if (4 - mod4 == 3) {
                            Logger.LogWarning("ImageProcessor.LoadImage - In this case, three characters were required, which is unusual and probably means that the image was truncated as it was uploaded.  "
                                +"Probably another manifestation of the session expiry issues.");
                        }
                    }

                    // use the standard convert method to get a byte array from the 64 bit string
                    byte[] bytes = Convert.FromBase64String(base64StringData);

                    // then stream this byte array into an image
                    using (MemoryStream ms = new MemoryStream(bytes)) {
                        image = Image.FromStream(ms);
                    }
                }
            } catch (Exception ex) {
                // 11-Feb-2016 - lets also include the base64string itself in the error message, then we can play with it...
                Logger.LogError(407, "ImageProcessor.LoadImage - Loading the image from the base 64 string failed: " + ex.ToString() + "\n\n" + base64String);
            }
            return image;
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Resizes an image based on the given new width and height....
        /// </summary>
        public static Bitmap ResizeImage(Image image, int width, int height) {
            Rectangle destRect;
            Bitmap destImage = null;

            try {

                // check for silly inputs ...
                if (image == null || width == 0 || height == 0) {
                    Logger.LogError(5, "ImageProcessor.ResizeImage - Resizing the image failed - either the image is null, or the width and / or height of the desired resize have been set to zero!  Dont do this!");
                } else {
                    destRect = new Rectangle(0, 0, width, height);
                    destImage = new Bitmap(width, height);

                    // retain the source image resolution ...
                    destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                    using (Graphics graphics = Graphics.FromImage(destImage)) {

                        // keep this image at a high quality!
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        // these next two specifically ensure that anti-aliasing is in place
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        using (ImageAttributes wrapMode = new ImageAttributes()) {
                            // the wrap mode stops the ghosting effect of lines appearing next to features
                            /*
                                The effect of TileFlipXY comes into play when the resizing algorithm gathers detail from neighboring pixels along the edges of the image.
                                TileFlipXY tells it to place horizontally and vertically flipped copies of the image next to itself, thereby putting similarly colored pixels next
                                to the ones at the border. By doing that, no more ghost borders will appear.
                             */
                            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);

                        }
                    }
                }
            } catch (Exception ex) {
                Logger.LogError(5, "ImageProcessor.ResizeImage - Resizing the image failed - here is the detailed error message: " + ex.ToString());
            } finally {
                // clean up!
                image = null;
            }

            return destImage;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The cropping UserControl facility does not always produce a small enough photo, so it is good to recheck this on the server.
        ///     This method resizes the image if it is too large (600*750 - now parameterised) which is double the proGres specifications and results in an image of around 30kb.
        ///     The method takes approximately 300 to 500 ms to run for an image of about 1000 * 800 px, which is significant, but not relative to the upload times for the image,
        ///     which are likely to run into the several seconds.
        ///     However, now the resampling is done on the client (which results in an approx 60kb) image and then resampled here further reducing the image size to about 30kb.
        ///     The original images were often over 100kb.
        ///     If no resampling is required, the conversion to an Image takes about 30ms, and if resampling from an image the same size is required, then the process
        ///     only takes about 70ms which is totally acceptable.
        ///
        ///     srcData is 64bit jpeg image and note that no checks are performed on the width:height ratio - it is assumed that the given new width and height are consistent
        ///     proportions to the given data ....
        /// </summary>
        public static StringBuilder ResizeImage(string srcData, int newWidth, int newHeight, out bool success) {
            StringBuilder base64Im = new StringBuilder();

            TimeSpan t1 = new TimeSpan(DateTime.Now.Ticks);
            success = false;

            try {

                if (string.IsNullOrEmpty(srcData) == false) {
                    base64Im.Append(srcData);

                    // lets see what the dimensions of the image are here ...
                    // if it is more than double the eventual proGres export, then lets resize it here ...
                    System.Drawing.Image im = ImageProcessor.LoadImage(srcData);

                    // optimise to 600 * 750
                    if (im == null) {
                        Logger.LogWarning("ImageProcessor.LoadImage failed to convert the photo into an image - possible issue!");
                    } else if (im != null && (im.Width >= newWidth || im.Height >= newHeight)) {

                        System.Drawing.Bitmap b = ImageProcessor.ResizeImage(im, newWidth, newHeight);

                        base64Im.Clear();
                        base64Im.Append(ImageProcessor.Base64JPEGMetadata);
                        base64Im.Append(ImageProcessor.Generate64BitString(b));

                        // test that the image actually has been resized!
                        //im = ImageProcessor.LoadImage(base64Im.ToString());

                        // Looking good
                        success = true;
                    } else {
                        // Photo already of a good size!
                        success = true;
                    }
                }
            } catch (Exception ex) {
                Logger.LogError(7, "Could not resize the given cropped photo data: " + ex.ToString());

                // set the base64 stringbuilder to be the src data by default ...
                if (string.IsNullOrEmpty(srcData) == false) {
                    base64Im.Clear();
                    base64Im.Append(srcData);
                }
            }

            TimeSpan t2 = new TimeSpan(DateTime.Now.Ticks);
            double ms = (t2.Subtract(t1)).TotalMilliseconds;

            return base64Im;
        }




        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     OLD OLD OLD
        ///
        ///     The image will be resized to the maximum of maxWidth and maxHeight,
        ///     Depending on the specific orientation of the image itself, the width and height may vary
        ///
        ///     Stream is normally a memorystream, but is a file stream, if the information from the image does not need to be resized ...
        ///
        /// </summary>
        public static bool ResizeImage(string imagePath, int maxWidth, int maxHeight, out MemoryStream memoryStream) {
            bool success = false;

            memoryStream = new MemoryStream();

            FileStream photoStream = null;

            try {


                //_____ Open a file stream to the image ....
                photoStream = new FileStream(imagePath, FileMode.Open);

                //_____ Decode the image and get at the metadata associated with the image
                BitmapDecoder photoDecoder = BitmapDecoder.Create(
                    photoStream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.None);

                //_____ Pull out the first frame of the image ...
                BitmapFrame photo = photoDecoder.Frames[0];

                //_____ Get the correctly scaled image, using the newWidth and newHeight as the maximum allowed ...
                int newWidth, newHeight;
                newWidth = newHeight = 0;

                bool scalingIsRequired = GetScaledWidthHeight(maxWidth, maxHeight, photo.PixelWidth, photo.PixelHeight, out newWidth, out newHeight);

//                if (scalingIsRequired == false) {

                    // just return the file stream itself ...
//                    memoryStream = new MemoryStream();

//                    byte[] targetBytes = new byte[photoStream.Length];
//                    photoStream.Read(targetBytes, 0, (int) photoStream.Length);
//                    photoStream.Flush();

//                    memoryStream.Write(targetBytes, 0, targetBytes.Length);

//                    success = scalingIsRequired;

//                } else {

                    TransformedBitmap target = new TransformedBitmap(
                        photo,
                        new ScaleTransform(
                            newWidth / photo.Width * 96 / photo.DpiX,
                            newHeight / photo.Height * 96 / photo.DpiY,
                            0, 0));

                    BitmapFrame thumbnail = BitmapFrame.Create(target);

                    //_____ Convert this thumbnail image to a memory stream ....
                    //                byte[] targetBytes = null;
                    //                using (memoryStream = new MemoryStream()) {

                    //                PngBitmapEncoder targetEncoder = new PngBitmapEncoder();
                    JpegBitmapEncoder targetEncoder = new JpegBitmapEncoder(); // PngBitmapEncoder();
                    targetEncoder.QualityLevel = myDefaultJpegCompressionQualityLevel;

                    targetEncoder.Frames.Add(thumbnail);
                    targetEncoder.Save(memoryStream);
                    //                    targetBytes = memoryStream.ToArray();
                    //                }

                    if (memoryStream.Length > 0) {
                        success = true;
                    }
//                }

            } catch (Exception ex) {

                Logger.LogError(5, "Couldn't resize the image " + ex.ToString());

            } finally {

                if (photoStream != null) {
                    photoStream.Flush();
                    photoStream.Close();
                }

            }

            return success;
        }




        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns true if the scaled width or height is different ....
        /// </summary>
        public static bool GetScaledWidthHeight(int maxWidth, int maxHeight, int currentWidth, int currentHeight, out int nWidth, out int nHeight) {
            bool success = false;

            nWidth = nHeight = 0;

            try {

                double sMaxRatio;

                double sRealRatio;



                if (currentWidth < 1 || currentHeight < 1 || maxWidth < 1 || maxHeight < 1) {
                    return false;
                } else {


                    sMaxRatio = (double)maxWidth / (double)maxHeight;

                    sRealRatio = (double)currentWidth / (double)currentHeight;



                    if (sMaxRatio < sRealRatio) {

                        nWidth = Math.Min(maxWidth, currentWidth);

                        nHeight = (int)Math.Round(nWidth / sRealRatio);

                    } else {

                        nHeight = Math.Min(maxHeight, currentHeight);

                        nWidth = (int)Math.Round(nHeight * sRealRatio);

                    }

                    // return whether or not the height information has been changed ...
                    success = (nHeight != currentHeight || nWidth != currentWidth );

                }
            } catch (Exception ex) {
                Logger.LogError(5, "ImageProcessor.GetScaledWidthHeight crashed when access was attempted: " + ex.ToString());
            }

                // look at the proportions and work out what to do


            return success;
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static bool Test(bool writeToFile) {
            bool success = false;

            try {
                //////////////////////////////////
                string dir = "C:\\Backups\\Photos_Testing_ImageResizing\\";
                string photoPath = dir + "ImageResizeTest1.jpg";
                int width = 1999;
                int height = 1999;

                MemoryStream ms = null;

                success = ImageProcessor.ResizeImage(photoPath, width, height, out ms);

                //_____ Convert this thumbnail image to a memory stream ....
                if (writeToFile) {
                    byte[] targetBytes = ms.ToArray();

                    // Finally - write this file back out ....
                    string fileName = dir + "Output_" + DateTimeInformation.GetCurrentDate("number")
                        + "_" + DateTimeInformation.GetCurrentTime()
                        + ".jpg";
                        //+ ".png";

//                    Image im = Image.FromStream(ms);
//                    im.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);


                    FileStream fs = new FileStream(fileName, FileMode.Create);

                    fs.Write(targetBytes, 0, targetBytes.Length);
                    fs.Flush();
                    fs.Close();

                    if (ms.Length > 0) {
                        success = true;
                    }
                } else {
                    if (ms != null && ms.Length > 0) {
                        success = true;
                    }
                }

            } catch (Exception ex) {

                string temp = ex.ToString();

            }

            return success;
        }



        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static bool TestOriginal() {
            bool success = false;

            try {
                //////////////////////////////////
                string dir = "C:\\Backups\\Photos_Testing_ImageResizing\\";
                string photoPath = dir + "ImageResizeTest1.jpg";
                int width = 1999;
                int height = 1999;



                //BitmapImage bi = new BitmapImage();
                //bi.BeginInit();
                //bi.UriSource = new Uri(photoPath);
                //bi.DecodePixelWidth = width;
                //bi.DecodePixelHeight = height;
                //bi.EndInit();

                //bi = null;

                FileStream photoStream = new FileStream(photoPath, FileMode.Open);

                BitmapDecoder photoDecoder = BitmapDecoder.Create(
                    photoStream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.None);

                BitmapFrame photo = photoDecoder.Frames[0];

                TransformedBitmap target = new TransformedBitmap(
                    photo,
                    new ScaleTransform(
                        width / photo.Width * 96 / photo.DpiX,
                        height / photo.Height * 96 / photo.DpiY,
                        0, 0));

                BitmapFrame thumbnail = BitmapFrame.Create(target);

                //_____ Convert this thumbnail image to a memory stream ....
                byte[] targetBytes = null;
                using (MemoryStream memoryStream = new MemoryStream()) {
                    JpegBitmapEncoder targetEncoder = new JpegBitmapEncoder(); // PngBitmapEncoder();
                    targetEncoder.Frames.Add(thumbnail);
                    targetEncoder.Save(memoryStream);
                    targetBytes = memoryStream.ToArray();
                }

                photoStream.Flush();
                photoStream.Close();

                // Finally - write this file back out ....
                FileStream fs = new FileStream(dir + "Output_" + DateTimeInformation.GetCurrentDate("number")
                    + "_" + DateTimeInformation.GetCurrentTime() + ".jpg", FileMode.Create);

                fs.Write(targetBytes, 0, targetBytes.Length);
                fs.Flush();
                fs.Close();



                if (targetBytes.Length > 0) {
                    success = true;
                }

            } catch (Exception ex) {

                string temp = ex.ToString();

            }

            return success;
        }



        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void TimingTest() {

            int iterations = 30;
            bool doWrite = true;

            TimeSpan t1 = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);

            for (int i = 0; i < iterations; i++) {
                ImageProcessor.Test(doWrite);
            }

            TimeSpan t2 = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);

            for (int i = 0; i < iterations; i++) {
                ImageProcessorGDI.Test(doWrite);
            }

            TimeSpan t3 = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);

            TimeSpan diff1 = t2.Subtract(t1);
            TimeSpan diff2 = t3.Subtract(t2);

//            string temp = "";
        }


    }
}
