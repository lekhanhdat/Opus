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
using System.Data.SqlClient;
using System.Text;
using System.Xml;

using LS;
using LS.SPWorkflowProcessor.SerializableObjects;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Utility.I18N;
using System.Diagnostics.CodeAnalysis;

namespace LS.SPWorkflowProcessor
{
    public class SPWorkflowSubListUnit
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        #region Serializable Data
        private SPWorkflowSubListSerializableData mSerializableData = null;
        public SPWorkflowSubListSerializableData SerializableData
        {
            get
            {
                if (mSerializableData == null)
                    mSerializableData = new SPWorkflowSubListSerializableData();
                return mSerializableData;
            }
            set
            {
                mSerializableData = value;
            }
        }
        #endregion

        #region From Serializable Data
        public List<SPWorkflowSubFileUnit> mTemplateFileUnits;
        public List<SPContentTypeUnit> mContentTypeUnits;
        #endregion

        public IAveList mSPList;
        public Dictionary<string, string> mContentTypeIdMapping;
        private SPFieldProcessor mFieldProcessor;
        private List<SPWFProcessorException> mInnerWarnings;


        public SPFieldProcessor FieldProcessor
        {
            get
            {
                if (mFieldProcessor == null)
                    mFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.List);
                return mFieldProcessor;
            }
        }
        public List<SPWFProcessorException> InnerWarnings
        {
            get
            {
                if (mInnerWarnings == null)
                    mInnerWarnings = new List<SPWFProcessorException>();
                return mInnerWarnings;
            }
        }

        public SPWorkflowSubListUnit()
        { }

        public SPWorkflowSubListUnit(SPWorkflowSubListSerializableData data)
        {
            SerializableData = data;

            if (data.mContentTypeDatas != null)
            {
                this.mContentTypeUnits = new List<SPContentTypeUnit>();
                foreach (SPContentTypeSerializableData ctData in data.mContentTypeDatas)
                    this.mContentTypeUnits.Add(new SPContentTypeUnit(ctData));
                data.mContentTypeDatas.Clear();
            }
            if (data.mTemplateFileDatas != null)
            {
                this.mTemplateFileUnits = new List<SPWorkflowSubFileUnit>();
                foreach (SPWorkflowSubFileSerializableData d in data.mTemplateFileDatas)
                    this.mTemplateFileUnits.Add(new SPWorkflowSubFileUnit(d));
                data.mTemplateFileDatas.Clear();
            }
        }

        public void Dispose()
        {
            if (mTemplateFileUnits != null)
                mTemplateFileUnits.Clear();
            if (mContentTypeUnits != null)
            {
                foreach (SPContentTypeUnit unit in mContentTypeUnits)
                {
                    unit.Dispose();
                }
                mContentTypeUnits.Clear();
            }
        }
        #region ************************Backup  Region************************
        public static SPWorkflowSubListUnit GetSubListInfo(IAveList list)
        {
            SPWorkflowSubListUnit listUnit = null;
            try
            {
                listUnit = new SPWorkflowSubListUnit();
                listUnit.SerializableData.mId = list.ID;
                if (list.TemplateFeatureId != null)
                    listUnit.SerializableData.mFeatureId = list.TemplateFeatureId;
                listUnit.SerializableData.mTitle = list.Title;
                listUnit.SerializableData.mUrl = list.RootFolder.ServerRelativeUrl.Substring(list.ParentWeb.ServerRelativeUrl.Length);
                listUnit.SerializableData.mLeafName = listUnit.SerializableData.mUrl.Substring(listUnit.SerializableData.mUrl.LastIndexOf('/') + 1);
                listUnit.SerializableData.mServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
                listUnit.SerializableData.mParentWebServerRelativeUrl = list.ParentWebUrl;
                listUnit.SerializableData.mDescription = list.Description;
                listUnit.SerializableData.mFieldSchema = list.Fields.SchemaXml;
                listUnit.SerializableData.mBaseTypeId = (int)list.BaseType;
                listUnit.SerializableData.mBaseTemplateId = (int)list.BaseTemplate;
                listUnit.SerializableData.mFlags = (long)list.Flags;// LSInvoker.GetProperty(list, "Flags");
                listUnit.SerializableData.mHidden = list.Hidden;

                //StringBuilder cts = new StringBuilder();
                //cts.Append("<ContentTypes>");
                //foreach (SPContentType ct in list.ContentTypes)
                //    cts.Append(ct.SchemaXml);
                //cts.Append("</ContentTypes>");

                //listUnit.mContentTypeSchema = cts.ToString();
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.SubListInfoGetError, ex);
            }

            return listUnit;

        }

        internal static bool GetInfoFromInternalName(string internalName, out string noCodeWorkflowName, out Guid noCodeWorkflowLibId, out int cfgFileItemId, out int cfgFileVersion)
        {
            noCodeWorkflowLibId = Guid.Empty;
            noCodeWorkflowName = string.Empty;
            cfgFileItemId = -1;
            cfgFileVersion = -1;
            try
            {
                //SPDWorkflowDemo <Xoml.4de94745_b540_42f1_a2d7_ed8729d36a59.2.512.-1.0.dll> <Cfg.4de94745_b540_42f1_a2d7_ed8729d36a59.3.512.>
                string[] splitedName = internalName.Split('\n');
                if (splitedName.Length > 1)
                {
                    noCodeWorkflowName = splitedName[0];

                    string cfgName = splitedName[splitedName.Length - 1];
                    if (cfgName.ToLower().StartsWith("<cfg.", StringComparison.OrdinalIgnoreCase) && cfgName.EndsWith(">", StringComparison.OrdinalIgnoreCase))
                    {
                        cfgName = cfgName.Substring(1, cfgName.Length - 2);
                        string[] splitedCfgName = cfgName.Split('.');
                        noCodeWorkflowLibId = new Guid(splitedCfgName[1].Replace('_', '-'));
                        cfgFileItemId = int.Parse(splitedCfgName[2]);
                        cfgFileVersion = int.Parse(splitedCfgName[3]);
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationCannotGetDefinitionFiles, e);
            }
        }

        public static SPWorkflowSubListUnit GenerateSPListUnit(SPWFAssociationUnit assoUnit, AveListTemplateType serverTemplate)
        {
            SPWorkflowSubListUnit listUnit = null;
            IAveWeb web = assoUnit.ParentWeb;
            string noCodeWorkflowName = null;
            int cfgFileItemId = -1;
            int cfgFileVersion = -1;
            Guid listId;
            string listTitle = string.Empty;
            string folderName = string.Empty;
            switch (serverTemplate)
            {
                case AveListTemplateType.Tasks:
                    listId = assoUnit.SerializableData.mTaskListId;
                    listTitle = assoUnit.SerializableData.mTaskListTitle;
                    break;
                case AveListTemplateType.WorkflowHistory:
                    listId = assoUnit.SerializableData.mHistoryListId;
                    listTitle = assoUnit.SerializableData.mHistoryListTitle;
                    break;
                case AveListTemplateType.NoCodeWorkflows:
                    GetInfoFromInternalName(assoUnit.SerializableData.mInternalName, out noCodeWorkflowName, out listId, out cfgFileItemId, out cfgFileVersion);
                    break;
                case AveListTemplateType.WFSVC:
                    IAveList wfSvcList = null;
                    listId = Guid.Empty;
                    string definitionPath = string.Empty;
                    definitionPath = assoUnit.mWorkflowDefinition.Properties.ContainsKey("Definition.Path") ? assoUnit.mWorkflowDefinition.Properties["Definition.Path"] : string.Empty;
                    string fullName = assoUnit.mWorkflowDefinition.Properties.ContainsKey("Definition.FullName") ? assoUnit.mWorkflowDefinition.Properties["Definition.FullName"] : string.Empty;
                    logger.Info("Workflow definitionPath:{0},fullName:{1}", definitionPath, fullName);
                    if (!string.IsNullOrEmpty(fullName))
                    {
                        string tempPath = fullName.Substring(0, fullName.LastIndexOf("/"));
                        if(!tempPath.Equals(definitionPath))
                        {
                            definitionPath = tempPath;
                        }
                    }

                    if (!string.IsNullOrEmpty(definitionPath))
                    {
                        folderName = definitionPath.Replace("/" + assoUnit.ParentWeb.ServerRelativeUrl.Trim(new char[] { '/' }) + "/", string.Empty);
                        //string[] tempPaths = folderName.Split(new char[] { '/' });
                        //listTitle = tempPaths[0];
                        //wfSvcList = assoUnit.ParentWeb.Lists.GetListByName(listTitle, false);
                        logger.Info("the worfklow definition path is {0},listtype:{1}.", definitionPath, serverTemplate.ToString());
                        wfSvcList = assoUnit.ParentWeb.GetList("/" + definitionPath.TrimStart(new char[] { '/' }));
                        if (wfSvcList != null)
                        {
                            listTitle = wfSvcList.Title;
                            listId = wfSvcList.ID;
                        }
                    }
                    if (wfSvcList == null)
                    {
                        wfSvcList = assoUnit.ParentWeb.Lists.GetListByName("wfsvc", false);
                        listId = wfSvcList.ID;
                        listTitle = "wfsvc";
                    }
                    break;
                default:
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationSubListNotSupported);
            }

            IAveList list = null;
            try
            {
                if (!string.IsNullOrEmpty(listTitle))
                {
                    list = web.Lists.GetListByName(listTitle, false);
                    if (list == null)
                    {
                        listTitle = SPWorkflowProcessorRuntime.OnLanguageMapping(LanguageMappingScopeEnum.ListTitle, listTitle);
                        list = web.Lists[listTitle];
                    }
                }
                else
                    list = web.Lists[listId];
                listUnit = GetSubListInfo(list);
                if (listUnit != null && serverTemplate == AveListTemplateType.NoCodeWorkflows)
                {
                    IAveListItem item = list.GetItemById(cfgFileItemId);
                    IAveFolder parentFolder = item.File.ParentFolder;
                    listUnit.mTemplateFileUnits = SPWorkflowSubFileUnit.GenerateSPFileUnitCollection(parentFolder, cfgFileVersion);
                }

                if (listUnit != null && serverTemplate == AveListTemplateType.Tasks)
                {
                    SPContentTypeProcessor ctProc = new SPContentTypeProcessor();
                    listUnit.mContentTypeUnits = ctProc.BackupContentTypes(list.ContentTypes);
                }

                if (listUnit != null && serverTemplate == AveListTemplateType.WFSVC)
                {
                    IAveFolder parentFolder = list.GetFolder(folderName);
                    listUnit.mTemplateFileUnits = SPWorkflowSubFileUnit.GenerateWFSvcFileUnitCollection(parentFolder);
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occured when get workflow releated wf lists.list type:{0};list name:{1};error message:{2}", listTitle, serverTemplate.ToString(), e.ToString());
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationSubListError, e, listTitle);
            }
            return listUnit;
        }
        #endregion

        private static IAveListTemplate GetSPListTemplateByFeatureId(IAveWeb web, Guid featureId, int type)
        {
            IAveListTemplate listTemplate = null;
            try
            {
                foreach (IAveListTemplate t in web.ListTemplates)
                {
                    if (t.FeatureId == featureId && t.Type_Client == type)
                    {
                        listTemplate = t;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, string.Format("An error occurred while finding list template id. WebUrl:{0}, ListFeatureId:{1}\n error message:{2}", web.Url, featureId, e));
                //mLog.Warn(e, "An error occurred while finding list template id. WebUrl:{0}, ListFeatureId:{1}", web.Url, featureId);
            }
            return listTemplate;
        }

        #region ************************Restore Region************************
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")] 
        public static IAveList GetOrCreateSPList(IAveListCollection listCollection, SPWorkflowSubListUnit listUnit, Dictionary<Guid, SPFieldProcessor> fieldProcessors)
        {
            IAveList list = null;
            bool isNewCreated = false;
            try
            {
                try
                {
                    listUnit.SerializableData.mTitle = SPWorkflowProcessorRuntime.OnLanguageMapping(LanguageMappingScopeEnum.ListTitle, listUnit.SerializableData.mTitle);
                    //if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                    //{
                    //    listCollection = listCollection.Web.Site.AllWebs[listCollection.Web.ID].Lists;
                    //}
                    object listObj = listCollection.GetListByName(listUnit.SerializableData.mTitle, false);// LSInvoker.CallMethod(listCollection, "GetListByName", new Type[] { typeof(string), typeof(bool) }, new object[] { listUnit.SerializableData.mTitle, false });
                    if (listObj == null) 
                    {
                        listObj = listCollection.GetListByName(listUnit.SerializableData.mLeafName, false);
                    }
                    list = (listObj == null) ? null : (IAveList)listObj;
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.MappingLanguageError, ex);
                }
              
                if (list == null)
                {
                    try
                    {
                        string tempUrl = listUnit.SerializableData.mServerRelativeUrl.Substring(listUnit.SerializableData.mParentWebServerRelativeUrl.Length).TrimStart('/');
                        Guid listId = Guid.Empty;
                        try
                        {
                            listId = listCollection.Add(listUnit.SerializableData.mTitle, listUnit.SerializableData.mDescription, tempUrl, listUnit.SerializableData.mFeatureId.ToString(), listUnit.SerializableData.mBaseTemplateId, null, AveQuickLaunchOptions.Off);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Create Workflows list exception. Detail: {0}", e.Message);
                            IAveListTemplate template = GetSPListTemplateByFeatureId(listCollection.Web, listUnit.SerializableData.mFeatureId, listUnit.SerializableData.mBaseTemplateId);
                            listId = listCollection.Add(listUnit.SerializableData.mTitle, listUnit.SerializableData.mDescription, template);
                        }
                        list = listCollection[listId];
                        isNewCreated = true;
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.ERROR, WrapperWorkflowResource.CreateWorkflowListError, listUnit.SerializableData.mTitle, e);
                        return list;
                    }
                }
                ArgumentCheck.CheckNotNull(list);
                list.EnableVersioning = (listUnit.SerializableData.mFlags & 0x80L) != 0L;
                if (listUnit.SerializableData.mHidden.HasValue && listUnit.SerializableData.mHidden.Value != list.Hidden)
                {
                    logger.Info("Update list {0}({1}) hidden property from {2} to {3}", list.Title, list.RootFolder.ServerRelativeUrl, list.Hidden, listUnit.SerializableData.mHidden.Value);
                    list.Hidden = listUnit.SerializableData.mHidden.Value;
                }
                else
                {
                    //兼容老数据，需要测试下是否好用
                    bool hidden=Ave2010ListFlags.Hidden((ulong)listUnit.SerializableData.mFlags);
                    if (hidden && !list.Hidden)
                    {
                        list.Hidden = hidden;
                    }
                }
                try
                {
                    list.Update();
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, "List update exception while processing workflow related list, listName:{0}, Detail{1}", list.Title, e.Message);
                }
                if (list.BaseTemplate == AveListTemplateType.Tasks || list.BaseTemplate == AveListTemplateType.TasksWithTimelineAndHierarchy || list.BaseTemplate == AveListTemplateType.WorkflowHistory)
                {
                    object fieldObj = null;
                    fieldObj = list.Fields.GetFieldByInternalName(SPWorkflowCommon.OriginalUniqueIdFieldName, false);// LSInvoker.CallMethod(list.Fields, "GetFieldByDisplayName", new Type[] { typeof(string), typeof(bool) }, new object[] { SPWorkflowCommon.OriginalUniqueIdFieldName, false });
                    IAveField workflowTaskField = (fieldObj == null) ? null : (IAveField)fieldObj;
                    if (workflowTaskField == null)
                    {
                        try
                        {
                            string isWorkflowTaskListFieldSchemaXml = "<Field ID=\"{" + SPWorkflowSubListSerializableData.IsWorkflowTaskListFieldId + "}\" DisplayName=\"" + SPWorkflowCommon.OriginalUniqueIdFieldName + "\" Type=\"Text\" ReadOnly=\"FALSE\" Sortable=\"FALSE\" Filterable=\"FALSE\" EnableLookup=\"TRUE\" Hidden=\"TRUE\" CanToggleHidden=\"TRUE\" ShowInFileDlg=\"FALSE\" DisplaceOnUpgrade=\"TRUE\" TextOnly=\"TRUE\"></Field>";
                            list.Fields.AddFieldAsXml(isWorkflowTaskListFieldSchemaXml, false, AveAddFieldOptions.AddToNoContentType);
                        }
                        catch (Exception e)
                        {
                            listUnit.InnerWarnings.Add(new SPWFProcessorException(SPWFProcessorErrorCode.CannotCreateUniqueIdField, e));
                        }
                    }

                    if (listUnit.mFieldProcessor == null)
                    {
                        if (fieldProcessors.ContainsKey(listUnit.SerializableData.mId))
                            listUnit.mFieldProcessor = fieldProcessors[listUnit.SerializableData.mId];
                        else
                        {
                            listUnit.mFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.List);
                            listUnit.FieldProcessor.InitializeAveFieldCollection(listUnit.SerializableData.mFieldSchema, list.Fields, true);
                            fieldProcessors.Add(listUnit.SerializableData.mId, listUnit.FieldProcessor);
                        }
                    }

                    SPContentTypeProcessor ctProc = new SPContentTypeProcessor();
                    try
                    {
                        ctProc.RestoreListContentTypes(list, listUnit.mContentTypeUnits);
                    }
                    catch (SPWFProcessorException procException)
                    {
                        listUnit.InnerWarnings.Add(procException);
                    }
                    listUnit.mContentTypeIdMapping = ctProc.ContentTypeIdMaping;
                }
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationSubListError, e, listUnit.SerializableData.mServerRelativeUrl);
            }
            listUnit.mSPList = list;

            return list;
        }
        #endregion


        internal SPWorkflowSubListSerializableData FixupSerializableData()
        {
            if (this.mContentTypeUnits != null)
            {
                this.SerializableData.mContentTypeDatas = new List<SPContentTypeSerializableData>();
                foreach (SPContentTypeUnit unit in this.mContentTypeUnits)
                {
                    this.SerializableData.mContentTypeDatas.Add(unit.FixupSerializableData());
                }
            }
            if (this.mTemplateFileUnits != null)
            {
                this.SerializableData.mTemplateFileDatas = new List<SPWorkflowSubFileSerializableData>();
                foreach (SPWorkflowSubFileUnit unit in this.mTemplateFileUnits)
                    this.SerializableData.mTemplateFileDatas.Add(unit.SerializableData);
            }
            return this.SerializableData;
        }
    }
}
