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

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    internal class OpenStackFileInfo : XFileInfo
    {
        private String name;

        public override String Name
        {
            get
            {
                return name;
            }
        }

        public override long FileSize { get; set; }

        //public OpenStackSystem System { set { this.openStackSystem = value; } }

        public override Boolean Exists
        {
            get
            {
                if (this.FileSize == -1)
                {
                    return false;
                }
                return true;
            }
        }


        public OpenStackFileInfo()
        {
        }

        public OpenStackFileInfo(String highName, String lowName, Int64 fileSize)
            : this(highName, lowName)
        {
            this.FileSize = fileSize;
        }

        public OpenStackFileInfo(String highName, String lowName, Int64 fileSize, String modified)
            : this(highName, lowName)
        {
            if (!String.IsNullOrEmpty(modified))
            {
                LastWriteTimeUtc = DateTime.Parse(modified).ToUniversalTime();
            }
            this.FileSize = fileSize;
        }

        public OpenStackFileInfo(String highName, String lowName, Int64 fileSize, String modified, String creation)
            : this(highName, lowName)
        {
            if (!String.IsNullOrEmpty(modified))
            {
                LastWriteTimeUtc = DateTime.Parse(modified).ToUniversalTime();
            }
            if (!String.IsNullOrEmpty(creation))
            {
                CreationTimeUtc = DateTime.Parse(creation).ToUniversalTime();
            }
            this.FileSize = fileSize;
        }

        public OpenStackFileInfo(String highName, String lowName)
        {
            this.name = lowName;
            this.HighName = highName;
            this.LowName = lowName;
        }

        
    }

    internal class OpenStackDirectoryInfo : StorageDirectoryInfo
    {
        Boolean isExists; 
        String name;

        //public Boolean IsExists
        //{
        //    get
        //    {
        //        return isExists;
        //    }
        //    set
        //    {
        //        isExists = value;
        //    }
        //}  // TODO 为什么要加一个属性，已经有Exists这个只读属性了
        public override bool Exists
        {
            get
            {
                return isExists;
            }
        }
        public override string Name
        {
            get
            {
                return name;
            }
        }

        public OpenStackDirectoryInfo()
        {
        }

        public OpenStackDirectoryInfo(String name)
        {
            this.name = name;
        }

        public OpenStackDirectoryInfo(String highName, String lowName)
        {
            HighName = highName;
            name = lowName;
            LowName = lowName;
        }

        public OpenStackDirectoryInfo(String highName, String lowName, Boolean exists)
        {
            HighName = highName;
            name = lowName;
            LowName = lowName;
            this.isExists = exists;
        }


        public OpenStackDirectoryInfo(String highName, String lowName, String modified)
        {
            HighName = highName;
            name = lowName;
            LowName = lowName;
            if (!String.IsNullOrEmpty(modified))
            {
                LastWriteTimeUtc = DateTime.Parse(modified).ToUniversalTime();
            }
        }

        public OpenStackDirectoryInfo(String highName, String lowName, String modified, String creation)
        {
            HighName = highName;
            name = lowName;
            LowName = lowName;
            if (!string.IsNullOrEmpty(modified))
            {
                LastWriteTimeUtc = DateTime.Parse(modified).ToUniversalTime();
            }
            if (!string.IsNullOrEmpty(creation))
            {
                CreationTimeUtc = DateTime.Parse(creation).ToUniversalTime();
            }
        }
    }
}
