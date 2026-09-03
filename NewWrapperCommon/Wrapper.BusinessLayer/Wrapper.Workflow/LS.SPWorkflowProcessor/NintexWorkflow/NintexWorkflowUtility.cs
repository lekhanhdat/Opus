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
namespace LS.SPWorkflowProcessor
{
    using System;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using LS.SPWorkflowProcessor.SerializableObjects;
    using System.Globalization;

    public class NintexWorkflowUtility
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static object ensureWebFieldLocker = new object();

        public static bool IsNintexWorkflow(SPWFAssociationUnit assoUnit)
        {
            bool isNintexWorkflow = false;
            if (assoUnit != null)
            {
                var customData = assoUnit.SerializableData.mSerializableCustomData as SPWorkflowSubListSerializableData;
                if (customData != null && !string.IsNullOrEmpty(customData.mUnitId) && string.Equals(customData.mUnitId, "NintexWorkflow", StringComparison.OrdinalIgnoreCase))
                {
                    isNintexWorkflow = true;
                }
            }
            return isNintexWorkflow;
        }

        /// <summary>
        ///  确保nintex workflow template library中用到的column在web上存在
        /// </summary>
        /// <param name="web"></param>
        /// <param name="factory"></param>
        /// <returns>restored fields or not</returns>
        public static void EnsureNintexWebFields(IAveWeb web, AveObjectModelFactory factory)
        {

            #region argument check

            if (web == null)
            {
                logger.Warn("Web is null in EnsureNintexWebFields.");
                return;
            }
            if (factory == null)
            {
                logger.Warn("Factory is null in EnsureNintexWebFields.");
                return;
            }

            #endregion

            lock (ensureWebFieldLocker)
            {
                IAveWeb rootWeb = web.IsRootWeb ? web : web.Site.RootWeb;
                bool restored = false;
                restored = EnsureWebField(rootWeb, factory, NWSharePointObjects.FieldWorkflowCategory, NWSharePointObjects.FieldWorkflowCategorySchema) || restored;
                restored = EnsureWebField(rootWeb, factory, NWSharePointObjects.FieldAssociatedContentType, NWSharePointObjects.FieldAssociatedContentTypeSchema) || restored;
                if (restored)
                {
                    web.AvailableFields.IsDirty = true;
                    web.AvailableContentTypes.IsDirty = true;
                }
            }
        }

        /// <summary>
        /// 参考nintex workflow UpgradeWorkflowContentType方法实现
        /// </summary>
        /// <param name="web"></param>
        /// <param name="factory"></param>
        /// <param name="fieldId"></param>
        /// <param name="fieldSchema"></param>
        /// <returns>need reload or not</returns>
        private static bool EnsureWebField(IAveWeb web, AveObjectModelFactory factory, Guid fieldId, string fieldSchema)
        {
            bool isRestored = false;
            try
            {
                if (!web.Fields.Contains(fieldId))
                {
                    web.Fields.AddFieldAsXml(fieldSchema);
                    isRestored = true;
                }
                IAveField field = web.Fields[fieldId];
                IAveContentTypeId ctId = factory.CreateContentTypeId(NWSharePointObjects.ContentTypeIdWorkflow);
                IAveContentType sPContentType = web.ContentTypes[ctId];
                IAveFieldLink fieldLink = factory.CreateFieldLink(field);
                sPContentType.FieldLinks.Add(fieldLink);
                sPContentType.Update();
                isRestored = true;
            }
            catch (Exception e)
            {
                logger.Debug("Ensure web field for nintex workflow failed.FieldInfo:{0},Error:{1}", fieldSchema, e);
            }
            return isRestored;
        }

