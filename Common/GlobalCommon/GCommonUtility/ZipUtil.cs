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




namespace AvePoint.GCommon
{
    #region using directives
    using ICSharpCode.SharpZipLib.Zip;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    #endregion

    /**
     *  Ionic zip default encoding is IBM- code page 437, which defined in zip specification,
     *  although the zip format support the entry name and comment in Unicode, but default the
     *  code page 437 does not support it, also, the windows shell zip utility, which known as
     *  a windows feature named "Compressed Folder", is not compatible for zip specification,
     *  that is, the windows Compressed Folder is not fully implemented by MicroSoft corporation.
     *  as this uri http://commons.apache.org/compress/zip.html describes:
     *
     *    Windows' "compressed folder" feature doesn't recognize any flag or extra field and
     *    creates archives using the platforms default encoding - and expects archives to be
     *    in that encoding when reading them.
     *
     *  wiki page: http://en.wikipedia.org/wiki/ZIP_(file_format) :
     *
     *  Versions of Microsoft Windows have included support for zip compression in Explorer
     *  since the Plus! pack was released for Windows 98.[41] Microsoft calls this feature
     *  "Compressed Folders". Not all zip features are supported by the Windows Compressed
     *  Folders capability. For example, AES Encryption, split or spanned archives, and Unicode
     *  entry encoding are not known to be readable or writable by the Compressed Folders
     *  feature in Windows XP or Windows Vista.
     *
     *  ZIP specification: http://www.pkware.com/documents/casestudies/APPNOTE.TXT
     */

    public class ZipUtil
    {
        public static void ZipFolder(
            String folderPath,
            String outputZipFile)
        {
            System.IO.Compression.ZipFile.CreateFromDirectory(folderPath, outputZipFile);
        }

        public static void ZipFolder(
            String folderPath,
            String outputZipFile,
            String password,
            Encoding encoding)
        {
            using (FileStream zipFile = File.Create(outputZipFile))
            {
                using (ZipOutputStream zip = new ZipOutputStream(zipFile))
                {
                    byte[] buffer = new byte[4096];
                    zip.ZipCryptoEncoding = encoding ?? Encoding.Default;
                    zip.UseZip64 = UseZip64.Dynamic;
                    zip.Password = password;
                    string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        if (file.Equals(outputZipFile))
                        {
                            continue;
                        }
                        string relativePath = Path.GetRelativePath(folderPath, file);
                        ZipEntry entry = new ZipEntry(relativePath);
                        zip.PutNextEntry(entry);

                        using (FileStream fileStream = File.OpenRead(file))
                        {
                            int bytesRead;
                            do
                            {
                                bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                                zip.Write(buffer, 0, bytesRead);
                            } while (bytesRead > 0);
                        }
                    }
                    zip.Finish();
                    zip.Close();
                }
            }
        }

        public static void ZipFolder(
            String folderPath,
            String outputZipFile,
            Encoding encoding)
        {
            System.IO.Compression.ZipFile.CreateFromDirectory(folderPath, outputZipFile, CompressionLevel.Optimal, false,encoding);
        }


