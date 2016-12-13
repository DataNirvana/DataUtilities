using System;
using System.IO;
using System.Collections;

using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;

using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
//using log4net;
//using ICSharpCode.SharpZipLib.Tar;


//-------------------------------------------------------------------------------------------------------------------------------
/**
            Name:       	SimpleIO
            Description:	Enables File input and output
            Type:				Extraction
            Author:			Edgar Scrase
            Date:				July 2003
            Version:			1.0

            Notes:
*/
namespace MGL.Data.DataUtilities
{

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Summary description for SimpleIO.
    /// </summary>
    public class SimpleIO
    {

        //#region Logger

        //protected ILog log = LogManager.GetLogger(typeof(SimpleIO));

        //#endregion


        private string className = "SimpleIO";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /**

        */
        public SimpleIO()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static bool DirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public static bool DirectoryCreate(string directoryPath)
        {
            if (DirectoryExists(directoryPath))
            {
                return true;
            }
            else
            {
                try
                {
                    DirectoryInfo tDir = new DirectoryInfo(directoryPath);

                    DirectoryInfo tCurrent = new DirectoryInfo(tDir.FullName);
                    List<DirectoryInfo> tChildren = new List<DirectoryInfo>();
                    while (tCurrent != null && !tCurrent.Exists)
                    {
                        tChildren.Add(tCurrent);
                        tCurrent = tCurrent.Parent;
                    }
                    if (tCurrent == null)
                    {
                        return false;
                    }
                    else
                    {
                        tChildren.Reverse();
                        foreach (DirectoryInfo tChild in tChildren)
                        {
                            tChild.Create();
                        }
                    }
                    return true;
                }
                catch(Exception ex)
                {
                    Logger.LogError(5, "Problem resulted in crash in SimpleIO.DirectoryCreate(): " + ex.ToString());
                    return false;
                }
            }
        }


