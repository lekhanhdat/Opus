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
using LS.SPWorkflowProcessor.SerializableObjects;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using AvePoint.Wrapper.Resource.Workflow;

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
        public Dictionary<string, string> AllContentTypeIdMapping;
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
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociation13ModelAPI.BackupAssociationUnit.BackupOneAssociation.BackupNintexWorkflowData.GetSubListInfo"))
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
                    listUnit.SerializableData.mParentWebUrl = list.ParentWeb.Url;
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

        }

        public static bool GetInfoFromInternalName(string internalName, out string noCodeWorkflowName, out Guid noCodeWorkflowLibId, out int cfgFileItemId, out int cfgFileVersion)
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
                    if (cfgName.ToLower(CultureInfo.CurrentCulture).StartsWith("<cfg.", StringComparison.OrdinalIgnoreCase) && cfgName.EndsWith(">", StringComparison.OrdinalIgnoreCase))
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "wfsvc")]
        public static SPWorkflowSubListUnit GenerateSPListUnit(SPWFAssociationUnit assoUnit, AveListTemplateType serverTemplate)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupOneAssociation.GenerateSPListUnit"))
            {
                SPWorkflowSubListUnit listUnit = null;

                IAveWeb web = assoUnit.ParentWeb;
                {
                    string noCodeWorkflowName = null;
                    int cfgFileItemId = -1;
                    int cfgFileVersion = -1;
                    Guid listId;
                    string listTitle = string.Empty;
                    string folderServerRelativeUrl = string.Empty;
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
                            //string definitionPath = string.Empty;
                            folderServerRelativeUrl = assoUnit.mWorkflowDefinition.Properties.ContainsKey("Definition.Path") ? assoUnit.mWorkflowDefinition.Properties["Definition.Path"] : string.Empty;
                            string folderName = string.Empty;
                            if (!string.IsNullOrEmpty(folderServerRelativeUrl))
                            {
                                if (assoUnit.ParentWeb.ServerRelativeUrl.Equals("/"))
                                {
                                    folderName = folderServerRelativeUrl.TrimStart(new char[] { '/' });
                                }
                                else
                                {
                                    folderName = folderServerRelativeUrl.Replace("/" + assoUnit.ParentWeb.ServerRelativeUrl.Trim(new char[] { '/' }) + "/", string.Empty);
                                }
                                string[] tempPaths = folderName.Split(new char[] { '/' });
                                listTitle = tempPaths[0];
                                wfSvcList = assoUnit.ParentWeb.Lists.TryGetList(listTitle);
                                if (wfSvcList != null)
                                {
                                    listId = wfSvcList.ID;
                                }
                            }
                            if (wfSvcList == null)
                            {
                                logger.Debug("Can not get wfsvc list by title: {0}, try get wfsvc list by wfsvc", listTitle);
                                wfSvcList = assoUnit.ParentWeb.Lists.TryGetList("wfsvc");
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
                        var isRootWebList = false;
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
                        {
                            try
                            {
                                list = web.GetList(listId);
                            }
                            catch (Exception e)
                            {
                                //ADO-80069， site collection level的Reusable Nintex Workflow的template 文件存在Root web下的list “wfpub” 下，Nintex template 文件存在Root Web的list “NintexWorkflows”下。故需要特殊处理。
                                logger.Info("Get the list failed while backing up the sub list unit.The Web url:{0},list id:{1}.Error message:{2}.", web.Url, listId, e);
                                list = web.Site.RootWeb.GetList(listId);
                                isRootWebList = true;
                            }
                        }
                        listUnit = GetSubListInfo(list);
                        listUnit.mSerializableData.IsRootWebList = isRootWebList;
                        if (listUnit != null && serverTemplate == AveListTemplateType.NoCodeWorkflows)
                        {
                            IAveListItem item = list.GetItemById(cfgFileItemId);
                            IAveFolder parentFolder = item.File.ParentFolder;
                            Guid BaseAssociationGuid = new Guid(item.File.Properties["BaseAssociationGuid"].ToString());
                            if (!BaseAssociationGuid.Equals(assoUnit.SerializableData.mBaseId))
                            {
                                parentFolder = FindcfgFileParentfolder(list, assoUnit.SerializableData.mBaseId) ?? parentFolder;
                            }
                            listUnit.mTemplateFileUnits = SPWorkflowSubFileUnit.GenerateSPFileUnitCollection(parentFolder, cfgFileVersion);
                        }

                        if (listUnit != null && serverTemplate == AveListTemplateType.Tasks)
                        {
                            SPContentTypeProcessor ctProc = new SPContentTypeProcessor();
                            listUnit.mContentTypeUnits = ctProc.BackupContentTypes(list.ContentTypes);
                        }

                        if (listUnit != null && serverTemplate == AveListTemplateType.WFSVC)
                        {
                            IAveFolder parentFolder = list.GetFolder(folderServerRelativeUrl);
                            listUnit.mTemplateFileUnits = SPWorkflowSubFileUnit.GenerateWFSvcFileUnitCollection(parentFolder);
                        }
                    }
                    catch (SPWFProcessorException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationSubListError, e, listTitle);
                    }
                }

                return listUnit;
            }
        }

        private static IAveFolder FindcfgFileParentfolder(IAveList list, Guid baseAssociationGuid)
        {
            IAveFolder parentFolder = null;
            var camlQuery = QueryAllItemsByGuid(baseAssociationGuid.ToString());
            var items = list.GetItems(camlQuery);
            if (items != null && items.Count != 0)
            {
                if (items.Count > 1)
                {
                    logger.Debug("Get {0} item while querying all items by baseAssociationGuid, we use the first one.", items.Count);
                }
                parentFolder = items[0].File.ParentFolder;
            }
            else
            {
                logger.Error("The item does not match parent folder, but can not get item while querying all items by baseAssociationGuid {0}.", baseAssociationGuid);
                //throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationSubListError, null, list.Title);
            }
            return parentFolder;
        }
        private static AveCamlQuery QueryAllItemsByGuid(string value)
        {
            AveCamlQuery camlQuery = new AveCamlQuery();
            if (!string.IsNullOrEmpty(value))
            {
                camlQuery.ViewXml = string.Format("<View Scope=\"Recursive\"><Query><Where><Eq><FieldRef Name=\"BaseAssociationGuid\" /><Value Type='Text'>{{{0}}}</Value></FieldRef></Eq></Where></Query></View>", value);
                camlQuery.FolderServerRelativeUrl = string.Empty;
            }
            return camlQuery;
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
        public static IAveList GetOrCreateSPList(SPWFAssociationUnit associationUnit,IAveListCollection listCollection, SPWorkflowSubListUnit listUnit, Dictionary<Guid, SPFieldProcessor> fieldProcessors)
        {
            using (AvePerformanceScope pf2 = new AvePerformanceScope("RestoreAssociationUnit.GetOrCreateSPList"))
            {
                IAveList list = null;
                try
                {
                    try
                    {
                        string sourceTitle = listUnit.SerializableData.mTitle;
                        listUnit.SerializableData.mTitle = SPWorkflowProcessorRuntime.OnLanguageMapping(LanguageMappingScopeEnum.ListTitle, listUnit.SerializableData.mTitle);
                        list = listCollection.GetListByName(listUnit.SerializableData.mTitle, false);
                        if (sourceTitle == listUnit.SerializableData.mTitle && list == null)
                        {
                            list = listCollection.GetListByName(listUnit.SerializableData.mLeafName, false);
                        }
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
                            Guid listId;
                            try
                            {
                                listId = listCollection.Add(listUnit.SerializableData.mTitle, listUnit.SerializableData.mDescription, tempUrl, listUnit.SerializableData.mFeatureId.ToString(), listUnit.SerializableData.mBaseTemplateId, null);
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Create Workflows list exception. Detail: {0}", e.Message);
                                IAveListTemplate template = GetSPListTemplateByFeatureId(listCollection.Web, listUnit.SerializableData.mFeatureId, listUnit.SerializableData.mBaseTemplateId);
                                listId = listCollection.Add(listUnit.SerializableData.mTitle, listUnit.SerializableData.mDescription, template);
                            }
                            list = listCollection[listId];
                            if (list.Hidden != listUnit.SerializableData.mHidden)
                            {
                                list.Hidden = listUnit.SerializableData.mHidden;
                                try
                                {
                                    list.Update();
                                }
                                catch (Exception e)
                                {
                                    logger.Log(AveLogLevel.WARN,
                                        "List update exception while processing workflow related list, listName:{0}, Detail{1}",
                                        list.Title, e);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.ERROR, WrapperWorkflowResource.CreateWorkflowListError, listUnit.SerializableData.mTitle, e);
                            return list;
                        }
                    }
                    else
                    {
                        //上面的list是用title取的，尽量保证还原过程中的list都用id取
                        list = listCollection.GetListById(list.ID, true);
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


                    //if related list is the association's parent list, do not need to update version setting
                    if (associationUnit.ParentList != null && list.ID == associationUnit.ParentList.ID)
                    {
                        logger.Debug("Workflow related list and parent list are the same list, don't need to restore it.ListInfo:{0},{1}", list.Title, list.ID);
                        listUnit.mSPList = list;
                        return list;
                    }

                   
                    list.EnableVersioning = (listUnit.SerializableData.mFlags & 0x80L) != 0L;
                    try
                    {
                        list.Update();
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN,
                            "List update exception while processing workflow related list, listName:{0}, Detail{1}",
                            list.Title, e);
                    }

                    if (list.BaseTemplate == AveListTemplateType.Tasks || list.BaseTemplate == (AveListTemplateType)171 || list.BaseTemplate == AveListTemplateType.WorkflowHistory)
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
                                logger.Log(AveLogLevel.DEBUG, "An error occurred while adding field as xml in workflow project, error message: {0}", e);
                                listUnit.InnerWarnings.Add(new SPWFProcessorException(SPWFProcessorErrorCode.CannotCreateUniqueIdField, e));
                            }
                        }

                        SPContentTypeProcessor ctProc = new SPContentTypeProcessor();
                        try
                        {
                            ctProc.RestoreListContentTypes(list, listUnit.mContentTypeUnits);
                        }
                        catch (SPWFProcessorException procException)
                        {
                            logger.Log(AveLogLevel.DEBUG, "An processor error occurred while restoring list content types in workflow project, error message: {0}", procException);
                            listUnit.InnerWarnings.Add(procException);
                        }
                        listUnit.mContentTypeIdMapping = ctProc.ContentTypeIdMaping;
                        listUnit.AllContentTypeIdMapping = ctProc.AllContentTypeIdMaping;
                    }
                }
                catch (Exception e)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationSubListError, e, listUnit.SerializableData.mServerRelativeUrl);
                }
                listUnit.mSPList = list;

                return list;
            }
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

        public SPWorkflowSubListSerializableData Save()
        {
            return FixupSerializableData();
        }

    }
}
