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



namespace AvePoint.Media.ClassicStorage
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.AccessControl;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.ClassicStorage.Util;
    using AvePoint.Media.StorageApi;

    #endregion using directives

    [Serializable]
    public class StorageDirectoryInfo : XDirectoryInfo
    {
        public StorageDirectoryInfo()
        {
        }

        private string name;

        public StorageDirectoryInfo(string name)
        {
            this.name = name;
        }

        public override string Name
        {
            get { return name; }
        }
    }

    [Serializable]
    public abstract class XDirectoryInfo : StorageInfo
    {
        // public string HighName { get; set; }
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Domain { get; set; }

        public virtual FileAttributes Attribute { get; set; }

        public virtual DirectoryInfo DirInfo { get; set; }

        public virtual string Owner { get { return null; } }

        private bool writable;

        public bool Writable
        {
            get
            {
                //FileAttributes.r
                return writable;
            }
        }

        private List<XFileInfo> subFiles = new List<XFileInfo>();

        public virtual List<XFileInfo> SubFiles { get { return subFiles; } }

        private List<XDirectoryInfo> subDirs = new List<XDirectoryInfo>();

        public virtual List<XDirectoryInfo> SubDirectories { get { return subDirs; } }

        public XDirectoryInfo()
        {
        }

        public XDirectoryInfo(string highName)
        {
            this.HighName = highName;
        }

        public override string FullName
        {
            get
            {
                return SecurityUtils.SafeCombinePath(HighName, LowName);
            }
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }

        public override bool Exists
        {
            get { throw new NotImplementedException(); }
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public new virtual DateTime CreationTime { get; set; }

        public new virtual DateTime CreationTimeUtc { get; set; }

        public new virtual DateTime LastAccessTime { get; set; }

        public new virtual DateTime LastAccessTimeUtc { get; set; }

        public new virtual DateTime LastWriteTime { get; set; }

        public new virtual DateTime LastWriteTimeUtc { get; set; }

        public virtual string ParentFullName { get; set; }

        public virtual DirectorySecurity AccessControl { get; set; }

        public virtual string DirFullPath { get; set; }

        public virtual string UNCFullPath { get; set; }

        public virtual bool IsEmpty
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}