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
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Salesforce;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Audit;

public class RMSalesforceDiscoveryConfigurationBeforeAuditHandler : IAsyncAuditBeforeHandler
{
    private readonly IRMDiscoveryConfigurationDao _configInfoDao = new RMDiscoveryConfigurationDao();
    private const string S_ScopeType = "RM_RC_Audit_Discovery_ScopeType";
    private const string S_SFSCopeType = "Salesforce";
    private const string S_SFTenant = "Salesforce Tenant";
    public async Task<RMAuditInfo> CollectAsync(RMAuditInfo auditInfo, AuditModule module, AuditAction action,
        AuditCategory category, object[] args)
    {
        if (action == AuditAction.SaveDiscoveryConfiguration)
        {
            var _oldSalesforceInfo = await GetSalesforceInfo();
            
            var scopeAudit = new AuditItem
            {
                TargetSetting = S_ScopeType,
                OldValue = S_SFSCopeType,
                NewValue = S_SFSCopeType
            };

            auditInfo.ModifyContent.Add(scopeAudit);

            AuditItem sfTenant = new()
            {
                TargetSetting = S_SFTenant,
                OldValue = string.Join(";\n ", _oldSalesforceInfo.Organizations.Select(organization => organization.Name)),
            };
            auditInfo.ModifyContent.Add(sfTenant);
        }

        return auditInfo;
    }

    private async Task<RMDiscoverySalesforceScopeInfo> GetSalesforceInfo()
    {
        try
        {
            var _oldSalesforceInfo =
                await _configInfoDao.GetAsync<RMDiscoverySalesforceScopeInfo>(RMDiscoveryConfigurationType.SalesforceNewlyScope);
            return _oldSalesforceInfo;
        }
        catch
        {
            return new RMDiscoverySalesforceScopeInfo();
        }
    }
}