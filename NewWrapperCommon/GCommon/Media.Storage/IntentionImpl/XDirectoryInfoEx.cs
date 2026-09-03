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
using AvePoint.Media.Storage;
using AvePoint.Media.Storage.FS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Media.Storage
{
    public class XDirectoryInfoEx
    {
        private bool mIsAlphaDirInfo;
        private AlphaFSDirectoryInfo alphaFSFileInfo;
        private XDirectoryInfo xFileInfo;
        public XDirectoryInfoEx(StorageInfo storageInfo)
        {
            if (storageInfo is AlphaFSDirectoryInfo)
            {
                mIsAlphaDirInfo = true;
                alphaFSFileInfo = storageInfo as AlphaFSDirectoryInfo;
            }
            else if (storageInfo is XDirectoryInfo)
            {
                xFileInfo = storageInfo as XDirectoryInfo;
            }
        }

        public string LowName
        {
            get
            {
                return mIsAlphaDirInfo ? alphaFSFileInfo.LowName : xFileInfo.LowName;
            }
        }

        public string HighName
        {
            get
            {
                return mIsAlphaDirInfo ? alphaFSFileInfo.HighName : xFileInfo.HighName;
            }
        }

        public string HighPlusLowName
        {
            get
            {
                return mIsAlphaDirInfo ? alphaFSFileInfo.HighPlusLowName : xFileInfo.HighPlusLowName;
            }
        }

        public long Length
        {
            get
            {
                return mIsAlphaDirInfo ? alphaFSFileInfo.Length : xFileInfo.Length;
            }
        }

        public string Owner
        {
            get
            {
                return mIsAlphaDirInfo ? alphaFSFileInfo.Owner : xFileInfo.Owner;
            }
        }

        public string UNCFullPath
        {
            get
            {
                return mIsAlphaDirInfo ? alphaFSFileInfo.UNCFullPath : xFileInfo.UNCFullPath;
            }
        }

        public string Name
        {
            get
            {
                return mIsAlphaDirInfo ? alphaFSFileInfo.Name : xFileInfo.Name;
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                return mIsAlphaDirInfo ? alphaFSFileInfo.AlDriInfo.CreationTimeUtc : xFileInfo.DirInfo.CreationTimeUtc;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                return mIsAlphaDirInfo ? alphaFSFileInfo.AlDriInfo.LastWriteTimeUtc : xFileInfo.DirInfo.LastWriteTimeUtc;
            }
        }       

        public DateTime LastAccessTimeUtc
        {
            get
            {
                return mIsAlphaDirInfo ? alphaFSFileInfo.AlDriInfo.LastAccessTimeUtc : xFileInfo.DirInfo.LastAccessTimeUtc;
            }
        }

        public DateTime LastWriteTime
        {
            get
            {
                return mIsAlphaDirInfo ? alphaFSFileInfo.AlDriInfo.LastWriteTime : xFileInfo.DirInfo.LastWriteTime;
            }
        }

        public DateTime CreationTime
        {
            get
            {
                return mIsAlphaDirInfo ? alphaFSFileInfo.AlDriInfo.CreationTime : xFileInfo.DirInfo.CreationTime;
            }
        }

        public string LocalFullPath
        {
            get
            {
                return mIsAlphaDirInfo ? alphaFSFileInfo.AlDriInfo.FullName : xFileInfo.DirFullPath;
            }
        }

        public bool IsHidden
        {
            get
            {
                return mIsAlphaDirInfo ? (alphaFSFileInfo.Attribute & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden 
                        : (xFileInfo.Attribute & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden;
            }
        }
    }
}
