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

using System;
using System.IO;

namespace AvePoint.RA.Common.Util
{
    public static class FileSystemUtil
    {
        #region Fields
        #endregion

        #region Properties
        /// <summary>
        /// Get Main Folder Path
        /// </summary>
        public static string PathProgramRootDir
        {
            get { return System.AppDomain.CurrentDomain.BaseDirectory; }
        }
        /// <summary>
        /// Get Bin Folder Path
        /// </summary>
        public static string PathBin
        {
            get { return PathProgramRootDir + "Bin"; }
        }
        /// <summary>
        /// Get Audit Folder Path
        /// </summary>
        public static string PathAudit
        {
            get { return PathProgramRootDir + "Audit"; }
        }
        /// <summary>
        /// Get web.config Path
        /// </summary>
        public static string PathWebConfig
        {
            get { return System.AppDomain.CurrentDomain.BaseDirectory + "//web.config"; }
        }
        #endregion

        public static bool CreateFolder(string folderPath)
        {
            DirectoryInfo _directoryInfo = null;
            try
            {
                if (!IsDirectoryExist(folderPath))
                {
                    _directoryInfo = Directory.CreateDirectory(folderPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("CreateFolder Exception:" + ex.ToString());
            }
        }

        #region Public Methods
        /// <summary>
        /// Judge Path Exist
        /// </summary>       
        public static bool IsExist(string path)
        {
            try
            {
                if (IsDirectoryExist(path) || IsFileExist(path))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("IsExist Exception:" + ex.ToString());
            }
        }
        /// <summary>
        /// Create File to File System. 
        /// RECO-20916 Need to use securityUtils.safecombinepath to validate filePath
        /// </summary>      
        public static bool CreateFile(string filePath, string context, bool isAppend)
        {
            StreamWriter _streamWriter = null;
            try
            {
                _streamWriter = new StreamWriter(filePath, isAppend);
                _streamWriter.Write(context);
                _streamWriter.Close();
                return true;
            }
            catch (Exception ex)
            {
                if (_streamWriter != null)
                {
                    _streamWriter.Dispose();
                }
                throw new Exception("CreateFile Exception:" + ex.ToString());
            }

        }

        public static void WriteFile(string path, FileMode fileMode, byte[] data)
        {
            using (FileStream stream = new FileStream(path, fileMode))
            {
                stream.Write(data, 0, data.Length);
            }
        }

        public static byte[] ReadFromStream(Stream inputStream)
        {
            var buffer = new byte[1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
            //Quality Issue


            //byte[] data = new byte[inputStream.Length];
            //inputStream.Read(data, 0, data.Length);
            //return data;
        }

        public static string ReadFile(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Get File size;
        /// </summary>      
        public static long FileSize(string filePath)
        {
            FileInfo _file = null;
            try
            {
                if (IsExist(filePath))
                {
                    _file = new FileInfo(filePath);
                    return _file.Length;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("FileSize Exception" + ex.ToString());
            }
        }
        #endregion

        #region Private Methods
        private static bool IsFileExist(string _filePath)
        {
            if (File.Exists(_filePath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private static bool IsDirectoryExist(string _folderPath)
        {
            if (Directory.Exists(_folderPath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
