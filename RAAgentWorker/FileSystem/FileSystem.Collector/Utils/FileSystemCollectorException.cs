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

namespace RAFileSystem.FileSystem.Collector
{
    public class FileSystemCollectorException : Exception
    {
        public string FullPath { get; set; }

        public string I18nMessageKey { get; set; }

        public FileSystemCollectorException() : this(string.Empty) { }

        public FileSystemCollectorException(string fullPath) : this(fullPath, ogininalException: null) { }

        public FileSystemCollectorException(string fullPath, Exception ogininalException) : this(fullPath, ogininalException, string.Empty) { }

        public FileSystemCollectorException(string fullPath, Exception ogininalException, string i18nMessageKey) : base(ogininalException?.Message, ogininalException)
        {
            FullPath = fullPath;
            I18nMessageKey = !string.IsNullOrEmpty(i18nMessageKey) ? i18nMessageKey : Message;
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}. FullPath: {FullPath}, I18nMessageKey: {I18nMessageKey}, Exception: {InnerException}";
        }
    }

    public class FileSystemCollectorUnauthorizedAccessException : FileSystemCollectorException
    {
        private const string defaultI18nMessageKey = "RM_JS_JMD_FS_PathCanNotAccess";

        public FileSystemCollectorUnauthorizedAccessException() : base() { }

        public FileSystemCollectorUnauthorizedAccessException(string fullPath) : this(fullPath, ogininalException: null) { }

        public FileSystemCollectorUnauthorizedAccessException(string fullPath, Exception ogininalException) : this(fullPath, ogininalException, defaultI18nMessageKey) { }

        public FileSystemCollectorUnauthorizedAccessException(string fullPath, Exception ogininalException, string i18nMessageKey) : base(fullPath, ogininalException, i18nMessageKey) { }
    }

    public class FileSystemCollectorPathNotFoundException : FileSystemCollectorException
    {
        private const string defaultI18nMessageKey = ""; // need default i18n message ?

        public FileSystemCollectorPathNotFoundException() : base() { } 

        public FileSystemCollectorPathNotFoundException(string fullPath) : this(fullPath, ogininalException: null) { }

        public FileSystemCollectorPathNotFoundException(string fullPath, Exception ogininalException) : this(fullPath, ogininalException, defaultI18nMessageKey) { }

        public FileSystemCollectorPathNotFoundException(string fullPath, Exception ogininalException, string i18nMessageKey) : base(fullPath, ogininalException, i18nMessageKey) { }
    }

    public class FileSystemCollectorPathTooLongException : FileSystemCollectorException
    {
        private const string defaultI18nMessageKey = ""; // need default i18n message ?

        public FileSystemCollectorPathTooLongException() : base() { }

        public FileSystemCollectorPathTooLongException(string fullPath) : this(fullPath, ogininalException: null) { }

        public FileSystemCollectorPathTooLongException(string fullPath, Exception ogininalException) : this(fullPath, ogininalException, defaultI18nMessageKey) { }

        public FileSystemCollectorPathTooLongException(string fullPath, Exception ogininalException, string i18nMessageKey) : base(fullPath, ogininalException, i18nMessageKey) { }
    }
}
