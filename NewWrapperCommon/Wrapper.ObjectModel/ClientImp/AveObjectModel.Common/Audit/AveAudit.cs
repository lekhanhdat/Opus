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
using System.Diagnostics.CodeAnalysis;

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

        private void GetRequestAudit()
        {
            bool mTrimAuditLog = false;
            AveRequestAudit requestAudit = mRequest.GetAuditValues();
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("AuditFlags",requestAudit.AuditFlags);
            base.DataCache.AddPropertyies(properties);
            mAuditFlags = requestAudit.AuditFlags;
            base.DataCache.PropertiesCache["TrimAuditLog"] = requestAudit.TrimAuditLog;
            base.DataCache.PropertiesCache["AuditLogTrimmingRetention"] = requestAudit.AuditLogTrimmingRetention;
        }

        public AveAuditMaskType AuditFlags
        {
            get
            {
                if (mAuditFlags == default(AveAuditMaskType))
                {
                    GetRequestAudit();
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "auditsettings.aspx is a sharepoint setting page")]
        public void Update()
        {
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                if (!base.DataCache.ChangedProperties.ContainsKey("AuditFlags"))
                {
                    base.DataCache.AddChangedProperty("AuditFlags", AuditFlags);
                }
                Dictionary<string, object> auditProperties = mRequest.UpdateAudit(base.DataCache.ChangedProperties);
                if (auditProperties.ContainsKey("UpdateAuditError"))
                {
                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_SharePointVersionNotSupportAudit);
                }
                base.DataCache.UpdateProperties(auditProperties);
            }
        }

        public int AuditLogTrimmingRetention
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AuditLogTrimmingRetention"))
                {
                    GetRequestAudit();
                }
                return base.DataCache.GetProperty<int>("AuditLogTrimmingRetention");
            }
            set
            {
                base.DataCache.AddChangedProperty("AuditLogTrimmingRetention", value);
            }
        }
        public bool RequestTrimAuditLog
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("TrimAuditLog"))
                {
                    GetRequestAudit();
                }
                return base.DataCache.GetProperty<bool>("TrimAuditLog");
            }
            set
            {
                base.DataCache.AddChangedProperty("TrimAuditLog", value);
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


        public AveAuditMaskType EffectiveAuditMask
        {
            get { return AveAuditMaskType.None; }
		}

        public bool WriteAuditEventUnlimitedData(AveAuditEventType eventId, string eventSource, string xmlData)
        {
            return false;
        }

        public bool WriteAuditEventUnlimitedData(string eventName, string eventSource, string xmlData)
        {
            return false;
    	}
	}
}
