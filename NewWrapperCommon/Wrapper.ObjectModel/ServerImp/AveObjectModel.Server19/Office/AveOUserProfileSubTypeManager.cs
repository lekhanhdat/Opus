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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.UserProfiles;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOUserProfileSubTypeManager : IAveOUserProfileSubTypeManager
    {
        private ProfileSubtypeManager profileSubTypeManager;

        public AveOUserProfileSubTypeManager()
        {
        }

        public AveOUserProfileSubTypeManager(ProfileSubtypeManager profileSubtypeManager)
        {
            this.profileSubTypeManager = profileSubtypeManager;
        }

        public int CountForProfileType(AveProfileType typeID)
        {
            return this.profileSubTypeManager.CountForProfileType((ProfileType)typeID);
        }
        public IAveOProfileSubtype CreateSubtype(string name, string displayName, AveProfileType typeID) 
        {
            return new AveOProfileSubtype(this.profileSubTypeManager.CreateSubtype(name, displayName, (ProfileType)typeID));
        }

        public void DeleteSubtype(string name)
        {
            this.profileSubTypeManager.DeleteSubtype(name);
        }

        public IAveOUserProfileSubTypeManager Get()
        {
            return new AveOUserProfileSubTypeManager(this.profileSubTypeManager = ProfileSubtypeManager.Get());
        }

        public IAveOUserProfileSubTypeManager Get(IAveServiceContext serviceContext)
        {
            return new AveOUserProfileSubTypeManager(this.profileSubTypeManager = ProfileSubtypeManager.Get((serviceContext as AveServiceContext).ServiceContext));
        }

        public string GetDefaultProfileName(AveProfileType type)
        {
            return ProfileSubtypeManager.GetDefaultProfileName((ProfileType)type);
        }

        public IAveOProfileSubtype GetProfileSubtype(int subtypeID)
        {
            return new AveOProfileSubtype(this.profileSubTypeManager.GetProfileSubtype(subtypeID));
        }

        public IAveOProfileSubtype GetProfileSubtype(string subtypeName)
        {
            var type = this.profileSubTypeManager.GetProfileSubtype(subtypeName);
            if (type == null)
            {
                return null;
            }
            return new AveOProfileSubtype(type);
        }

        public ICollection GetSubtypesForProfileType(AveProfileType typeID)
        {
            return this.profileSubTypeManager.GetSubtypesForProfileType((ProfileType)typeID).Cast<ProfileSubtype>().Select<ProfileSubtype, AveOProfileSubtype>(subType => new AveOProfileSubtype(subType)).ToList();
        }
    }
}
