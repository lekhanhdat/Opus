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
using System.Security.AccessControl;
using System.Security.Principal;
using AvePoint.GCommon;
namespace AvePoint.Media.Storage.FS
{
    public class FSFileInfo : XFileInfo, IDisposable
    {
        AveLogger logger = new AveLogger(typeof(FileInfo));
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
                        return this.FileInfo.DirectoryName;
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
                        return this.FileInfo.FullName;
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
                        return this.FileInfo.Length;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get FileSize failed. Error:{0}", e);
                    throw;
                }
            }
        }
        public override bool Exists
        {
            get
            {
                bool result = false;
                try
                {
                    using (identity.Impersonate())
                    {
                        result = this.FileInfo.Exists;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("File is not found. Error:{0}", e);
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
                        attr = this.FileInfo.Attributes;
                        return attr;
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Get Attribute failed. Error:{0}", e);
                    throw;
                }
            }
            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        File.SetAttributes(this.FileInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Set Attribute failed. Error:{0}", e);
                    throw;
                }

            }
        }

        public FSFileInfo()
        {
        }

        public FSFileInfo(FileInfo fileInfo, string highName, string lowName)
            : base(highName, lowName)
        {
            this.FileInfo = fileInfo;
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
                        return this.FileInfo.LastWriteTimeUtc;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get LastWriteTimeUtc failed. Error:{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        File.SetLastWriteTimeUtc(this.FileInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set LastWriteTimeUtc failed. Error:{0}", e);
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
                        return this.FileInfo.LastWriteTime;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get LastWriteTime failed. Error:{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {

                    using (identity.Impersonate())
                    {
                        File.SetLastWriteTime(this.FileInfo.FullName, value);
                    }

                }
                catch (Exception e)
                {
                    this.logger.Error("Set LastWriteTime failed. Error:{0}", e);
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
                        return this.FileInfo.LastAccessTime;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get LastAccessTime failed. Error:{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        File.SetLastAccessTime(this.FileInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set LastAccessTime failed. Error:{0}", e);
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
                        return this.FileInfo.LastAccessTimeUtc;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get LastAccessTimeUtc failed. Error:{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        File.SetLastAccessTimeUtc(this.FileInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set LastAccessTimeUtc failed. Error:{0}", e);
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
                        return this.FileInfo.CreationTime;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get CreationTime failed. Error:{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        File.SetCreationTime(this.FileInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set CreationTime failed. Error:{0}", e);
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
                        return this.FileInfo.CreationTimeUtc;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get CreationTimeUtc failed. Error:{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        File.SetCreationTimeUtc(this.FileInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set CreationTimeUtc failed. Error:{0}", e);
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
                            using (identity.Impersonate())
                            {
                                UNCObject obj = UNCObject.ValueOf(this.FileInfo.FullName);
                                var fs = File.GetAccessControl(this.FileInfo.FullName);
                                var sid = fs.GetOwner(typeof(SecurityIdentifier));
                                fileOwner = AccountUtil.GetAcountNameBySid(obj == null ? null : obj.Host, sid.ToString());
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
                    return this.FileInfo.GetAccessControl();
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        byte[] bt = value.GetSecurityDescriptorBinaryForm();
                        FileSecurity newSec = new FileSecurity();
                        newSec.SetSecurityDescriptorBinaryForm(bt);
                        File.SetAccessControl(this.FileInfo.FullName, newSec);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Set fs file access control failed. Error:{0}", e);
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
    }
}
