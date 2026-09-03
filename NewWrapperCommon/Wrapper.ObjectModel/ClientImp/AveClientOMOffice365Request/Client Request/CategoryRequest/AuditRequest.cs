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
namespace AvePoint.ObjectModel.ClientOM
{

    using System;
    using AvePoint.Wrapper.Common;
    using System.Collections.Generic;

    public partial class AveClientOMOffice365Request
        : AveClientOM2019Request
    {

        [ReplaceByAPI]
        public override AveRequestAudit GetAuditValues()
        {
            try
            {
                using (var context = CreateContext(mWebUrl))
                {
                    context.Load(context.Site, site => site.AuditLogTrimmingRetention, site => site.TrimAuditLog);
                    context.Load(context.Site.Audit, audit => audit.AuditFlags);
                    context.ExecuteQuery();

                    return new AveRequestAudit()
                    {
                        AuditFlags = (AveAuditMaskType)context.Site.Audit.AuditFlags,
                        AuditLogTrimmingRetention = context.Site.AuditLogTrimmingRetention,
                        TrimAuditLog = context.Site.TrimAuditLog,
                    };
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get site audit flags failed. Message:{0}", ex);
            }
            return new AveRequestAudit();
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> UpdateAudit(Dictionary<string, object> needUpdateProperties)
        {
            return base.UpdateAudit(needUpdateProperties);
        }
    }
}
