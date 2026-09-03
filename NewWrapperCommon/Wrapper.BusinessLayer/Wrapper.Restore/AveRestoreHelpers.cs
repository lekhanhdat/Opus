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

namespace AvePoint.Wrapper.Restore
{
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Core.SPBackupDto;
    using AvePoint.Wrapper.Core.SPRestore;
    using LS.SPWorkflowProcessor;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// workflow restore helper
    /// </summary>
    //static class AveWorkflowRestoreHelper
    //{
    //    /// <summary>
    //    /// Restore workflow instance
    //    /// </summary>
    //    /// <param name="includePerformanceDetails"></param>
    //    /// <param name="metadata"></param>
    //    /// <param name="listItem"></param>
    //    /// <param name="list"></param>
    //    /// <param name="workflowRestoreOption"></param>
    //    /// <returns></returns>
    //    public static MetadataRestoreReport RestoreWorkflowInstance(bool includePerformanceDetails, AveMetadata metadata, IAveListItem listItem, AveSPList list, SPWorkflowRestoreOption workflowRestoreOption)
    //    {
    //        MetadataRestoreReport report = new MetadataRestoreReport(metadata.MetadataType);

    //        using (AvePoint.Wrapper.Core.Common.WrapperStopwatch.CreateInstance(includePerformanceDetails, report.AddTimeUsage))
    //        {
    //            report.Details = AveWorkflowRestoreHelper.RestoreWorkflowInstance(
    //                                                       metadata.GetMetadata<List<AveWorkflowInfo>>(),
    //                                                       listItem, list,
    //                                                       workflowRestoreOption);
    //        }

    //        return RestoreActionExecutor.ExecuteAction(AveMetadataType.WorkflowInstance,
    //                                                   includePerformanceDetails,
    //                                                   () =>
    //                                                   AveWorkflowRestoreHelper.RestoreWorkflowInstance(
    //                                                       metadata.GetMetadata<List<AveWorkflowInfo>>(),
    //                                                       listItem, list,
    //                                                       workflowRestoreOption));
    //    }

    //    /// <summary>
    //    /// Restore workflow instance
    //    /// 
    //    /// 重载，还原web的workflow instance
    //    /// </summary>
    //    /// <param name="includePerformanceDetails"></param>
    //    /// <param name="metadata"></param>
    //    /// <param name="web"></param>
    //    /// <param name="spWeb"></param>
    //    /// <param name="workflowRestoreOption"></param>
    //    /// <returns></returns>
    //    public static MetadataRestoreReport RestoreWorkflowInstance(bool includePerformanceDetails, AveMetadata metadata, IAveWeb web, AveSPWeb spWeb, SPWorkflowRestoreOption workflowRestoreOption)
    //    {
    //        return RestoreActionExecutor.ExecuteAction(AveMetadataType.WorkflowInstance,
    //                                                   includePerformanceDetails,
    //                                                   () =>
    //                                                   AveWorkflowRestoreHelper.RestoreWorkflowInstance(
    //                                                       metadata.GetMetadata<List<AveWorkflowInfo>>(),
    //                                                       web, spWeb,
    //                                                       workflowRestoreOption));
    //    }

    //    /// <summary>
    //    /// Restore workflow instance
    //    /// </summary>
    //    /// <param name="workflowInfos"></param>
    //    /// <param name="listItem"></param>
    //    /// <param name="list"></param>
    //    /// <param name="restoreOption"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreDetails RestoreWorkflowInstance(List<AveWorkflowInfo> workflowInfos, IAveListItem listItem, AveSPList list, SPWorkflowRestoreOption restoreOption)
    //    {
    //        if (listItem == null)
    //        {
    //            throw new ArgumentNullException("listItem");
    //        }

    //        if (list == null)
    //        {
    //            throw new ArgumentNullException("list");
    //        }

    //        if (restoreOption == null)
    //        {
    //            throw new ArgumentNullException("restoreOption");
    //        }

    //        if (workflowInfos != null && workflowInfos.Count > 0)
    //        {
    //            using (var wfResolution = WFConflictResolution.Instance)
    //            {
    //                //设置还原workflow instance的option
    //                SetRestoreWorkflowInstanceOption(wfResolution, restoreOption);

