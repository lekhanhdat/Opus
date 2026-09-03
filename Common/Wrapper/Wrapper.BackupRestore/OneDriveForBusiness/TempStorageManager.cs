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
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using AvePoint.GCommon;

namespace AvePoint.Wrapper.BackupRestore
{
    internal class TempStorageManager : IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private string mName;
        private long mAvailableSize;
        private readonly long SizeThreshold;
        private readonly bool EnableSizeLimit = false;
        private object locker = new object();
        private AutoResetEvent mSizeEvent = null;

        public long AvailableSize
        {
            get
            {
                lock (locker)
                {
                    return this.mAvailableSize;
                }
            }
        }

        public TempStorageManager(string name, long sizeThreshold)
        {
            this.mName = name;
            if (sizeThreshold > 0)
            {
                EnableSizeLimit = true;
                this.mSizeEvent = new AutoResetEvent(false);
                mLog.Info("Temp file size threshold:{0}", sizeThreshold.ToString());
            }
            this.SizeThreshold = sizeThreshold;
            this.mAvailableSize = sizeThreshold;
        }

        #region Reserve
        //public string Add(Stream content, string fileName, bool overwrite)
        //{
        //    string filePath = string.Empty;
        //    FileStream file = null;
        //    bool successful = false;
        //    try
        //    {
        //        filePath = Path.Combine(this.mPath, fileName);
        //        file = File.Create(filePath);
        //        byte[] buffer = new byte[65535];
        //        int offset = 0;
        //        int length = 0;
        //        while ((length = content.Read(buffer, offset, buffer.Length)) > 0)
        //        {
        //            file.Write(buffer, 0, length);
        //        }
        //        file.Flush();
        //        successful = true;
        //    }
        //    finally
        //    {
        //        if (file != null)
        //        {
        //            file.Dispose();
        //        }
        //        if (!successful)
        //        {
        //            if (File.Exists(filePath))
        //            {
        //                File.Delete(filePath);
        //            }
        //            filePath = string.Empty;
        //        }
        //    }
        //    return filePath;
        //}

        //public bool Delete(string fileName)
        //{
        //    string filePath = Path.Combine(this.mPath, fileName);

        //    if (File.Exists(filePath))
        //    {
        //        File.Delete(filePath);
        //    }
        //    return true;
        //}

        //public Stream Read(string fileName)
        //{
        //    string filePath = Path.Combine(this.mPath, fileName);
        //    if (File.Exists(filePath))
        //    {
        //        return File.OpenRead(filePath);
        //    }

        //    return null;
        //}
        #endregion
        /// <summary>
        /// check available disk size of temp folder
        /// </summary>
        /// <returns>true if available disk size greater than file size, otherwise false</returns>
        private bool ReserveDiskSize(long fileSize, bool forceReserve)
        {
            if (!EnableSizeLimit) return true;
            if (fileSize <= 0) return true;

            while (true)
            {
                if (fileSize > SizeThreshold)
                {
                    lock(locker)
                    {
                        if (this.mAvailableSize > 0)
                        {
                            this.mAvailableSize = 0;
                            return true;
                        }
                    }
                }

                if (this.mAvailableSize >= fileSize)
                {
                    lock (locker)
                    {
                        if (this.mAvailableSize >= fileSize)
                        {
                            this.mAvailableSize = this.mAvailableSize - fileSize;
                            return true;
                        }
                        else
                        {
                            if (forceReserve)
                            {
                                this.mAvailableSize = 0;
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    mLog.Info("Current available size:{0}, file size:{1}", this.mAvailableSize.ToString(), fileSize.ToString());
                }

                return false;
            }
        }

        /// <summary>
        /// check available disk size of temp folder
        /// if available disk size greater than file size, reserve disk, or wait until get enough space
        /// </summary>
        public void ReserveDiskSize(long fileSize)
        {
            if (!EnableSizeLimit) return;
            if (fileSize <= 0) return;

            if (fileSize > SizeThreshold)
            {
                lock (locker)
                {
                    if (this.mAvailableSize > 0)
                    {
                        this.mAvailableSize = 0;
                        return;
                    }
                }
            }

            while (fileSize > AvailableSize)
            {
                if (fileSize > SizeThreshold && AvailableSize == SizeThreshold)
                {
                    mLog.Info("File size greater than SizeThreshold");
                    fileSize = SizeThreshold;
                    break;
                }
                mLog.Warn("No enough space. AvailableSize:{0}, FileSize:{1}", AvailableSize.ToString(), fileSize.ToString());
                this.mSizeEvent.WaitOne();
            }

            lock(locker)
            {
                this.mAvailableSize = this.mAvailableSize - fileSize;
                if (this.mAvailableSize < 0)
                {
                    mLog.Error("Something wrong with calculating available size. AvailableSize:{0}, FileSize:{1}", this.mAvailableSize.ToString(), fileSize.ToString());
                }
            }

            return;
        }

        public void ReleaseFileUsage(long fileSize)
        {
            if (!EnableSizeLimit) return;
            if (fileSize <= 0) return;

            lock (locker)
            {
                this.mAvailableSize = this.mAvailableSize + fileSize;
                if (this.mAvailableSize > SizeThreshold)
                {
                    this.mAvailableSize = SizeThreshold;
                }
            }
            mSizeEvent.Set();
            mLog.Info("Release file usage. Current available size:{0}", this.mAvailableSize.ToString());
        }
        
        public void Dispose()
        {
            if (this.mSizeEvent != null)
            {
                this.mSizeEvent.Dispose();
            }
        }
    }
}
