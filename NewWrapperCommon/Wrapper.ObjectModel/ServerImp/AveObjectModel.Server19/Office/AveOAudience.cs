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



using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.Audience;
using System;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOAudience : IAveOAudience
    {
        private Audience mAudience;

        internal AveOAudience(Audience audience)
        {
            mAudience = audience;
        }

        #region IAveOAudience Members

        public string AudienceName
        {
            get
            {
                return mAudience.AudienceName;
            }
            set
            {
                mAudience.AudienceName = value;
            }
        }

        public Guid AudienceID
        {
            get 
            {
                return mAudience.AudienceID;
            }
        }

        #endregion
    }
}