        // This is also in utilities now as well.....
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string BuildList(List<string> list)
        {
            string csvList = "";

            if (list != null)
            {
                StringBuilder strBld = new StringBuilder();
                foreach (string id in list)
                {
                    strBld.Append("," + id);
                }
                if (strBld.Length > 0)
                {
                    strBld.Remove(0, 1);
                    csvList = strBld.ToString();
                }
            }
            return csvList;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string BuildList(List<int> list)
        {
            string csvList = "";

            if (list != null)
            {
                StringBuilder strBld = new StringBuilder();
                foreach (int id in list)
                {
                    strBld.Append("," + id);
                }
                if (strBld.Length > 0)
                {
                    strBld.Remove(0, 1);
                    csvList = strBld.ToString();
                }
            }
            return csvList;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string BuildList(List<long> list)
        {
            string csvList = "";

            if (list != null)
            {
                StringBuilder strBld = new StringBuilder();
                foreach (long id in list)
                {
                    strBld.Append("," + id);
                }
                if (strBld.Length > 0)
                {
                    strBld.Remove(0, 1);
                    csvList = strBld.ToString();
                }
            }
            return csvList;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string BuildList(ArrayList list, string listType)
        {
            string csvList = "";

            if (list != null)
            {
                if (listType.Equals("long"))
                {
                    StringBuilder strBld = new StringBuilder();
                    foreach (long id in list)
                    {
                        strBld.Append("," + id);
                    }
                    if (strBld.Length > 0)
                    {
                        strBld.Remove(0, 1);
                        csvList = strBld.ToString();
                    }
                }
                else if (listType.Equals("string"))
                {
                    StringBuilder strBld = new StringBuilder();
                    foreach (string item in list)
                    {
                        strBld.Append("," + item);
                    }
                    if (strBld.Length > 0)
                    {
                        strBld.Remove(0, 1);
                        csvList = strBld.ToString();
                    }
                }
            }

            return csvList;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string BuildList(string[] list)
        {
            string csvList = "";

            StringBuilder strBld = new StringBuilder();
            foreach (string item in list)
            {
                strBld.Append("," + item);
            }
            if (strBld.Length > 0)
            {
                strBld.Remove(0, 1);
                csvList = strBld.ToString();
            }


            return csvList;
        }



        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /**
            This is for memory streaming - not necessary right now
        */
        //		public byte[] Compress(string strInput) {
        //		public byte[] Compress(string filePath) {
        /*		public bool Compress(string sourceFilePath, string zipFile) {
                    try {
                        //////////////////////////////////////////
                        StreamReader sr = File.OpenText(filePath);

                        string strInput = sr.ReadToEnd();

                        /////////////////////////////////////////
                        byte[] bytData = System.Text.Encoding.UTF8.GetBytes(strInput);
                        //MessageBox.Show(bytData.Length.ToString());
                        MemoryStream ms = new MemoryStream();
                        Stream s        = new DeflaterOutputStream(ms);
                        s.Write(bytData, 0, bytData.Length);
                        s.Close();
                        byte[] compressedData = (byte[])ms.ToArray();
                        //				MessageBox.Show("Original: " +bytData.Length.ToString()+": " +"Compressed: " +compressedData.Length.ToString());

                        WriteToFile( zipFile, ref compressedData );
                        return true;
                    }
                    catch(Exception e) {
                        //				MessageBox.Show( e.ToString());
                        return false;
                    }

                }
        */

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Both file names should include the FULL path</summary>
        public bool Compress(string sourceFileName, string zipFileName)
        {
            bool success = false;

            try
            {
                string[] fileBits = sourceFileName.Split('/');
                string fileName = fileBits[fileBits.Length - 1];

                ZipOutputStream zos = new ZipOutputStream(File.Create(zipFileName));

                zos.SetLevel(5); // 0 - store only to 9 - means best compression

                FileStream fs = File.OpenRead(sourceFileName);

                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();

                ZipEntry entry = new ZipEntry(fileName);

                zos.PutNextEntry(entry);

                zos.Write(buffer, 0, buffer.Length);
                zos.Flush();

                zos.Finish();
                zos.Close();

                success = true;
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - Compress - " + ex.ToString(), LoggerHighlightType.Error, 5);
//                MGLErrorLog.LogError(className, "Compress", ex, LoggerErrorTypes.FileIO);
            }
            return success;
        }



        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool DeCompress(string sourceZipFile, string destinationFile)
        {
            bool success = false;

            try
            {
                ZipInputStream zos = new ZipInputStream(File.OpenRead(sourceZipFile));

                // just get the first one for now
                ZipEntry theEntry = zos.GetNextEntry();

                if (theEntry != null)
                {
                    int length = (int)theEntry.Size; // note that this limits us to 4gb by default

                    // need to get the data
                    byte[] data = new byte[length];
                    zos.Read(data, 0, length);

                    // write the data to the file
                    success = WriteToFile(destinationFile, ref data);
                }
                zos.Close();

            }
            catch (Exception ex)
            {
                Logger.Log(className + " - DeCompress - " + ex.ToString(), LoggerHighlightType.Error, 5);
//                MGLErrorLog.LogError(className, "DeCompress", ex, LoggerErrorTypes.FileIO);
            }

            return success;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Both file names should include the FULL path</summary>
        public bool CompressZip(string[] sourceFilePaths, string zipFileName) {
            bool success = false;

            try {
                ZipOutputStream zos = new ZipOutputStream(File.Create(zipFileName));
                zos.SetLevel(5); // 0 - store only to 9 - means best compression

                foreach (string sourceFilePath in sourceFilePaths) {

                    FileStream fs = File.OpenRead(sourceFilePath);

                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();

                    string fileName = GetFileName(sourceFilePath);
                    ZipEntry entry = new ZipEntry(fileName);

                    // 11-Feb-2016 - Make the date time of the entry in the zip file be the same as the source data
                    // This is clear to the user than the same date as the creation of the zip file
                    // in the future this could be a parameter of the method ...
                    entry.DateTime = File.GetLastWriteTime(sourceFilePath);

                    zos.PutNextEntry(entry);

                    /*
                     * * This does not really help reduce the VM as the data still eventually ends up on the zip file construct in memory
                    int offset = 0;
                    int blockSize = 4096;
                    if (buffer.Length < blockSize) {

                        zos.Write(buffer, 0, buffer.Length);

                    } else {

                        while (offset < buffer.Length - blockSize) {
                            zos.Write(buffer, offset, blockSize);
                            offset += blockSize;
                        }
                        // write the last little bit if there is any and reset the last offset, so that it is less than the length of the data ...
                        //    int remainder = row.Value.Length % blockSize;
                        int lastGoodPosition = ((offset - blockSize) > 0) ? (offset - blockSize) : 0;
                        zos.Write(buffer, lastGoodPosition, buffer.Length - lastGoodPosition);
                    }
                    */
                    zos.Write(buffer, 0, buffer.Length);
                    zos.Flush();

                }

                zos.Finish();
                zos.Close();

                success = true;
            } catch (Exception ex) {
                Logger.Log(className + " - Compress - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //                MGLErrorLog.LogError(className, "Compress", ex, LoggerErrorTypes.FileIO);
            }
            return success;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///     21-Jun-15 - create a zip file from a list of keyvalue pair filenames with the associated data blob for the file.
        ///     These are zipped up using memory streams and then converted into the overall byte array that is passed back to the calling program ..
        /// </summary>
        public bool CompressZip(List<KeyValuePair<string, byte[]>> data, out byte[] zipFileData) { // MemoryStream zipMS) {
            bool success = false;

            zipFileData = null;
            MemoryStream zipMS = new MemoryStream();
            ZipOutputStream zos = null;

            try {
                zos = new ZipOutputStream(zipMS);
                zos.SetLevel(5); // 0 - store only to 9 - means best compression

                foreach (KeyValuePair<string, byte[]> row in data) {

                    ZipEntry entry = new ZipEntry(row.Key);
                    zos.PutNextEntry(entry);

                    /*
                     * This does not really help reduce the VM as the data still eventually ends up on the zip file construct in memory
                    int offset = 0;
                    int blockSize = 4096;
                    if (row.Value.Length < blockSize) {

                        zos.Write(row.Value, 0, row.Value.Length);

                    } else {

                        while (offset < row.Value.Length - blockSize) {
                            zos.Write(row.Value, offset, blockSize);
                            offset += blockSize;
                        }
                    // write the last little bit if there is any and reset the last offset, so that it is less than the length of the data ...
                    //    int remainder = row.Value.Length % blockSize;
                        int lastGoodPosition = ((offset - blockSize) > 0) ? (offset - blockSize) : 0;
                        zos.Write(row.Value, lastGoodPosition, row.Value.Length - lastGoodPosition);
                    }
                     */
                    zos.Write(row.Value, 0, row.Value.Length);
                    zos.CloseEntry();
                }

                zos.Flush();
                // ensure that the ZOS does not own the memory stream and then close the ZOS object
                zos.IsStreamOwner = false;
                zos.Close();

                // get the byte array and then close the zip memory stream
                zipFileData = zipMS.ToArray();
                zipMS.Close();

                success = true;
            } catch (Exception ex) {
                Logger.LogError(6, className + " - CompressZip from memory stream.  Some kind of serious problem encountered ... - " + ex.ToString());
            } finally {
                // clean up!
                zipMS = null;
                zos = null;
            }
            return success;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool DeCompressZip(string sourceZipFile, string destinationDirectory)
        {
            bool success = false;

            try
            {
                ZipInputStream zos = new ZipInputStream(File.OpenRead(sourceZipFile));

                // just get the first one for now
                ZipEntry theEntry = zos.GetNextEntry();
                while (theEntry != null)
                {

                    int length = (int)theEntry.Size; // note that this limits us to 4gb by default

                    // need to get the data
                    byte[] data = new byte[length];
                    zos.Read(data, 0, length);

                    // write the data to the file
                    success = WriteToFile(destinationDirectory + theEntry.Name, ref data);

                    theEntry = zos.GetNextEntry();
                }
                zos.Close();

            }
            catch (Exception ex)
            {
                Logger.Log(className + " - DeCompress - " + ex.ToString(), LoggerHighlightType.Error, 5);
//                MGLErrorLog.LogError(className, "DeCompress", ex, LoggerErrorTypes.FileIO);
            }

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public List<string> ZipEntries(string sourceZipFile)
        {
            List<string> names = new List<string>();

            try
            {
                ZipInputStream zos = new ZipInputStream(File.OpenRead(sourceZipFile));

                // just get the first one for now
                ZipEntry theEntry = zos.GetNextEntry();
                while (theEntry != null)
                {
                    names.Add(theEntry.Name);
                    theEntry = zos.GetNextEntry();
                }
                zos.Close();
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - ZipEntries - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "ZipEntries", ex, LoggerErrorTypes.FileIO);
            }

            return names;
        }






        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /**
            compress as a GZip file
        */
        public bool GCompress(string sourceFilePath, string gzipFile)
        {
            bool success = false;

            try
            {

                char[] separators = { '/' };
                string[] fileBits = sourceFilePath.Split(separators);
                string fileName = fileBits[fileBits.Length - 1];

                GZipOutputStream gzos = new GZipOutputStream(File.Create(gzipFile));


                FileStream fs = File.OpenRead(sourceFilePath);

                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);

                gzos.Write(buffer, 0, buffer.Length);
                gzos.Flush();
                gzos.Finish();
                gzos.Close();
                fs.Close();


                success = true;
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - GCompress - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "GCompress", ex, LoggerErrorTypes.FileIO);
            }

            return success;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Source and Destination should include their respective paths</summary>
        public bool GCompress(string sourceFile, string destinationFile, bool eddiesFix)
        {
            bool success = false;

            try
            {

                GZipOutputStream gzos = new GZipOutputStream(File.Create(destinationFile));

                FileStream fs = File.OpenRead(sourceFile);

                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);

                gzos.Write(buffer, 0, buffer.Length);
                gzos.Flush();
                gzos.Finish();
                gzos.Close();
                fs.Close();


                success = true;
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - GCompress - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "GCompress", ex, LoggerErrorTypes.FileIO);
            }

            return success;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool GDeCompress(string sourceGZipFile, string destinationFile)
        {
            bool success = false;

            FileStream newFile = null;
            GZipInputStream gzis = null;
            try
            {
                //                gzis = new GZipInputStream(File.OpenRead(sourceGZipFile));

                // need to get the data
                byte[] data = new byte[2048];
                int offset = 2048;

                // read the data

                gzis = new GZipInputStream(File.OpenRead(sourceGZipFile));
                newFile = new FileStream(destinationFile, FileMode.Create);  // create the output file

                while (offset > 0)
                {

                    offset = gzis.Read(data, 0, offset);

                    // Write data to the file
                    newFile.Write(data, 0, offset);

                }

                gzis.Flush();
                newFile.Flush();

                success = true;
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - GDeCompress - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "GDeCompress", ex, LoggerErrorTypes.FileIO);
            }
            finally
            {
                try
                {
                    // Close files
                    newFile.Close();
                    gzis.Close();
                }
                catch (Exception)
                {
                }
            }

            return success;
        }

        /// <summary>
        /// Compress the folder and create a zip file.  If you need that output zip file to be on the same
        /// location, leave 'destinationFolder' empty.
        /// </summary>
        /// <param name="sourceFolderPath"></param>
        /// <returns></returns>


        public bool CompressFolder(string sourceFolderPath, string destinationFolder)
        {
            bool result = false;
            try
            {
                ICSharpCode.SharpZipLib.Zip.FastZip z = new ICSharpCode.SharpZipLib.Zip.FastZip();
                z.CreateEmptyDirectories = true;
                z.CreateZip(destinationFolder + ".zip", sourceFolderPath, true, "");

                result = File.Exists(destinationFolder + ".zip");

            }
            catch (Exception ex)
            {
                Logger.Log(className + " - Error whilst trying to compress the folder:" + ex.ToString(), LoggerHighlightType.Error, 5);
//                MGLErrorLog.LogError("Error whilst trying to compress the folder:" + ex);
            }

            return result;
        }

    //{


    //        bool success = false;
    //        string outPath = string.Empty;
    //        string zipTypePrefix = ".zip";
    //        ZipOutputStream oZipStream = null;
    //        try
    //        {
    //            ArrayList ar = GenerateFileList(sourceFolderPath); // generate file list
    //            int TrimLength = (Directory.GetParent(sourceFolderPath)).ToString().Length;

    //            // find number of chars to remove 	// from orginal file path

    //            if (destinationFolder == null || destinationFolder == "" || destinationFolder == string.Empty)
    //            {
    //                outPath = sourceFolderPath + zipTypePrefix;
    //            }
    //            else
    //            {
    //                if (ISDestFolderContainsZipExt(destinationFolder))
    //                {
    //                    outPath = destinationFolder;
    //                }
    //                else
    //                {
    //                    outPath = destinationFolder + zipTypePrefix;
    //                }
    //            }

    //            TrimLength += 1; //remove '\'
    //            FileStream ostream = null;
    //            byte[] obuffer;

    //            oZipStream = new ZipOutputStream(File.Create(outPath)); // create zip stream @"g:\newfile.zip"

    //            //Lets just use default compression
    //            //oZipStream.SetLevel(9); // maximum compression

    //            ZipEntry oZipEntry;
    //            foreach (string Fil in ar) // for each file, generate a zipentry
    //            {
    //                string fileName = Fil.Remove(0, TrimLength);

    //                 fileName = ZipEntry.CleanName(fileName);

    //                oZipEntry = new ZipEntry(fileName);

    //                if (Fil.EndsWith(@"/") || Fil.EndsWith(@"\")) // if a file ends with '/' its a directory
    //                {
    //                       oZipStream.PutNextEntry(oZipEntry);
    //                }
    //                else
    //                {

    //                    FileInfo fInfo = new FileInfo(Fil);

    //                    if (fInfo == null)
    //                    {
    //                        MGLErrorLog.LogError("Failed to open file: " + Fil + ", this will not be added to the zip file");
    //                    }
    //                    else
    //                    {
    //                        oZipEntry.Size = fInfo.Length;

    //                        oZipStream.PutNextEntry(oZipEntry);

    //                        try
    //                        {
    //                            ostream = File.OpenRead(Fil);
    //                            obuffer = new byte[ostream.Length];
    //                            ostream.Read(obuffer, 0, obuffer.Length);
    //                            oZipStream.Write(obuffer, 0, obuffer.Length);
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                            MGLErrorLog.LogError("Error whilst trying to compress folder " + sourceFolderPath + " to " + destinationFolder, ex);
    //                            MGLErrorLog.LogError(className, "GCompressFolder", ex, LoggerErrorTypes.FileIO);
    //                        }
    //                        finally
    //                        {
    //                            if (ostream != null)
    //                            {
    //                                ostream.Flush();
    //                                ostream.Close();
    //                            }
    //                        }
    //                    }
    //                }
    //            }

    //            success = true;
    //        }
    //        catch (Exception ex)
    //        {
    //            MGLErrorLog.LogError("Error whilst trying to compress folder " + sourceFolderPath + " to " + destinationFolder, ex);
    //            MGLErrorLog.LogError(className, "GCompressFolder", ex, LoggerErrorTypes.FileIO);
    //        }
    //        finally
    //        {
    //            if (oZipStream != null)
    //            {
    //                oZipStream.Finish();
    //                oZipStream.Close();
    //            }
    //        }

    //        return success;
    //    }


        private static ArrayList GenerateFileList(string Dir)
        {
            ArrayList fils = new ArrayList();
            bool Empty = true;
            foreach (string file in Directory.GetFiles(Dir)) // add each file in directory
            {
                fils.Add(file);
                Empty = false;
            }

            if (Empty)
            {
                if (Directory.GetDirectories(Dir).Length == 0)
                // if directory is completely empty, add it
                {
                    fils.Add(Dir + @"/");
                }
            }

            foreach (string dirs in Directory.GetDirectories(Dir)) // recursive
            {
                foreach (object obj in GenerateFileList(dirs))
                {
                    fils.Add(obj);
                }
            }
            return fils; // return file list
        }

        private bool ISDestFolderContainsZipExt(string destinationFolder)
        {
            bool isExtGiven = false;
            string suffix = string.Empty;
            string ext = ".zip";
            try
            {
                string[] splitStrings = destinationFolder.Split('.');
                if (splitStrings != null && splitStrings.Length > 0)
                {
                    suffix = splitStrings[splitStrings.Length - 1];
                    if (suffix != null || suffix != "")
                    {
                        if (suffix.ToLower() == ext)
                        {
                            isExtGiven = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - ISDestFolderContainsZipExt - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "ISDestFolderContainsZipExt", ex, LoggerErrorTypes.NoData);
            }
            return isExtGiven;
        }





        // This is all a bit dodgy ...
        //////////////-----------------------------------------------------------------------------------------------------------------------------------------------------------
        ////////////public bool TarArchive(string tarFileName, string[] filePaths) {
        ////////////    bool success = false;

        ////////////    try {
        ////////////        TarOutputStream tos = new TarOutputStream(File.Create(tarFileName));

        ////////////        if (filePaths != null && filePaths.Length > 0) {

        ////////////            foreach (string filePath in filePaths) {
        ////////////                FileStream fs = File.OpenRead(filePath);
        ////////////                byte[] buffer = new byte[fs.Length];
        ////////////                fs.Read(buffer, 0, buffer.Length);
        ////////////                fs.Close();

        ////////////                TarEntry entry = TarEntry.CreateEntryFromFile(filePath);
        ////////////                //                        entry.Name = GetFileName(filePath);
        ////////////                tos.PutNextEntry(entry);

        ////////////                tos.Write(buffer, 0, buffer.Length);
        ////////////                tos.Flush();
        ////////////            }
        ////////////            tos.Finish();
        ////////////            tos.Close();

        ////////////            success = true;
        ////////////        }

        ////////////    } catch (Exception ex) {
        ////////////        MGLErrorLog.LogError(className, "TarArchive", ex, LoggerErrorTypes.FileIO);
        ////////////    }

        ////////////    return success;
        ////////////}


        //////////////-----------------------------------------------------------------------------------------------------------------------------------------------------------
        ////////////public bool TarUnArchive(string tarFilePath) {
        ////////////    bool success = false;

        ////////////    //            string tempDir = Environment.CurrentDirectory;

        ////////////    try {
        ////////////        //                Environment.CurrentDirectory;

        ////////////        // get the file name and create the sub directory
        ////////////        string destinationDirectory = tarFilePath.Split('.')[0] + "/";
        ////////////        CreateDirectory(destinationDirectory);

        ////////////        TarInputStream tos = new TarInputStream(File.OpenRead(tarFilePath));


        ////////////        TarEntry te = tos.GetNextEntry();
        ////////////        while (te != null) {
        ////////////            if (te != null) {

        ////////////                int length = (int)te.Size; // note that this limits us to 4gb by default

        ////////////                // need to get the data
        ////////////                //int byteBlock = 2048;
        ////////////                int count = 2048;
        ////////////                byte[] data = new byte[length];

        ////////////                tos.Read(data, 0, length);

        ////////////                success = WriteToFile(destinationDirectory + GetFileName(te.Name), ref data);
        ////////////            }
        ////////////            te = tos.GetNextEntry();
        ////////////        }
        ////////////        tos.Close();

        ////////////    } catch (Exception ex) {
        ////////////        MGLErrorLog.LogError(className, "DeCompress", ex, LoggerErrorTypes.FileIO);
        ////////////    } finally {
        ////////////        //                Environment.CurrentDirectory = tempDir;
        ////////////    }
        ////////////    return success;
        ////////////}



        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public List<string> ReadFromFile(string fileName) {
            return ReadFromFile(fileName, null);
        }
            //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public List<string> ReadFromFile(string fileName, Encoding encoding)
        {
            List<string> lines = new List<string>();

            StreamReader sr = null;
            try {
                if (encoding == null) {
                    sr = new StreamReader(fileName);
                } else {
                    sr = new StreamReader(fileName, encoding);
                }

                string line = "";
                while (line != null) {

                    line = sr.ReadLine();

                    if (line != null) {
                        lines.Add(line);
                    }

                }

            } catch (Exception ex) {
                Logger.Log(className + " - ReadFromFile - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "ReadFromFile", ex, LoggerErrorTypes.FileIO);
            } finally {
                if (sr != null) {
                    sr.Close();
                }
            }

            return lines;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public int NumberOfLines(string fileName)
        {
            int count = 0;

            try
            {
                count = ReadFromFile(fileName).Count;
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - NumberOfLines - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "NumberOfLines", ex, LoggerErrorTypes.FileIO);
            }

            return count;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /**

        */
        public FileStream GetFileStream(string strPath, bool appendData)
        {
            FileMode fileMode = FileMode.Create;
            if (File.Exists(strPath) == false)
            {
                fileMode = FileMode.CreateNew;
            }
            else if (appendData == true)
            {
                fileMode = FileMode.Append;
            }
            else
            {
                fileMode = FileMode.Truncate;
            }

            return new FileStream(strPath, fileMode);
        }



        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool WriteToFile(string strPath, ref byte[] buffer)
        {
            return WriteToFile(strPath, ref buffer, false);
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool WriteToFile(string strPath, ref byte[] buffer, bool appendData)
        {
            bool success = false;

            try
            {
                FileStream newFile = GetFileStream(strPath, appendData);

                // Write data to the file
                newFile.Write(buffer, 0, buffer.Length);

                // Close file
                newFile.Close();
                success = true;
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - WriteToFile - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "WriteToFile", ex, LoggerErrorTypes.FileIO);
            }

            return success;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool WriteToFile(string strPath, ref ArrayList data)
        {
            return WriteToFile(strPath, ref data, false);
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool WriteToFile(string strPath, ref ArrayList data, bool appendData)
        {
            bool success = false;

            try
            {
                FileStream newFile = GetFileStream(strPath, appendData);

                StreamWriter sw = new StreamWriter(newFile);

                // Write data to the file
                foreach (string temp in data)
                {
                    sw.WriteLine(temp);
                }

                // Close file
                sw.Close();
                newFile.Close();
                success = true;
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - WriteToFile - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "WriteToFile", ex, LoggerErrorTypes.FileIO);
            }

            return success;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool WriteToFile(string strPath, List<string> data, bool appendData)
        {
            bool success = false;

            try
            {
                FileStream newFile = GetFileStream(strPath, appendData);

                StreamWriter sw = new StreamWriter(newFile);

                // Write data to the file
                foreach (string temp in data)
                {
                    sw.WriteLine(temp);
                }

                // Close file
                sw.Close();
                newFile.Close();
                success = true;
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - WriteToFile - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "WriteToFile", ex, LoggerErrorTypes.FileIO);
            }

            return success;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool WriteToFile(string strPath, ref string[] data)
        {
            return WriteToFile(strPath, ref data, false);
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool WriteToFile(string strPath, ref string[] data, bool appendData)
        {
            bool success = false;

            FileStream newFile = null;
            StreamWriter sw = null;

            try
            {
                newFile = GetFileStream(strPath, appendData);

                sw = new StreamWriter(newFile);

                // Write data to the file
                foreach (string temp in data)
                {
                    sw.WriteLine(temp);
                }

                // Close file
                sw.Close();
                newFile.Close();
                success = true;
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - WriteToFile - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "WriteToFile", ex, LoggerErrorTypes.FileIO);
            }
            finally
            {
                if (newFile != null)
                {
                    newFile.Close();
                }
                if (sw != null)
                {
                    sw.Close();
                }
            }

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool WriteToFile(string strPath, string line, bool appendData)
        {
            //            bool success = false;

            string[] temp = new string[1];
            temp[0] = line;

            return WriteToFile(strPath, ref temp, appendData);
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Writes the line as is ...</summary>
        public bool WriteToFile(string strPath, List<byte> byteList)
        {
            bool success = false;

            try
            {
                FileStream newFile = GetFileStream(strPath, false);

                //                line.T


                byte[] bytes = new byte[byteList.Count];
                int count = 0;
                foreach (byte b in byteList)
                {
                    bytes[count] = b;
                    count++;
                }

                newFile.Write(bytes, 0, bytes.Length);
                //                StreamWriter sw = new StreamWriter(newFile);
                //                sw.Write(line);

                // Close file
                //                sw.Close();
                newFile.Close();
                success = true;
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - WriteToFile - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "WriteToFile", ex, LoggerErrorTypes.FileIO);
            }

            return success;
        }




        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Recursively deletes the given directory and all of the files and directoryies within it</summary>
        public void DeleteDirectory(string dirName)
        {
            try
            {
                Directory.Delete(dirName, true); // deletes recursively
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - DeleteDirectory - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "DeleteDirectory", ex, LoggerErrorTypes.FileIO);
            }

        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Opens a stream writer for the given path</summary>
        public StreamWriter OpenFileToWrite(string strPath, bool appendData)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(strPath, appendData);
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - OpenFileToWrite - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "OpenFileToWrite", ex, LoggerErrorTypes.FileIO);
            }
            return sw;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Closes the given stream writer</summary>
        public bool CloseFileAfterWriting(StreamWriter sw)
        {
            bool success = false;

            try
            {
                sw.Flush();
                sw.Close();
                success = true;
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - CloseFileAfterWriting - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "CloseFileAfterWriting", ex, LoggerErrorTypes.FileIO);
            }
            return success;
        }




        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public void DeleteFile(string sourceFileName)
        {

            try
            {
                File.Delete(sourceFileName);
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - DeleteFile - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "DeleteFile", ex, LoggerErrorTypes.FileIO);
            }

        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public void CopyFile(string sourceFileName, string destFileName)
        {
            try
            {
                File.Copy(sourceFileName, destFileName, true);
            }
            catch (Exception ex)
            {
                Logger.Log(className + " - CopyFile - " + ex.ToString(), LoggerHighlightType.Error, 5);
                //MGLErrorLog.LogError(className, "CopyFile", ex, LoggerErrorTypes.FileIO);
            }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string GenerateFileName(string strPath, string name)
        {
            if (strPath != null && strPath.EndsWith("/") == true)
            {
                return strPath + name;
            }
            else
            {
                return strPath + "/" + name;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool CreateDirectory(string path)
        {
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            if (Directory.Exists(path) == true)
            {
                return true;
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string[] ListFileNames(string path)
        {
            if (Directory.Exists(path) == true)
            {
                return Directory.GetFiles(path);
            }
            return null;
        }
        public string[] ListFileNames(string path, bool isListSubDirs)
        {
            if (Directory.Exists(path) == true)
            {
                return ListFileNames(path, "*", isListSubDirs);
            }

            return null;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public string[] ListFileNames(string path, string searchPattern)
        {

            if (Directory.Exists(path) == true)
            {
                return ListFileNames(path, searchPattern, false);
            }

            return null;
        }
        public string[] ListFileNames(string path, string searchPattern, bool isListSubDirs)
        {
            if (Directory.Exists(path) == true)
            {
                if (isListSubDirs)
                    return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
                else
                    return Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
            }

            return null;
        }



        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /**

        */
        public string GetFileName(string filePath)
        {

            string fileName = null;

            if (filePath != null)
            {

                // split on \ and / and return the last bit in the sequence
                try
                {
                    string[] temp = filePath.Split('\\', '/');
                    fileName = temp[temp.Length - 1];
                }
                catch { }
            }

            return fileName;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public long GetDirectoryDataVolume(string sourceDir)
        {
            return DirectoryDataVolume(sourceDir);
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static long DirectoryDataVolume(string sourceDir)
        {
            long dSize = 0;
            if (Directory.Exists(sourceDir))
            {

                try
                {
                    long size = 0;

                    try
                    {
                        FileInfo[] files = (new DirectoryInfo(sourceDir)).GetFiles();
                        if (files != null)
                        {
                            foreach (FileInfo file in files)
                            {
                                size += file.Length;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(5, "Problem resulted in crash in SimpleIO.DirectoryDataVolume(1): " + ex.ToString());
                    }

                    dSize = size;

                    DirectoryInfo[] subFolders = (new DirectoryInfo(sourceDir)).GetDirectories();
                    if (subFolders != null)
                    {
                        foreach (DirectoryInfo subFolder in subFolders)
                        {
                            dSize += DirectoryDataVolume(subFolder.FullName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(5, "Problem resulted in crash in SimpleIO.DirectoryDataVolume(2): " + ex.ToString());
                }
            }
            return dSize;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string GetFileExtension(string filePath)
        {
            string ext = "*";

            try
            {
                // split on the full stop
                string[] bits = filePath.Split('.');
                if (bits.Length > 1)
                {
                    ext = bits[bits.Length - 1];
                    ext = ext.ToLower();
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(5, "SimpleIO.GetFileExtension crashed when access was attempted: " + ex.ToString());
            }
            return ext;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string BuildFileFilter(string extension)
        {
            string filter = "";

            if (extension != null && extension != "")
            {
                filter = "All " + extension + " files (*." + extension + ")|*." + extension;
            }

            return filter;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool AppendFile(string masterFilePath, string filePathToBeAppended, bool ignoreFirstLineOfSecondFile)
        {
            bool success = false;
            if (masterFilePath != null && filePathToBeAppended != null && SimpleIO.FileExists(masterFilePath) && SimpleIO.FileExists(filePathToBeAppended))
            {

                try
                {
                    StreamReader sr = new StreamReader(filePathToBeAppended);

                    string line = "";
                    int lineCount = 0;
                    while (line != null)
                    {

                        line = sr.ReadLine();

                        if (line != null && (ignoreFirstLineOfSecondFile == false || (ignoreFirstLineOfSecondFile && lineCount > 0)))
                        {
                            line = line.Replace("\\N", "\"\"");
                            WriteToFile(masterFilePath, line, true);
                        }
                        lineCount++;
                    }

                    sr.Close();
                    success = true;
                }
                catch (Exception ex)
                {
                    Logger.Log(className + " - AppendFile - " + ex.ToString(), LoggerHighlightType.Error, 5);
                    //MGLErrorLog.LogError(className, "AppendFile", ex, LoggerErrorTypes.FileIO);
                }
            }
            return success;
        }



        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Completetly overwrite the file with exactly the same volume of data</summary>
        public bool WipeFile(string filePath)
        {
            bool success = false;

            if (filePath != null && FileExists(filePath))
            {
                List<string> lines = ReadFromFile(filePath);
                if (lines != null && lines.Count > 0)
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        StringBuilder tempLine = new StringBuilder();
                        int ll = 0;
                        while (ll < lines[i].Length)
                        {
                            tempLine.Append("x");
                            ll++;
                        }
                        lines[i] = tempLine.ToString();
                    }
                    success = WriteToFile(filePath, lines, false);
                }
            }

            return success;
        }

        public bool ChangeFolderName(string rootPath, string srcFolderName, string destFolderName)
        {
            bool isChanged = false;
            string sourceFolderFullPath = string.Empty;
            string destFullPath = string.Empty;
            try
            {
                if (rootPath == null || rootPath == string.Empty)
                {
                    Logger.Log("Root path is not provided. Quitting!", LoggerHighlightType.Error, 5);
                    //MGLErrorLog.LogError("Root path is not provided. Quitting!");
                    return false;
                }
                if (srcFolderName == null || srcFolderName == string.Empty)
                {
                    Logger.Log("Source folder name is not provided. Quitting!", LoggerHighlightType.Error, 5);
//                    MGLErrorLog.LogError("Source folder name is not provided. Quitting!");
                    return false;
                }
                if (destFolderName == null || destFolderName == string.Empty)
                {
                    Logger.Log("Destination folder name is not provided. Quitting!", LoggerHighlightType.Error, 5);
//                    MGLErrorLog.LogError("Destination folder name is not provided. Quitting!");
                    return false;
                }
                if (!DirectoryExists(rootPath))
                   {
                       Logger.Log("The directory: " + rootPath + " does not exist. Quitting!", LoggerHighlightType.Error, 5);
//                    MGLErrorLog.LogError("The directory: " + rootPath + " does not exist. Quitting!");
                    return false;
                }
                sourceFolderFullPath = rootPath + srcFolderName;
                if (!DirectoryExists(sourceFolderFullPath))
                {
                    Logger.Log("The source directory: " + sourceFolderFullPath + " does not exist. Quitting!", LoggerHighlightType.Error, 5);
//                    MGLErrorLog.LogError("The source directory: " + sourceFolderFullPath + " does not exist. Quitting!");
                    return false;
                }
                destFullPath = rootPath + destFolderName;
                if (DirectoryExists(destFullPath))
                {
                    Logger.Log("The destination directory: " + destFullPath + " already exists. Quitting!", LoggerHighlightType.Error, 5);
//                    MGLErrorLog.LogError("The destination directory: " + destFullPath + " already exists. Quitting!");
                    return false;
                }
                Directory.Move(sourceFolderFullPath, destFullPath);

                if (DirectoryExists(sourceFolderFullPath) || !DirectoryExists(destFullPath))
                {
                    Logger.Log("Failed to rename the directory.", LoggerHighlightType.Error, 5);
//                    MGLErrorLog.LogError("Failed to rename the directory.");
                    isChanged = false;
                }
                else
                {
                    isChanged = true;

                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error changing folder name at: " + ":" + ex, LoggerHighlightType.Error, 5);
//                MGLErrorLog.LogError("Error changing folder name at: " + ":" + ex);
                isChanged = false;
            }
            return isChanged;
        }



        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string GetDirectoryFromFileName(string fileName) {
            string dir = "";

            if (fileName != null && fileName != "") {
                string[] bits = fileName.Split(new string[] { "\\", "/" }, StringSplitOptions.None);
                dir = fileName.Replace(bits[bits.Length - 1], "");
                dir = dir.Replace("/", "\\");
                if (dir.EndsWith("\\") == false) {
                    dir = dir + "\\";
                }
            }
            return dir;
        }



        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string GetFileNameFromFilePath(string fileName, bool removeSuffix) {
            string tempName = "";

            if (fileName != null && fileName != "") {
                string[] bits = fileName.Split(new string[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries);
                tempName = bits[bits.Length - 1];

                if (removeSuffix) {
                    bits = tempName.Split('.');
                    if (bits.Length > 1) {
                        tempName = bits[0];
                    }
                }
            }

            return tempName;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string GetWindowsCompliantDirectoryName(string fileName) {

            if (fileName != null && fileName != "") {
                fileName = fileName.Replace("/", "\\");
            }

            if (fileName.EndsWith("\\") == false && fileName.EndsWith("/") == false) {
                fileName = fileName + "\\";
            }

            return fileName;
        }





    } // End of Class
}
