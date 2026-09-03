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


namespace AvePoint.Media.Storage.FS
{
    #region using directives

    using System;
    using System.IO;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using AvePoint.Media.Storage.Util;

    #endregion using directives

    class FSDirectoryInfo : XDirectoryInfo, IDisposable
    {
        private string name;

        private StorageLogger logger = new StorageLogger(typeof(FileInfo));
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

        public FSDirectoryInfo(DirectoryInfo dirInfo, string highName)
            : base(highName)
        {
            this.DirInfo = dirInfo;
            this.name = dirInfo.Name;
        }

        private string fileOwner;

        public override StorageInfo Parent
        {
            get
            {
                var result = default(StorageInfo);
                if (!String.IsNullOrEmpty(HighPlusLowName))
                {
                    var dirPath = HighPlusLowName.TrimEnd('\\');
                    int lastIndex = dirPath.LastIndexOf('\\');
                    if (lastIndex > 0)
                    {
                        result = new StorageInfo(dirPath.Substring(0, lastIndex), "");
                    }
                    else
                    {
                        result = new StorageInfo("\\", "");
                    }
                }
                else
                {
                    result = new StorageInfo("\\", "");
                }
                return result;
            }
        }

        public override string ParentFullName
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return (this.DirInfo.Parent == null) ? "" : this.DirInfo.Parent.FullName;
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Get ParentFullName failed. Error{0}", e);
                    throw;
                }
            }
        }

        public override string DirFullPath
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return this.DirInfo.FullName;
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Get DirFullPath failed. Error{0}", e);
                    throw;
                }
            }
        }
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
                                UNCObject obj = UNCObject.ValueOf(this.DirInfo.FullName);
                                var fs = File.GetAccessControl(this.DirInfo.FullName);
                                var sid = fs.GetOwner(typeof(SecurityIdentifier));
                                fileOwner = AccountUtil.GetAcountNameBySid(obj == null ? null : obj.Host, sid.ToString());
                            }
                        }
                    }
                }
                return fileOwner;
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
                        result = this.DirInfo.Exists;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message, e);
                }

                return result;
            }
        }

        public override FileAttributes Attribute
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return this.DirInfo.Attributes;
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Get Attribute failed. Error{0}", e);
                    throw;
                }
            }
            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        this.DirInfo.Attributes = value;
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Set Attribute failed. Error{0}", e);
                    throw;
                }
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }

        public override DateTime LastWriteTimeUtc
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return this.DirInfo.LastWriteTimeUtc;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get LastWriteTimeUtc failed. Error{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        Directory.SetLastWriteTimeUtc(this.DirInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set LastWriteTimeUtc failed. Error{0}", e);
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
                        return this.DirInfo.LastWriteTime;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get LastWriteTime failed. Error{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        Directory.SetLastWriteTime(this.DirInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set LastWriteTime failed. Error{0}", e);
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
                        return this.DirInfo.LastAccessTime;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get LastAccessTime failed. Error{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        Directory.SetLastAccessTime(this.DirInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set LastAccessTime failed. Error{0}", e);
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
                        return this.DirInfo.LastAccessTimeUtc;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get LastAccessTimeUtc failed. Error{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        Directory.SetLastAccessTimeUtc(this.DirInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set LastAccessTimeUtc failed. Error{0}", e);
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
                        return this.DirInfo.CreationTime;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get CreationTime failed. Error{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        Directory.SetCreationTime(this.DirInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set CreationTime failed. Error{0}", e);
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
                        return this.DirInfo.CreationTimeUtc;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get CreationTimeUtc failed. Error{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        Directory.SetCreationTimeUtc(this.DirInfo.FullName, value);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set CreationTimeUtc failed. Error{0}", e);
                    throw;
                }
            }
        }

        public override Int32 FileCount
        {
            get
            {
                using (identity.Impersonate())
                {
                    return this.DirInfo.GetFiles().Length;
                }
            }
        }

        public override DirectorySecurity AccessControl
        {
            get
            {
                using (identity.Impersonate())
                {
                    return this.DirInfo.GetAccessControl();
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        byte[] bt = value.GetSecurityDescriptorBinaryForm();
                        DirectorySecurity newSec = new DirectorySecurity();
                        newSec.SetSecurityDescriptorBinaryForm(bt);
                        Directory.SetAccessControl(this.DirInfo.FullName, newSec);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Set fs directory access control failed. Error:{0}", e);
                }
            }
        }

        public override bool IsEmpty
        {
            get
            {
                using (identity.Impersonate())
                {
                    if (this.DirInfo.GetFiles().Length == 0 && this.DirInfo.GetDirectories().Length == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
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