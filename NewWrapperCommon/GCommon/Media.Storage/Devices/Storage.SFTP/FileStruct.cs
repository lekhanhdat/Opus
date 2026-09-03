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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Media.Storage.SFTP
{
    struct FileStruct
    {
        private string flags;
        public string Flags
        {
            get { return flags; }
            set { flags = value; }
        }

        private string owner;
        public string Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        private string group;
        public string Group
        {
            get { return group; }
            set { group = value; }
        }

        private bool isDirectory;
        public bool IsDirectory
        {
            get { return isDirectory; }
            set { isDirectory = value; }
        }

        private DateTime createTime;
        public DateTime CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }  
}
