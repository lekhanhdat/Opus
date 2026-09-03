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
    public class ItemMetadataForHSMConnector
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(ItemMetadata));
        private static readonly HashSet<string> NoRestoreFieldMap;
        private static readonly HashSet<string> NeedRestoreFieldMap;

        private IAveListItem mItem;
        private AveSPList mList;
        private int originalVersion;
        private int originalRowId;
        private Dictionary<string, object> mItemUserData;
        private List<Dictionary<string, object>> mItemJunctionData;
        private AveObjectModelFactory mAveObjectModelFactory;

        private int webLanguage
        {
            get
            {
                int tempWebLanguage = 1033;
                try
                {
                    tempWebLanguage = (int)(mItem == null ? mList.SPList.ParentWeb.Language : mItem.ParentList.ParentWeb.Language);
                }
                catch (Exception ex)
                {
                    log.Warn($"An error occurred while get ParentWeb Language.Error message:{ex}");
                }
                return tempWebLanguage;
            }
        }


        static ItemMetadataForHSMConnector()
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

        public ItemMetadataForHSMConnector(AveObjectModelFactory aveObjectModelFactory, IAveListItem mItem, int originalVersion, int originalRowId, Dictionary<string, object> mItemUserData, List<Dictionary<string, object>> mItemJunctionData)
        {
            this.mAveObjectModelFactory = aveObjectModelFactory;
            this.mItem = mItem;
            this.originalVersion = originalVersion;
            this.originalRowId = originalRowId;
            this.mItemUserData = mItemUserData;
            this.mItemJunctionData = mItemJunctionData;
        }
        public ItemMetadataForHSMConnector(AveObjectModelFactory aveObjectModelFactory, AveSPList list, int originalVersion, int originalRowId, Dictionary<string, object> mItemUserData, List<Dictionary<string, object>> mItemJunctionData)
        {
            this.mAveObjectModelFactory = aveObjectModelFactory;
            this.originalVersion = originalVersion;
            this.originalRowId = originalRowId;
            this.mItemUserData = mItemUserData;
            this.mItemJunctionData = mItemJunctionData;
            this.mList = list;
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
                        finalValue = ProcessDataWithoutMapping(mAveObjectModelFactory, fieldName, xmlField, destField, pair.Value);

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
            bool containsField = mItem == null ? mList.SPList.Fields.ContainsField(fieldName) : mItem.ParentList.Fields.ContainsField(fieldName);
            if (containsField)
            {
                string columnSchemaXml = string.Empty;
                try
                {
                    if (mItem == null)
                    {
                        destField = mList.SPList.Fields.GetFieldByInternalName(fieldName);
                        if (!string.IsNullOrEmpty(destField.SchemaXml) && destField.SchemaXml.EqualIgnoreCase("<Fields></Fields>"))
                        {
                            log.Info($"GetXmlFieldAndDestField.mList.SPList.Fields.SchemaXml is Empty and reget SPList.Fields.ColumnName:{fieldName}.");
                            mList.SPList.Reload();
                            destField = mList.SPList.Fields.GetFieldByInternalName(fieldName);
                        }
                    }
                    else
                    {
                        destField = mItem.ParentList.Fields.GetFieldByInternalName(fieldName);
                        if (!string.IsNullOrEmpty(destField.SchemaXml) && destField.SchemaXml.EqualIgnoreCase("<Fields></Fields>"))
                        {
                            log.Info($"GetXmlFieldAndDestField.mItem.ParentList.Fields is Empty and reget ParentList.Fields.ColumnName:{fieldName}.");
                            mItem.ParentList.Reload();
                            destField = mItem.ParentList.Fields.GetFieldByInternalName(fieldName);
                        }
                    }
                    //buildIn column没有xmlfield，需要根据目的端的column创建出来
                    XmlDocument schemaXml = new XmlDocument();
                    columnSchemaXml = destField.SchemaXml;
                    schemaXml.LoadXml(columnSchemaXml);
                    if (string.IsNullOrEmpty(columnSchemaXml))
                    {
                        log.Info($"GetXmlFieldAndDestField.columnSchemaXml is null.ColumnName:{fieldName}.");
                    }
                    xmlField = new AveXmlField(schemaXml.DocumentElement, webLanguage);
                }
                catch (Exception ex)
                {
                    log.Warn($"GetXmlFieldAndDestField error.ColumnName:{fieldName}.Message:{ex}.columnSchemaXml:{columnSchemaXml}.");
                    throw;
                }
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
                        destField = mItem == null?mList.SPList.Fields.GetFieldById(sourceFieldId, true) : mItem.ParentList.Fields.GetFieldById(sourceFieldId, true);
                    }
                    catch (Exception ex)
                    {
                        log.Warn("Restore data junction ensure field failed.Exception:{0}", ex);
                    }
                    if (destField == null || string.IsNullOrEmpty(destField.SchemaXml))
                    {
                        continue;
                    }
                    XmlDocument schemaXml = new XmlDocument();
                    schemaXml.LoadXml(destField.SchemaXml);
                    var xmlField = new AveXmlField(schemaXml.DocumentElement, webLanguage);
                    if (!NeedRestoreDataJunction(xmlField, destField))
                    {
                        continue;
                    }

                    string fieldName = xmlField.FieldInternalName;
                    object finalValue;
                    finalValue = ProcessDataWithoutMapping(mAveObjectModelFactory, fieldName, xmlField, destField, pair.Value);


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
                //fieldName = listFieldCollection.SourceTextTaxonomyDic.ContainsKey(fieldName) ? listFieldCollection.SourceTextTaxonomyDic[fieldName] : fieldName;
            }
            return fieldName;
        }

      /*  private string GetUserLoginName(object info)
        {
            string loginName = string.Empty;
            if (info is AveUserInfo)
            {
                var userInfo = info as AveUserInfo;
                loginName = userInfo.DomainGroup ? userInfo.Title : userInfo.Login;
            }
            else if (info is AveGroupInfo)
            {
                loginName = (info as AveGroupInfo).Title;
            }
            return loginName;
        }
*/
        private object ProcessDataWithoutMapping(AveObjectModelFactory aveObjectModelFactory, string fieldName, AveXmlField xmlField, IAveField destField, object value)
        {
            var dataFormatObj = DataFormatFactoryForHSMConnector.CreateInstance(xmlField, destField, mItem, originalVersion, originalRowId, mItemUserData, aveObjectModelFactory);
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
            //if (listFieldCollection.SourceTextTaxonomyDic.Values.Contains(userDataKey))
            //{
            //    return true;
            //}

            return false;
        }

        private bool NeedRestoreUserData(string fieldName, IAveField destField)
        {
            //此时已经走过反插逻辑，所以如果目的端field依然为空，那么不需要还原该data
            if (destField == null)
            {
                return false;
            }

            if (AveBuiltInFieldId.ContentTypeId == destField.ID)
            {
                return false;
            }

            //workflow status需要在还原workflow instance的时候还原，所以在此处过滤掉
            if (destField.TypeAsString == "WorkflowStatus")
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
