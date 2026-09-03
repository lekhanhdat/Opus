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



using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon.Utility;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOProfileBase : IAveOProfileBase
    {
        private ProfileBase mProfileBase;

        public string DisplayName
        {
            get { return mProfileBase.DisplayName; }
            set { mProfileBase.DisplayName = value; }
        }

        public Guid ID
        {
            get
            {
                return mProfileBase.ID;
            }
        }

        public AveOProfileBase(ProfileBase profileBase)
        {
            mProfileBase = profileBase;
        }

        public Uri PublicUrl
        {
            get
            {
                return mProfileBase.PublicUrl;
            }
        }

        public long RecordId
        {
            get
            {
                return mProfileBase.RecordId;
            }
        }
    }
}
