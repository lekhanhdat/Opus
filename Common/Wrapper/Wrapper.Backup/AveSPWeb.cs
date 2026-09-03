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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPWeb : IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private AveSPSite mAveSPSite = null;
        private IAveWeb mSPWeb = null;
        private IAveBackupStream mSender = null;
        private IAveBackupRestoreQueryService mQueryService = null;
        private Guid mId;
        private string mName;
        private string mScope = string.Empty;
        private Guid mScopeId;
        private string mTaxonomyList;
        private string timeZoneInfoId;

        public string TaxonomyList
        {
            get { return mTaxonomyList; }
            set { mTaxonomyList = value; }
        }

        protected DateTime mModifyTime;

        public DateTime ModifyTime
        {
            get { return mModifyTime; }
        }

        public IAveBackupRestoreQueryService QueryService
        {
            get { return mQueryService; }
        }

        public IAveBackupStream Sender
        {
            get { return mSender; }
        }

        public AveSPSite ParentSite
        {
            get { return mAveSPSite; }
        }

        public IAveWeb SPWeb
        {
            get { return mSPWeb; }
        }

        public string ScopeString
        {
            get { return mScope; }
        }

        public Guid ScopeId
        {
            get { return mScopeId; }
        }

        public AveSPWeb(AveSPSite _AveSite, Guid _WebId, string _name)
        {
            using (new AvePerformanceScope("Backup.AveSPWeb.Constructor"))
            {
                mAveSPSite = _AveSite;
                mSender = _AveSite.Sender;
                mQueryService = _AveSite.QueryService;
                mId = _WebId;
                mName = _name;
                mReloadWebAndParentForSPRequestTimeout = ReloadWebAndParentInternalForSPRequestTimeout;
                if (mReloadWebAndParentForSPRequestTimeout != null)
                {
                    mReloadWebAndParentForSPRequestTimeout(false);
                }
                mSPWeb = mAveSPSite.SPSite.OpenWeb(mId);//.AllWebs[mId];
                mScope = mSPWeb.ServerRelativeUrl.Substring(1);
                mScopeId = mSPWeb.RoleAssignments.ID;
                //GetTaxonomyList();
                //mReloadWebAndParentForSPRequestTimeout = ReloadWebAndParentInternalForSPRequestTimeout;
                mLog.Debug("Current user{0}\\{1}", System.Environment.UserDomainName, System.Environment.UserName);
            }
        }

        public string Name
        {
            get { return mName; }
        }

        public void Dispose()
        {
            mSPWeb.Dispose();
        }

        public bool HasUniqueRoleAssignments
        {
            get { return mSPWeb.HasUniqueRoleAssignments; }
        }

        public bool HasUniqueRoleDefinitions
        {
            get { return mSPWeb.HasUniqueRoleDefinitions; }
        }

        internal Action<bool> mReloadWebAndParentForSPRequestTimeout;

        public void SetReloadWebAndParentForSPRequestTimeout(Action<bool> reloadMethod)
        {
            mReloadWebAndParentForSPRequestTimeout = reloadMethod;
        }

        /// <summary>
        /// 如果程序运行一天以上，访问Web的一些属性，例如WebPartManager或者CreatList对象，都会出现如下错误：
        /// System.Runtime.InteropServices.COMException (0x80090317): The context has expired and can no longer be used.
        /// </summary>
        /// <param name="ingoreTimeout"></param>
        internal void ReloadWebAndParentInternalForSPRequestTimeout(bool ingoreTimeout)
        {
            if (ingoreTimeout || ParentSite.mSPRequestTimeout.AddHours(ParentSite.mHoursReloadSite) < DateTime.UtcNow)
            {
                this.ParentSite.ReloadSite();
                this.ReloadWeb();
            }
        }

        public void ReloadWeb()
        {
            try
            {
                if (mSPWeb != null)
                {
                    mSPWeb.ReloadWeb();
                }
                //InitializeMembers();
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.WARN, string.Format("Reload web failed. web name:{0}\n error message:{1}", mName, e));
            }
        }

        public void ExportFullTextIndex(IAveBackupStream output, Dictionary<string, object> customFieldValues)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.ExportFullTextIndex"))
            {
                var index = new FullTextIndex() 
                {
                    TimeZoneInfoID = TimeZoneInfoId,
                };
                if (customFieldValues != null)
                {
                    index.SetCustomColumnValues(customFieldValues);
                }
                output.WriteMetadata(AveMetadataType.FullTextIndex, index);
            }
        }

        public void ExportWorkflows(IAveBackupStream output)
        {
            ExportWorkflows(output, null);
        }

        public void ExportWorkflows(IAveBackupStream output,Func<AveWorkflowAssociationInfo, bool> filterFunc)
        {
            LS.SPWorkflowProcessor.SPWorkflowProcessorRuntime.ProcessAssociation = true;

            AveWorkflow webWorkflow = new AveWorkflow();
            webWorkflow.ExportReusableWorkflowTemplates(output, this, filterFunc);
            webWorkflow.ExportWebContentTypeWFAssociation(output, this, filterFunc);
            webWorkflow.ExportWebWFAssociation(output, this, filterFunc);
            //TODO:Not supported in DocAve Online
            //webWorkflow.ExportWebWorkflowInstance(output, this);
        }
        public void ExportUserCustomActions(IAveBackupStream output)
        {
            AveSPUserCustomActionCollection spUserCustomActionCollection = new AveSPWebUserCustomActionCollection(this);
            output.WriteMetadata(AveMetadataType.WebUserCustomAction, spUserCustomActionCollection.GetUserCustomActionInfos());
        }
        public string TimeZoneInfoId 
        {
            get 
            {
                if (string.IsNullOrEmpty(this.timeZoneInfoId))
                {
                    this.timeZoneInfoId = AveTimeZoneUtility.ToTimeZoneInfoId(SPWeb.RegionalSettings.TimeZone.ID);
                }
                return this.timeZoneInfoId;
            }
        }

        public AveFeatureInfoBox GetFeatures()
        {
            var featureManager = AveSPFeature.CreateInstance(this);
            return featureManager.GetFeatures();
        }

        public List<AveEventReceiverInfo> GetEventReceivers()
        {
            var events = AveSPEventReceiver.CreateInstance(this);
            return events.GetReceivers();
        }

        #region add for DPM

        public void ExportFields(IAveBackupStream output, AveBackupOption backupColumnOption = null)
        {
            var fields = AveSPFieldCollection.CreateInstance(this);
            if (backupColumnOption == null)
            {
                backupColumnOption = new AveBackupOption();
            }
            AveFieldCollectionInfo fieldCollectionInfo = fields.GetFieldInfoObj(backupColumnOption);
            //if (backupColumnOption.BeforeExportFieldsAction != null)
            //{
            //    backupColumnOption.BeforeExportFieldsAction(fieldCollectionInfo);
            //}
            if (fieldCollectionInfo.RelatedMetadataInfo.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.MetadataService, fieldCollectionInfo.RelatedMetadataInfo);
            }
            output.WriteMetadata(AveMetadataType.WebField, fieldCollectionInfo.AveSchemaXml);
        }

        public void ExportContentTypes(IAveBackupStream output, List<string> filterContentTypes = null)
        {
            var option = new AveBackupOption();

            if (filterContentTypes != null && filterContentTypes.Count > 0)
            {
                option.BeforeExportContentTypesAction = new Action<AveContentTypeCollectionInfo>(info => FilterContentType(filterContentTypes, info));
            }
            this.ExportContentTypes(output, option);
        }

        private static void FilterContentType(List<string> filterContentTypes, AveContentTypeCollectionInfo result)
        {
            for (int i = result.ContentTypes.Count - 1; i >= 0; --i)
            {
                if (!filterContentTypes.Contains(result.ContentTypes[i].Name))
                {
                    result.ContentTypes.RemoveAt(i);
                }
            }
        }

        public void ExportContentTypes(IAveBackupStream output, AveBackupOption backupContentTypeOption)
        {
            var contentTypes = AveSPContentTypeCollection.CreateInstance(this);
            var result = contentTypes.GetContentTypeCollectionInfoObj();
            if (backupContentTypeOption != null && backupContentTypeOption.BeforeExportContentTypesAction != null)
            {
                backupContentTypeOption.BeforeExportContentTypesAction(result);
            }
            output.WriteMetadata(AveMetadataType.WebContentType.ToString(), result);
        }
        #endregion
    }
}