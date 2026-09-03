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
using AvePoint.RA.I18N.Core;

namespace AvePoint.RA.Contract.Common
{
    public class DefaultSecurityContainerNameHelper
    {
        public static string GetI18NName(string containerName)
        {
            if (containerName == RMConstants.DEFAULT_O365_SITES_GROUP)
            {
                return I18N.Core.I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer");
            }
            else if (containerName == RMConstants.DEFAULT_SPSITES_GROUP)
            {
                return I18N.Core.I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup");
            }
            else if (containerName == RMConstants.DEFAULT_SKYDRIVEPROS_GROUP)
            {
                return I18N.Core.I18NEntity.GetString("RM_SPS_DefaultOneDriveforBusinessGroup");
            }
            else if (containerName == RMConstants.DefaultPrivateChannelSitesGroup)
            {
                return I18N.Core.I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer");
            }
            else if (string.Equals(containerName, RMConstants.DEFAULT_MAILBOX_GROUP))
            {
                return I18N.Core.I18NEntity.GetString("RM_EXO_Default_Container");  //"RM_EXO_Default_Container";
            }
            else if (string.Equals(containerName, RMConstants.DEFAULT_O365_GROUPS_GROUP))
            {
                return I18N.Core.I18NEntity.GetString("Default Microsoft 365 Group Mailbox Container"); //"Default Microsoft 365 Group Mailbox Container";
            }
            if (string.Equals(containerName, RMConstants.DEFAULT_GOOGLE_USER_GROUP))
            {
                return I18NEntity.GetString("RM_GoogleUser_Default_Container");
            }
            if (string.Equals(containerName, RMConstants.DEFAULT_GOOGLE_SHARED_DRIVE_GROUP))
            {
                return I18NEntity.GetString("RM_GoogleSharedDrive_Default_Container");
            }
            if (containerName == JobMonitor.JobType.RecordsDisposal.ToString() || containerName == "RM_SP_Virtual_Container")
            {
                return I18NEntity.GetString("RM_SP_Virtual_Container");
            }
            if (containerName == JobMonitor.JobType.OneDriveRecordsDisposal.ToString() || containerName == "RM_OD_Virtual_Container")
            {
                return I18NEntity.GetString("RM_OD_Virtual_Container");
            }
            if (containerName == JobMonitor.JobType.TeamsRecordsDisposal.ToString() || containerName == "RM_Teams_Virtual_Container")
            {
                return I18NEntity.GetString("RM_Teams_Virtual_Container");
            }
            return containerName;
        }
    }
}