        /// <summary>
        /// 替换nintex workflow user defined actions中的id的逻辑在nintex数据和workflow template中都要用到
        /// </summary>
        /// <param name="xomlValue"></param>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="previousSiteId"></param>
        /// <returns></returns>
        public static string ReplaceIdsInUserDefinedAction(string xomlValue, Guid siteId, Guid webId, Guid previousSiteId)
        {
            try
            {
                var udaMapping= SPWorkflowProcessorRuntime.UDAMappingManager.TryGetUDAIDMapping(siteId, webId);
                //site id不应该这样处理，因为可能存在其他site的site id,最好在restore入口传入，目前nintex中这种action都不好使，spd中没有这种action
                //使用mapping中source site id来处理,对于老数据(6.6之前的备份)，取不到site id，仍使用原有方法截取site id
                //
                if (previousSiteId == Guid.Empty)
                {
                    previousSiteId = SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.SourceSiteInfo.Id;
                    if (previousSiteId == Guid.Empty)
                    {
                        var siteMark = " SiteId=\"";
                        var index = xomlValue.IndexOf(siteMark, StringComparison.OrdinalIgnoreCase);
                        if (index >= 0 && index + siteMark.Length + 36 <= xomlValue.Length)
                        {
                            try
                            {
                                previousSiteId = new Guid(xomlValue.Substring(index + siteMark.Length, 36));
                            }
                            catch (Exception e)
                            {
                                logger.Info("Cannot get the old site id. Error:{0}.", e);
                            }
                        }
                    }
                }
                KeyValuePair<Guid, Guid> siteCollectionIdMapping = previousSiteId != Guid.Empty ? new KeyValuePair<Guid, Guid>(previousSiteId, siteId) : new KeyValuePair<Guid, Guid>();

                Regex guidRE = new Regex(AveRegexCommon.GUIDREG, RegexOptions.IgnoreCase);
                //the GUIDs include siteId, webId, listId,user defined action static id
                var siteMappingManager = SPWorkflowProcessorRuntime.MappingManager == null ? null : SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager;
                var replaceProcessor = new UserDefinedActionMappingReplaceProcessor(siteMappingManager, udaMapping, siteCollectionIdMapping);
                xomlValue = guidRE.Replace(xomlValue, replaceProcessor.GetMappedId);
                guidRE = new Regex(AveRegexCommon.GUIDREG_WITH_HTML_ENCODE, RegexOptions.IgnoreCase);
                xomlValue = guidRE.Replace(xomlValue, replaceProcessor.GetMappedId);
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while replace id in user defined actions.", ex);
            }
            return xomlValue;
        }
    }

    internal class UserDefinedActionMappingReplaceProcessor
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private KeyValuePair<Guid, Guid> siteCollectionIdMapping;

        private UserDefiniedActionIdMapping uDAMapping;

        private AveSiteMappingManager mappingManager;

        internal UserDefinedActionMappingReplaceProcessor(AveSiteMappingManager aveSiteMappingManager, UserDefiniedActionIdMapping userDefiniedActionIdMapping, KeyValuePair<Guid, Guid> siteIdMapping)
        {
            siteCollectionIdMapping = siteIdMapping;
            uDAMapping = userDefiniedActionIdMapping;
            mappingManager = aveSiteMappingManager;
        }

        /// <summary>
        /// get mapped id from site,web,list id mapping or user defined action static id mapping
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        internal string GetMappedId(Match m)
        {
            string replacedValue = m.Value;
            Guid id = new Guid(m.Value);
            bool valueMapped = false;

            //空Id不需要替换
            if (id == Guid.Empty)
            {
                return replacedValue;
            }

            if (siteCollectionIdMapping.Key == id)
            {
                replacedValue = siteCollectionIdMapping.Value.ToString();
                valueMapped = true;
            }
            if (!valueMapped)
            {
                if (mappingManager != null)
                {
                    Guid valueGuid;
                    if (mappingManager.WebIDMapping.TryGetValue(id, out valueGuid))
                    {
                        replacedValue = valueGuid.ToString();
                        valueMapped = true;
                    }
                    else if (mappingManager.GetValueFromListIdMapping(id, out valueGuid))
                    {
                        replacedValue = valueGuid.ToString();
                        valueMapped = true;
                    }
                }
            }
            if (!valueMapped)
            {
                if (uDAMapping != null)
                {
                    Guid valueGuid;
                    if (uDAMapping.TryGetValue(id, out valueGuid))
                    {
                        replacedValue = valueGuid.ToString();
                        valueMapped = true;
                    }
                }
            }
            if (valueMapped)
            {
                replacedValue = replacedValue.ToUpper(CultureInfo.InvariantCulture);
                logger.Debug("Mapping id value in UserDefinedAction of workflow from {0} to {1}.", m.Value, replacedValue);
            }
            return replacedValue;
        }
    }
}
