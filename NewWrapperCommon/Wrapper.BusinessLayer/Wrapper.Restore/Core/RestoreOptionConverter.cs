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
using AvePoint.Wrapper.Restore;
using LS.SPWorkflowProcessor;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core.SPRestore.Mapping;
using AvePoint.Wrapper.Mapping;

namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// 用来兼容以前的option，这个是负责把新的option转换为旧的option。
    /// </summary>
    internal static class RestoreOptionConverter
    {
        /// <summary>
        /// 把SPFolderRestoreOption转换为restoreOption
        /// </summary>
        /// <param name="spFolderRestoreOption"></param>
        /// <param name="restoreOption"></param>
        /// <returns></returns>
        public static AveRestoreOption ToAveRestoreOption(this SPFolderRestoreOption spFolderRestoreOption, AveRestoreOption restoreOption)
        {
            if (spFolderRestoreOption == null)
            {
                throw new ArgumentNullException("spFolderRestoreOption");
            }

            return ToAveItemRestoreOption(spFolderRestoreOption, restoreOption);
        }

        /// <summary>
        /// 把SPFileRestoreOption转换为restoreOption
        /// </summary>
        /// <param name="spFileRestoreOption"></param>
        /// <returns></returns>
        public static AveRestoreOption ToAveRestoreOption(this SPFileRestoreOption spFileRestoreOption)
        {
            return ToAveRestoreOption(spFileRestoreOption, null);
        }

        ///// <summary>
        ///// 把SPFolderRestoreOption转换为restoreOption
        ///// </summary>
        ///// <param name="spFolderRestoreOption"></param>
        ///// <returns></returns>
        //public static AveRestoreOption ToAveRestoreOption(this SPFolderRestoreOption spFolderRestoreOption)
        //{
        //    return ToAveRestoreOption(spFolderRestoreOption, null);
        //}

        /// <summary>
        /// 把SPListItemRestoreOption转换为restoreOption
        /// </summary>
        /// <param name="spListItemRestoreOption"></param>
        /// <returns></returns>
        public static AveRestoreOption ToAveRestoreOption(this SPListItemRestoreOption spListItemRestoreOption)
        {
            return ToAveRestoreOption(spListItemRestoreOption, null);
        }

        /// <summary>
        /// 把SPFileRestoreOption转换为restoreOption
        /// </summary>
        /// <param name="spFileRestoreOption"></param>
        /// <param name="restoreOption"></param>
        /// <returns></returns>
        public static AveRestoreOption ToAveRestoreOption(this SPFileRestoreOption spFileRestoreOption, AveRestoreOption restoreOption)
        {
            if (spFileRestoreOption == null)
            {
                throw new ArgumentNullException("spFileRestoreOption");
            }

            return ToAveItemRestoreOption(spFileRestoreOption, restoreOption);
        }

        ///// <summary>
        ///// 把SPFolderRestoreOption转换为restoreOption
        ///// </summary>
        ///// <param name="spFolderRestoreOption"></param>
        ///// <param name="restoreOption"></param>
        ///// <returns></returns>
        //public static AveRestoreOption ToAveRestoreOption(this SPFolderRestoreOption spFolderRestoreOption, AveRestoreOption restoreOption)
        //{
        //    if (spFolderRestoreOption == null)
        //    {
        //        throw new ArgumentNullException("spFolderRestoreOption");
        //    }

        //    return ToAveItemRestoreOption(spFolderRestoreOption, spFolderRestoreOption.ConflictCheckOption, restoreOption);
        //}

        /// <summary>
        /// 把SPListItemRestoreOption转换为restoreOption
        /// </summary>
        /// <param name="spListItemRestoreOption"></param>
        /// <param name="restoreOption"></param>
        /// <returns></returns>
        public static AveRestoreOption ToAveRestoreOption(this SPListItemRestoreOption spListItemRestoreOption, AveRestoreOption restoreOption)
        {
            if (spListItemRestoreOption == null)
            {
                throw new ArgumentNullException("spListItemRestoreOption");
            }

            return ToAveItemRestoreOption(spListItemRestoreOption, restoreOption);
        }

        /// <summary>
        /// 把SPItemRestoreOption转换为restoreOption
        /// </summary>
        /// <param name="spItemRestoreOption"></param>
        /// <param name="restoreOption"></param>
        /// <returns></returns>
        private static AveRestoreOption ToAveItemRestoreOption(this SPItemRestoreOption spItemRestoreOption, AveRestoreOption restoreOption)
        {
            if (restoreOption == null)
            {
                restoreOption = new AveRestoreOption();
            }

            restoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME = false;
            restoreOption.mAveItemRestoreOption.DELETE_ITEM = false;

            //switch (conflictCheckOption)
            //{
            //    case SPItemConflictCheckOption.None:
            //        break;
            //    //case SPItemConflictCheckOption.CheckModifiedTime:
            //    //    restoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME = true;
            //    //    break;
            //    //case SPItemConflictCheckOption.CheckNewChanged:
            //    //    break;
            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}

            //var restoreOption = new AveRestoreOption();
            restoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME = false;

            //switch (spItemRestoreOption.RestoreAction)
            //{
            //    case SPItemRestoreAction.Default:
            //        break;
            //    case SPItemRestoreAction.Overwrite:
            //        restoreOption.mAveItemRestoreOption.DELETE_ITEM = true;
            //        break;
            //    //case SPItemRestoreAction.Append:
            //    //    break;
            //    //case SPItemRestoreAction.AppendVersion:
            //    //    break;
            //    case SPItemRestoreAction.Skip:
            //        break;
            //    case SPItemRestoreAction.DiscardCheckOut:
            //        restoreOption.mAveItemRestoreOption.DISCARD_ITEM_ONLY = true;
            //        break;
            //    //case SPItemRestoreAction.MoveToConflictFolder:
            //    //    restoreOption.mAveItemRestoreOption.MOVE_ITEM_TO_CONFLICT_FOLDER = true;
            //    //    break;
            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}

            if (spItemRestoreOption.MetadataRestoreOption != null)
            {
                restoreOption.mAveItemRestoreOption.KEEP_ITEM_TPGUID =
                    spItemRestoreOption.MetadataRestoreOption.KeepTP_GUID;
                restoreOption.mAveItemRestoreOption.VerifyPageLayout =
                    spItemRestoreOption.MetadataRestoreOption.VerifyPageLayout;
            }


            return restoreOption;
        }

        /// <summary>
        /// 把SPRoleAssignmentsRestoreOption转换为SecurityRestoreOption
        /// </summary>
        /// <param name="spRoleAssignmentsRestoreOption"></param>
        /// <returns></returns>
        public static SecurityRestoreOption ToSecurityRestoreOption(
            this SPRoleAssignmentsRestoreOption spRoleAssignmentsRestoreOption)
        {
            if (spRoleAssignmentsRestoreOption == null)
            {
                throw new ArgumentNullException("spRoleAssignmentsRestoreOption");
            }

            var restoreOption = new SecurityRestoreOption
            {
                ConflictResolutionForSecurityObject = spRoleAssignmentsRestoreOption.ConflictResolution ==
                                                      SPRoleAssignmentsConflictResolution.Merge
                                                          ? ConflictResolutionForSecurityObject.Merge
                                                          : ConflictResolutionForSecurityObject.OverWrite,
                ConflictResolutionForPincipal = spRoleAssignmentsRestoreOption.ConflictResolutionPerUser ==
                                                SPRoleAssignmentConflictResolution.Merge
                                                    ? ConflictResolutionForPincipal.Merge
                                                    : ConflictResolutionForPincipal.OverWrite,
                NeedRestore = true,
                MergePermissionFromInheritanceWeb = spRoleAssignmentsRestoreOption.MergePermissionFromInheritance,
                PromotePermissionToRootWeb = spRoleAssignmentsRestoreOption.MergePermissionFromInheritance,
            };

            return restoreOption;
        }

        //public static LS.SPWorkflowProcessor.TemplateFileConflictRulesEnum ToWFTemplateFileConflictRules(this SPWFTemplateFileConflictRules templateFileConflictRules)
        //{
        //    if(templateFileConflictRules == SPWFTemplateFileConflictRules.KeepSource)
        //    {
        //        return LS.SPWorkflowProcessor.TemplateFileConflictRulesEnum.KeepSource;
        //    }

        //    return LS.SPWorkflowProcessor.TemplateFileConflictRulesEnum.KeepTarget;
        //}

        public static void ToWFInstanceSetting(this SPWFInstanceRestoreOption restoreOption)
        {
            if(restoreOption == null)
            {
                throw new ArgumentNullException("restoreOption");
            }

            SPWorkflowProcessorRuntime.RestoreParentAssociationIfNotFound = restoreOption.RestoreParentAssociationIfNotFound;
            SPWorkflowProcessorRuntime.ProcessInstance = true;
            switch(restoreOption.RunningWorkflowRestoreAction)
            {
                case SPRunningWorkflowRestoreAction.Skip:
                    SPWorkflowProcessorRuntime.SkipRunningInstance = true;
                    break;
                case SPRunningWorkflowRestoreAction.Restart:
                    SPWorkflowProcessorRuntime.SkipRunningInstance = false;
                    SPWorkflowProcessorRuntime.RestartRunningInstance = true;
                    break;
                case SPRunningWorkflowRestoreAction.KeepRunning:
                    SPWorkflowProcessorRuntime.SkipRunningInstance = false;
                    SPWorkflowProcessorRuntime.RestoreHistoryOnly = false;
                    break;
            }
        }

        public static MembersRestoreOption ToMembersRestoreOption(this SPUserGroupRestoreOption userGroupRestoreOption, bool isSiteLevel)
        {
            if(userGroupRestoreOption == null)
            {
                throw new ArgumentNullException("userGroupRestoreOption");
            }

            return new MembersRestoreOption()
            {
                NeedDeleteUser = userGroupRestoreOption.UpdateDeletedSetting,
                OverWrite = userGroupRestoreOption.OverWrite,
                SkipWithoutPermissions = userGroupRestoreOption.SkipWithoutPermissions,
                UpdateAdminSetting = userGroupRestoreOption.UpdateAdminSetting,
                IsSiteLevel = isSiteLevel,
            };
        }

        public static ListRestoreOption ToListRestoreOption(this SPListFindOption findOption)
        {
            switch(findOption)
            {
                case SPListFindOption.TitleAndUrl:
                    return ListRestoreOption.TitleAndUrl;
                    break;
                case SPListFindOption.Url:
                    return ListRestoreOption.Url;
                    break;
                default:
                    return ListRestoreOption.Title;
                    break;
            }

            //throw new NotSupportedException(findOption.ToString());
        }

        public static IAveCustomFieldMapping ToIAveFieldMapping(this IFieldMapping fieldMapping, object mappingWebOrListInfo)
        {
            var builtinMapping = fieldMapping as BuiltinFieldMapping;
            if (builtinMapping != null)
            {
                var fatory = new AveCustomFieldMappingForXmlFatory(builtinMapping.XDoc);
                return fatory.GetMappingForListOrWeb(mappingWebOrListInfo);
            }
            var excelMapping = fieldMapping as ExcelFieldMapping;
            if (excelMapping != null)
            {
                return new AveCustomFieldMappingForExcel(excelMapping.ExcelPath);
            }
            var dynamicMapping = fieldMapping as DynamicFieldMapping;
            if (dynamicMapping != null)
            {
                var fatory= new AveCustomFieldMappingForDynamic(dynamicMapping.Assembly, dynamicMapping.FullTypeName);
                return fatory.GetMappingForListOrWeb(mappingWebOrListInfo);
            }
            return null;
        }

        public static IAveCustomContentTypeMapping ToIAveContentTypeMapping(this IContentTypeMapping contentTypeMapping, object mappingWebOrListInfo)
        {
            var builtinCTMapping = contentTypeMapping as BuiltinContentTypeMapping;
            if (builtinCTMapping != null)
            {
                var fatory = new AveCustomContentTypeMappingForXmlFatory(builtinCTMapping.XDoc, false);
                return fatory.GetMappingForListOrWeb(mappingWebOrListInfo);
            }
            var dynamicCTMapping = contentTypeMapping as DynamicContentTypeMapping;
            if (dynamicCTMapping != null)
            {
                var fatory = new AveCustomContentTypeMappingForDynamic(dynamicCTMapping.Assembly, dynamicCTMapping.FullTypeName);
                return fatory.GetMappingForListOrWeb(mappingWebOrListInfo);
            }
            return null;
        }
    }
}
