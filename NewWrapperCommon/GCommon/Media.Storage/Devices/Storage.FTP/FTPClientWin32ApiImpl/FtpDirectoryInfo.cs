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




namespace AvePoint.Media.Storage.FTP
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using AvePoint.Media.Storage.Util;

    /// <summary>
    /// The <c>FtpDirectoryInfo</c> class encapsulates a remote FTP directory.
    /// </summary>
    [Serializable]    
    class FtpDirectoryInfo : FileSystemInfo
    {
        private DateTime? creationTime;
        private DateTime? lastAccessTime;
        private DateTime? lastWriteTime;
        
        public FtpDirectoryInfo(FtpConnection ftp, string path)
        {
            this.FtpConnection = ftp;
            this.FullPath = path;
        }
        
        protected FtpDirectoryInfo(SerializationInfo info, StreamingContext context) : base(info, context)
        {         
        }

        public FtpConnection FtpConnection { get; internal set; }

        public new DateTime? LastAccessTime
        {
            get { return this.lastAccessTime.HasValue ? (DateTime?)this.lastAccessTime.Value : null; }
            internal set { this.lastAccessTime = value; }
        }

        public new DateTime? CreationTime
        {
            get { return this.creationTime.HasValue ? (DateTime?)this.creationTime.Value : null; }
            internal set { this.creationTime = value; }
        }

        public new DateTime? LastWriteTime
        {
            get { return this.lastWriteTime.HasValue ? (DateTime?)this.lastWriteTime.Value : null; }
            internal set { this.lastWriteTime = value; }
        }

        public new DateTime? LastAccessTimeUtc
        {
            get { return this.lastAccessTime.HasValue ? (DateTime?)this.lastAccessTime.Value.ToUniversalTime() : null; }
        }

        public new DateTime? CreationTimeUtc
        {
            get { return this.creationTime.HasValue ? (DateTime?)this.creationTime.Value.ToUniversalTime() : null; }
        }

        public new DateTime? LastWriteTimeUtc
        {
            get { return this.lastWriteTime.HasValue ? (DateTime?)this.lastWriteTime.Value.ToUniversalTime() : null; }
        }

        public new FileAttributes Attributes { get; set; }

        public override bool Exists
        {
            get { return this.FtpConnection.DirectoryExists(this.FullName); }
        }

        public override string Name
        {
            get { return Path.GetFileName(this.FullPath); }
        }

        public override void Delete()
        {
            this.FtpConnection.DeleteDirectory(this.Name);
        }

        public FtpDirectoryInfo[] GetDirectories()
        {
            return this.FtpConnection.GetDirectories(this.FullPath);
        }

        public FtpDirectoryInfo[] GetDirectories(string path)
        {
            path = PathUtil.CombinePath(this.FullPath, path);
            return this.FtpConnection.GetDirectories(path);
        }

        public FtpFileInfo[] GetFiles()
        {
            return this.GetFiles(this.FtpConnection.GetCurrentDirectory());
        }

        public FtpFileInfo[] GetFiles(string mask)
        {
            return this.FtpConnection.GetFiles(mask);
        }

        /// <summary>
        /// No specific impelementation is needed of the GetObjectData to serialize this object
        /// because all attributes are redefined.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data. </param>
        /// <param name="context">The destination for this serialization. </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {   
            base.GetObjectData(info, context);
        }
    }
}