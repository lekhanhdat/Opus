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


namespace AvePoint.Wrapper.Common
{
    using AvePoint.GCommon.Utility;
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class CabinetFileInfo : FileSystemInfo
    {
        private FileAttributes attributes;
        private int cabEnd;
        private int cabFolder;
        private CabinetInfo cabinetInfo;
        private int cabStart;
        private bool exists;
        private bool initialized;
        private DateTime lastWriteTime;
        private long length;
        private string name;
        private string path;

        //private CabinetFileInfo(SerializationInfo info, StreamingContext context) : base(info, context)
        //{
        //    this.cabinetInfo = (CabinetInfo)info.GetValue("cabinetInfo", typeof(CabinetInfo));
        //    this.name = info.GetString("name");
        //    this.path = info.GetString("path");
        //    this.initialized = info.GetBoolean("initialized");
        //    this.exists = info.GetBoolean("exists");
        //    this.cabFolder = info.GetInt32("cabFolder");
        //    this.cabStart = info.GetInt32("cabStart");
        //    this.cabEnd = info.GetInt32("cabEnd");
        //    this.attributes = (FileAttributes)info.GetValue("attributes", typeof(FileAttributes));
        //    this.lastWriteTime = info.GetDateTime("lastWriteTime");
        //    this.length = info.GetInt64("length");
        //}

        internal CabinetFileInfo(string name, string path, int cabFolder, int cabStart, int cabEnd, FileAttributes attributes, DateTime lastWriteTime, long length)
        {
            this.name = name;
            this.path = path;
            this.exists = true;
            this.cabFolder = cabFolder;
            this.cabStart = cabStart;
            this.cabEnd = cabEnd;
            this.attributes = attributes;
            this.lastWriteTime = lastWriteTime;
            this.length = length;
            this.initialized = true;
        }

        public void CopyTo(string destFileName)
        {
            this.CopyTo(destFileName, false);
        }

        public void CopyTo(string destFileName, bool overwrite)
        {
            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName");
            }
            if (!overwrite && File.Exists(destFileName))
            {
                throw new IOException();
            }
            if (this.Cabinet == null)
            {
                throw new InvalidOperationException();
            }
            this.Cabinet.ExtractFile(System.IO.Path.Combine(this.Path, this.Name), destFileName);
        }

        public override void Delete()
        {
            throw new NotSupportedException();
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("cabinetInfo", this.cabinetInfo);
            info.AddValue("name", this.name);
            info.AddValue("path", this.path);
            info.AddValue("initialized", this.initialized);
            info.AddValue("exists", this.exists);
            info.AddValue("cabFolder", this.cabFolder);
            info.AddValue("cabStart", this.cabStart);
            info.AddValue("cabEnd", this.cabEnd);
            info.AddValue("attributes", this.attributes);
            info.AddValue("lastWriteTime", this.lastWriteTime);
            info.AddValue("length", this.length);
        }

        public void Refresh()
        {
            base.Refresh();
            if (this.Cabinet != null)
            {
                string path = System.IO.Path.Combine(this.Path, this.Name);
                CabinetFileInfo file = this.Cabinet.GetFile(path);
                if (file == null)
                {
                    throw new FileNotFoundException("File not found in cabinet.", path);
                }
                this.exists = file.exists;
                this.length = file.length;
                this.attributes = file.attributes;
                this.lastWriteTime = file.lastWriteTime;
                this.cabFolder = file.cabFolder;
            }
        }

        public override string ToString()
        {
            return this.FullName;
        }

        public FileAttributes Attributes
        {
            get
            {
                if (!this.initialized)
                {
                    this.Refresh();
                }
                return this.attributes;
            }
        }

        public CabinetInfo Cabinet
        {
            get
            {
                return this.cabinetInfo;
            }
            internal set
            {
                this.cabinetInfo = value;
                base.OriginalPath = value.FullName;
                base.FullPath = value.FullName;
            }
        }

        public int CabinetFolderNumber
        {
            get
            {
                if (!this.initialized)
                {
                    this.Refresh();
                }
                return this.cabFolder;
            }
        }

        public string CabinetName
        {
            get
            {
                if (this.Cabinet == null)
                {
                    return null;
                }
                return this.Cabinet.FullName;
            }
        }

        public int EndCabinetNumber
        {
            get
            {
                return this.cabEnd;
            }
            internal set
            {
                this.cabEnd = value;
            }
        }

        public override bool Exists
        {
            get
            {
                if (!this.initialized)
                {
                    this.Refresh();
                }
                return this.exists;
            }
        }

        public override string FullName
        {
            get
            {
                if (this.Cabinet != null)
                {
                    return SecurityUtils.SafeCombinePath(this.CabinetName, SecurityUtils.SafeCombinePath(this.Path, this.Name));
                }
                return null;
            }
        }

        public DateTime LastWriteTime
        {
            get
            {
                if (!this.initialized)
                {
                    this.Refresh();
                }
                return this.lastWriteTime;
            }
        }

        public long Length
        {
            get
            {
                if (!this.initialized)
                {
                    this.Refresh();
                }
                return this.length;
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }

        public string Path
        {
            get
            {
                return this.path;
            }
        }

        public int StartCabinetNumber
        {
            get
            {
                return this.cabStart;
            }
        }
    }
}

