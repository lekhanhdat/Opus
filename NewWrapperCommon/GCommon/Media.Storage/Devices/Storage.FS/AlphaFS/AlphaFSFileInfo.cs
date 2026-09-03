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
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using AvePoint.GCommon;
namespace AvePoint.Media.Storage.FS
{
    public class AlphaFSFileInfo : XFileInfo, IDisposable
    {
        AveLogger logger = new AveLogger(typeof(AlphaFSFileInfo));
        private AbstractXSystem system;
        public AbstractXSystem System
        {
            set
            {
                this.system = value;
                identity = new FSIdentity(system as FSSystem);
            }
        }
        private FSIdentity identity;

        public override string ParentFullName
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return AlphaFSUtil.ConvertPathToCommonUNC(this.AlFileInfo.Directory.FullName);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get ParentFullName failed. Error:{0}", e);
                    throw;
                }
            }
        }

        public override string FileFullPath
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return AlphaFSUtil.ConvertPathToCommonUNC(this.AlFileInfo.FullName);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get FileFullPath failed. Error:{0}", e);
                    throw;
                }
            }
        }

        public override long FileSize
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return this.AlFileInfo.Length;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get FileSize failed. Error:{0}", e);
                    throw;
                }
            }
        }
        public Alphaleonis.Win32.Filesystem.FileInfo AlFileInfo { get; set; }

        public override bool Exists
        {
            get
            {
                bool result = false;
                try
                {
                    using (identity.Impersonate())
                    {
                        result = this.AlFileInfo.Exists;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("File is not found.Error:{0}", e);
                    throw;
                }

                return result;
            }

        }

        public override FileAttributes Attribute
        {
            get
            {
                FileAttributes attr;
                try
                {
                    using (identity.Impersonate())
                    {
                        attr = this.AlFileInfo.Attributes;
                        return (FileAttributes)Enum.Parse(typeof(FileAttributes), attr.ToString());
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Get attribute failed. Error:{0}", e);
                    throw;
                }
            }
            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        Alphaleonis.Win32.Filesystem.File.SetAttributes(this.AlFileInfo.FullName, (FileAttributes)Enum.Parse(typeof(FileAttributes), value.ToString()));
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Set attribute failed. Error:{0}", e);
                    throw;
                }
            }
        }

        public AlphaFSFileInfo(Alphaleonis.Win32.Filesystem.FileInfo fileInfo, string highName, string lowName)
            : base(highName, lowName)
        {
            this.AlFileInfo = fileInfo;
            this.HighName = highName;
            this.LowName = lowName;
        }

        public override DateTime LastWriteTimeUtc
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return this.AlFileInfo.LastWriteTimeUtc;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get LastWriteTimeUtc failed. Error:{0} ", e);
                    throw;
                }
            }
            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        Alphaleonis.Win32.Filesystem.File.SetLastWriteTimeUtc(this.AlFileInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set LastWriteTimeUtc failed. Error:{0} ", e);
                    throw;
                }
            }
        }

        public override DateTime LastWriteTime
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return this.AlFileInfo.LastWriteTime;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get LastWriteTime failed. Error:{0} ", e);
                    throw;
                }
            }
            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        Alphaleonis.Win32.Filesystem.File.SetLastWriteTime(this.AlFileInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set LastWriteTime failed. Error:{0} ", e);
                    throw;
                }
            }
        }

        public override DateTime LastAccessTime
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return this.AlFileInfo.LastAccessTime;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get LastAccessTime failed. Error:{0} ", e);
                    throw;
                }
            }
            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        Alphaleonis.Win32.Filesystem.File.SetLastAccessTime(this.AlFileInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set LastAccessTime failed. Error:{0} ", e);
                    throw;
                }

            }
        }


        public override DateTime LastAccessTimeUtc
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return this.AlFileInfo.LastAccessTimeUtc;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get LastAccessTimeUtc failed. Error:{0} ", e);
                    throw;
                }
            }
            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        Alphaleonis.Win32.Filesystem.File.SetLastAccessTimeUtc(this.AlFileInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set LastAccessTimeUtc failed. Error:{0} ", e);
                    throw;
                }

            }
        }

        public override DateTime CreationTime
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return this.AlFileInfo.CreationTime;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get CreationTime failed. Error:{0} ", e);
                    throw;
                }
            }
            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        Alphaleonis.Win32.Filesystem.File.SetCreationTime(this.AlFileInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set CreationTime failed. Error:{0} ", e);
                    throw;
                }

            }
        }

        public override DateTime CreationTimeUtc
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return this.AlFileInfo.CreationTimeUtc;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get CreationTimeUtc failed. Error:{0} ", e);
                    throw;
                }
            }
            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        Alphaleonis.Win32.Filesystem.File.SetCreationTimeUtc(this.AlFileInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set CreationTimeUtc failed. Error:{0} ", e);
                    throw;
                }

            }
        }

        private string fileOwner;

        public override string Owner
        {
            get
            {
                if (string.IsNullOrEmpty(fileOwner))
                {
                    if (system != null)
                    {
                        if (identity != null)
                        {
                            try
                            {
                                using (identity.Impersonate())
                                {
                                    UNCObject obj = UNCObject.ValueOf(this.system.SystemLocation);
                                    var fs = Alphaleonis.Win32.Filesystem.File.GetAccessControl(this.AlFileInfo.FullName);
                                    var sid = fs.GetOwner(typeof(SecurityIdentifier));
                                    fileOwner = AccountUtil.GetAcountNameBySid(obj == null ? null : obj.Host, sid.ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceWarning("Get dir [{0}] owner failed. Error:{1}", this.AlFileInfo.FullName, ex);
                                fileOwner = string.Empty;
                            }
                        }
                    }
                }
                return fileOwner;
            }
        }

        public override FileSecurity AccessControl
        {
            get
            {
                using (identity.Impersonate())
                {
                    return this.AlFileInfo.GetAccessControl();

                }
            }

            set
            {
                try
                {
                    if (system == null)
                        base.AccessControl = value;
                    else
                    {
                        using (identity.Impersonate())
                        {
                            byte[] bt = value.GetSecurityDescriptorBinaryForm();
                            FileSecurity newSec = new FileSecurity();
                            newSec.SetSecurityDescriptorBinaryForm(bt);
                            Alphaleonis.Win32.Filesystem.File.SetAccessControl(this.AlFileInfo.FullName, newSec, AccessControlSections.All);//need confirm
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Set alpha fs file access control failed. Error:{0}", e);
                }
            }

        }
        public void Dispose()
        {
            if (identity != null)
            {
                identity.Dispose();
                identity = null;
            }
        }
        public AlphaFSFileInfo()
        {
        }
    }
}
