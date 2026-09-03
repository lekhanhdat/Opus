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
using System.Collections;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    class AveTaxonomyField
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveTaxonomyField));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aveSite"></param>
        /// <param name="field"></param>
        /// <param name="xmlField"></param>
        /// <returns>bool need update</returns>
        public static bool UpdateTaxonomyFieldCommonProperties(AveSPSite aveSite, IAveField field, AveXmlField xmlField)
        {

            using (new AvePerformanceScope("Restore.AveTaxonomyField.UpdateTaxonomyFieldCommonProperties"))
            {

                IAveTaxonomyField taxField = field as IAveTaxonomyField;
                AveMetadataService metadataService = aveSite.MetadataService;
                if (taxField == null || metadataService == null)
                {
                    return false;
                }
                var context = new TaxonomyContext();

                var needUpdate = PreUpdateTaxonomyFieldProperties(xmlField, taxField);

                EnsureTermStore(aveSite.SPSite, xmlField, metadataService, context);
                EnsureTermGroup(aveSite.SPSite, xmlField, metadataService, context);
                EnsureTermSet(aveSite.SPSite, xmlField, metadataService, taxField, context);
                EnsureAnchorId(aveSite.SPSite, xmlField, metadataService, context);

                needUpdate = UpdateTaxonomyFieldProperties(aveSite.SPSite, xmlField, taxField, context, needUpdate, metadataService);

                return needUpdate;

            }

        }

        private static bool PreUpdateTaxonomyFieldProperties(AveXmlField xmlField, IAveTaxonomyField taxField)
        {
            bool needUpdate = false;
//ADO-154055 Indexed和AllowMultipleValues更新需要有先后顺序//已经修复SetBaseValue没有排除TaxonomyField的问题。
            bool needUpdateIndexedFirst = xmlField.AllowMultipleValues & taxField.Indexed;
            bool needUpdateMultipleFirst = xmlField.Indexed & taxField.AllowMultipleValues;
            if (needUpdateIndexedFirst)
            {
                taxField.Indexed = xmlField.Indexed;
                taxField.Update();
                taxField.AllowMultipleValues = xmlField.AllowMultipleValues;
                needUpdate = true;
            }
            else if (needUpdateMultipleFirst)
            {
                taxField.AllowMultipleValues = xmlField.AllowMultipleValues;
                taxField.Update();
                taxField.Indexed = xmlField.Indexed;
                needUpdate = true;
            }
            else
            {
                if (taxField.AllowMultipleValues != xmlField.AllowMultipleValues)
                {
                    taxField.AllowMultipleValues = xmlField.AllowMultipleValues;
                    needUpdate = true;
                }
                if (taxField.Indexed != xmlField.Indexed)
                {
                    taxField.Indexed = xmlField.Indexed;
                    needUpdate = true;
                }
            }
            taxField.CreateValuesInEditForm = (bool) xmlField.GetCustomerProperty("CreateValuesInEditForm");
            return needUpdate;
        }

        private static bool UpdateTaxonomyFieldProperties(IAveSite spSite, AveXmlField xmlField, IAveTaxonomyField taxField,TaxonomyContext context, bool needUpdate, AveMetadataService metadataService)
        {
            object customProperty;
            if (context.SspId != Guid.Empty && taxField.SspId != context.SspId)
            {
                taxField.SspId = context.SspId;
                needUpdate = true;
            }
            if (taxField.GetCustomProperty("GroupId") != null)
            {
                if (context.GroupId != Guid.Empty && new Guid(taxField.GetCustomProperty("GroupId").ToString()) != context.GroupId)
                {
                    taxField.SetCustomProperty("GroupId", context.GroupId);
                    needUpdate = true;
                }
            }
            if (context.TermSetId != Guid.Empty && taxField.TermSetId != context.TermSetId)
            {
                taxField.TermSetId = context.TermSetId;
                needUpdate = true;
            }

            if (taxField.AnchorId != context.AnchorId)
            {
                taxField.AnchorId = context.AnchorId;
                needUpdate = true;
            }

            string defaultValue = string.Empty;
            if (!string.IsNullOrEmpty(taxField.DefaultValue))
            {
                defaultValue = taxField.DefaultValue;
            }
            else if (!string.IsNullOrEmpty(xmlField.DefaultValue))
            {
                defaultValue = xmlField.DefaultValue;
            }
            Dictionary<Guid, Guid> termIdMapping = null;
            if (metadataService != null)
            {
                termIdMapping = metadataService.TermIdMapping;
            }
            if (AveTaxonomyFieldUtility.ResetTaxnomyFieldDefaultValue(spSite, taxField, defaultValue, termIdMapping))
            {
                needUpdate = true;
            }

            customProperty = xmlField.GetCustomerProperty("IsPathRendered");
            bool isPathRendered = false;
            if (customProperty != null)
            {
                isPathRendered = (bool) customProperty;
            }
            if (taxField.IsPathRendered != isPathRendered)
            {
                taxField.IsPathRendered = isPathRendered;
                needUpdate = true;
            }

            customProperty = xmlField.GetCustomerProperty("Open");
            bool open = false;
            if (customProperty != null)
            {
                open = (bool) customProperty;
            }
            if (taxField.Open != open)
            {
                taxField.Open = open;
                needUpdate = true;
            }


            //customProperty = xmlField.GetCustomerProperty("TextField");
            //Guid textField = Guid.Empty;
            //if (customProperty != null)
            //{
            //    textField = new Guid(customProperty.ToString());
            //}
            //if (taxField.TextField != textField)
            //{
            //    taxField.TextField = textField;
            //    needUpdate = true;
            //}

            customProperty = xmlField.GetCustomerProperty("UserCreated");
            bool userCreated = false;
            if (customProperty != null)
            {
                userCreated = (bool) customProperty;
            }
            if (taxField.UserCreated != userCreated)
            {
                taxField.UserCreated = userCreated;
                needUpdate = true;
            }
            return needUpdate;
        }

        private static void EnsureAnchorId(IAveSite spSite, AveXmlField xmlField, AveMetadataService metadataService,TaxonomyContext context)
        {
            var customProperty = xmlField.GetCustomerProperty("AnchorId");
            if (customProperty != null)
            {
                bool destExist = false;
                string termName = string.Empty;
                if (customProperty.ToString().Contains('|'))
                {
                    var temp = customProperty.ToString().Split('|');
                    if (temp.Length == 2)
                    {
                        context.AnchorId = new Guid(temp[0]);
                        termName = temp[1];
                    }
                }
                else
                {
                    context.AnchorId = new Guid(customProperty.ToString());
                }
                if (context.AnchorId != Guid.Empty && metadataService != null && metadataService.TermIdMapping.ContainsKey(context.AnchorId))
                {
                    context.AnchorId = metadataService.TermIdMapping[context.AnchorId];
                    destExist = true;
                }
                else if (!string.IsNullOrEmpty(termName))
                {
                    var tempAnchorId = AveTaxonomyFieldUtility.GetTermId(spSite, context.SspId, context.TermSetId, termName);
                    if (!tempAnchorId.Equals(Guid.Empty))
                    {
                        if (context.AnchorId != Guid.Empty && metadataService != null && !metadataService.TermIdMapping.ContainsKey(context.AnchorId))
                        {
                            metadataService.TermIdMapping[context.AnchorId] = tempAnchorId;
                        }
                        context.AnchorId = tempAnchorId;
                        destExist = true;
                    }
                    if (context.AnchorId != Guid.Empty && metadataService != null && !destExist)
                    {
                        context.AnchorId = metadataService.TryRestoreTerm(context.SspId, context.GroupId, context.TermSetId, context.AnchorId);
                    }
                    if (context.AnchorId == Guid.Empty)
                    {
                        throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_TermNotFound, termName);
                    }
                }
            }
        }

        private static void EnsureTermStore(IAveSite spSite, AveXmlField xmlField, AveMetadataService metadataService, TaxonomyContext context)
        {
            var customProperty = xmlField.GetCustomerProperty("SspId");
            if (customProperty != null)
            {
                bool destExist = false;
                string termStoreName = string.Empty;
                if (customProperty.ToString().Contains('|'))
                {
                    string[] temp = customProperty.ToString().Split('|');
                    if (temp.Length == 2)
                    {
                        context.SspId = new Guid(temp[0]);
                        termStoreName = temp[1];
                    }
                }
                else
                {
                    context.SspId = new Guid(customProperty.ToString());
                }
                if (context.SspId != Guid.Empty && metadataService.TermStoreIdMapping.ContainsKey(context.SspId))
                {
                    context.SspId = metadataService.TermStoreIdMapping[context.SspId];
                    destExist = true;
                }
                else if (!string.IsNullOrEmpty(termStoreName))
                {
                    var tempSspId = AveTaxonomyFieldUtility.GetTermStoreId(spSite, termStoreName);
                    if (!tempSspId.Equals(Guid.Empty))
                    {
                        if (context.SspId != Guid.Empty && !metadataService.TermStoreIdMapping.ContainsKey(context.SspId))
                        {
                            metadataService.TermStoreIdMapping[context.SspId] = tempSspId;
                        }
                        context.SspId = tempSspId;
                        destExist = true;
                    }
                    else
                    {
                        throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_TermStoreNotFound, termStoreName);
                    }
                }
                if (context.SspId != Guid.Empty && metadataService != null && !destExist)
                {
                    context.SspId = metadataService.TryRestoreTermStore(context.SspId);
                }
            }
        }

        private static void EnsureTermGroup(IAveSite spSite, AveXmlField xmlField, AveMetadataService metadataService, TaxonomyContext context)
        {
            string groupName = string.Empty;
            var customProperty = xmlField.GetCustomerProperty("GroupId");
            if (customProperty != null)
            {
                bool destExist = false;
                if (customProperty.ToString().Contains('|'))
                {
                    string[] temp = customProperty.ToString().Split('|');
                    if (temp.Length == 2)
                    {
                        context.GroupId = new Guid(temp[0]);
                        groupName = temp[1];
                    }
                }
                else
                {
                    context.GroupId = new Guid(customProperty.ToString());
                }
                if (context.GroupId != Guid.Empty && metadataService != null && metadataService.TermGroupIdMapping.ContainsKey(context.GroupId))
                {
                    context.GroupId = metadataService.TermGroupIdMapping[context.GroupId];
                    destExist = true;
                }
                else if (!string.IsNullOrEmpty(groupName))
                {
                    var tempStoreId = context.SspId;
                    var tempGroupId = AveTaxonomyFieldUtility.GetTermGroupId(spSite, ref tempStoreId, groupName);
                    context.SspId = tempStoreId;
                    if (!tempGroupId.Equals(Guid.Empty))
                    {
                        if (metadataService != null && (context.GroupId != Guid.Empty && !metadataService.TermGroupIdMapping.ContainsKey(context.GroupId)))
                        {
                            metadataService.TermGroupIdMapping[context.GroupId] = tempGroupId;
                        }
                        context.GroupId = tempGroupId;
                        destExist = true;
                    }
                }
                if (context.GroupId != Guid.Empty && metadataService != null && !destExist)
                {
                    context.GroupId = metadataService.TryRestoreGroup(context.SspId, context.GroupId);
                }
                if (context.GroupId == Guid.Empty)
                {
                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_TermGroupNotFound, groupName);
                }
            }
        }

        private static void EnsureTermSet(IAveSite spSite, AveXmlField xmlField,AveMetadataService metadataService, IAveTaxonomyField taxField,TaxonomyContext context)
        {
            string termSetName = string.Empty;
            var customProperty = xmlField.GetCustomerProperty("TermSetId");
            if (customProperty != null)
            {
                bool destExist = false;
                if (customProperty.ToString().Contains('|'))
                {
                    string[] temp = customProperty.ToString().Split('|');
                    if (temp.Length == 2)
                    {
                        context.TermSetId = new Guid(temp[0]);
                        termSetName = temp[1];
                    }
                }
                else
                {
                    context.TermSetId = new Guid(customProperty.ToString());
                }
                if (context.TermSetId != Guid.Empty && metadataService != null && metadataService.TermSetIdMapping.ContainsKey(context.TermSetId))
                {
                    context.TermSetId = metadataService.TermSetIdMapping[context.TermSetId];
                    destExist = true;
                }
                else if (!string.IsNullOrEmpty(termSetName))
                {

                    var tempTermSetId = AveTaxonomyFieldUtility.GetTermSetId(spSite,context, termSetName);
                    if (!tempTermSetId.Equals(Guid.Empty))
                    {
                        if (metadataService != null && (context.TermSetId != Guid.Empty && !metadataService.TermSetIdMapping.ContainsKey(context.TermSetId)))
                        {
                            metadataService.TermSetIdMapping[context.TermSetId] = tempTermSetId;
                        }
                        context.TermSetId = tempTermSetId;
                        destExist = true;
                    }
                }
                if (context.TermSetId != Guid.Empty && metadataService != null && !destExist)
                {
                    context.TermSetId = metadataService.TryResotreTermSet(context.SspId, context.GroupId, context.TermSetId);
                }
                if (context.TermSetId == Guid.Empty)
                {
                    bool isNeedSkipColum = taxField.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38")) ||//KeyWords Colum
                                           taxField.ID.Equals(new Guid("333b1bc2-0532-4872-96f1-bbbdead35a56"));//HashTags Colum
                    if (!isNeedSkipColum)
                    {
                        throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_TermSetNotFound, termSetName);
                    }
                }
            }
        }

        //判断taxonomy field冲突，如果taxonomy field关联的termset不同则冲突
        public static bool CheckConflict(AveSPSite aveSite, IAveField field, AveXmlField xmlField)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTaxonomyField.CheckConflict"))
            {

                IAveTaxonomyField taxField = field as IAveTaxonomyField;
                AveMetadataService metadataService = aveSite.MetadataService;
                if (taxField != null)
                {
                    object customProperty = null;
                    Guid sspId = Guid.Empty;
                    Guid groupId = Guid.Empty;
                    Guid termSetId = Guid.Empty;
                    Guid anchorId = Guid.Empty;
                    customProperty = xmlField.GetCustomerProperty("SspId");
                    if (customProperty != null)
                    {
                        string termStoreName = string.Empty;
                        if (customProperty.ToString().Contains('|'))
                        {
                            string[] temp = customProperty.ToString().Split('|');
                            if (temp.Length == 2)
                            {
                                sspId = new Guid(temp[0].ToString());
                                termStoreName = temp[1];
                            }
                        }
                        else
                        {
                            sspId = new Guid(customProperty.ToString());
                        }
                        if (sspId != Guid.Empty && metadataService != null && metadataService.TermStoreIdMapping.ContainsKey(sspId))
                        {
                            sspId = metadataService.TermStoreIdMapping[sspId];
                        }
                    }

                    customProperty = xmlField.GetCustomerProperty("GroupId");
                    if (customProperty != null)
                    {
                        string groupName = string.Empty;
                        if (customProperty.ToString().Contains('|'))
                        {
                            string[] temp = customProperty.ToString().Split('|');
                            if (temp.Length == 2)
                            {
                                groupId = new Guid(temp[0].ToString());
                                groupName = temp[1];
                            }
                        }
                        else
                        {
                            groupId = new Guid(customProperty.ToString());
                        }
                        if (groupId != Guid.Empty && metadataService != null && metadataService.TermGroupIdMapping.ContainsKey(groupId))
                        {
                            groupId = metadataService.TermGroupIdMapping[groupId];
                        }
                    }

                    customProperty = xmlField.GetCustomerProperty("TermSetId");
                    if (customProperty != null)
                    {
                        string termSetName = string.Empty;
                        if (customProperty.ToString().Contains('|'))
                        {
                            string[] temp = customProperty.ToString().Split('|');
                            if (temp.Length == 2)
                            {
                                termSetId = new Guid(temp[0].ToString());
                                termSetName = temp[1];
                            }
                        }
                        else
                        {
                            termSetId = new Guid(customProperty.ToString());
                        }
                        if (termSetId != Guid.Empty && metadataService != null && metadataService.TermSetIdMapping.ContainsKey(termSetId))
                        {
                            termSetId = metadataService.TermSetIdMapping[termSetId];
                        }
                    }

                    customProperty = xmlField.GetCustomerProperty("AnchorId");
                    if (customProperty != null)
                    {
                        string termName = string.Empty;
                        if (customProperty.ToString().Contains('|'))
                        {
                            string[] temp = customProperty.ToString().Split('|');
                            if (temp.Length == 2)
                            {
                                anchorId = new Guid(temp[0].ToString());
                                termName = temp[1];
                            }
                        }
                        else
                        {
                            anchorId = new Guid(customProperty.ToString());
                        }
                        if (anchorId != Guid.Empty && metadataService != null && metadataService.TermIdMapping.ContainsKey(anchorId))
                        {
                            anchorId = metadataService.TermIdMapping[anchorId];
                        }
                    }
                    //如果taxonomy field关联的termset不同则冲突，相同则不冲突。
                    if (termSetId != taxField.TermSetId)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;

            }

        }

        public static bool UpdateTaxonomyFieldValue(AveSPSite site, IAveListItem item, IAveField field, ArrayList valueList)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTaxonomyField.UpdateTaxonomyFieldValue"))
            {

                //bool needUpdate = false;
                Guid taxonomyListId = new Guid(site.SPSite.RootWeb.Properties["TaxonomyHiddenList"]);
                IAveTaxonomyField taxField = field as IAveTaxonomyField;
                if (taxField == null)
                {
                    return false;
                }
                if (field.TypeAsString == "TaxonomyFieldType")
                {
                    IAveTaxonomyFieldValue value = site.ObjectModelFactory.CreateTaxonomyFieldValue(field);
                    int id = (int)valueList[0];
                    if (site.MappingManager.SiteMappingManager.TaxonomyItemMapping.ContainsKey(id))
                    {
                        value.TermGuid = site.MappingManager.SiteMappingManager.TaxonomyItemMapping[id].ToString();
                    }
                    else
                    {
                        value.WssId = id;
                    }
                    try
                    {
                        item[field.ID] = value;
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetItemFieldError, field.ID, value.ToString(), e);
                    }
                    item[taxField.TextField] = value.WssId + "|" + value.TermGuid;
                }
                else
                {
                    IAveTaxonomyFieldValueCollection valueCollection = site.ObjectModelFactory.CreateTaxonomyFieldValueCollection(field);
                    foreach (int id in valueList)
                    {
                        IAveTaxonomyFieldValue value = site.ObjectModelFactory.CreateTaxonomyFieldValue(field);
                        if (site.MappingManager.SiteMappingManager.TaxonomyItemMapping.ContainsKey(id))
                        {
                            value.TermGuid = site.MappingManager.SiteMappingManager.TaxonomyItemMapping[id].ToString();
                        }
                        else
                        {
                            value.WssId = id;
                        }
                        valueCollection.Add(value);
                    }
                    try
                    {
                        item[field.ID] = valueCollection;
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetItemFieldError, field.ID, valueCollection.ToString(), e);
                    }
                    item[taxField.TextField] = GetMultipleValueText(valueCollection);
                }

                return true;


            }

        }

        private static string GetMultipleValueText(IAveTaxonomyFieldValueCollection taxCollection)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTaxonomyField.GetMultipleValueText"))
            {

                StringBuilder builder = new StringBuilder();
                bool flag = true;
                foreach (IAveTaxonomyFieldValue value2 in taxCollection)
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        builder.Append(';');
                    }
                    builder.Append(value2.WssId);
                    builder.Append('|');
                    builder.Append(value2.TermGuid);
                }
                return builder.ToString();

            }

        }
    }
}
