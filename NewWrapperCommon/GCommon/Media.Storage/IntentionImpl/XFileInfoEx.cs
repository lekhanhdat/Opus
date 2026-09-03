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
    public class XFileInfoEx
    {
        private bool mIsAlphaFileInfo;
        private AlphaFSFileInfo alphaFSFileInfo;
        private XFileInfo xFileInfo;
        public XFileInfoEx(StorageInfo storageInfo)
        {
            if (storageInfo is AlphaFSFileInfo)
            {
                mIsAlphaFileInfo = true;
                alphaFSFileInfo = storageInfo as AlphaFSFileInfo;
            }
            else if (storageInfo is XFileInfo)
            {
                xFileInfo = storageInfo as XFileInfo;              
            }
        }

        public string LowName
        {
            get
            {
                return mIsAlphaFileInfo ? alphaFSFileInfo.LowName : xFileInfo.LowName;
            }
        }

        public string HighName
        {
            get
            {
                return mIsAlphaFileInfo ? alphaFSFileInfo.HighName : xFileInfo.HighName;
            }
        }

        public string HighPlusLowName
        {
            get
            {
                return mIsAlphaFileInfo ? alphaFSFileInfo.HighPlusLowName : xFileInfo.HighPlusLowName;
            }
        }

        public string Owner
        {
            get
            {
                return mIsAlphaFileInfo ? alphaFSFileInfo.Owner : xFileInfo.Owner;
            }
        }

        public string FileFullPath
        {
            get
            {
                return mIsAlphaFileInfo ? alphaFSFileInfo.FileFullPath : xFileInfo.FileFullPath;
            }
        }

        public string Name
        {
            get
            {
                return mIsAlphaFileInfo ? alphaFSFileInfo.Name : xFileInfo.Name;
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                return mIsAlphaFileInfo ? alphaFSFileInfo.AlFileInfo.CreationTimeUtc : xFileInfo.FileInfo.CreationTimeUtc;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                return mIsAlphaFileInfo ? alphaFSFileInfo.AlFileInfo.LastWriteTimeUtc : xFileInfo.FileInfo.LastWriteTimeUtc;
            }
        }

        public long FileSize
        {
            get
            {
                return mIsAlphaFileInfo ? alphaFSFileInfo.FileSize : xFileInfo.FileSize;
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                return mIsAlphaFileInfo ? alphaFSFileInfo.AlFileInfo.LastAccessTimeUtc : xFileInfo.FileInfo.LastAccessTimeUtc;
            }
        }

        public bool IsHidden
        {
            get
            {
                return mIsAlphaFileInfo ? (alphaFSFileInfo.Attribute & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden
                    : (xFileInfo.Attribute & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden;
            }
        }
    }
}
