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
using AvePoint.RA.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMSynchronize.SyncNodeFromAOS
{
    public class RMSyncNodeConverter
    {
        public static string ContainerNameConvertToDB(string containerName)
        {
            if(RMConstants.DEFAULT_O365_GROUP == containerName)
            {
                return RMConstants.DEFAULT_O365_SITES_GROUP;
            }

            return containerName;
        }

        public static string ContainerNameConvertToJobDetail(string containerName) => containerName switch
        {
            RMConstants.DEFAULT_O365_SITES_GROUP => "RM_SPS_DefaultGroupTeamSiteContainer",
            RMConstants.DEFAULT_SPSITES_GROUP => "RM_SPS_DefaultSharePointSitesGroup",
            RMConstants.DEFAULT_SKYDRIVEPROS_GROUP => "RM_SPS_DefaultOneDriveforBusinessGroup",
            RMConstants.DefaultPrivateChannelSitesGroup => "RM_SPS_DefaultPrivateChannelSitesContainer",
            RMConstants.DEFAULT_MAILBOX_GROUP => "RM_EXO_Default_Container",
            RMConstants.DEFAULT_GOOGLE_USER_GROUP => "RM_GoogleUser_Default_Container",
            RMConstants.DEFAULT_GOOGLE_SHARED_DRIVE_GROUP => "RM_GoogleSharedDrive_Default_Container",
            _ => containerName
        };
        
    }
}
