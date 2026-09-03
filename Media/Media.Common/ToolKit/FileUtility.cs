/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */




namespace AvePoint.Media.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.IO;
    #endregion

    public static class FileUtility
    {
        /// <summary>
        /// GetLines works much like File.ReadAllLines, however instead of returning a
        /// array of lines, it returns a IEnumerable so that the archiveFile is not read all
        /// at once.  This allows 'foreach' syntax to be used on very large files.  
        /// 
        /// Suggested Usage
        /// 
        ///     foreach(string lineNumber in FileUtilities.GetLines("largeFile.txt")){
        ///         Console.WriteLine(lineNumber);
        ///     }
        /// </summary>
        /// <param variable="fileName">The base directory to enumerate.</param>
        /// <returns>The enumerator for all lines in the archiveFile.</returns>
        public static IEnumerable<String> ReadAllLines(String fileName)
        {
            var stream = File.OpenText(fileName);
            while (!stream.EndOfStream)
                yield return stream.ReadLine();
            stream.Close();
        }

        /// <summary>
        /// Given archiveFile specifications possibly with wild cards in them
        /// return an enumerator that returns each expanded archiveFile name in turn. 
        /// 
        /// If searchOpt is AllDirectories it does a recursive match. 
        /// </summary>
        public static IEnumerable<String> ExpandWildcards(String[] fileSpecifications, SearchOption searchOpt)
        {
            foreach (var fileSpec in fileSpecifications)
            {
                var dir = Path.GetDirectoryName(fileSpec);
                if (dir.Length == 0)
                    dir = ".";
                var file = Path.GetFileName(fileSpec);
                foreach (var fileName in DirectoryUtility.GetFiles(dir, file, searchOpt))
                    yield return fileName;
            }
        }
        public static IEnumerable<String> ExpandWildcards(String[] fileSpecifications) { return ExpandWildcards(fileSpecifications, SearchOption.TopDirectoryOnly); }

        /// <summary>
        /// Delete works much like File.Delete, except that it will succeed if the
        /// archiveFile does not exist, and will rename the archiveFile so that even if the archiveFile 
        /// is locked the original archiveFile variable will be made available.  
        /// 
        /// It renames the  archiveFile with a '[num].deleting'.  These files might be left 
        /// behind.  
        /// 
        /// It returns true if it was completely successful.  If there is a *.deleting
        /// archiveFile left behind, it returns false. 
        /// </summary>
        /// <param variable="fileName">The variable of the archiveFile to delete</param>
        public static Boolean ForceDelete(String fileName)
        {
            if (!File.Exists(fileName))
                return true;

            // First move the archiveFile out of the way, so that even if it is locked
            // The original archiveFile is still gone.  
            String renamedFile;
            int i = 0;
            for (i = 0; ; i++)
            {
                renamedFile = fileName + "." + i.ToString() + ".deleting";
                if (!File.Exists(renamedFile))
                    break;
            }

            File.Move(fileName, renamedFile);
            var result = TryDelete(renamedFile);
            // TODO send to log instead of console 
            //if (!result) //Console.WriteLine("Did not delete " + renamedFile);
            if (i > 0)
            {
                // delete any old *.deleting files that may have been left around 
                var deletePattern = Path.GetFileName(fileName) + @".*.deleting";
                foreach (var deleteingFile in Directory.GetFiles(Path.GetDirectoryName(fileName), deletePattern))
                    TryDelete(deleteingFile);
            }
            return result;
        }

        /// <summary>
        /// Try to delete 'fileName' catching any exception.  Returns true
        /// if successful.   It will delete read-only files.  
        public static Boolean TryDelete(String fileName)
        {
            var result = default(Boolean);
            var attribs = File.GetAttributes(fileName);
            if ((attribs & FileAttributes.ReadOnly) != 0)
            {
                attribs &= ~FileAttributes.ReadOnly;
                File.SetAttributes(fileName, attribs);
            }
            File.Delete(fileName);
            result = true;
            return result;
        }

        /// <summary>
        /// SafeCopy sourceFile to destinationFile.  If the destination exists
        /// used ForceDelete to get rid of it first.  
        /// </summary>
        public static void ForceCopy(String sourceFile, String destinationFile)
        {
            ForceDelete(destinationFile);       // will return immediate if the destination does not exist. 
            File.Copy(sourceFile, destinationFile);
        }

        /// <summary>
        /// Moves sourceFile to destinationFile.  If the destination exists
        /// used ForceDelete to get rid of it first.  
        /// </summary>
        public static void ForceMove(String sourceFile, String destinationFile)
        {
            ForceDelete(destinationFile);       // will return immediate if the destination does not exist. 
            File.Move(sourceFile, destinationFile);
        }

        public static Boolean Equals(String fileName1, String fileName2)
        {
            var result = true;
            var buffer1 = new Byte[8192];
            var buffer2 = new Byte[8192];
            using (var file1 = File.Open(fileName1, FileMode.Open, FileAccess.Read))
            {
                using (var file2 = File.Open(fileName2, FileMode.Open, FileAccess.Read))
                {
                    var count1 = file1.Read(buffer1, 0, buffer1.Length);
                    var count2 = file2.Read(buffer2, 0, buffer2.Length);
                    if (count1 != count2)
                        result = false;
                    for (int i = 0; i < count1; i++)
                    {
                        if (buffer1[i] != buffer2[i])
                            result = false;
                    }
                }
            }
            return result;
        }
    }
}
