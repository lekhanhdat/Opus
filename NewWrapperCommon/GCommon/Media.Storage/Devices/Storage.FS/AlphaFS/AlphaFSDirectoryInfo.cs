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
    using System.Diagnostics;
    using System.IO;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using AvePoint.Media.Storage.Util;
    #endregion

    class AlphaFSDirectoryInfo : XDirectoryInfo, IDisposable
    {
        string name;

        StorageLogger logger = new StorageLogger(typeof(AlphaFSDirectoryInfo));
        private AbstractXSystem system;
        public Alphaleonis.Win32.Filesystem.DirectoryInfo AlDriInfo { set; get; }
        public AbstractXSystem System
        {
            set
            {
                this.system = value;
                identity = new FSIdentity(system as FSSystem);
            }
        }
        private FSIdentity identity;
        public AlphaFSDirectoryInfo(Alphaleonis.Win32.Filesystem.DirectoryInfo dirInfo, string highName)
            : base(highName)
        {
            this.AlDriInfo = dirInfo;
            this.name = dirInfo.Name;
        }

        private string fileOwner;

        public override string ParentFullName
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return (this.AlDriInfo.Parent == null) ? "" : AlphaFSUtil.ConvertPathToCommonUNC(this.AlDriInfo.Parent.FullName);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Get ParentFullName failed. Error:{0}", e);
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
                        return AlphaFSUtil.ConvertPathToCommonUNC(this.AlDriInfo.FullName);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Get DirFullPath failed. Error:{0}", e);
                    throw;
                }
            }
        }

        public override string UNCFullPath
        {
            get
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        return AlphaFSUtil.ConvertPathToCommonUNC(this.AlDriInfo.FullName);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Get UNCFullPath failed. Error:{0}", e);
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
                            try
                            {
                                using (identity.Impersonate())
                                {
                                    UNCObject obj = UNCObject.ValueOf(system.SystemLocation);
                                    var fs = Alphaleonis.Win32.Filesystem.Directory.GetAccessControl(this.AlDriInfo.FullName);
                                    var sid = fs.GetOwner(typeof(SecurityIdentifier));
                                    fileOwner = AccountUtil.GetAcountNameBySid(obj == null ? null : obj.Host, sid.ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceWarning("Get dir [{0}] owner failed. Error:{1}", this.AlDriInfo.FullName, ex);
                                fileOwner = string.Empty;
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
                using (identity.Impersonate())
                {
                    return this.AlDriInfo.Exists;
                }
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
                        attr = this.AlDriInfo.Attributes;
                        return (FileAttributes)Enum.Parse(typeof(FileAttributes), attr.ToString());
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Get file attribute failed. Error:{0}", e);
                }

                return FileAttributes.Normal;
            }
            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        this.AlDriInfo.Attributes = (FileAttributes)Enum.Parse(typeof(FileAttributes), value.ToString());
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Set file attribute failed. Error: {0}", e);
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
                        return this.AlDriInfo.LastWriteTimeUtc;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get last write time UTC failed. Error:{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        this.AlDriInfo.LastWriteTimeUtc = value;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set last write time UTC failed. Error:{0}", e);
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
                        return this.AlDriInfo.LastWriteTime;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get last write time failed. Error:{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        this.AlDriInfo.LastWriteTimeUtc = value;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set last write time failed. Error:{0}", e);
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
                        return this.AlDriInfo.LastAccessTime;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get last access time failed. Error:{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        this.AlDriInfo.LastWriteTimeUtc = value;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set last access time failed. Error:{0}", e);
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
                        return this.AlDriInfo.LastAccessTimeUtc;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get last access time UTC failed. Error:{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {

                    using (identity.Impersonate())
                    {
                        this.AlDriInfo.LastWriteTimeUtc = value;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set last access time UTC failed. Error:{0}", e);
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
                        return this.AlDriInfo.CreationTime;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get creation time failed. Error:{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        this.AlDriInfo.LastWriteTimeUtc = value;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set creation time failed. Error:{0}", e);
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
                        return this.AlDriInfo.CreationTimeUtc;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Get creation time UTC failed. Error:{0}", e);
                    throw;
                }
            }

            set
            {
                try
                {
                    using (identity.Impersonate())
                    {
                        this.AlDriInfo.LastWriteTimeUtc = value;
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error("Set creation time UTC failed. Error:{0}", e);
                    throw;
                }
            }
        }

        public override DirectorySecurity AccessControl
        {
            get
            {
                using (identity.Impersonate())
                {
                    return this.AlDriInfo.GetAccessControl();
                }
            }
        }

        public override bool IsEmpty
        {
            get
            {
                using (identity.Impersonate())
                {
                    if (this.AlDriInfo.GetFiles().Length == 0 && this.AlDriInfo.GetDirectories().Length == 0)
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