    //                foreach (var unit in workflowInfos)
    //                {
    //                    var wfAssociationUnit = SPWFInstanceUnit.Load(unit.AssociationUnit);
    //                    wfResolution.HandleInstanceConflict(wfAssociationUnit, listItem);
    //                }
    //            }
    //            //由于对象不一致，导致在还原workflow instance时list.update（UpdateListSettings）出错，现在增加list的reload操作，重新获取一下list对象
    //            list.ReloadList();
    //        }

    //        return new MetadataRestoreDetails();
    //    }

    //    /// <summary>
    //    /// Restore workflow instance
    //    /// 
    //    /// 重载，用于还原web的workflow instance
    //    /// </summary>
    //    /// <param name="workflowInfos"></param>
    //    /// <param name="web"></param>
    //    /// <param name="spWeb"></param>
    //    /// <param name="restoreOption"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreDetails RestoreWorkflowInstance(List<AveWorkflowInfo> workflowInfos, IAveWeb web, AveSPWeb spWeb, SPWorkflowRestoreOption restoreOption)
    //    {
    //        if (web == null)
    //        {
    //            throw new ArgumentNullException("web");
    //        }

    //        if (spWeb == null)
    //        {
    //            throw new ArgumentNullException("spWeb");
    //        }

    //        if (restoreOption == null)
    //        {
    //            throw new ArgumentNullException("restoreOption");
    //        }

    //        if (workflowInfos != null && workflowInfos.Count > 0)
    //        {
    //            using (var wfResolution = WFConflictResolution.Instance)
    //            {
    //                //设置还原workflow instance的option
    //                SetRestoreWorkflowInstanceOption(wfResolution, restoreOption);

    //                foreach (var unit in workflowInfos)
    //                {
    //                    wfResolution.RestoreInstanceData(unit, web);
    //                }
    //            }
    //        }

    //        return new MetadataRestoreDetails();
    //    }

    //    /// <summary>
    //    /// 设置还原workflow instance的option
    //    /// </summary>
    //    /// <param name="wfResolution"></param>
    //    /// <param name="restoreOption"></param>
    //    private static void SetRestoreWorkflowInstanceOption(WFConflictResolution wfResolution, SPWorkflowRestoreOption restoreOption)
    //    {
    //        wfResolution.InstanceOption = restoreOption.ConflictResolutionOption;
    //        SPWorkflowProcessorRuntime.ProcessInstance = true;
    //        SPWorkflowProcessorRuntime.RestoreParentAssociationIfNotFound = restoreOption.RestoreParentAssociationIfNotFound;
    //        switch (restoreOption.RunningWorkflowRestoreAction)
    //        {
    //            case SPRunningWorkflowRestoreAction.Skip:
    //                SPWorkflowProcessorRuntime.SkipRunningInstance = true;
    //                break;
    //            case SPRunningWorkflowRestoreAction.Restart:
    //                SPWorkflowProcessorRuntime.RestartRunningInstance = true;
    //                break;
    //            case SPRunningWorkflowRestoreAction.KeepRunning:
    //                SPWorkflowProcessorRuntime.RestoreHistoryOnly = false;
    //                break;
    //            default:
    //                throw new ArgumentOutOfRangeException();
    //        }
    //    }

    //    /// <summary>
    //    /// Restore workflow schedule
    //    /// 
    //    /// scheduleObj只能是IAveListItem或者IAveWeb
    //    /// </summary>
    //    /// <param name="includePerformanceDetails"></param>
    //    /// <param name="metadata"></param>
    //    /// <param name="scheduleObj"></param>
    //    /// <returns></returns>
    //    public static MetadataRestoreReport RestoreWorkflowSchedule(bool includePerformanceDetails, AveMetadata metadata, object scheduleObj)
    //    {
    //        return RestoreActionExecutor.ExecuteAction(AveMetadataType.WorkflowSchedule,
    //                                                   includePerformanceDetails,
    //                                                   () =>
    //                                                   AveWorkflowRestoreHelper.RestoreWorkflowSchedule(
    //                                                       metadata.GetMetadata<List<AveWorkflowInfo>>(),
    //                                                       scheduleObj));
    //    }

