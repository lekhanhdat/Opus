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

using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using System;

namespace AvePoint.ObjectModel.Server16
{
    public class AveInformationRightsManagementSettings : IAveInformationRightsManagementSettings
    {
        private SPInformationRightsManagementSettings informationRightsManagementSettings;

        public AveInformationRightsManagementSettings(SPInformationRightsManagementSettings informationRightsManagementSettings)
        {
            this.informationRightsManagementSettings = informationRightsManagementSettings;
        }

        #region Properties
        public bool AllowPrint
        {
            get
            {
                return this.informationRightsManagementSettings.AllowPrint;
            }
            set
            {
                this.informationRightsManagementSettings.AllowPrint = value;
            }
        }

        public bool AllowScript
        {
            get
            {
                return this.informationRightsManagementSettings.AllowScript;
            }
            set
            {
                this.informationRightsManagementSettings.AllowScript = value;
            }
        }

        public bool AllowWriteCopy
        {
            get
            {
                return this.informationRightsManagementSettings.AllowWriteCopy;
            }
            set
            {
                this.informationRightsManagementSettings.AllowWriteCopy = value;
            }
        }

        public bool DisableDocumentBrowserView
        {
            get
            {
                return this.informationRightsManagementSettings.DisableDocumentBrowserView;
            }
            set
            {
                this.informationRightsManagementSettings.DisableDocumentBrowserView = value;
            }
        }

        public int DocumentAccessExpireDays
        {
            get
            {
                return this.informationRightsManagementSettings.DocumentAccessExpireDays;
            }
            set
            {
                this.informationRightsManagementSettings.DocumentAccessExpireDays = value;
            }
        }

        public DateTime DocumentLibraryProtectionExpireDate
        {
            get
            {
                return this.informationRightsManagementSettings.DocumentLibraryProtectionExpireDate;
            }
            set
            {
                this.informationRightsManagementSettings.DocumentLibraryProtectionExpireDate = value;
            }
        }

        public bool EnableDocumentAccessExpire
        {
            get
            {
                return this.informationRightsManagementSettings.EnableDocumentAccessExpire;
            }
            set
            {
                this.informationRightsManagementSettings.EnableDocumentAccessExpire = value;
            }
        }

        public bool EnableGroupProtection
        {
            get
            {
                return this.informationRightsManagementSettings.EnableGroupProtection;
            }
            set
            {
                this.informationRightsManagementSettings.EnableGroupProtection = value;
            }
        }

        public bool EnableLicenseCacheExpire
        {
            get
            {
                return this.informationRightsManagementSettings.EnableLicenseCacheExpire;
            }
            set
            {
                this.informationRightsManagementSettings.EnableLicenseCacheExpire = value;
            }
        }

        public string GroupName
        {
            get
            {
                return this.informationRightsManagementSettings.GroupName;
            }
            set
            {
                this.informationRightsManagementSettings.GroupName = value;
            }
        }

        public int LicenseCacheExpireDays
        {
            get
            {
                return this.informationRightsManagementSettings.LicenseCacheExpireDays;
            }
            set
            {
                this.informationRightsManagementSettings.LicenseCacheExpireDays = value;
            }
        }

        public string PolicyDescription
        {
            get
            {
                return this.informationRightsManagementSettings.PolicyDescription;
            }
            set
            {
                this.informationRightsManagementSettings.PolicyDescription = value;
            }
        }

        public string PolicyTitle
        {
            get
            {
                return this.informationRightsManagementSettings.PolicyTitle;
            }
            set
            {
                this.informationRightsManagementSettings.PolicyTitle = value;
            }
        }
        #endregion

        #region Methods
        public void Reset()
        {
            this.informationRightsManagementSettings.Reset();
        }

        public void Update()
        {
            this.informationRightsManagementSettings.Update();
        }
        #endregion
    }
}
