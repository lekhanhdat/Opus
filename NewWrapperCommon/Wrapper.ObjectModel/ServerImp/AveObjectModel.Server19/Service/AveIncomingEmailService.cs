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

namespace AvePoint.ObjectModel.Server19
{
    class AveIncomingEmailService : AveService, IAveIncomingEmailService
    {
        private SPIncomingEmailService mIncomingEmailService;

        public AveIncomingEmailService(SPService service)
            : base(service)
        {
            mIncomingEmailService = (SPIncomingEmailService)service;
        }

        public string DirectoryManagementServiceUrl
        {
            get
            {
                return mIncomingEmailService.DirectoryManagementServiceUrl;
            }
            set
            {
                mIncomingEmailService.DirectoryManagementServiceUrl = value;
            }
        }

        public bool DistributionGroupsEnabled
        {
            get
            {
                return mIncomingEmailService.DistributionGroupsEnabled;
            }
            set
            {
                mIncomingEmailService.DistributionGroupsEnabled = value;
            }
        }

        public bool DLsRequireAuthenticatedSenders
        {
            get
            {
                return mIncomingEmailService.DLsRequireAuthenticatedSenders;
            }
            set
            {
                mIncomingEmailService.DLsRequireAuthenticatedSenders = value;
            }
        }

        public string DropFolder
        {
            get
            {
                return mIncomingEmailService.DropFolder;
            }
            set
            {
                mIncomingEmailService.DropFolder = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return mIncomingEmailService.Enabled;
            }
            set
            {
                mIncomingEmailService.Enabled = value;
            }
        }

        public string ServerAddress
        {
            get
            {
                return mIncomingEmailService.ServerAddress;
            }
            set
            {
                mIncomingEmailService.ServerAddress = value;
            }
        }

        public string ServerDisplayAddress
        {
            get
            {
                return mIncomingEmailService.ServerDisplayAddress;
            }
            set
            {
                mIncomingEmailService.ServerDisplayAddress = value;
            }
        }

        public bool RemoteDirectoryManagementService
        {
            get
            {
                return mIncomingEmailService.RemoteDirectoryManagementService;
            }
            set
            {
                mIncomingEmailService.RemoteDirectoryManagementService = value;
            }
        }

        public bool UseAutomaticSettings
        {
            get
            {
                return mIncomingEmailService.UseAutomaticSettings;
            }
            set
            {
                mIncomingEmailService.UseAutomaticSettings = value;
            }
        }

        public bool UseDirectoryManagementService
        {
            get
            {
                return mIncomingEmailService.UseDirectoryManagementService;
            }
            set
            {
                mIncomingEmailService.UseDirectoryManagementService = value;
            }
        }

        public void ChangeIPRestrictionList(string[] value)
        {
            mIncomingEmailService.ChangeIPRestrictionList(value);
        }

        public string[] GetIPRestrictionList()
        {
            return mIncomingEmailService.GetIPRestrictionList();
        }

        public void ScheduleAutomaticSettingsUpdate()
        {
            mIncomingEmailService.ScheduleAutomaticSettingsUpdate();
        }
    }
}