    //    /// <summary>
    //    /// Restore schedule
    //    /// 
    //    /// scheduleObj只能是IAveListItem或者IAveWeb
    //    /// </summary>
    //    /// <param name="schedules"></param>
    //    /// <param name="scheduleObject"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreDetails RestoreWorkflowSchedule(List<AveWorkflowInfo> schedules, object scheduleObject)
    //    {
    //        if (scheduleObject == null)
    //        {
    //            throw new ArgumentNullException("schedule object");
    //        }

    //        if (schedules != null && schedules.Count > 0)
    //        {
    //            using (IWFConflictResolution wfResolution = WFConflictResolution.Instance)
    //            {
    //                // 如果是false则不restore schedule，只调用SPWFInstanceUnit.Load。
    //                SPWorkflowProcessorRuntime.ProcessAssociation = true;
    //                foreach (var unit in schedules)
    //                {
    //                    if (scheduleObject is IAveListItem)
    //                    {
    //                        wfResolution.RestoreScheduleData(unit, (IAveListItem)scheduleObject);
    //                    }
    //                    else if (scheduleObject is IAveWeb)
    //                    {
    //                        wfResolution.RestoreScheduleData(unit, (IAveWeb)scheduleObject);
    //                    }
    //                }
    //            }
    //        }
    //        return new MetadataRestoreDetails();
    //    }

    //    /// <summary>
    //    /// Restore workflow dto
    //    /// </summary>
    //    /// <param name="includePerformanceDetails"></param>
    //    /// <param name="metadata"></param>
    //    /// <param name="listItem"></param>
    //    /// <param name="list"></param>
    //    /// <param name="restoreOption"></param>
    //    /// <returns></returns>
    //    public static MetadataRestoreReport RestoreWorkflowDto(bool includePerformanceDetails, AveMetadata metadata, IAveListItem listItem, AveSPList list, SPWorkflowRestoreOption restoreOption)
    //    {
    //        return RestoreActionExecutor.ExecuteAction(AveMetadataType.WorkflowDto,
    //                                                  includePerformanceDetails,
    //                                                   () =>
    //                                                   {
    //                                                       var dto = metadata.GetMetadata<SPWorkflowDto>();

    //                                                       var instanceDetails =
    //                                                           RestoreWorkflowInstance(dto.Instances, listItem, list,
    //                                                                                   restoreOption);
    //                                                       var scheduleDetails =
    //                                                           RestoreWorkflowSchedule(dto.Schedules, listItem);

    //                                                       return instanceDetails.Combine(scheduleDetails);
    //                                                   });
    //    }

    //    /// <summary>
    //    /// Restore Workflow Associations
    //    /// 
    //    /// restore: true表示要restore，false只cache
    //    /// associationParentObject: IAve*对象
    //    /// </summary>
    //    /// <param name="metadataType"></param>
    //    /// <param name="includePerformanceDetails"></param>
    //    /// <param name="metadata"></param>
    //    /// <param name="option"></param>
    //    /// <param name="restore"></param>
    //    /// <param name="associationParentObject"></param>
    //    public static MetadataRestoreReport RestoreWFAssociation(AveMetadataType metadataType, bool includePerformanceDetails, AveMetadata metadata, SPWorkflowAssociationRestoreOption option, bool restore, object associationParentObject)
    //    {
    //        return RestoreActionExecutor.ExecuteAction(metadataType, includePerformanceDetails,
    //                                                   () =>
    //                                                   {
    //                                                       var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //                                                       WFConflictResolution wfResolution = WFConflictResolution.Instance;

    //                                                       // set options for restore workflow associations
    //                                                       wfResolution.AssociationOption = option.ConflictResolutionOption;
    //                                                       wfResolution.WebContentTypeAssociation = option.WebContentTypeAssociation;
    //                                                       // ProcessAssociation的值如果是false，在RestoreAssociationData中就只CacheAssociationData。
    //                                                       SPWorkflowProcessorRuntime.ProcessAssociation = true;
    //                                                       switch (option.TemplateFileConflictRules)
    //                                                       {
    //                                                           case TemplateFileConflictRules.KeepSource:
    //                                                               {
    //                                                                   SPWorkflowProcessorRuntime.TemplateFileConflictRules = TemplateFileConflictRulesEnum.KeepSource;
    //                                                                   break;
    //                                                               }
    //                                                           case TemplateFileConflictRules.KeepTarget:
    //                                                               {
    //                                                                   SPWorkflowProcessorRuntime.TemplateFileConflictRules = TemplateFileConflictRulesEnum.KeepTarget;
    //                                                                   break;
    //                                                               }
    //                                                       }


