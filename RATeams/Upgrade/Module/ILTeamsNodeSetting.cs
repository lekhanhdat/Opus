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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;

namespace RATeams.Upgrade.Module
{
    public class ILTeamsNodeSetting : RMSharePointSetting
    {
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ILTeamsNodeSetting))
                return false;

            ILTeamsNodeSetting other = (ILTeamsNodeSetting)obj;

            // 比较所有属性
            return ColumnName == other.ColumnName
                && TermStoreId == other.TermStoreId
                && TermSetId == other.TermSetId
                && TermId == other.TermId
                && DefaultTermId == other.DefaultTermId
                && TermSetName == other.TermSetName
                && Description == other.Description
                && TermName == other.TermName
                && DefaultTermName == other.DefaultTermName
                && DescriptionOfContainer == other.DescriptionOfContainer
                && IsInheritParentTerm == other.IsInheritParentTerm
                && TermNameOfContainer == other.TermNameOfContainer
                && TermIdOfContainer == other.TermIdOfContainer
                && isEnableClassification == other.isEnableClassification
                && isFailedConfigClassification == other.isFailedConfigClassification
                && isFailedConfigMetaDataColumn == other.isFailedConfigMetaDataColumn
                && IsEnableHoldPhyical == other.IsEnableHoldPhyical
                && ExistColumnName == other.ExistColumnName
                && IsUsingExistColumnName == other.IsUsingExistColumnName
                && SetDocLevelTermForExistColumn == other.SetDocLevelTermForExistColumn
                && HaveConfigSetting == other.HaveConfigSetting
                && NeedCheckDefaultValue == other.NeedCheckDefaultValue
                && EMailToRecordOwner == other.EMailToRecordOwner
                && IsDisplyaTermPath == other.IsDisplyaTermPath
                && ApplyExistType == other.ApplyExistType
                && EnableRelatedRecords == other.EnableRelatedRecords
                && EnableRecordManagement == other.EnableRecordManagement
                && IncludeDeclaredRecords == other.IncludeDeclaredRecords
                && ColumnRequired == other.ColumnRequired
                && ColumnHidden == other.ColumnHidden
                && IsShowUniqueId == other.IsShowUniqueId
                && IsRunning == other.IsRunning
                && SharePointSettingJobId == other.SharePointSettingJobId
                && DeployTermMethod == other.DeployTermMethod
                && AutoJobOption == other.AutoJobOption
                && RunAutoFullJob == other.RunAutoFullJob
                && IsSyncData == other.IsSyncData
                && ApprovalType == other.ApprovalType
                && WorkflowReferenceId == other.WorkflowReferenceId
                && ApplyTermIncludeFolder == other.ApplyTermIncludeFolder
                && IsKeepSharePointDefaultValue == other.IsKeepSharePointDefaultValue
                && AITermUseType == other.AITermUseType
                && AIApprovalType == other.AIApprovalType
                && AISendEMail == other.AISendEMail
                && AIThenIsDefaultTermMethod == other.AIThenIsDefaultTermMethod
                && AIThenDefaultTermId == other.AIThenDefaultTermId
                && AIThenDefaultTermName == other.AIThenDefaultTermName
                && SetTermForEmptyDefaultValue == other.SetTermForEmptyDefaultValue
                && AlwaysScanAllExistDocuments == other.AlwaysScanAllExistDocuments
                && AutoClassificationRules == other.AutoClassificationRules;
        }

        // 重写 GetHashCode 方法，基于所有比较的属性
        public override int GetHashCode()
        {
            unchecked // 防止整数溢出
            {
                int hash = 17;

                hash = hash * 23 + (ColumnName?.GetHashCode() ?? 0);
                hash = hash * 23 + TermStoreId.GetHashCode();
                hash = hash * 23 + TermSetId.GetHashCode();
                hash = hash * 23 + TermId.GetHashCode();
                hash = hash * 23 + DefaultTermId.GetHashCode();
                hash = hash * 23 + (TermSetName?.GetHashCode() ?? 0);
                hash = hash * 23 + (Description?.GetHashCode() ?? 0);
                hash = hash * 23 + (TermName?.GetHashCode() ?? 0);
                hash = hash * 23 + (DefaultTermName?.GetHashCode() ?? 0);
                hash = hash * 23 + (DescriptionOfContainer?.GetHashCode() ?? 0);
                hash = hash * 23 + IsInheritParentTerm.GetHashCode();
                hash = hash * 23 + (TermNameOfContainer?.GetHashCode() ?? 0);
                hash = hash * 23 + TermIdOfContainer.GetHashCode();
                hash = hash * 23 + isEnableClassification.GetHashCode();
                hash = hash * 23 + isFailedConfigClassification.GetHashCode();
                hash = hash * 23 + isFailedConfigMetaDataColumn.GetHashCode();
                hash = hash * 23 + IsEnableHoldPhyical.GetHashCode();
                hash = hash * 23 + (ExistColumnName?.GetHashCode() ?? 0);
                hash = hash * 23 + IsUsingExistColumnName.GetHashCode();
                hash = hash * 23 + SetDocLevelTermForExistColumn.GetHashCode();
                hash = hash * 23 + HaveConfigSetting.GetHashCode();
                hash = hash * 23 + NeedCheckDefaultValue.GetHashCode();
                hash = hash * 23 + EMailToRecordOwner.GetHashCode();
                hash = hash * 23 + IsDisplyaTermPath.GetHashCode();
                hash = hash * 23 + ApplyExistType.GetHashCode();
                hash = hash * 23 + EnableRelatedRecords.GetHashCode();
                hash = hash * 23 + EnableRecordManagement.GetHashCode();
                hash = hash * 23 + IncludeDeclaredRecords.GetHashCode();
                hash = hash * 23 + (ColumnRequired?.GetHashCode() ?? 0);
                hash = hash * 23 + (ColumnHidden?.GetHashCode() ?? 0);
                hash = hash * 23 + (IsShowUniqueId?.GetHashCode() ?? 0);
                hash = hash * 23 + IsRunning.GetHashCode();
                hash = hash * 23 + (SharePointSettingJobId?.GetHashCode() ?? 0);
                hash = hash * 23 + DeployTermMethod.GetHashCode();
                hash = hash * 23 + AutoJobOption.GetHashCode();
                hash = hash * 23 + RunAutoFullJob.GetHashCode();
                hash = hash * 23 + IsSyncData.GetHashCode();
                hash = hash * 23 + ApprovalType.GetHashCode();
                hash = hash * 23 + (WorkflowReferenceId?.GetHashCode() ?? 0);
                hash = hash * 23 + (ApplyTermIncludeFolder?.GetHashCode() ?? 0);
                hash = hash * 23 + IsKeepSharePointDefaultValue.GetHashCode();
                hash = hash * 23 + AITermUseType.GetHashCode();
                hash = hash * 23 + AIApprovalType.GetHashCode();
                hash = hash * 23 + AISendEMail.GetHashCode();
                hash = hash * 23 + AIThenIsDefaultTermMethod.GetHashCode();
                hash = hash * 23 + AIThenDefaultTermId.GetHashCode();
                hash = hash * 23 + (AIThenDefaultTermName?.GetHashCode() ?? 0);
                hash = hash * 23 + SetTermForEmptyDefaultValue.GetHashCode();
                hash = hash * 23 + AlwaysScanAllExistDocuments.GetHashCode();
                hash = hash * 23 + (AutoClassificationRules?.GetHashCode() ?? 0); 
                return hash;
            }
        }
    }
}