        /// <summary>
        /// 将zip压缩包内的所有file解压到basefolder中
        /// </summary>
        /// <param name="filePath">zip文件的路径</param>
        /// <param name="baseFolder">解压到的folder路径</param>
        public static void UnZipFile(string filePath, string baseFolder)
        {
            System.IO.Compression.ZipFile.ExtractToDirectory(filePath, baseFolder,true);
        }
        public static void ZipFolderForLongPath(
    String folderPath,
    String outputZipFile,
    String password,
    Encoding encoding)
        {
            using (ZipOutputStream zip = new ZipOutputStream(File.Create(outputZipFile)))
            {
                zip.ZipCryptoEncoding = encoding ?? Encoding.Default;
                zip.UseZip64 = UseZip64.Dynamic;
                if (!string.IsNullOrEmpty(password))
                {
                    zip.Password = password;
                }
                ZipFolderV1(new DirectoryInfo(folderPath).Name, folderPath, zip, outputZipFile);
                zip.Finish();
                zip.Close();
            }
        }
        public static void ZipFolderV1(String entryPath, String topDirFullPath, ZipOutputStream zipFile,string zipFilePath)
        {
            foreach (var dirInfo in new DirectoryInfo(topDirFullPath).GetDirectories())
            {
                ZipFolderV1(entryPath + "/" + dirInfo.Name, dirInfo.FullName, zipFile, zipFilePath);
            }

            foreach (var fileInfo in new DirectoryInfo(topDirFullPath).GetFiles())
            {
                if (fileInfo.FullName == zipFilePath)
                {
                    continue;
                }
                //ZipFileV1(entryPath + "/" + fileInfo.Name, fileInfo.FullName, zipFile);
                ZipFileV1(entryPath + "/" + fileInfo.Name, fileInfo, zipFile);
            }
        }
        public static void ZipFileV1(String entryPath, FileInfo flieInfo, ZipOutputStream zipFile)
        {
            ZipEntry entry = new ZipEntry(entryPath);
            byte[] buffer = new byte[4096];
            zipFile.PutNextEntry(entry);
            using (FileStream fs = File.OpenRead(flieInfo.FullName))
            {
                int sourceBytes;
                do
                {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    zipFile.Write(buffer, 0, sourceBytes);
                } while (sourceBytes > 0);
            }
        }
    }

    public class DeepPathZipService
    {
        private readonly int _maxEntryPathLength;

        public DeepPathZipService(int maxEntryPathLength = 240)
        {
            if (maxEntryPathLength < 10)
                throw new ArgumentOutOfRangeException(nameof(maxEntryPathLength));
            _maxEntryPathLength = maxEntryPathLength;
        }

        public void Zip(string sourceDir, string outputZipPath,
                        string? password = null, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            sourceDir = Path.GetFullPath(sourceDir).TrimEnd(Path.DirectorySeparatorChar);

            if (!Directory.Exists(sourceDir))
                throw new DirectoryNotFoundException($"Directory not found: {sourceDir}");

            int iteration = 0;
            const int maxIterations = 10_000;

            while (iteration++ < maxIterations)
            {
                var longest = FindLongestRelativePath(sourceDir);

                if (longest is null || longest.Length <= _maxEntryPathLength)
                    break;

                var folderToZip = FindAncestorToZip(sourceDir, longest);

                if (folderToZip is null)
                    throw new InvalidOperationException(
                        $"Cannot reduce path. A file/folder name directly under root may exceed " +
                        $"{_maxEntryPathLength} characters. Path: {longest}");

                ZipAndReplaceFolder(folderToZip, password, encoding);
            }

            if (iteration >= maxIterations)
                throw new InvalidOperationException("Exceeded maximum iteration count.");

            ZipUtil.ZipFolder(sourceDir, outputZipPath, password, encoding);
        }

        private string? FindLongestRelativePath(string rootDir)
        {
            string? longest = null;
            int maxLen = 0;

            foreach (var file in Directory.EnumerateFiles(rootDir, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(rootDir, file);
                if (relative.Length > maxLen)
                {
                    maxLen = relative.Length;
                    longest = relative;
                }
            }

            return longest;
        }

        private string? FindAncestorToZip(string rootDir, string longestRelative)
        {
            var segments = longestRelative.Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 2)
                return null;

            for (int i = 0; i < segments.Length - 1; i++)
            {
                var innerPath = string.Join(
                    Path.DirectorySeparatorChar,
                    segments.Skip(i + 1));

                if (innerPath.Length <= _maxEntryPathLength)
                {
                    var folderRelative = string.Join(
                        Path.DirectorySeparatorChar,
                        segments.Take(i + 1));

                    var folderFull = Path.Combine(rootDir, folderRelative);

                    if (Directory.Exists(folderFull))
                        return folderFull;
                }
            }

            return null;
        }

        private void ZipAndReplaceFolder(string folderPath, string? password, Encoding encoding)
        {
            var zipPath = folderPath + ".zip";

            if (File.Exists(zipPath))
            {
                int counter = 1;
                do { zipPath = $"{folderPath}_{counter++}.zip"; }
                while (File.Exists(zipPath));
            }

            ZipUtil.ZipFolder(folderPath, zipPath, password, encoding);
            Directory.Delete(folderPath, recursive: true);
        }
    }
}