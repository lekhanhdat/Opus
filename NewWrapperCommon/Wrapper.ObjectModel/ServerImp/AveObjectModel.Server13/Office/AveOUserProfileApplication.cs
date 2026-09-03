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
using AvePoint.Wrapper.Common.Office;
using Microsoft.SharePoint.Administration.Backup;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOUserProfileApplication : AveIisWebServiceApplication, IAveOUserProfileApplication
    {
        private static readonly string mAveOUserProfileApplication_Type = "Microsoft.Office.Server.Administration.UserProfileApplication";
        protected AveOProfileDatabase mProfileDatabase;
        private object mAveOUserProfileApplication;

        public AveOUserProfileApplication(object obj)
            : base((SPIisWebServiceApplication)obj)
        {
            mAveOUserProfileApplication = obj;
        }

        public AveOUserProfileApplication()
            : this(AveAssemblyUtility.CreateInstance(mAveOUserProfileApplication_Type))
        { }

        public IAveOProfileDatabase ProfileDatabase
        {
            get
            {
                return new AveOProfileDatabase((AveAssemblyUtility.GetFieldValue(mAveOUserProfileApplication, "m_ProfileDatabase") as SPDatabase));
            }
        }

        public IAveOSocialDatabase SocialDatabase
        {
            get
            {
                return new AveOSocialDatabase(AveAssemblyUtility.GetFieldValue(mAveOUserProfileApplication, "m_SocialDatabase"));
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "m_rgPartitionIDs is a key")]
        public List<Guid> PartitionIDs
        {
            get
            {
                return (List<Guid>)AveAssemblyUtility.GetPropertyValue(mAveOUserProfileApplication, "PartitionIDs");
            }
        }

        public string GetMySitePortalUrl(AveUrlZone zone, Guid partitionID)
        {
            return (string)AveAssemblyUtility.InvokeMethod(mAveOUserProfileApplication, "GetMySitePortalUrl", new Type[] { typeof(SPUrlZone), typeof(Guid) }, new object[] { (SPUrlZone)zone, partitionID });
        }

    }
}