    //                                                       wfResolution.AssociationParentObject = associationParentObject;

    //                                                       // restore workflow associations
    //                                                       foreach (AveWorkflowInfo unit in wfInfo)
    //                                                       {
    //                                                           if (restore)
    //                                                           {
    //                                                               wfResolution.RestoreAssociationData(unit);
    //                                                           }
    //                                                           else
    //                                                           {
    //                                                               wfResolution.CacheAssociationData(unit);
    //                                                           }
    //                                                       }

    //                                                       return new MetadataRestoreDetails();
    //                                                   });
    //    }

    //    /// <summary>
    //    /// Restore ContentType Workflow Associations
    //    /// 
    //    /// restore: true表示要restore，false只cache
    //    /// associationParentObject: IAve*对象
    //    /// </summary>
    //    /// <param name="metadataType"></param>
    //    /// <param name="includePerformanceDetails"></param>
    //    /// <param name="metadata"></param>
    //    /// <param name="option"></param>
    //    /// <param name="restore"></param>
    //    /// <param name="associationParentObject"></param>
    //    /// <param name="spContentTypes"></param>
    //    /// <param name="aveContentTypes"></param>
    //    /// <returns></returns>
    //    public static MetadataRestoreReport RestoreCTWFAssociation(AveMetadataType metadataType, bool includePerformanceDetails, AveMetadata metadata, SPWorkflowAssociationRestoreOption option, bool restore, object associationParentObject, AveSPContentTypeCollection spContentTypes, IAveContentTypeCollection aveContentTypes)
    //    {
    //        return RestoreActionExecutor.ExecuteAction(metadataType, includePerformanceDetails,
    //            () =>
    //            {
    //                var ctWFInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //                var ctWFResolution = WFConflictResolution.Instance;

    //                // set还原的选项
    //                // 区分是否是web的content type，如果是false则是list的content type
    //                ctWFResolution.WebContentTypeAssociation = option.WebContentTypeAssociation;
    //                // ProcessAssociation的值如果是false，在RestoreAssociationData中就只CacheAssociationData。
    //                SPWorkflowProcessorRuntime.ProcessAssociation = true;

    //                ctWFResolution.AssociationOption = option.ConflictResolutionOption;

    //                switch (option.TemplateFileConflictRules)
    //                {
    //                    case TemplateFileConflictRules.KeepSource:
    //                        {
    //                            SPWorkflowProcessorRuntime.TemplateFileConflictRules =
    //                                TemplateFileConflictRulesEnum.KeepSource;
    //                            break;
    //                        }
    //                    case TemplateFileConflictRules.KeepTarget:
    //                        {
    //                            SPWorkflowProcessorRuntime.TemplateFileConflictRules =
    //                                TemplateFileConflictRulesEnum.KeepTarget;
    //                            break;
    //                        }
    //                }

    //                foreach (AveWorkflowInfo unit in ctWFInfo)
    //                {
    //                    if (restore)
    //                    {
    //                        string contentTypeId = string.Empty;
    //                        if ((contentTypeId = spContentTypes.ContentTypeMapping.GetMappingRestoredContentTypeId(unit.CTId)) != null)
    //                        {
    //                            IAveContentType wfCT = null;
    //                            foreach (IAveContentType ct in aveContentTypes)
    //                            {
    //                                if (ct.ID.ToString().Equals(unit.CTId))
    //                                {
    //                                    wfCT = ct;
    //                                }
    //                            }
    //                            if (wfCT == null)
    //                            {
    //                                IAveContentType ct = aveContentTypes[unit.CTName];
    //                                wfCT = ct;
    //                            }
    //                            if (wfCT != null)
    //                            {   //重新确认WF association的CTName,避免所属CT由于冲突处理name后缀_1后，通过name查找错误
    //                                unit.CTName = wfCT.Name;
    //                            }
    //                            ctWFResolution.AssociationParentObject = wfCT;
    //                            ctWFResolution.RestoreAssociationData(unit);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        ctWFResolution.CacheAssociationData(unit);
    //                    }
    //                }
    //                return new MetadataRestoreDetails();
    //            });
    //    }
    //}

