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



using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveQuota : AveAutoSerializingObject, IAveQuota
    {
        private SPQuota mQuota;

        public AveQuota(SPQuota quota)
            : base(quota)
        {
            mQuota = quota;
        }

        internal SPQuota Quota
        {
            get
            {
                return mQuota;
            }
        }

        #region IAveQuota Members

        public ushort QuotaID
        {
            get
            {
                return mQuota.QuotaID;
            }
            set
            {
                mQuota.QuotaID = value;
            }
        }

        public int InvitedUserMaximumLevel
        {
            get
            {
                return mQuota.InvitedUserMaximumLevel;
            }
            set
            {
                mQuota.InvitedUserMaximumLevel = value;
            }
        }

        public long StorageMaximumLevel
        {
            get
            {
                return mQuota.StorageMaximumLevel;
            }
            set
            {
                mQuota.StorageMaximumLevel = value;
            }
        }

        public long StorageWarningLevel
        {
            get
            {
                return mQuota.StorageWarningLevel;
            }
            set
            {
                mQuota.StorageWarningLevel = value;
            }
        }

        public double UserCodeMaximumLevel
        {
            get
            {
                return mQuota.UserCodeMaximumLevel;
            }
            set
            {
                mQuota.UserCodeMaximumLevel = value;
            }
        }

        public double UserCodeWarningLevel
        {
            get
            {
                return mQuota.UserCodeWarningLevel;
            }
            set
            {
                mQuota.UserCodeWarningLevel = value;
            }
        }

        #endregion
    }
}
