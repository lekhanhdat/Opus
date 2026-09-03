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
using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOProfileBaseChangeQuery : IAveOProfileBaseChangeQuery
    {
        private ProfileBaseChangeQuery mProfileBaseChangeQuery;
        private AveOUserProfileChangeToken mChangeToken;

        public AveOProfileBaseChangeQuery(ProfileBaseChangeQuery profileBaseChangeQuery)
        {
            mProfileBaseChangeQuery = profileBaseChangeQuery;
        }

        public AveOProfileBaseChangeQuery(bool AllChangeObjectTypes, bool AllChangeTypes)
        {
            mProfileBaseChangeQuery = AveAssemblyUtility.CreateInstance("Microsoft.Office.Server.UserProfiles.ProfileBaseChangeQuery", new Type[] { typeof(bool), typeof(bool) }, new object[] { AllChangeObjectTypes, AllChangeTypes }) as ProfileBaseChangeQuery;
        }

        internal ProfileBaseChangeQuery ProfileBaseChangeQuery
        {
            get
            {
                return mProfileBaseChangeQuery;
            }
        }

        public IAveOUserProfileChangeToken ChangeTokenStart
        {
            get
            {
                return new AveOUserProfileChangeToken(mProfileBaseChangeQuery.ChangeTokenStart);
            }
            set
            {
                mChangeToken = value as AveOUserProfileChangeToken;
                if (mChangeToken != null)
                {
                    mProfileBaseChangeQuery.ChangeTokenStart = mChangeToken.UserProfileChangeToken;
                }
                else
                {
                    mProfileBaseChangeQuery.ChangeTokenStart = null;
                }
            }
        }
    }
}