    /// <summary>
    /// security restore helper
    /// </summary>
    static class AveSecurityRestoreHelper
    {
        /// <summary>
        /// Restore Group Cache
        /// </summary>
        /// <param name="includePerformanceDetails"></param>
        /// <param name="site"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static MetadataRestoreReport RestoreGroupCache(bool includePerformanceDetails, AveSPSite site, AveMetadata metadata)
        {
            return RestoreActionExecutor.ExecuteAction(AveMetadataType.GroupCache, includePerformanceDetails,
                                                       () =>
                                                       {
                                                           site.RestoreGroup(metadata.GetMetadata<AveGroupList>());
                                                           return null;
                                                       }
                );
        }

        /// <summary>
        /// Restore User Cache
        /// </summary>
        /// <param name="includePerformanceDetails"></param>
        /// <param name="site"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static MetadataRestoreReport RestoreUserCache(bool includePerformanceDetails, AveSPSite site, AveMetadata metadata)
        {
            return RestoreActionExecutor.ExecuteAction(AveMetadataType.UserCache, includePerformanceDetails,
                                                       () =>
                                                       {
                                                           site.RestoreUser(metadata.GetMetadata<AveUserList>());
                                                           return null;
                                                       }
                );
        }

        /// <summary>
        /// Restore Inheritance 
        /// </summary>
        /// <param name="includePerformanceDetails"></param>
        /// <param name="metadata"></param>
        /// <param name="securityUtility"></param>
        /// <param name="restoreOption"></param>
        /// <returns></returns>
        public static MetadataRestoreReport RestoreInheritance(bool includePerformanceDetails, AveMetadata metadata, AveObjectSecurity securityUtility, SPRoleAssignmentsRestoreOption restoreOption)
        {
            return RestoreActionExecutor.ExecuteAction(AveMetadataType.RoleAssignmentsDto, includePerformanceDetails,
                                                       () =>
                                                       {
                                                           var dto = new SPRoleAssignmentsDto
                                                           {
                                                               IsInherit = metadata.GetMetadata<bool>()
                                                           };

                                                           return RestoreRoleAssignments(securityUtility, dto,
                                                                                         restoreOption);
                                                       }
                );
        }

        /// <summary>
        /// Restore Role Assignments
        /// </summary>
        /// <param name="includePerformanceDetails"></param>
        /// <param name="metadata"></param>
        /// <param name="securityUtility"></param>
        /// <param name="restoreOption"></param>
        /// <returns></returns>
        public static MetadataRestoreReport RestoreRoleAssignments(bool includePerformanceDetails, AveMetadata metadata, AveObjectSecurity securityUtility, SPRoleAssignmentsRestoreOption restoreOption)
        {
            return RestoreActionExecutor.ExecuteAction(AveMetadataType.RoleAssignmentsDto, includePerformanceDetails,
                                                       () =>
                                                       {
                                                           var dto = new SPRoleAssignmentsDto
                                                           {
                                                               RoleAssignmentInfos =
                                                                   metadata
                                                                       .GetMetadata<List<AveRoleAssignmentInfo>>
                                                                       ()
                                                           };

                                                           return RestoreRoleAssignments(securityUtility, dto,
                                                                                         restoreOption);
                                                       }
                );
        }

        /// <summary>
        /// Restore role assignments dto
        /// </summary>
        /// <param name="restoreDto"></param>
        /// <returns></returns>
        public static MetadataRestoreReport RestoreRoleAssignmentsDto(bool includePerformanceDetails, AveMetadata metadata, AveObjectSecurity securityUtility, SPRoleAssignmentsRestoreOption restoreOption)
        {
            return RestoreActionExecutor.ExecuteAction(AveMetadataType.RoleAssignmentsDto, includePerformanceDetails,
                                                       () =>
                                                       RestoreRoleAssignments(securityUtility,
                                                                              metadata
                                                                                  .GetMetadata
                                                                                  <SPRoleAssignmentsDto>(),
                                                                              restoreOption)
                );
        }

