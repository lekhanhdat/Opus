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
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.I18N.Core;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Audit
{
    public class RMDiscoveryOffice365ConfigurationAuditCommentUtils
    {
        private readonly IRMDiscoveryOffice365SizeRangeDao _sizeRangeDao = new RMDiscoveryOffice365SizeRangeDao();

        private readonly IRMDiscoveryOffice365FileExtensionDao _fileExtensionDao = new RMDiscoveryOffice365FileExtensionDao();

        private readonly IRMDiscoveryOffice365WithoutInDateDao _withoutInDateDao = new RMDiscoveryOffice365WithoutInDateDao();

        private readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();

        public string ProfileRenameSortBy(string name)
        {
            return name switch
            {
                "FileTotalSize" => "RM_FA_ROTRule_TreeNode_SizeDataSize",
                "FileSumCount" => "RM_DA_Profile_ProfileFileCount",
                "InactiveFileTotalSize" => "RM_DA_Profile_ProfileInactiveDataSizeGB",
                "InactiveFileSumCount" => "RM_DA_Profile_ProfileInactiveFileCount",
                "RotFileTotalSize" => "RM_FA_ROT_TableColumn_ROTTotalSize",
                "RCategoryFileTotalSize" => "RM_FA_ROTRule_TreeNode_RedundantDataSize",
                "OCategoryFileTotalSize" => "RM_FA_ROTRule_TreeNode_ObsoleteDataSize",
                "TCategoryFileTotalSize" => "RM_FA_ROTRule_TreeNode_TrivialDataSize",
                _ => string.Empty
            };
        }
        public async Task<(string SizeRagesOrrotRuleName, string FileTypeName, string DateRangesName)> GetProfileDetailsAsync(AuditCategory category, RMDiscoveryProfileDataInfo profileInfo)
        {
            var newSizeRagesOrrotRuleName = string.Empty;

            if (category == AuditCategory.InactiveData)
            {
                var newSizeRages = await _sizeRangeDao.GetAsync(profileInfo.SizeRange);
                newSizeRagesOrrotRuleName = newSizeRages?.DisplayName ?? string.Empty;
                if (profileInfo.SizeRange == -1)
                {
                    newSizeRagesOrrotRuleName = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll");
                }

            }
            if (category == AuditCategory.ROTData)
            {
                var newROTRule = await _ruleInfoDao.GetByIdsAsync(profileInfo.RuleIds.ToArray());
                newSizeRagesOrrotRuleName = string.Join(";", newROTRule.Select(item => item.Name));
            }

            var newFileType = await _fileExtensionDao.GetAsync(profileInfo.O365TenantId, profileInfo.FileExtensionIds);
            var newFileTypeName = profileInfo.FileExtensionIds.Count == 0 ?
                I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll") :
                string.Join(", ", newFileType.Select(item => item.Name));

            var greaterThanEqualWithoutInDate = await _withoutInDateDao.GetAsync(profileInfo.GreaterThanEqualWithoutInDate);
            var lessThanEqualWithoutInDate = await _withoutInDateDao.GetAsync(profileInfo.LessThanEqualWithoutInDate);
            var newDateRangesName = $"{I18NEntity.GetString("RM_FA_Inactive_SummaryTab_ModifiedFrom")} {(profileInfo.GreaterThanEqualWithoutInDate == -1 ?
                I18NEntity.GetString("RM_FA_Inactive_ModifiedOption_Latest") :
                greaterThanEqualWithoutInDate.Unit + " " + I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"))} {I18NEntity.GetString("RM_FA_Inactive_SummaryTab_ModifiedTo")} {(profileInfo.LessThanEqualWithoutInDate == 999 ?
                I18NEntity.GetString("RM_FA_Inactive_ModifiedOption_Max") :
                lessThanEqualWithoutInDate.Unit + " " + I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"))}";

            return (newSizeRagesOrrotRuleName, newFileTypeName, newDateRangesName);
        }
    }
}
