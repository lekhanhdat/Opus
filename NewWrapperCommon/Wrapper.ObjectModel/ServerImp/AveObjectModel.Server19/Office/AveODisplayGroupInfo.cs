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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveODisplayGroupInfo : IAveODisplayGroupInfo
    {
        private DisplayGroupInfo mDisplayGroupInfo;

        public AveODisplayGroupInfo(DisplayGroupInfo displayGroupInfo)
        {
            mDisplayGroupInfo = displayGroupInfo;
        }

        public AveODisplayGroupInfo()
        {
            mDisplayGroupInfo = new DisplayGroupInfo();
        }

        internal DisplayGroupInfo DisplayGroupInfo
        {
            get
            {
                return mDisplayGroupInfo;
            }
        }

        public string ConsumerName
        {
            get
            {
                return mDisplayGroupInfo.ConsumerName;
            }
            set
            {
                mDisplayGroupInfo.ConsumerName = value;
            }
        }

        public int DefaultScopeID
        {
            get
            {
                return mDisplayGroupInfo.DefaultScopeID;
            }
            set
            {
                mDisplayGroupInfo.DefaultScopeID = value;
            }
        }

        public string Description
        {
            get
            {
                return mDisplayGroupInfo.Description;
            }
            set
            {
                mDisplayGroupInfo.Description = value;
            }
        }

        public bool DisplayInAdminUI
        {
            get
            {
                return mDisplayGroupInfo.DisplayInAdminUI;
            }
            set
            {
                mDisplayGroupInfo.DisplayInAdminUI = value;
            }
        }

        public int ID
        {
            get
            {
                return mDisplayGroupInfo.Id;
            }
            set
            {
                mDisplayGroupInfo.Id = value;
            }
        }

        public bool IsDeleted
        {
            get
            {
                return mDisplayGroupInfo.IsDeleted;
            }
            set
            {
                mDisplayGroupInfo.IsDeleted = value;
            }
        }

        public bool IsUndeletable
        {
            get
            {
                return mDisplayGroupInfo.IsUndeletable;
            }
            set
            {
                mDisplayGroupInfo.IsUndeletable = value;
            }
        }

        public string LastModifiedBy
        {
            get
            {
                return mDisplayGroupInfo.LastModifiedBy;
            }
            set
            {
                mDisplayGroupInfo.LastModifiedBy = value;
            }
        }

        public DateTime LastModifiedTime
        {
            get
            {
                return mDisplayGroupInfo.LastModifiedTime;
            }
            set
            {
                mDisplayGroupInfo.LastModifiedTime = value;
            }
        }

        public string Name
        {
            get
            {
                return mDisplayGroupInfo.Name;
            }
            set
            {
                mDisplayGroupInfo.Name = value;
            }
        }

        public string SiteUrl
        {
            get
            {
                return mDisplayGroupInfo.SiteUrl;
            }
            set
            {
                mDisplayGroupInfo.SiteUrl = value;
            }
        }
    }
}
