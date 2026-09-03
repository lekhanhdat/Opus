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
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.I18N.Core;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Google.Audit
{
    public class RMDiscoveryGoogleConfigurationAuditCommentUtils
    {
        private readonly IRMDiscoveryGoogleSizeRangeDao _sizeRangeDao = new RMDiscoveryGoogleSizeRangeDao();

        private readonly IRMDiscoveryGoogleFileExtensionDao _fileExtensionDao = new RMDiscoveryGoogleFileExtensionDao();

        private readonly IRMDiscoveryGoogleWithoutInDateDao _withoutInDateDao = new RMDiscoveryGoogleWithoutInDateDao();

        private readonly IRMDiscoveryGoogleRuleInfoDao _ruleInfoDao = new RMDiscoveryGoogleRuleInfoDao();
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
        public async Task<(string sizeRangesOrRotRuleName, string fileTypeName, string dateRangesName)> GetProfileDetailsAsync(AuditCategory category, RMDiscoveryGoogleProfileDataInfo profileInfo)
        {
            var newSizeRangesOrRotRuleName = string.Empty;

            if (category == AuditCategory.InactiveData)
            {
                var newSizeRages = await _sizeRangeDao.GetAsync(profileInfo.SizeRange);
                newSizeRangesOrRotRuleName = newSizeRages?.DisplayName ?? string.Empty;
                if (profileInfo.SizeRange == -1)
                {
                    newSizeRangesOrRotRuleName = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll");
                }

            }
            if (category == AuditCategory.ROTData)
            {
                var newRotRule = await _ruleInfoDao.GetByIdsAsync(profileInfo.RuleIds.ToArray());
                newSizeRangesOrRotRuleName = string.Join(";", newRotRule.Select(item => item.Name));
            }

            var newFileType = await _fileExtensionDao.GetAsync(profileInfo.OrganizationId, profileInfo.FileExtensionIds);
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

            return (newSizeRangesOrRotRuleName, newFileTypeName, newDateRangesName);
        }
    }
}
