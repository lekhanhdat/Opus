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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;

namespace HSMCommon
{
   public class ItemMetadata
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(ItemMetadata));
        private static readonly HashSet<string> NoRestoreFieldMap;
        private static readonly HashSet<string> NeedRestoreFieldMap;

        private AveSPItem mItem;
        private int originalVersion;
        private int originalRowId;
        private Dictionary<string, object> mItemUserData;
        private List<Dictionary<string, object>> mItemJunctionData;
        private AveObjectModelFactory mAveObjectModelFactory;


        private AveSPListFieldCollection listFieldCollection
        {
            get
            {
                return this.mItem.ParentList.AveFields;
            }
        }

        private int webLanguage
        {
            get
            {
                int tempWebLanguage = 1033;
                try
                {
                    tempWebLanguage = (int)(mItem.ParentList.ParentWeb.SPWeb.Language);
                }
                catch (Exception ex)
                {
                    log.Warn($"An error occurred while get SPWeb Language.Error message:{ex}");
                }
                return tempWebLanguage;
            }
        }



        static ItemMetadata()
        {
            NoRestoreFieldMap = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                                    {
                                        "#tp_ID",
                                        "#tp_ListId",
                                        "#tp_SiteId",
                                        "#tp_RowOrdinal",
                                        "#tp_Version",
                                        "#tp_Ordering",
                                        "#tp_ThreadIndex",
                                        "#tp_HasAttachment",
                                        "#tp_ModerationStatus",
                                        "#tp_IsCurrent",
                                        //"#tp_ItemOrder",
                                        "#tp_InstanceID",
                                        "#tp_GUID",
                                        "#tp_CopySource",
                                        "#tp_HasCopyDestinations",
                                        "#tp_AuditFlags",
                                        "#tp_InheritAuditFlags",
                                        "#tp_Size",
                                        "#tp_WorkflowVersion",
                                        "#tp_WorkflowInstanceID",
                                        "#tp_ParentId",
                                        "#tp_DocId",
                                        "#tp_DeleteTransactionId",
                                        "#uniqueidentifier1",
                                        "#tp_Level",
                                        "#tp_IsCurrentVersion",
                                        "#tp_UIVersion",
                                        "#tp_CalculatedVersion",
                                        "#tp_UIVersionString",
                                        "#tp_DraftOwnerId",
                                        "FileType",
                                        "PreviewOnForm",
                                        "ImageSize",

                                        //DOC-67843
                                        //report metadata 下的item的这个column指向的是report template下面的doc的guid，这里不能使用源端数据
                                        "_dlc_Reporting_TemplateId",
                                        "_dlc_Reporting_QueryAssembly",
                                        "_dlc_Reporting_InjectionAssembly",
                                        "_dlc_Reporting_InjectionClass",
                                        "_dlc_Reporting_IconUrl",
                                        "_dlc_Reporting_HttpContentType",

                                        //don't restore holds field values
                                        "_vti_ItemHoldRecordStatus",
                                        //"IconOverlay",
                                        "DescendantLikesCount",
                                        "AppAuthor",
                                        "AppEditor"
                                    };


            //应用field filter的时候，有些field是不能被filter的，添加到NEED_RESTORE_FIELD_MAP中,用小写字符表示
            NeedRestoreFieldMap = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                                      {
                                          "WikiField",
                                          "Editor",
                                          "Author",
                                          "Modified",
                                          "Created",
                                          "PublishingPageImage",
                                          "SummaryLinks",
                                      };
        }

        public ItemMetadata(AveObjectModelFactory aveObjectModelFactory, AveSPItem mItem, int originalVersion, int originalRowId, Dictionary<string, object> mItemUserData, List<Dictionary<string, object>> mItemJunctionData)
        {
            this.mItem = mItem;
            this.originalVersion = originalVersion;
            this.originalRowId = originalRowId;
            this.mItemUserData = mItemUserData;
            this.mItemJunctionData = mItemJunctionData;
            this.mAveObjectModelFactory = aveObjectModelFactory;
        }



        /// <summary>
        /// 获取经过处理的UserData和DataJunction
        /// 包括需要有特定处理的value，需要column value mapping的value，和column value相应的check是否合法等过程现
        /// </summary>
        public Dictionary<string, AveFieldValueInfo> ProcessItemMetadata()
        {
            using (new AvePerformanceScope("Restore.ItemUserAndJunctionData.GetItemFieldValue"))
            {
                Dictionary<string, AveFieldValueInfo> finalFieldValues = new Dictionary<string, AveFieldValueInfo>();
                //处理UserData
                ProcessUserData(finalFieldValues);
                //处理DataJunction
                ProcessDataJunction(finalFieldValues);
                return finalFieldValues;
            }
        }



        /// <summary>
        /// 包括userdata的所有处理过程
        /// </summary>
        /// <param name="finalColumnValues"></param>
        /// <param name="isMergeToFolder"></param>
        /// <returns>needReload</returns>
        private void ProcessUserData(Dictionary<string, AveFieldValueInfo> finalColumnValues)
        {
            using (new AvePerformanceScope("Restore.ItemUserAndJunctionData.CheckUserData"))
            {
                foreach (var pair in mItemUserData)
                {
                    try
                    {
                        //根据user data的key得到源端column的internal name
                        string fieldName = GetFieldInternalName(pair.Key);
                        //把不需要还原的value直接过滤，这样就可以不用取XmlField和DestField
                        if (NeedSkippedData(fieldName, pair.Key, pair.Value))
                        {
                            continue;
                        }
                        AveXmlField xmlField = null;
                        IAveField destField = null;
                        GetXmlFieldAndDestField(fieldName, pair.Value != null, ref xmlField, ref destField);
                        //找到源端和目的端的column后，判断当前的column是否有还原的必要
                        if (!NeedRestoreUserData(fieldName, destField))
                        {
                            continue;
                        }
                        //处理备份过来的column value，check是否有相应的value mapping，并且检查该value还原到目的端是否合法
                        object finalValue;
                        finalValue = ProcessDataWithoutMapping(fieldName, xmlField, destField, pair.Value);

                        //ADO-135572 当lookup column没有关联的list时，365更新空的value值会抛错
                        if (destField.Type == AveFieldType.Lookup)
                        {
                            var lookupDestField = destField as IAveFieldLookup;
                            if (lookupDestField.LookupList == null)
                            {
                                continue;
                            }
                        }

                        finalColumnValues[destField.InternalName] = CreateFieldValueInfo(xmlField.FieldInternalName, finalValue, destField, finalColumnValues);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while Check field value. key:{0}, value:{1}\n error message:{2}", pair.Key, pair.Value, e);
                    }
                }
            }
        }

        /// <summary>
        /// 获取源端column对应的xmlField和目的端column
        /// </summary>
        private void GetXmlFieldAndDestField(string fieldName, bool ensureField, ref AveXmlField xmlField, ref IAveField destField)
        {
            if (mItem.ParentList.SPList.Fields.ContainsField(fieldName))
            {
                destField = mItem.ParentList.SPList.Fields.GetFieldByInternalName(fieldName);
                //buildIn column没有xmlfield，需要根据目的端的column创建出来
                XmlDocument schemaXml = new XmlDocument();
                schemaXml.LoadXml(destField.SchemaXml);
                xmlField = new AveXmlField(schemaXml.DocumentElement, webLanguage);
            }
        }

        /// <summary>
        /// 该方法里面处理JunctionData，通过源端的column 信息，得到column对应哪些value。
        /// </summary>
        private Dictionary<Guid, Dictionary<int, string>> PrepareJunctionData()
        {
            using (new AvePerformanceScope("Restore.ItemUserAndJunctionData.PrepareJunctionData"))
            {
                Dictionary<Guid, Dictionary<int, string>> fieldValues = new Dictionary<Guid, Dictionary<int, string>>();//<FieldId,<ItemId,DisplayValue>>
                foreach (Dictionary<string, object> dic in mItemJunctionData)
                {
                    Guid fieldId = (Guid)dic["tp_FieldId"];
                    int id = (int)dic["tp_Id"];
                    if (!fieldValues.ContainsKey(fieldId))
                    {
                        fieldValues.Add(fieldId, new Dictionary<int, string>());
                    }
                    //ADO-153203
                    if (!fieldValues[fieldId].ContainsKey(id))
                    {
                        StringBuilder tempString = new StringBuilder();
                        object displayValue;
                        if (dic.TryGetValue("DisplayValue", out displayValue))
                        {
                            tempString.Append(displayValue.ToString());
                        }
                        object itemTPGuid;
                        if (dic.TryGetValue("tp_Guid", out itemTPGuid))
                        {
                            //tempString.Append("#");
                            tempString.Append("#GUID#");
                            tempString.Append(itemTPGuid.ToString());
                        }
                        Object itemLeafName;
                        if (dic.TryGetValue("itemLeafName", out itemLeafName))
                        {
                            //tempString.Append("&");
                            tempString.Append("&leafName&");
                            tempString.Append(itemLeafName.ToString());
                        }
                        if (dic.ContainsKey("NeedRestoreItemLookupColumnNameAndValue"))
                        {
                            tempString.Append("*");
                        }
                        fieldValues[fieldId].Add(id, tempString.ToString());
                    }
                }
                return fieldValues;
            }
        }

        /// <summary>
        /// 包括datajunction的所有处理过程
        /// </summary>
        /// <param name="finalColumnValues"></param>
        /// <returns>need reload list</returns>
        private bool ProcessDataJunction(Dictionary<string, AveFieldValueInfo> finalColumnValues)
        {
            bool needReload = false;
            if (mItemJunctionData == null || mItemJunctionData.Count == 0)
            {
                return needReload;
            }
            using (new AvePerformanceScope("Restore.ItemUserAndJunctionData.CheckJunctionData"))
            {
                var junctionData = PrepareJunctionData();
                foreach (var pair in junctionData)
                {
                    Guid sourceFieldId = pair.Key;
                    
                    IAveField destField = null;
                    try
                    {
                        destField = mItem.ParentFolder.ParentList.AveFields.GetFieldById(sourceFieldId);
                    }
                    catch (Exception ex)
                    {
                        log.Warn("Restore data junction ensure field failed.Exception:{0}", ex);
                    }
                    XmlDocument schemaXml = new XmlDocument();
                    schemaXml.LoadXml(destField?.SchemaXml);
                    var xmlField = new AveXmlField(schemaXml.DocumentElement, webLanguage);
                    if (!NeedRestoreDataJunction(xmlField, destField))
                    {
                        continue;
                    }

                    string fieldName = xmlField.FieldInternalName;
                    object finalValue;
                    finalValue = ProcessDataWithoutMapping(fieldName, xmlField, destField, pair.Value);


                    //ADO-135572 当lookup column没有关联的list时，365更新空的value值会抛错
                    if (destField.Type == AveFieldType.Lookup)
                    {
                        var lookupDestField = destField as IAveFieldLookup;
                        if (lookupDestField.LookupList == null)
                        {
                            continue;
                        }
                    }

                    finalColumnValues[destField.InternalName] = CreateFieldValueInfo(xmlField.FieldInternalName, finalValue, destField, finalColumnValues);
                }
            }
            return needReload;
        }

        /// <summary>
        /// 处理userdata时部分user data的key值并不是column的internal name，该部分column一般为build in column
        /// </summary>
        private string GetFieldInternalName(string userDataKey)
        {
            string fieldName = userDataKey;
            if (fieldName.Equals("#tp_ContentTypeId", StringComparison.OrdinalIgnoreCase))
            {
                fieldName = "ContentType";
            }
            else if (fieldName.Equals("#tp_ItemOrder", StringComparison.OrdinalIgnoreCase))
            {
                fieldName = "Order";
            }
            else if (fieldName.Equals("#tp_HasCopyDestinations", StringComparison.OrdinalIgnoreCase))
            {
                fieldName = "_HasCopyDestinations";
            }
            else if (fieldName.Equals("#tp_AppAuthor", StringComparison.OrdinalIgnoreCase))
            {
                fieldName = "AppAuthor";
            }
            else if (fieldName.Equals("#tp_AppEditor", StringComparison.OrdinalIgnoreCase))
            {
                fieldName = "AppEditor";
            }
            else
            {
                //如果是taxonomy field关联的Text Field，将fieldName设置成对应taxonomy field的Name
                //我们还原taxonomy field的value是通过对应Text Field上的value来还原的
                fieldName = listFieldCollection.SourceTextTaxonomyDic.ContainsKey(fieldName) ? listFieldCollection.SourceTextTaxonomyDic[fieldName] : fieldName;
            }
            return fieldName;
        }



        private object ProcessDataWithoutMapping(string fieldName, AveXmlField xmlField, IAveField destField, object value)
        {
            var dataFormatObj = DataFormatFactory.CreateInstance(xmlField, destField, mItem, originalVersion, originalRowId, mItemUserData);
            return dataFormatObj.CheckFieldValue(value);
        }


        private bool NeedSkippedData(string fieldName, string userDataKey, object value)
        {
            //过滤掉item的Size，RowId，UIVersion等不需要还原的column value
            if (NoRestoreFieldMap.Contains(fieldName))
            {
                return true;
            }
            //Url类型的Column有特殊处理，不需要还原对应的Description Field。
            if (fieldName.Contains(AveConstants.FIELD_SEPARATOR))
            {
                return true;
            }
            //Metadata类型的column有特殊处理，对应的Text Column 不需要还原
            if (listFieldCollection.SourceTextTaxonomyDic.Values.Contains(userDataKey))
            {
                return true;
            }

            //LinkFile的'URL' column的value不需要更新,由Connector更新。
            bool isConnectorList = mItem.ParentList.SPList.IsConnectorList.HasValue ? mItem.ParentList.SPList.IsConnectorList.Value : false;
            string stringValue = value as string;
            if (isConnectorList
                && fieldName.Equals("URL", StringComparison.OrdinalIgnoreCase)
                && stringValue != null && stringValue.IndexOf("FSDLDownload.aspx", StringComparison.OrdinalIgnoreCase) != -1)
            {
                return true;
            }
            return false;
        }

        private bool NeedRestoreUserData(string fieldName, IAveField destField)
        {
            //此时已经走过反插逻辑，所以如果目的端field依然为空，那么不需要还原该data
            if (destField == null)
            {
                return false;
            }
            //Metadata类型的column，如果还需要还原成metadata类型，那么需要备份的userdata中存在对应的Text Column和metadata column的对应关系，如果没有的话该metadata column不能被还原
            //SourceTextTaxonomyDic<string,string>代表metadata text column于其metadata column的对应关系
            //fieldName已经在前面GetFieldInternalName方法中从SourceTextTaxonomyDic中取出，此时fieldName是metadata column本身
            if (listFieldCollection.SourceTextTaxonomyDic.Values.Contains(fieldName) && !mItemUserData.ContainsKey(fieldName) && destField.TypeAsString == "TaxonomyFieldType")
            {
                return false;
            }
            //todo：该判断从源代码中拷贝，不确定具体作用
            if (AveBuiltInFieldId.ContentTypeId == destField.ID)
            {
                return false;
            }

            //workflow status需要在还原workflow instance的时候还原，所以在此处过滤掉
            /*ADO-171234  ADO-189818
            Nintex workflow publish到online站点时 会创建与该workflow同名的column，
            而源端也存在一个同名的column，但是这两个column除了名字相同 其他的都不同，而在转移item上的column value时，
            会将源端的column value覆盖到目的端，但是由于这两个column 类型不同，无法覆盖，进而导致item创建失败
            因此，需要添加过滤逻辑，过滤掉该field value的还原
            */
            //nliu deleted
            //if (this.listFieldCollection.WorkflowStatusFields.Contains(fieldName,StringComparer.OrdinalIgnoreCase))
            //{
            //    return false;
            //}

            //workflow status需要在还原workflow instance的时候还原，所以在此处过滤掉
            if (destField.TypeAsString == "WorkflowStatus")
            {
                return false;
            }

            //外围设置的filter掉不需要还原的column
            if (!NeedRestoreFieldMap.Contains(destField.InternalName) && FilterOut(destField))
            {
                return false;
            }
            return true;
        }

        private bool NeedRestoreDataJunction(AveXmlField xmlField, IAveField destField)
        {
            if (xmlField != null &&
                !xmlField.TypeAsString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase)
                && !xmlField.TypeAsString.Equals("UserMulti", StringComparison.OrdinalIgnoreCase)
                && !xmlField.TypeAsString.Equals("Facilities", StringComparison.OrdinalIgnoreCase)
                 && !xmlField.TypeAsString.Equals("SendTo", StringComparison.OrdinalIgnoreCase)
                )
            {
                return false;
            }
            if (destField == null)
            {
                return false;
            }
            return true;
        }



        /// <summary>
        /// 该方法为AveListFieldCollection中的方法，不确定是否有用
        /// </summary>
        private bool FilterOut(IAveField field)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.FilterOut"))
            {
                if (mItem.ParentList.ParentWeb.ParentSite.ItemFieldFilter != null)
                {
                    int mode = mItem.ParentList.ParentWeb.ParentSite.ItemFieldFilter.Mode;
                    HashSet<string> includeFields = mItem.ParentList.ParentWeb.ParentSite.ItemFieldFilter.IncludeFields;
                    HashSet<string> excludeFields = mItem.ParentList.ParentWeb.ParentSite.ItemFieldFilter.ExcludeFields;

                    if (mode == 0)
                    {
                        if (includeFields.Contains(field.Title))
                        {
                            return false;
                        }
                        if (excludeFields.Contains(field.Title))
                        {
                            return true;
                        }
                    }
                    else if (mode == 1)
                    {
                        //include all
                        return false;
                    }
                    else if (mode == 2)
                    {
                        //exclude all
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 该方法为用于将合法的value封装到AveFieldValueInfo中
        /// </summary>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Special file in SharePoint,wrkstat.aspx")]
        private AveFieldValueInfo CreateFieldValueInfo(string xmlFieldInternalName, object value, IAveField destField, Dictionary<string, AveFieldValueInfo> finalColumnValue)
        {
            using (new AvePerformanceScope("Restore.ItemUserAndJunctionData.PostCheckMappedValue"))
            {
                var aveField = new AveFieldValueInfo { ColValue = value, ColName = destField.ColName, FieldType = destField.Type, RowOrdinal = destField.RowOrdinal, Id = destField.ID };
                if (destField is IAveFieldUrl)
                {
                    var fieldUrlValue = value as IAveFieldUrlValue;
                    if (fieldUrlValue.Url.IndexOf("wrkstat.aspx", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        const string workflowPattern = @"(?<=wrkstat.aspx\?List=)([A-F0-9]{8}-([A-F0-9]{4}-){3}[A-F0-9]{12})&WorkflowInstanceName=([A-F0-9]{8}-([A-F0-9]{4}-){3}[A-F0-9]{12})";
                        Regex regex = new Regex(workflowPattern, RegexOptions.IgnoreCase);
                        if (regex.IsMatch(fieldUrlValue.Url))
                        {
                            return null;
                        }
                    }
                    var descriptionValue = new AveFieldValueInfo { ColValue = fieldUrlValue.Description, FieldType = AveFieldType.URL, Id = destField.ID };
                    descriptionValue.ColName = destField.GetFieldAttributeValue("ColName2");
                    int descriptionRowOrdinal = 0;
                    try
                    {
                        string rowOrdinal2 = destField.GetFieldAttributeValue("RowOrdinal2");
                        if (!string.IsNullOrEmpty(rowOrdinal2))
                        {
                            int.TryParse(rowOrdinal2, out descriptionRowOrdinal);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, "An error occurred while converting XML to field attribute. Error message:{0}.", e.ToString());
                        descriptionRowOrdinal = 0;
                    }
                    descriptionValue.RowOrdinal = descriptionRowOrdinal;
                    finalColumnValue[destField.InternalName + "#2"] = descriptionValue;
                    aveField.ColValue = fieldUrlValue.Url;
                }

                if (xmlFieldInternalName.Equals("ContentTypeId"))
                {
                    aveField.ColName = "tp_ContentTypeId";
                }
                return aveField;
            }
        }

    }
}