        /// <summary>
        /// Restore Role Assignments
        /// </summary>
        /// <param name="securityUtility"></param>
        /// <param name="roleAssignmentsDto"></param>
        /// <param name="restoreOption"></param>
        /// <returns></returns>
        private static MetadataRestoreDetails RestoreRoleAssignments(AveObjectSecurity securityUtility, SPRoleAssignmentsDto roleAssignmentsDto, SPRoleAssignmentsRestoreOption restoreOption)
        {
            if (restoreOption.RestoreInheritance)
            {
                securityUtility.SourceHasUniqueRoleAssignment = !roleAssignmentsDto.IsInherit;
            }

            securityUtility.ParentSite.RestoreUser(roleAssignmentsDto.UserCache);
            securityUtility.ParentSite.RestoreGroup(roleAssignmentsDto.GroupCache);

            securityUtility.RestoreRoleAssignments(roleAssignmentsDto.RoleAssignmentInfos, restoreOption.ToSecurityRestoreOption());

            return new MetadataRestoreDetails();
        }
    }

    /// <summary>
    /// Social Restore helper
    /// </summary>
    static class AveSocialRestoreHelper
    {
        /// <summary>
        /// Restore Social Info
        /// </summary>
        /// <returns></returns>
        public static MetadataRestoreReport RestoreSocialoDto(bool includePerformanceDetails, AveSPSite site, AveMetadata metadata, string url)
        {
            return RestoreActionExecutor.ExecuteAction(AveMetadataType.SocialDto, includePerformanceDetails,
                                                      () =>
                                                      {
                                                          var dto = metadata.GetMetadata<SPSocialDto>();
                                                          var tagDetails = RestoreSocialTag(dto.Tags, url, site);
                                                          var commentDetails = RestoreSocialComment(dto.Comments, url, site);
                                                          return tagDetails.Combine(commentDetails);
                                                      }
               );
        }

        /// <summary>
        /// Restore Social details
        /// </summary>
        /// <param name="includePerformanceDetails"></param>
        /// <param name="site"></param>
        /// <param name="metadata"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static MetadataRestoreReport RestoreSocialComment(bool includePerformanceDetails, AveSPSite site, AveMetadata metadata, string url)
        {
            return RestoreActionExecutor.ExecuteAction(AveMetadataType.SocialComment, includePerformanceDetails,
                                                       () =>
                                                       RestoreSocialComment(
                                                           metadata.GetMetadata<List<AveSocialCommentInfo>>(), url, site)
                );
        }

        /// <summary>
        /// Restore Social tag
        /// </summary>
        /// <param name="includePerformanceDetails"></param>
        /// <param name="site"></param>
        /// <param name="metadata"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static MetadataRestoreReport RestoreSocialTag(bool includePerformanceDetails, AveSPSite site, AveMetadata metadata, string url)
        {
            return RestoreActionExecutor.ExecuteAction(AveMetadataType.SocialTag, includePerformanceDetails,
                                                       () =>
                                                       RestoreSocialTag(
                                                           metadata.GetMetadata<List<AveSocialTagInfo>>(), url, site)
                );
        }

        /// <summary>
        /// Restore Social tag
        /// </summary>
        /// <param name="includePerformanceDetails"></param>
        /// <param name="site"></param>
        /// <param name="metadata"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static MetadataRestoreReport RestoreDocumentTag(bool includePerformanceDetails, AveSPSite site, AveMetadata metadata, string url)
        {
            return RestoreActionExecutor.ExecuteAction(AveMetadataType.SocialTag, includePerformanceDetails,
                                                       () =>
                                                       RestoreDocumentTag(
                                                           metadata.GetMetadata<List<AveDocumentTaggingInfo>>(), url, site)
                );
        }



        /// <summary>
        /// Restore Social details
        /// </summary>
        /// <param name="commentInfos"></param>
        /// <param name="url"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        private static MetadataRestoreDetails RestoreSocialComment(List<AveSocialCommentInfo> commentInfos, string url,
                                                              AveSPSite site)
        {
            using (var socialComment = new AveSPSocialComment(url, site))
            {
                socialComment.Restore(commentInfos);
            }

            return new MetadataRestoreDetails();
        }

