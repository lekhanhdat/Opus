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
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;

namespace AvePoint.Wrapper.Restore
{
    class AveTaxonomyField
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveTaxonomyField));

        public static bool UpdateTaxonomyFieldCommonProperties(AveSPSite aveSite, IAveField field, AveXmlField xmlField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTaxonomyField.UpdateTaxonomyFieldCommonProperties"))
            {
#endif
                IAveTaxonomyField taxField = field as IAveTaxonomyField;
                AveMetadataService metadataService = aveSite.MetadataService;
                bool needUpdate = false;
                if (taxField != null)
                {
                    object customProperty = null;
                    Guid sspId = Guid.Empty;
                    Guid tempSspId = Guid.Empty;
                    Guid groupId = Guid.Empty;
                    Guid tempGroupId = Guid.Empty;
                    Guid termSetId = Guid.Empty;
                    Guid tempTermSetId = Guid.Empty;
                    Guid anchorId = Guid.Empty;
                    Guid tempAnchorId = Guid.Empty;
                    if (taxField.AllowMultipleValues != xmlField.AllowMultipleValues)
                    {
                        taxField.AllowMultipleValues = xmlField.AllowMultipleValues;
                        needUpdate = true;
                    }
                    customProperty = xmlField.GetCustomerProperty("SspId");
                    if (customProperty != null)
                    {
                        bool destExist = false;
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
                        if (sspId != Guid.Empty && metadataService != null && !string.IsNullOrEmpty(AveTaxonomyFieldUtility.GetTermStoreName(aveSite.SPSite, sspId)))
                        {
                            destExist = true;
                        }
                        else if (sspId != Guid.Empty && metadataService != null && metadataService.TermStoreIdMapping.ContainsKey(sspId))
                        {
                            sspId = metadataService.TermStoreIdMapping[sspId];
                            destExist = true;
                        }
                        else if (!string.IsNullOrEmpty(termStoreName))
                        {
                            tempSspId = AveTaxonomyFieldUtility.GetTermStoreId(aveSite.SPSite, termStoreName);
                            if (!tempSspId.Equals(Guid.Empty))
                            {
                                if (sspId != Guid.Empty && metadataService != null && !metadataService.TermStoreIdMapping.ContainsKey(sspId))
                                {
                                    metadataService.TermStoreIdMapping[sspId] = tempSspId;
                                }
                                sspId = tempSspId;
                                destExist = true;
                            }
                        }
                        if (sspId != Guid.Empty && metadataService != null && !destExist)
                        {
                            sspId = metadataService.TryRestoreTermStore(sspId);
                        }
                    }

                    string groupName = string.Empty;
                    string termSetName = string.Empty;
                    bool hasGroup = false;
                    customProperty = xmlField.GetCustomerProperty("GroupId");
                    if (customProperty != null && !string.Equals(customProperty.ToString(), Guid.Empty.ToString()))
                    {
                        hasGroup = true;
                        bool destExist = false;
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
                            destExist = true;
                        }
                        else if (!string.IsNullOrEmpty(groupName))
                        {
                            tempGroupId = AveTaxonomyFieldUtility.GetTermGroupId(aveSite.SPSite, ref sspId, groupName);
                            if (!tempGroupId.Equals(Guid.Empty))
                            {
                                if (groupId != Guid.Empty && metadataService != null && !metadataService.TermGroupIdMapping.ContainsKey(groupId))
                                {
                                    metadataService.TermGroupIdMapping[groupId] = tempGroupId;
                                }
                                groupId = tempGroupId;
                                destExist = true;
                            }
                        }
                        if (groupId != Guid.Empty && metadataService != null && !destExist)
                        {
                            groupId = metadataService.TryRestoreGroup(sspId, groupId);
                        }
                    }

                    customProperty = xmlField.GetCustomerProperty("TermSetId");
                    if (customProperty != null)
                    {
                        bool destExist = false;
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

                        if (termSetId != Guid.Empty && !string.IsNullOrEmpty(AveTaxonomyFieldUtility.GetTermSetName(aveSite.SPSite, ref sspId, ref groupId, termSetId)))
                        {
                            destExist = true;
                        }
                        else if (termSetId != Guid.Empty && metadataService != null && metadataService.TermSetIdMapping.ContainsKey(termSetId))
                        {
                            termSetId = metadataService.TermSetIdMapping[termSetId];
                            destExist = true;
                        }
                        else if (!string.IsNullOrEmpty(termSetName))
                        {
                            tempTermSetId = AveTaxonomyFieldUtility.GetTermSetId(aveSite.SPSite, ref sspId, ref groupId, termSetName);
                            if (!tempTermSetId.Equals(Guid.Empty))
                            {
                                if (termSetId != Guid.Empty && metadataService != null && !metadataService.TermSetIdMapping.ContainsKey(termSetId))
                                {
                                    metadataService.TermSetIdMapping[termSetId] = tempTermSetId;
                                }
                                termSetId = tempTermSetId;
                                destExist = true;
                            }
                        }
                        if (termSetId != Guid.Empty && metadataService != null && !destExist)
                        {
                            termSetId = metadataService.TryResotreTermSet(sspId, groupId, termSetId);
                        }
                        if ((groupId == Guid.Empty && hasGroup) || termSetId == Guid.Empty)
                        {
                            bool isNeedSkipColum = taxField.ID.Equals(new Guid("b66e9b50-a28e-469b-b1a0-af0e45486874")) ||  //KeyWords Colum
                                                                       taxField.ID.Equals(new Guid("23f27201-bee3-471e-b2e7-b64fd8b7ca38")) ||  //Enterprise Keywords                  
                                                                       taxField.ID.Equals(new Guid("333b1bc2-0532-4872-96f1-bbbdead35a56"));   //HashTags Colum
                            if (!isNeedSkipColum)
                            {
                                throw new AveTermSetNotFoundException(WrapperReportResourceKey.Wrapper_TermSetNotFound.ToString(), WrapperRestoreReportResource.Wrapper_TermSetNotFound, groupName, termSetName);
                            }
                        }
                    }

                    customProperty = xmlField.GetCustomerProperty("AnchorId");
                    if (customProperty != null)
                    {
                        bool destExist = false;
                        string termNames = string.Empty;
                        string[] temp = new string[0];
                        if (customProperty.ToString().Contains('|'))
                        {
                            temp = customProperty.ToString().Split('|');
                            if (temp.Length == 2)
                            {
                                anchorId = new Guid(temp[0].ToString());
                                termNames = temp[1];
                            }
                        }
                        else
                        {
                            anchorId = new Guid(customProperty.ToString());
                        }
                        if (anchorId != Guid.Empty && metadataService != null && metadataService.TermIdMapping.ContainsKey(anchorId))
                        {
                            anchorId = metadataService.TermIdMapping[anchorId];
                            destExist = true;
                        }
                        else if (!string.IsNullOrEmpty(termNames))
                        {
                            tempAnchorId = AveTaxonomyFieldUtility.GetTermId(aveSite.SPSite, sspId, termSetId, termNames);
                            if (!tempAnchorId.Equals(Guid.Empty))
                            {
                                if (anchorId != Guid.Empty && !metadataService.TermIdMapping.ContainsKey(anchorId))
                                {
                                    metadataService.TermIdMapping[anchorId] = tempAnchorId;
                                }
                                anchorId = tempAnchorId;
                                destExist = true;
                            }
                        }
                        if (anchorId != Guid.Empty && metadataService != null && !destExist)
                        {
                            anchorId = metadataService.TryRestoreTerm(sspId, groupId, termSetId, anchorId);
                        }
                    }

                    if (sspId != Guid.Empty && taxField.SspId != sspId)
                    {
                        taxField.SspId = sspId;
                        needUpdate = true;
                    }
                    if (termSetId != Guid.Empty && taxField.TermSetId != termSetId)
                    {
                        taxField.TermSetId = termSetId;
                        needUpdate = true;
                    }

                    if (anchorId != Guid.Empty && taxField.AnchorId != anchorId)
                    {
                        taxField.AnchorId = anchorId;
                        needUpdate = true;
                    }

                    if (!string.IsNullOrEmpty(taxField.DefaultValue))
                    {
                        Dictionary<Guid, Guid> termIdMapping = null;
                        if (metadataService != null)
                        {
                            termIdMapping = metadataService.TermIdMapping;
                        }
                        if (AveTaxonomyFieldUtility.ResetTaxnomyFieldDefaultValue(aveSite.SPSite, taxField, taxField.DefaultValue, termIdMapping))
                        {
                            needUpdate = true;
                        }
                    }

                    customProperty = xmlField.GetCustomerProperty("IsPathRendered");
                    bool isPathRendered = false;
                    if (customProperty != null)
                    {
                        Boolean.TryParse(customProperty.ToString(), out isPathRendered);
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
                        Boolean.TryParse(customProperty.ToString(), out open);
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
                        Boolean.TryParse(customProperty.ToString(), out userCreated);
                    }
                    if (taxField.UserCreated != userCreated)
                    {
                        taxField.UserCreated = userCreated;
                        needUpdate = true;
                    }
                }

                return needUpdate;
#if PerformanceLog
            }
#endif
        }

        //判断taxonomy field冲突，如果taxonomy field关联的termset不同则冲突
        public static bool CheckConflict(AveSPSite aveSite, IAveField field, AveXmlField xmlField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTaxonomyField.CheckConflict"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        public static List<string> GetListTaxonomyFields(IAveList list)
        {
            List<string> taxonomyFields = new List<string>();
            try
            {
                if (list != null)
                {
                    foreach (IAveField field in list.Fields)
                    {
                        if (field is IAveTaxonomyField)
                        {
                            taxonomyFields.Add(field.InternalName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetListTaxonomyFieldsFailed, list == null ? string.Empty : list.Title, e);
            }
            return taxonomyFields;
        }

        public static bool UpdateTaxonomyFieldValue(AveSPSite site, IAveListItem item, IAveField field, ArrayList valueList)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTaxonomyField.UpdateTaxonomyFieldValue"))
            {
#endif
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

#if PerformanceLog
            }
#endif
        }

        private static string GetMultipleValueText(IAveTaxonomyFieldValueCollection taxCollection)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTaxonomyField.GetMultipleValueText"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }
    }
}
