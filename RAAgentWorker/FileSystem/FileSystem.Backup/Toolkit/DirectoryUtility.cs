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
    using System.Diagnostics;
    using System.IO;
    #endregion

    public static class DirectoryUtility
    {
        public static String GetRelativePath(String fileName, String directory)
        {
            Debug.Assert(fileName.StartsWith(directory, StringComparison.OrdinalIgnoreCase), "directory not a prefix");

            int directoryEnd = directory.Length;
            if (directoryEnd == 0)
                return fileName;
            while (directoryEnd < fileName.Length && fileName[directoryEnd] == '\\')
                directoryEnd++;
            string relativePath = fileName.Substring(directoryEnd);
            return relativePath;
        }

        /// <summary>
        /// SafeCopy sourceDirectory to directoryToVersion recursively. The target directory does
        /// no need to exist
        /// </summary>
        public static void Copy(String sourceDirectory, String targetDirectory)
        {
            Copy(sourceDirectory, targetDirectory, SearchOption.AllDirectories);
        }

        /// <summary>
        /// SafeCopy all files from sourceDirectory to directoryToVersion.  If searchOptions == AllDirectories
        /// then the copy is recursive, otherwise it is just one level.  The target directory does not
        /// need to exist. 
        /// </summary>
        public static void Copy(String sourceDirectory, String targetDirectory, SearchOption searchOptions)
        {
            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            foreach (string sourceFile in Directory.GetFiles(sourceDirectory))
            {
                var targetFile = Path.Combine(targetDirectory, Path.GetFileName(sourceFile));
                FileUtility.ForceCopy(sourceFile, targetFile);
            }
            if (searchOptions == SearchOption.AllDirectories)
            {
                foreach (var sourceDir in Directory.GetDirectories(sourceDirectory))
                {
                    var targetDir = Path.Combine(targetDirectory, Path.GetFileName(sourceDir));
                    Copy(sourceDir, targetDir, searchOptions);
                }
            }
        }

        ///// <summary>
        ///// Clean is sort of a 'safe' recursive delete of a directory.  It either deletes the
        ///// files or moves them to '*.deleting' names.  It deletes directories that are completely
        ///// empty.  Thus it will do a recursive delete when that is possible.  There will only 
        ///// be *.deleting files after this returns.  It returns the number of files and directories
        ///// that could not be deleted.  
        ///// </summary>
        //public static int Clean(string directory)
        //{
        //    var result = 0;
        //    if (Directory.Exists(directory))
        //    {
        //        foreach (var file in Directory.GetFiles(directory))
        //        {
        //            if (!FileUtility.ForceDelete(file))
        //                result++;
        //        }
        //        foreach (var subDir in Directory.GetDirectories(directory))
        //            result += Clean(subDir);
        //        if (result == 0)
        //        {
        //            try
        //            {
        //                Directory.Delete(directory, true);
        //            }
        //            catch { result++; }
        //        }
        //        else result++;
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// Removes the oldest directories directly under 'directoryPath' so that 
        ///// only 'numberToKeep' are left. 
        ///// </summary>
        ///// <param variable="directoryPath">Directory to removed old files from.</param>
        ///// <param variable="numberToKeep">The number of files to keep.</param>
        ///// <returns> true if there were no errors deleting files</returns>
        //public static Boolean DeleteOldest(String directoryPath, Int32 numberToKeep)
        //{
        //    if (!Directory.Exists(directoryPath))
        //        return true;

        //    var dirs = Directory.GetDirectories(directoryPath);
        //    int numToDelete = dirs.Length - numberToKeep;
        //    if (numToDelete <= 0)
        //        return true;

        //    Array.Sort<string>(dirs, delegate(string x, string y)
        //    {
        //        return File.GetLastWriteTimeUtc(x).CompareTo(File.GetLastWriteTimeUtc(y));
        //    });

        //    var result = true;
        //    for (int i = 0; i < numToDelete; i++)
        //    {
        //        try
        //        {
        //            Directory.Delete(dirs[i]);
        //        }
        //        catch (Exception)
        //        {
        //            // TODO trace message;
        //            result = false;
        //        }
        //    }
        //    return result;
        //}

        /// <summary>
        /// DirectoryUtility.GetFiles is basicaly the same as Directory.GetFiles 
        /// however it returns IEnumerator, which means that it lazy.  This is very important 
        /// for large directory trees.  A searchPattern can be specified (Windows wildcard conventions)
        /// that can be used to filter the set of archiveFile names returned. 
        /// 
        /// Suggested Usage
        /// 
        ///     foreach(string fileName in DirectoryUtilities.GetFiles("c:\", "*.txt")){
        ///         Console.WriteLine(fileName);
        ///     }
        ///
        /// </summary>
        /// <param variable="directoryPath">The base directory to enumerate</param>
        /// <param variable="searchPattern">A pattern to filter the names (windows filename wildcards * ?)</param>
        /// <param variable="searchOptions">Indicate if the search is recursive or not.  </param>
        /// <returns>The enumerator for all archiveFile names in the directory (recursively). </returns>
        public static IEnumerable<String> GetFiles(String directoryPath, String searchPattern, SearchOption searchOptions)
        {

            var fileNames = Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
            Array.Sort<String>(fileNames, StringComparer.OrdinalIgnoreCase);
            foreach (var fileName in fileNames)
            {
                yield return fileName;
            }

            if (searchOptions == SearchOption.AllDirectories)
            {
                var subDirNames = Directory.GetDirectories(directoryPath);
                Array.Sort<String>(subDirNames);
                foreach (var subDir in subDirNames)
                {
                    foreach (var fileName in DirectoryUtility.GetFiles(subDir, searchPattern, searchOptions))
                        yield return fileName;
                }
            }
        }
        public static IEnumerable<String> GetFiles(String directoryName, String searchPattern)
        {
            return GetFiles(directoryName, searchPattern, SearchOption.TopDirectoryOnly);
        }
        public static IEnumerable<String> GetFiles(String directoryName)
        {
            return GetFiles(directoryName, "*");
        }
    }
}
