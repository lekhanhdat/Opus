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
using System.Globalization;
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Core.Internal.Restore
{
    public interface IWebImport : IDisposable
    {
        /// <summary>
        /// 获取Web对象
        /// </summary>
        /// <returns>True:成功获取；False：获取失败</returns>
        bool LoadWeb();

        /// <summary>
        /// 是否是app web
        /// </summary>
        bool IsAppWeb { get; }

        /// <summary>
        /// app web 的app instance 是否是installed
        /// </summary>
        bool IsAppInstanceInstalled { get; }

        /// <summary>
        /// 根据初始化的site related Url判断是否回收站里冲突
        /// </summary>
        /// <returns></returns>
        bool IsConflictWithRecycle(); 

        /// <summary>
        /// 创建SPWeb对象
        /// </summary>
        /// <param name="webCreationParameters">Webs.Add()需要的参数</param>
        void CreateWeb(WebCreationParameters webCreationParameters);

        /// <summary>
        /// 删除当前获取的Web对象，包括所有sub site，sub app，lists。
        /// rootWeb只删除sub site，sub app，lists，不删除本身。
        /// </summary>
        bool DeleteWeb();

        /// <summary>
        /// 判断是否是content type workflow association，在外围可以赋值。
        /// </summary>
        bool IsWebContentTypeAssociation { get; set; }

        /// <summary>
        /// Web的语言环境
        /// </summary>
        CultureInfo UICulture { get; }

        void RestoreEventReceiver(List<Wrapper.Common.AveEventReceiverInfo> eventReceivers, SPRestore.SPWebConfigurationRestoreOption sPWebConfigurationRestoreOption, SPRestore.ISPWebImportProfiler profiler);

        void RestoreUsers(List<Wrapper.Common.AveUserInfo> userInfos, SPRestore.SPWebSecurityRestoreOption sPWebSecurityRestoreOption, SPRestore.ISPWebImportProfiler profiler);

        void RestoreGroups(List<Wrapper.Common.AveGroupInfo> groupInfos, SPRestore.SPWebSecurityRestoreOption sPWebSecurityRestoreOption, SPRestore.ISPWebImportProfiler profiler);

        void RestoreRoles(AveRoleInfoList roles, SPRestore.SPWebSecurityRestoreOption sPWebSecurityRestoreOption, SPRestore.ISPWebImportProfiler profiler);

        void InitCurrentLanguageMapping(uint lcId);

        void RestoreRoleAssignments(AveRoleAssignmentInfoList roleAssignments, SPRestore.SPWebSecurityRestoreOption sPWebSecurityRestoreOption, SPRestore.ISPWebImportProfiler profiler);

        void RestoreFeatures(Wrapper.Common.AveFeatureInfoBox featureInfo, SPRestore.SPWebConfigurationRestoreOption spWebConfigurationRestoreOption, SPRestore.ISPWebImportProfiler profiler);

        void RestoreSettings(Wrapper.Common.AveWebSettingInfo webSettingInfo, SPRestore.SPWebConfigurationRestoreOption spWebConfigurationRestoreOption, SPRestore.ISPWebImportProfiler profiler, SPRestore.SPWebSecurityRestoreOption spWebSecurityRestoreOption);

        /// <summary>
        /// 还原web 级别的workflow association。
        /// </summary>
        /// <param name="workflowInfo"></param>
        /// <param name="wfOption"></param>
        /// <param name="profiler"></param>
        void RestoreWorkflowAssociation(Wrapper.Common.AveWorkflowInfo workflowInfo, SPRestore.SPWorkflowRestoreOption wfOption, SPRestore.ISPWebImportProfiler profiler);

        /// <summary>
        /// 还原web level content type上的workflow association。
        /// </summary>
        /// <param name="workflowInfo"></param>
        /// <param name="wfOption"></param>
        /// <param name="profiler"></param>
        void RestoreWebCTWorkflowAssociation(Wrapper.Common.AveWorkflowInfo workflowInfo, SPRestore.SPWorkflowRestoreOption wfOption, SPRestore.ISPWebImportProfiler profiler);


        /// <summary>
        /// 还原web level的workflow instance。
        /// </summary>
        /// <param name="workflowInfo"></param>
        /// <param name="wfOption"></param>
        /// <param name="profiler"></param>
        void RestoreWorkflowInstance(Wrapper.Common.AveWorkflowInfo workflowInfo, SPRestore.SPWorkflowRestoreOption wfOption, SPRestore.ISPWebImportProfiler profiler);


        /// <summary>
        /// 还原web level的workflow schedual。
        /// </summary>
        /// <param name="workflowInfo"></param>
        /// <param name="wfOption"></param>
        /// <param name="profiler"></param>
        void RestoreWorkflowSchedule(Wrapper.Common.AveWorkflowInfo workflowInfo, SPRestore.SPWorkflowRestoreOption wfOption, SPRestore.ISPWebImportProfiler profiler);

        /// <summary>
        /// 还原web level的workflowtemplate。
        /// </summary>
        /// <param name="workflowInfo"></param>
        /// <param name="wfOption"></param>
        /// <param name="profiler"></param>
        void RestoreWorkflowTemplate(Wrapper.Common.AveWorkflowInfo workflowInfo, SPRestore.SPWorkflowRestoreOption wfOption, SPRestore.ISPWebImportProfiler profiler);

        /// <summary>
        /// 还原web level fields。
        /// </summary>
        /// <param name="fieldSchemaXml"></param>
        /// <param name="spWebConfigurationRestoreOption"></param>
        /// <param name="profiler"></param>
        void RestoreWebFields(string fieldSchemaXml, SPRestore.SPWebConfigurationRestoreOption spWebConfigurationRestoreOption, SPRestore.ISPWebImportProfiler profiler);


        #region 不是真正还原功能的方法

        /// <summary>
        /// 如果option中判断不去还原workflow association，则可以将association加到缓存中，在外围可以调用。
        /// </summary>
        /// <param name="workflowInfo"></param>
        /// <param name="spWebConfigurationRestoreOption"></param>
        /// <param name="profiler"></param>
        void CacheNotRestoredWorkflowAssociation(Wrapper.Common.AveWorkflowInfo workflowInfo, SPRestore.SPWorkflowRestoreOption wfOption, SPRestore.ISPWebImportProfiler profiler);

        /// <summary>
        /// 还原web navigation时将对应的navigation info加到post action中处理。
        /// </summary>
        /// <param name="navigationInfoList"></param>
        void AddToNavNodesCache(AveNavigationInfoList navigationInfoList);

        #endregion

        #region for post action
        //bool RestoreMasterPageInfoPostAction(Guid webId, AvePoint.Wrapper.Common.AveWebMasterPageInfo masterWebPageInfo);
        //todo:@mzhang, 对于这种要在postaction中执行的接口方法，命名本身不应该含有PostAction字样。及时不在postaction中调用也会有同样的behavior
        bool RestoreWebLastModifiedTimePostAction(Guid webId, DateTime webLastModifiedTime);
        bool RestoreUrlPostAction(string strKey, string metaValue);
        //todo:@mzhang, RetoreWebAllProperties??
        bool RetoreWebAllPropertiesPostAction(Guid webId, Dictionary<string, string> metaInfoDictionary);
        #endregion

    }

    /// <summary>
    ///  建立这个主要是为了以后添加接口的时候不需要改动07代码了。
    /// </summary>
    abstract class WebImportBase : IWebImport
    {

        protected static IAveLogger logger = AveLogger.GetInstance(typeof(WebImportBase));

        public abstract bool LoadWeb();
        public abstract bool IsAppWeb { get; }
        public abstract bool IsAppInstanceInstalled { get; }
        public abstract bool IsConflictWithRecycle();
        public abstract void CreateWeb(WebCreationParameters webCreationParameters);
        public abstract bool DeleteWeb();
        public abstract CultureInfo UICulture { get; }

        public abstract bool IsWebContentTypeAssociation { get; set; }

        public abstract void InitCurrentLanguageMapping(uint lcId);
        public abstract void RestoreEventReceiver(List<Wrapper.Common.AveEventReceiverInfo> eventReceivers, SPRestore.SPWebConfigurationRestoreOption sPWebConfigurationRestoreOption, SPRestore.ISPWebImportProfiler profiler);
        public abstract void RestoreUsers(List<Wrapper.Common.AveUserInfo> userInfos, SPRestore.SPWebSecurityRestoreOption sPWebSecurityRestoreOption, SPRestore.ISPWebImportProfiler profiler);
        public abstract void RestoreGroups(List<Wrapper.Common.AveGroupInfo> groupInfos, SPRestore.SPWebSecurityRestoreOption sPWebSecurityRestoreOption, SPRestore.ISPWebImportProfiler profiler);
        public abstract void RestoreRoles(AveRoleInfoList roles, SPRestore.SPWebSecurityRestoreOption sPWebSecurityRestoreOption, SPRestore.ISPWebImportProfiler profiler);
        public abstract void RestoreRoleAssignments(AveRoleAssignmentInfoList roleAssignments, SPRestore.SPWebSecurityRestoreOption sPWebSecurityRestoreOption, SPRestore.ISPWebImportProfiler profiler);
     
        public abstract void RestoreFeatures(Wrapper.Common.AveFeatureInfoBox featureInfo, SPRestore.SPWebConfigurationRestoreOption spWebConfigurationRestoreOption, SPRestore.ISPWebImportProfiler profiler);
        public abstract void RestoreSettings(Wrapper.Common.AveWebSettingInfo webSettingInfo, SPRestore.SPWebConfigurationRestoreOption spWebConfigurationRestoreOption, SPRestore.ISPWebImportProfiler profiler, SPRestore.SPWebSecurityRestoreOption spWebSecurityRestoreOption);

        public abstract void RestoreWorkflowAssociation(Wrapper.Common.AveWorkflowInfo workflowInfo, SPRestore.SPWorkflowRestoreOption wfOption, SPRestore.ISPWebImportProfiler profiler);

        public abstract void RestoreWebCTWorkflowAssociation(Wrapper.Common.AveWorkflowInfo workflowInfo, SPRestore.SPWorkflowRestoreOption wfOption, SPRestore.ISPWebImportProfiler profiler);

        public abstract void RestoreWorkflowInstance(Wrapper.Common.AveWorkflowInfo workflowInfo, SPRestore.SPWorkflowRestoreOption wfOption, SPRestore.ISPWebImportProfiler profiler);

        public abstract void RestoreWorkflowSchedule(Wrapper.Common.AveWorkflowInfo workflowInfo, SPRestore.SPWorkflowRestoreOption wfOption, SPRestore.ISPWebImportProfiler profiler);

        public abstract void RestoreWorkflowTemplate(Wrapper.Common.AveWorkflowInfo workflowInfo, SPRestore.SPWorkflowRestoreOption wfOption, SPRestore.ISPWebImportProfiler profiler);

        public abstract void RestoreWebFields(string fieldSchemaXml, SPRestore.SPWebConfigurationRestoreOption spWebConfigurationRestoreOption, SPRestore.ISPWebImportProfiler profiler);

        #region 不是真正还原功能的方法

        public abstract void CacheNotRestoredWorkflowAssociation(Wrapper.Common.AveWorkflowInfo workflowInfo, SPRestore.SPWorkflowRestoreOption wfOption, SPRestore.ISPWebImportProfiler profiler);

        public abstract void AddToNavNodesCache(AveNavigationInfoList navigationInfoList);

        #endregion

        #region for post action

        public abstract bool RestoreWebLastModifiedTimePostAction(Guid webId, DateTime webLastModifiedTime);
        public abstract bool RestoreUrlPostAction(string strKey, string metaValue);
        public abstract bool RetoreWebAllPropertiesPostAction(Guid webId, Dictionary<string, string> metaInfoDictionary);
        #endregion
        public void Dispose()
        {
            this.Close();
        }
        protected abstract void Close();
    }
}
