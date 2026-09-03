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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.Services.AccountManager;
using Azure.ResourceManager.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ControlPanel.Utils
{
    public class SecurityValidateUtil
    {

        //private static readonly RALogger logger = RALogger.GetInstance(typeof(SecurityGroupManagementService));
        private SecurityGroupDto _group;
        private Func<List<SecurityDataSourceScopeDto>, Task<bool>> _checkDataSourceLicenseAction = null;
        private List<SourceFlag> _sourceFlagsList = new();
        private ILicenseHelperService _licenseHelperService;
        public SecurityValidateUtil() { }

        public SecurityValidateUtil(SecurityGroupDto group, ILicenseHelperService LicenseHelperService, Func<List<SecurityDataSourceScopeDto>, Task<bool>> action = null)
        {
            _group = group;
            _licenseHelperService = LicenseHelperService;
            if (action != null)
            {
                _checkDataSourceLicenseAction = action;
            }
            InitNeedValidateDataSource();
        }

        public async System.Threading.Tasks.Task CheckSecurityGroupAsync()
        {
            if (string.IsNullOrEmpty(_group.Name.Trim()))
            {
                throw new Exception("It's not legal group.");
            }

            if (_group.SecurityGroupControlType == SecurityGroupControlType.DataScope && ExistInvalidGroupScopesInfo())
            {
                throw new Exception("It's not legal group.");
            }

            if (_licenseHelperService.HasOpusILLicense)
            {
                if (_checkDataSourceLicenseAction != null && !await _checkDataSourceLicenseAction(_group.DataSourceScopeInfo))
                {
                    throw new Exception("It's not legal group.");
                }
            }
        }

        private bool ExistInvalidGroupScopesInfo()
        {
            List<SecurityDataSourceScopeDto> groupScopesInfo = _group.DataSourceScopeInfo;
            if (groupScopesInfo == null || groupScopesInfo.Count == 0)
            {
                return true;
            }

            if (!groupScopesInfo.Any(o => _sourceFlagsList.Contains(o.DataSourceType)))
            {
                return true;
            }
            
            if (_sourceFlagsList.Any(o => !IsValidScopeInfo(o)))
            {
                return true;
            }

            return false;
        }

        private void InitNeedValidateDataSource()
        {
            if (_licenseHelperService.HasOpusILLicense)
            {
                _sourceFlagsList.AddRange(new List<SourceFlag> {
                    SourceFlag.SharePoint,
                    SourceFlag.Exchange,
                    SourceFlag.Physical,
                    SourceFlag.FileSystem,
                    SourceFlag.SharePointOnPrem,
                    SourceFlag.OneDrive,
                    SourceFlag.AzureFileShare,
                    SourceFlag.Box,
                    SourceFlag.Teams,
                });
            }

            if(_licenseHelperService.HasOpusGoogleLicense)
            {
                if(!_sourceFlagsList.Contains(SourceFlag.Physical))
                    _sourceFlagsList.Add(SourceFlag.Physical);
                if (!_sourceFlagsList.Contains(SourceFlag.FileSystem))
                    _sourceFlagsList.Add(SourceFlag.FileSystem);
                _sourceFlagsList.Add(SourceFlag.Google);
            }

            if (_licenseHelperService.HasOpusSOLicense)
            {
                if (!_sourceFlagsList.Contains(SourceFlag.SharePoint))
                {
                    _sourceFlagsList.Add(SourceFlag.SharePoint);
                }
                if (!_sourceFlagsList.Contains(SourceFlag.OneDrive))
                {
                    _sourceFlagsList.Add(SourceFlag.OneDrive);
                }
                if (!_sourceFlagsList.Contains(SourceFlag.Teams))
                {
                    _sourceFlagsList.Add(SourceFlag.Teams);
                }
            }
        }
       
        private bool IsValidScopeInfo(SourceFlag sourceType)
        {
            var groupScopesInfo = _group.DataSourceScopeInfo;
            var isValidScope = true;
            var scopeInfo = groupScopesInfo.Where(o => o.DataSourceType == sourceType).FirstOrDefault();
            if (scopeInfo != null)
            {
                switch (sourceType)
                {
                    case SourceFlag.SharePoint:
                    case SourceFlag.Exchange:
                    case SourceFlag.OneDrive:
                    case SourceFlag.Teams:
                        if (scopeInfo.ScopeIds == null || scopeInfo.ScopeIds.Count == 0)
                        {
                            isValidScope = false;
                        }
                        break;
                    case SourceFlag.Physical:
                        if (scopeInfo.SubPermission != SubPermissionType.Admin && scopeInfo.SubPermission != SubPermissionType.EndUser)
                        {
                            isValidScope = false;
                        }
                        break;
                    default:
                        break;
                }
            }
            return isValidScope;
        }
    }
}
