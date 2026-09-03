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
    class AveAudit : AveClientObject, IAveAudit
    {
        private IAveRequest mRequest;
        private AveSite mSite;

        public AveAudit(IAveRequest request, AveSite site, Dictionary<string, object> prop)
        {
            mRequest = request;
            mSite = site;
            base.DataCache.AddPropertyies(prop);
        }
        #region IAveAudit Members

        private AveAuditMaskType mAuditFlags;
        public AveAuditMaskType AuditFlags
        {
            get
            {
                if (mAuditFlags == default(AveAuditMaskType))
                {
                    int flags = mRequest.GetAuditFlags();
                    Dictionary<string, object> properties = new Dictionary<string, object>();
                    properties.Add("AuditFlags", flags);
                    base.DataCache.AddPropertyies(properties);
                    mAuditFlags = base.DataCache.GetProperty<AveAuditMaskType>("AuditFlags");
                }
                return mAuditFlags;
            }
            set
            {
                base.DataCache.AddChangedProperty("AuditFlags", (int)value);
            }
        }

        public bool UseAuditFlagCache
        {
            get
            {
                return base.DataCache.GetProperty<bool>("UseAuditFlagCache");
            }
            set
            {
                base.DataCache.AddChangedProperty("UseAuditFlagCache", value);
            }
        }

        public void Update()
        {
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                Dictionary<string, object> auditProperties = mRequest.UpdateAudit(mSite.CompatibilityLevel, base.DataCache.ChangedProperties);
                base.DataCache.UpdateProperties(auditProperties);
            }
        }

        public void TrimAuditLog(DateTime deleteEndDate)
        {
            throw new NotImplementedException();
        }

        #endregion

        public IAveAuditEntryCollection GetEntries(IAveAuditQuery query)
        {
            return null;
        }

        public IAveAuditEntryCollection GetEntries()
        {
            return null;
        }
    }
}
