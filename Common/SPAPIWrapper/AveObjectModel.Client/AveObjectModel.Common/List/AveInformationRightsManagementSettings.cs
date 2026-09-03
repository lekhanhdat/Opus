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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveInformationRightsManagementSettings : AveClientObject, IAveInformationRightsManagementSettings
    {
        private IAveRequest mRequest;
        private IAveList mList;

        public AveInformationRightsManagementSettings(IAveRequest request, IAveList list, Dictionary<string, object> settings)
        {
            mRequest = request;
            mList = list;
            base.DataCache.AddPropertyies(settings);
        }

        #region Properties
        public bool AllowPrint
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowPrint");
            }
            set
            {
                if (!AllowPrint.Equals(value))
                {
                    base.DataCache.AddChangedProperty("AllowPrint", value);
                }
            }
        }

        public bool AllowScript
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowScript");
            }
            set
            {
                if (!AllowScript.Equals(value))
                {
                    base.DataCache.AddChangedProperty("AllowScript", value);
                }
            }
        }

        public bool AllowWriteCopy
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowWriteCopy");
            }
            set
            {
                if (!AllowWriteCopy.Equals(value))
                {
                    base.DataCache.AddChangedProperty("AllowWriteCopy", value);
                }
            }
        }

        public bool DisableDocumentBrowserView
        {
            get
            {
                return base.DataCache.GetProperty<bool>("DisableDocumentBrowserView");
            }
            set
            {
                if (!DisableDocumentBrowserView.Equals(value))
                {
                    base.DataCache.AddChangedProperty("DisableDocumentBrowserView", value);
                }
            }
        }

        public int DocumentAccessExpireDays
        {
            get
            {
                return base.DataCache.GetProperty<int>("DocumentAccessExpireDays");
            }
            set
            {
                if (!DocumentAccessExpireDays.Equals(value))
                {
                    base.DataCache.AddChangedProperty("DocumentAccessExpireDays", value);
                }
            }
        }

        public DateTime DocumentLibraryProtectionExpireDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("DocumentLibraryProtectionExpireDate");
            }
            set
            {
                if (!DocumentLibraryProtectionExpireDate.Equals(value))
                {
                    base.DataCache.AddChangedProperty("DocumentLibraryProtectionExpireDate", value);
                }
            }
        }

        public bool EnableDocumentAccessExpire
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableDocumentAccessExpire");
            }
            set
            {
                if (!EnableDocumentAccessExpire.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableDocumentAccessExpire", value);
                }
            }
        }

        public bool EnableGroupProtection
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableGroupProtection");
            }
            set
            {
                if (!EnableGroupProtection.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableGroupProtection", value);
                }
            }
        }

        public bool EnableLicenseCacheExpire
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableLicenseCacheExpire");
            }
            set
            {
                if (!EnableLicenseCacheExpire.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableLicenseCacheExpire", value);
                }
            }
        }

        public string GroupName
        {
            get
            {
                return base.DataCache.GetProperty<string>("GroupName");
            }
            set
            {
                if (!string.Equals(GroupName, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("GroupName", value);
                }
            }
        }

        public int LicenseCacheExpireDays
        {
            get
            {
                return base.DataCache.GetProperty<int>("LicenseCacheExpireDays");
            }
            set
            {
                if (!LicenseCacheExpireDays.Equals(value))
                {
                    base.DataCache.AddChangedProperty("LicenseCacheExpireDays", value);
                }
            }
        }

        public string PolicyDescription
        {
            get
            {
                return base.DataCache.GetProperty<string>("PolicyDescription");
            }
            set
            {
                if (!string.Equals(PolicyDescription, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("PolicyDescription", value);
                }
            }
        }

        public string PolicyTitle
        {
            get
            {
                return base.DataCache.GetProperty<string>("PolicyTitle");
            }
            set
            {
                if (!string.Equals(PolicyTitle, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("PolicyTitle", value);
                }
            }
        }
        #endregion

        #region Methods
        public void Reset()
        {
            Dictionary<string, object> resetProperties = mRequest.ResetListInformationRightsManagementSettings(mList.ParentWebUrl, mList.ID);
            base.DataCache.UpdateProperties(resetProperties);
        }

        public void Update()
        {
            Dictionary<string, object> resetProperties = mRequest.UpdateListInformationRightsManagementSettings(mList.ParentWebUrl, mList.ID, base.DataCache.ChangedProperties);
            base.DataCache.UpdateProperties(resetProperties);
        }
        #endregion
    }
}
