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
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOColleagueManager : IAveOColleagueManager
    {
        private ColleagueManager mColleagueManager;

        public AveOColleagueManager(ColleagueManager colleagueManager)
        {
            this.mColleagueManager = colleagueManager;
        }

        #region IAveColleagueManager Members

        public IAveOColleague[] GetItems()
        {
            Colleague[] colleagues = mColleagueManager.GetItems();
            AveOColleague[] aveColleague = new AveOColleague[colleagues.Length];
            for (int i = 0; i < colleagues.Length; i++)
            {
                aveColleague[i] = new AveOColleague(this, colleagues[i]);
            }
            return aveColleague;
        }

        public IAveOColleague this[IAveOUserProfile userProfile]
        {
            get
            {
                return new AveOColleague(this, mColleagueManager[(userProfile as AveOUserProfile).UserProfile]);
            }
        }

        public bool IsColleague(Guid userId)
        {
            return mColleagueManager.IsColleague(userId);
        }

        public IAveOColleague Create(IAveOUserProfile colleague, AveColleagueGroupType colleagueGroupType, string strGroup, bool isInWorkgroup, AvePrivacy privacyLevel)
        {
            Colleague tempColleague = mColleagueManager.Create((colleague as AveOUserProfile).UserProfile, (ColleagueGroupType)colleagueGroupType, strGroup, isInWorkgroup, (Privacy)privacyLevel);
            return new AveOColleague(this, tempColleague);
        }

        #endregion
    }
}
