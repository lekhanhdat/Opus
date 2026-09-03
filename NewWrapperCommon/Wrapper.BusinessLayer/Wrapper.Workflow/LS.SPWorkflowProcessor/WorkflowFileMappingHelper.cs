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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace LS.SPWorkflowProcessor
{
    internal class WorkflowFileMappingHelper
    {

        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static WorkflowFileReplaceMapping GenerateMapping(SPWFAssociationUnit associationUnit, SPWorkflowSubListUnit containerListUnit)
        {

            var replaceConfigDic = new Dictionary<string, object>();
            var replaceDic = new Dictionary<string, object>();
            var workflowReplaceMapping = new WorkflowFileReplaceMapping
            {
                ConfigFileMapping = replaceConfigDic,
                TemplateFileMapping = replaceDic
            };

            if (associationUnit == null)
            {
                throw new ArgumentNullException("associationUnit");
            }
            if (containerListUnit == null)
            {
                logger.Warn("containerListUnit is null.");
                return workflowReplaceMapping;
            }

            GenerateWorkflowRelativeIdMapping(associationUnit,workflowReplaceMapping);

            GenerateWorkflowRelativeListIdMapping(associationUnit, containerListUnit, workflowReplaceMapping);

            GenerateWorkflowRelativeContentTypeIdMapping(associationUnit, containerListUnit, workflowReplaceMapping);

            GenerateAssemblyVersionMappping(associationUnit.ParentWeb, workflowReplaceMapping.TemplateFileMapping);

            //todo:wbhu,replace tempalte file version in config file, need move the todo to other place
            return workflowReplaceMapping;
        }

        private static void CheckContentTypeExistanceAndAddMapping(string contentString,SPWFAssociationUnit associationUnit, Dictionary<string, object> outMapping)
        {
            if (associationUnit.TaskListUnit != null && associationUnit.TaskListUnit.AllContentTypeIdMapping != null && contentString != null)
            {
                var avaliableMappings = associationUnit.TaskListUnit.AllContentTypeIdMapping.Where(mapping => contentString.Contains(mapping.Key, StringComparison.OrdinalIgnoreCase));
                outMapping.AddRange(avaliableMappings);
            }
        }

        private static void GenerateWorkflowRelativeContentTypeIdMapping(SPWFAssociationUnit associationUnit, SPWorkflowSubListUnit containerListUnit, WorkflowFileReplaceMapping workflowReplaceMapping)
        {
            foreach (var fileUnit in containerListUnit.mTemplateFileUnits)
            {
                switch (fileUnit.FileType())
                {
                    case SPWorkflowFileContentProcType.Config:
                        
                        string contentString = Encoding.UTF8.GetString(fileUnit.SerializableData.mContent, 0, fileUnit.SerializableData.mContent.Length);
                        CheckContentTypeExistanceAndAddMapping(contentString,associationUnit,workflowReplaceMapping.ConfigFileMapping);
                        break;
                    case SPWorkflowFileContentProcType.Xoml:
                    case SPWorkflowFileContentProcType.Rules:
                        contentString = Encoding.UTF8.GetString(fileUnit.SerializableData.mContent, 0, fileUnit.SerializableData.mContent.Length);
                        CheckContentTypeExistanceAndAddMapping(contentString, associationUnit, workflowReplaceMapping.TemplateFileMapping);
                        break;
                }
            }
        }

        private static void GenerateWorkflowRelativeListIdMapping(SPWFAssociationUnit associationUnit, SPWorkflowSubListUnit containerListUnit, WorkflowFileReplaceMapping workflowReplaceMapping)
        {
            var replaceConfigDic = workflowReplaceMapping.ConfigFileMapping;
            var replaceDic = workflowReplaceMapping.TemplateFileMapping;

            #region Get template library

            IAveWeb templateLibraryParentWeb = associationUnit.mTemplateLibUnit != null && associationUnit.mTemplateLibUnit.SerializableData.IsRootWebList ? associationUnit.ParentWeb.Site.RootWeb : associationUnit.ParentWeb;
            //GetListByName 在client中会通过request重取list，避免取到以前cache的list
            //client 需要重取list，否则list下的folder，file都是从cache中取，如果还原多个version的template文件就会有问题
            if (associationUnit.mTemplateLibUnit != null)
            {
                IAveList templateLibrary = templateLibraryParentWeb.GetListByName(associationUnit.mTemplateLibUnit.SerializableData.mTitle, false);
                if (templateLibrary == null)
                {
                    logger.Debug("Cannot get template library by title {0},trying to get it by name {1}.", containerListUnit.SerializableData.mTitle, containerListUnit.SerializableData.mLeafName);
                    templateLibrary = templateLibraryParentWeb.GetListByName(associationUnit.mTemplateLibUnit.SerializableData.mLeafName, true);
                }
                if (templateLibrary != null)
                {
                    replaceConfigDic.AddEx("TemplateListId", templateLibrary.ID.ToString("B").ToUpper(CultureInfo.InvariantCulture));
                    replaceDic.AddEx(associationUnit.mTemplateLibUnit.SerializableData.mId.ToString().ToUpper(CultureInfo.InvariantCulture), templateLibrary.ID.ToString().ToUpper(CultureInfo.InvariantCulture));
                }
                else
                {
                    logger.Warn("Cannot find workflow template library {0},Workflow Name:{1}", associationUnit.mTemplateLibUnit.SerializableData.mTitle, associationUnit.SerializableData.mName);
                }
            }

            #endregion Get template library 

            IAveList taskList = (associationUnit.mTaskListUnit != null) ? associationUnit.mTaskListUnit.mSPList : null;
            if (associationUnit.mTaskListUnit != null && taskList != null)
            {
                replaceConfigDic.AddEx("TaskListId", taskList.ID.ToString("B").ToUpper(CultureInfo.InvariantCulture));
                replaceDic.AddEx(associationUnit.mTaskListUnit.SerializableData.mId.ToString().ToUpper(CultureInfo.InvariantCulture), taskList.ID.ToString().ToUpper(CultureInfo.InvariantCulture));
            }
            IAveList histList = (associationUnit.mHistListUnit != null) ? associationUnit.mHistListUnit.mSPList : null;
            if (associationUnit.mHistListUnit != null && histList != null)
            {
                replaceConfigDic.AddEx("HistListId", histList.ID.ToString("B").ToUpper(CultureInfo.InvariantCulture));
                replaceDic.AddEx(associationUnit.mHistListUnit.SerializableData.mId.ToString().ToUpper(CultureInfo.InvariantCulture), histList.ID.ToString().ToUpper(CultureInfo.InvariantCulture));
            }
        }

        private static void GenerateWorkflowRelativeIdMapping(SPWFAssociationUnit associationUnit,WorkflowFileReplaceMapping mapping)
        {
            var replaceConfigDic = mapping.ConfigFileMapping;
            var replaceDic = mapping.TemplateFileMapping;
            replaceConfigDic.AddEx("ParentId", associationUnit.ParentId.ToUpper(CultureInfo.InvariantCulture));
            replaceConfigDic.AddEx("BaseID", associationUnit.SerializableData.mBaseId.ToString("B").ToUpper(CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(associationUnit.OriginalParentId) && !string.IsNullOrEmpty(associationUnit.ParentId))
            {
                string originalParentId = associationUnit.OriginalParentId.ToUpper(CultureInfo.InvariantCulture);
                string parentId = associationUnit.ParentId.ToUpper(CultureInfo.InvariantCulture);
                replaceDic.AddEx(originalParentId, parentId);
                //在处理CT association时，原端备份的OriginalParentId就是CT.id.tostring()，而不像list,web那样是id.tostring("B")，所以CT association的OriginalParentId是不存在{}的.
                string originalParentIdTrim = originalParentId.Trim(new char[] {'{', '}'});
                if (!replaceDic.ContainsKey(originalParentIdTrim))
                {
                    replaceDic.AddEx(originalParentIdTrim, parentId.Trim(new char[] {'{', '}'}));
                }
            }

            #region ParentContentTypeId Mapping

            object tempSiteCT = string.IsNullOrEmpty(associationUnit.reusableWFContentTypeName) ? null : associationUnit.ParentWeb.ContentTypes[associationUnit.reusableWFContentTypeName];
            if (tempSiteCT == null)
            {
                if (!string.IsNullOrEmpty(associationUnit.reusableWFContentTypeName) && associationUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
                {
                    tempSiteCT = associationUnit.ParentContentType.ParentList.ContentTypes[associationUnit.reusableWFContentTypeName].Parent;
                }
            }
            if (tempSiteCT != null)
            {
                replaceConfigDic.AddEx("ContentTypeId", ((IAveContentType)tempSiteCT).ID.ToString());
            }

            #endregion get web ContentTypeId
        }

        private static void GenerateAssemblyVersionMappping(IAveWeb web, Dictionary<string, object> replaceDic)
        {
            var version = web.Site.SPVersion;
            //简单替换10mode workflow 中的version信息，避免16-15的case中还过去的workflow无法使用
            if (version.StartsWith("15", StringComparison.OrdinalIgnoreCase))
            {
                replaceDic.AddEx("12.0.0.0", "15.0.0.0");
                replaceDic.AddEx("14.0.0.0", "15.0.0.0");
                replaceDic.AddEx("16.0.0.0", "15.0.0.0");
            }
            if (version.StartsWith("16", StringComparison.OrdinalIgnoreCase))
            {
                replaceDic.AddEx("12.0.0.0", "16.0.0.0");
                replaceDic.AddEx("14.0.0.0", "16.0.0.0");
                replaceDic.AddEx("15.0.0.0", "16.0.0.0");
            }
        }

    }

    internal class WorkflowFileReplaceMapping
    {
        public Dictionary<string, object> TemplateFileMapping { get; set; }
        public Dictionary<string, object> ConfigFileMapping { get; set; }
    }

    internal static class SPWorkflowSubFileUnitExtension
    {
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "wfconfig is part of file name")]
        public static SPWorkflowFileContentProcType FileType(this SPWorkflowSubFileUnit fileUnit)
        {
            if (fileUnit == null)
            {
                throw new ArgumentNullException("fileUnit");
            }

            if (fileUnit.SerializableData.mName.EndsWith(".xoml", StringComparison.OrdinalIgnoreCase))
            {
                return SPWorkflowFileContentProcType.Xoml;
            }
            if (fileUnit.SerializableData.mName.EndsWith(".xoml.wfconfig.xml", StringComparison.OrdinalIgnoreCase))
            {
                return SPWorkflowFileContentProcType.Config;
            }
            if (fileUnit.SerializableData.mName.EndsWith(".rule", StringComparison.OrdinalIgnoreCase))
            {
                return SPWorkflowFileContentProcType.Rules;
            }
            return SPWorkflowFileContentProcType.Invalid;
        }
    }
}