        /// <summary>
        /// Restore social Tags
        /// </summary>
        /// <param name="tagInfos"></param>
        /// <param name="url"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        private static MetadataRestoreDetails RestoreSocialTag(List<AveSocialTagInfo> tagInfos, string url,
                                                              AveSPSite site)
        {
            using (var socialTags = new AveSPSocialTag(url, site))
            {
                socialTags.Restore(tagInfos);
            }

            return new MetadataRestoreDetails();
        }
        /// <summary>
        /// Restore document tag
        /// </summary>
        /// <param name="documentTaggingInfos"></param>
        /// <param name="url"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        private static MetadataRestoreDetails RestoreDocumentTag(List<AveDocumentTaggingInfo> documentTaggingInfos, string url, AveSPSite site)
        {
            using (var docmentTagging = new AveDocumentTagging(url, site))
            {
                docmentTagging.Restore(documentTaggingInfos);
            }

            return new MetadataRestoreDetails();
        }
    }

    /// <summary>
    /// alert restore helper
    /// </summary>
    static class AveAlertRestoreHelper
    {

        /// <summary>
        /// Restore User Cache
        /// </summary>
        /// <param name="includePerformanceDetails"></param>
        /// <param name="site"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        private static MetadataRestoreDetails RestoreUserCache(AveSPSite site, AveUserList userCache)
        {
            var restoreDetails = new MetadataRestoreDetails();
            site.RestoreUser(userCache);
            return restoreDetails;
        }


        /// <summary>
        /// Restore Alert
        /// </summary>
        /// <param name="alert"></param>
        /// <param name="data"></param>
        /// <param name="isSched"></param>
        /// <returns></returns>
        private static MetadataRestoreDetails RestoreAlertSubscriptions(AveSPAlert alert, List<Dictionary<string, object>> data, bool isSched)
        {
            var restoreDetails = new MetadataRestoreDetails();

            if (data != null && data.Count > 0)
            {
                foreach (Dictionary<string, object> val in data)
                {
                    alert.RestoreAlert(val, isSched);
                }
            }

            return restoreDetails;
        }

        /// <summary>
        /// Restore schedule subscriptions
        /// </summary>
        /// <param name="alert"></param>
        /// <param name="metadata"></param>
        /// <param name="includePerformanceDetails"></param>
        /// <returns></returns>
        public static MetadataRestoreReport RestoreDocSchedSubscriptions(bool includePerformanceDetails, AveSPAlert alert, AveMetadata metadata)
        {
            return RestoreActionExecutor.ExecuteAction(AveMetadataType.DocSchedSubscriptions,
                                                       includePerformanceDetails,
                                                       () => RestoreAlertSubscriptions(alert, metadata.GetMetadata<List<Dictionary<string, object>>>(), true)
                                                       );
        }

        /// <summary>
        /// Restore immediately subscriptions
        /// </summary>
        /// <param name="includePerformanceDetails"></param>
        /// <param name="alert"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static MetadataRestoreReport RestoreDocImmedSubscriptions(bool includePerformanceDetails, AveSPAlert alert, AveMetadata metadata)
        {
            return RestoreActionExecutor.ExecuteAction(AveMetadataType.DocImmedSubscriptions,
                                                       includePerformanceDetails,
                                                       () => RestoreAlertSubscriptions(alert, metadata.GetMetadata<List<Dictionary<string, object>>>(), false)
                                                       );
        }

        /// <summary>
        /// Restore schedule
        /// </summary>
        /// <param name="includePerformanceDetails"></param>
        /// <param name="alert"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static MetadataRestoreReport RestoreAlertDto(bool includePerformanceDetails, AveSPAlert alert, AveSPSite site, AveMetadata metadata)
        {
            return RestoreActionExecutor.ExecuteAction(AveMetadataType.DocImmedSubscriptions,
                                                       includePerformanceDetails,
                                                       () =>
                                                       {
                                                           var userCacheRestoreReport =
                                                               RestoreUserCache(site, metadata.GetMetadata<SPAlertsDto>().UserCache);
                                                           var scheduleRestoreReport =
                                                               RestoreAlertSubscriptions(alert, metadata.GetMetadata<SPAlertsDto>().SchedSubscriptions, true);
                                                           var immediatelyRestoreReport =
                                                               RestoreAlertSubscriptions(alert, metadata.GetMetadata<SPAlertsDto>().ImmedSubscriptions, false);

                                                           return userCacheRestoreReport.Combine(scheduleRestoreReport).Combine(immediatelyRestoreReport);
                                                       }
                                                       );
        }
    }
}
