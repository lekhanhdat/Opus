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




namespace AvePoint.Media.ClassicStorage.Cloud.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    #endregion

    [Serializable]
    public class CloudFileInfo : XFileInfo
    {

        private CloudSystem cloudSystem;
        public CloudSystem System { set { this.cloudSystem = value; } }

        public CloudFileInfo(string highName, string lowName, long fileSize)
            : this(highName, lowName)
        {
            this.FileSize = fileSize;
        }

        public CloudFileInfo(string highName, string lowName, long fileSize, string modified)
            : this(highName, lowName)
        {
            if (!String.IsNullOrEmpty(modified))
            {
                LastWriteTimeUtc = DateTime.Parse(modified).ToUniversalTime();
            }
            this.FileSize = fileSize;
        }

        private string name;

        public CloudFileInfo(string highName, string lowName)
        {
            this.name = lowName;
            this.HighName = highName;
            this.LowName = lowName;
        }

        private long fileSize;

        public override long FileSize
        {
            get
            {
                if (fileSize == -1)
                {
                    if (cloudSystem != null)
                    {
                        XFileInfo info = cloudSystem.OpenFile(new StorageInfo() { HighName = this.HighName, LowName = this.name });
                        fileSize = info.FileSize;
                    }
                }
                return fileSize;
            }
            set
            {
                fileSize = value;
            }
        }

        public override string Name
        {
            get
            {
                return name;
            }
        }


        public CloudFileInfo()
        {
        }

        public override bool Exists
        {
            get
            {
                if (this.FileSize == -1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

    }

    [Serializable]
    public class CloudDirectoryInfo : XDirectoryInfo
    {
        bool isExists;

        public bool IsExists
        {
            get
            {
                return isExists;
            }
            set
            {
                isExists = value;
            }
        }

        private string name;

        public CloudDirectoryInfo(string highName, string lowName)
        {
            HighName = highName;
            name = lowName;
            LowName = lowName;
        }

        public CloudDirectoryInfo(string highName, string lowName, string modified)
        {
            HighName = highName;
            name = lowName;
            LowName = lowName;
            if (!String.IsNullOrEmpty(modified))
            {
                LastWriteTimeUtc = DateTime.Parse(modified).ToUniversalTime();
            }
        }

        public CloudDirectoryInfo(string name)
        {
            this.name = name;
        }

        public override string Name
        {
            get
            {
                return name;
            }
        }


        public CloudDirectoryInfo()
        {
        }

        public override bool Exists
        {
            get
            {
                return isExists;
            }
        }
    }
}
