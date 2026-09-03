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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Model;
using RAGoogle.Models;

namespace RAGoogle.Helper
{
    public static class ConvertHelper
    {

        #region convert setting node
        public static GoogleSettingDto ConvertRMSetting2Dto(RMGoogleSetting setting)
        {

            return new()
            {
                LabelId = setting.LabelId,
                LabelName = setting.LabelName,
                DefaultLabelId = setting.DefaultLabelId,
                DefaultLabelName = setting.DefaultLabelName,
                ContainerId = setting.ContainerId.ToString(),
                DriveId = setting.DriveId.ToString(),
                FullPath = setting.FullPath,
                AutoClassificationRules = setting.AutoClassificationRules != null ? SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(setting.AutoClassificationRules) : null,
                RunAutoFullJob = setting.RunAutoFullJob,
                DeployLabelMethod = (DeployLabelMethod)setting.DeployLabelMethod,
                AutoJobOption = (AutoJobOption)setting.AutoJobOption,
                ApplyExistType = setting.ApplyExistType,
                NeedCheckDefaultValue = setting.NeedCheckDefaultValue,
                EnableRecordManagement = setting.EnableRecordManagement,
                IsActive = setting.IsActive,
                EnableSyncData = setting.EnableSyncData,
                NodeInfo = setting.NodeInfo != null ? SerializerHelper.DeserializeByDataContractSerializer<RMGoogleTreeNode>(setting.NodeInfo) : null,
                AITermUseType = setting.AITermUseType,
                AIThenDefaultTermId = setting.AIThenDefaultTermId.ToString(),
                AIThenDefaultTermName = setting.AIThenDefaultTermName,
                AIThenIsDefaultTermMethod = setting.AIThenIsDefaultTermMethod,
                AIApprovalType = (int)setting.AIApprovalType,

            };
        }
        #endregion

        #region convert tree node
        public static GoogleDriveData ConvertDtoNodeTreeToData(GoogleDriveTreeNodeDto node, string tenantId)
        {
            bool isShared = false;
            if (node.Level == NodeLevel.GoogleSharedDrive)
            {
                isShared = true;
            }
            return new GoogleDriveData()
            {
                Id = node.ObjectId,
                TenantId = tenantId,
                DriveName = isShared ? node.ObjectId : node.DisplayName, // DriveName is used to store the email of the my drive, and drive id of the shared drive
                Name = node.DisplayName, // my drive is user email and shared drive is name.
                Type = isShared ? DriveType.SharedDrive : DriveType.MyDrive,
                Level = node.Level,
            };
        }

        public static GoogleDriveTreeNodeDto ConvertGoogleRM2Dto(RMGoogleTreeNode node)
        {
            return new()
            {
                ID = node.Id,
                Name = node.Name ?? "",
                Title = node.Title ?? "",
                FullPath = node.FullPath ?? node.Name,
                Level = (NodeLevel)node.Level,
                DisplayName = node.DisplayName,
                Expanded = node.Expanded,
                ChildrenCount = node.ChildrenCount,
                CheckNumber = node.CheckNumber,
                Parent = node.Parent != null ? ConvertGoogleRM2Dto(node.Parent) : null,
                Children = node.Children?.ConvertAll(x => ConvertGoogleRM2Dto(x)),
                ParentId = node.ParentId,
                NodeId = node.DriveId,
                ContainerId = node.ContainerId,
                ObjectId = node.ObjectId,
                TenantId = node.GoogleTenantId,
                PredictionModeType = (int)node.PredictionModeType,
                IsNodeProcessFromGControl = node.IsNodeProcessFromGControl
            };
        }
        #endregion
    }
}