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
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    class ItemMetadata : IItemMetadata
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(ItemMetadata));
        private static readonly HashSet<string> NoRestoreFieldMap;
        private static readonly HashSet<string> NeedRestoreFieldMap;

        private AveSPItem mItem;
        private int originalVersion;
        private int originalRowId;
        private Dictionary<string, object> mItemUserData;
        private List<Dictionary<string, object>> mItemJunctionData;

        private IAveFieldMapping fieldMapping
        {
            get
            {
                return this.mItem.ParentList.AveFields.FieldMapping;
            }
        }

        private AveSPListFieldCollection listFieldCollection
        {
            get
            {
                return this.mItem.ParentList.AveFields;
            }
        }

        private Dictionary<string, AveXmlField> xmlFields
        {
            get
            {
                return this.mItem.ParentList.AveFields.XmlFields;
            }
        }

        //todo:改成SiteLevel的cache，参考Guid
        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> lookupListItemIDAndValues = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

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

        public ItemMetadata(AveSPItem mItem, int originalVersion, int originalRowId, Dictionary<string, object> mItemUserData, List<Dictionary<string, object>> mItemJunctionData)
        {
            this.mItem = mItem;
            this.originalVersion = originalVersion;
            this.originalRowId = originalRowId;
            this.mItemUserData = mItemUserData;
            this.mItemJunctionData = mItemJunctionData;
        }

        public Dictionary<string, AveFieldValueInfo> ProcessItemMetadata(bool isMergeToFolder = false)
        {
            return ProcessItemMetadata(new MetadataOption {
                isMergToFolder = isMergeToFolder,
            });
        }

        /// <summary>
        /// 获取经过处理的UserData和DataJunction
        /// 包括需要有特定处理的value，需要column value mapping的value，和column value相应的check是否合法等过程现
        /// </summary>
        /// Dictionary<string, AveFieldValueInfo> ProcessItemMetadata(MetadataOption option);
        public Dictionary<string, AveFieldValueInfo> ProcessItemMetadata(MetadataOption option)
        {
            using (new AvePerformanceScope("Restore.ItemUserAndJunctionData.GetItemFieldValue"))
            {
                bool needReload = false;
                Dictionary<string, AveFieldValueInfo> finalFieldValues = new Dictionary<string, AveFieldValueInfo>();
                //处理UserData
                needReload |= ProcessUserData(finalFieldValues, option);
                //处理DataJunction
                needReload |= ProcessDataJunction(finalFieldValues, option);
                if (needReload)
                {
                    mItem.ParentList.AveList.Reload();
                    mItem.ParentFolder.SPFolder.Reload(false);
                }
                return finalFieldValues;
            }
        }

        /// <summary>
        /// 根据目的端的web信息，修改相应的user data
        /// 根据Mapping更新UserData
        /// </summary>
        private void UpdateNeedRestoreUserData()
        {
            using (new AvePerformanceScope("Restore.ItemMetadata.UpdateNeedRestoreUserData"))
            {
                //通过excel创建出来的column value需要添加到user data中
                if (xmlFields != null)//对于web下的文件，mXmlFields为空
                {
                    foreach (var fieldName in xmlFields.Keys)
                    {
                        //把源端时是Null的Value改成目的端的DefaultValue
                        var mappedNullValue = fieldMapping.GetMappingNullValue(fieldName);
                        if (mappedNullValue != null && !mItemUserData.ContainsKey(fieldName))
                        {
                            mItemUserData.Add(fieldName, mappedNullValue);
                        }
                        var mapping = xmlFields[fieldName].CustomFieldInfo;
                        //ExcelMapping中原来是Null的Value需要将其加入UserData中，方便遍历
                        //当userData的count=0时，说明该item不需要还原user data，所以不需要向user data中添加mapping的value
                        if (mapping != null && mItemUserData.Count != 0 && !mItemUserData.ContainsKey(fieldName))
                        {
                            string typeAsString = xmlFields[fieldName].TypeAsString;
                            if ((string.Equals("TaxonomyFieldType", typeAsString, StringComparison.OrdinalIgnoreCase)
                                     || string.Equals("TaxonomyFieldTypeMulti", typeAsString, StringComparison.OrdinalIgnoreCase))
                                && listFieldCollection.SourceTextTaxonomyDic.ContainsValue(fieldName))
                            {//对于excel mapping，metadata column如果源端没有值在excel中设置了值，需要把textField也add进去
                                foreach (KeyValuePair<string, string> kv in listFieldCollection.SourceTextTaxonomyDic)
                                {
                                    if (!mItemUserData.ContainsKey(kv.Key) && kv.Value.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        mItemUserData.Add(kv.Key, null);
                                        break;
                                    }
                                }
                            }
                            mItemUserData.Add(fieldName, null);
                        }
                    }
                }
                //将ExcelMapping中新添加的Column加入到UserData中，方便遍历
                var tmpFields = fieldMapping.GetNewFieldsBeforeAdd();
                if (tmpFields != null && mItemUserData.ContainsKey("#tp_ID"))
                {
                    foreach (var field in tmpFields)
                    {
                        if (!String.IsNullOrEmpty(field.InternalName)
                            && !field.InternalName.Equals("ID", StringComparison.OrdinalIgnoreCase)
                            && !field.InternalName.Equals("FileLeafRef", StringComparison.OrdinalIgnoreCase)
                            && !mItemUserData.ContainsKey(field.InternalName))
                        {
                            mItemUserData.Add(field.InternalName, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 包括userdata的所有处理过程
        /// </summary>
        /// <param name="finalColumnValues"></param>
        /// <param name="isMergeToFolder"></param>
        /// <returns>needReload</returns>
        private bool ProcessUserData(Dictionary<string, AveFieldValueInfo> finalColumnValues, MetadataOption option)
        {
            using (new AvePerformanceScope("Restore.ItemUserAndJunctionData.CheckUserData"))
            {
                bool needReload = false;
                if (mItemUserData == null)
                {
                    return needReload;
                }
                //根据目的端的信息修改相应的user data
                if (!(this.mItem is AveSPFolder))   // folder 没有meta data(doc set 除外，有客户需要再考虑),  不需要走这个mapping
                {
                    UpdateNeedRestoreUserData();
                }

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
                        needReload |= GetXmlFieldAndDestField(fieldName, pair.Value != null, ref xmlField, ref destField);
                        //找到源端和目的端的column后，判断当前的column是否有还原的必要
                        if (!NeedRestoreUserData(fieldName, destField))
                        {
                            continue;
                        }
                        //单值Lookup
                        if (fieldName.Equals("AppAuthor", StringComparison.OrdinalIgnoreCase) && mItem.ParentWeb.AppAuthorId > 0)
                        {
                            finalColumnValues[destField.InternalName] = new AveFieldValueInfo { ColValue = mItem.ParentWeb.AppAuthorId, ColName = destField.ColName, FieldType = destField.Type, RowOrdinal = destField.RowOrdinal, Id = destField.ID };
                            continue;
                        }
                        //处理备份过来的column value，check是否有相应的value mapping，并且检查该value还原到目的端是否合法
                        object finalValue;
                        if (!NeedMappingFieldValue(xmlField, option.isMergToFolder, pair.Value))
                        {
                            finalValue = ProcessDataWithoutMapping(fieldName, xmlField, destField, pair.Value, option);
                        }
                        else
                        {
                            finalValue = ProcessDataWithMapping(fieldName, xmlField, destField, pair.Value, option);
                        }

                        //ADO-135572 当lookup column没有关联的list时，365更新空的value值会抛错
                        if (destField.Type == AveFieldType.Lookup)
                        {
                            var lookupDestField = destField as IAveFieldLookup;
                            if (lookupDestField.LookupList == null)
                            {
                                continue;
                            }
                        }

                        if (destField.TypeAsString.Equals("RelatedItems"))
                        {
                            this.mItem.relatedItemsInfo = new AveRelatedItemsInfo() { WebId = this.mItem.ParentWeb.SPWeb.ID, ListId = this.mItem.ParentList.SPList.ID, Version = this.mItem.Version, Schema = finalValue.ToString() };
                            continue;
                        }

                        finalColumnValues[destField.InternalName] = CreateFieldValueInfo(xmlField.FieldInternalName, finalValue, destField, finalColumnValues);

                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while Check field value. key:{0}, value:{1}\n error message:{2}", pair.Key, pair.Value, e);
                    }
                }

                return needReload;
            }
        }

        /// <summary>
        /// 处理field mapping中ChangeToDestination的特殊case
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="mappedValue"></param>
        /// <param name="mappedValues"></param>
        /// <param name="xmlField"></param>
        /// <param name="destField"></param>
        private void ProcessFieldValueByTypeChanged(string fieldName, ref List<string> mappedValues, AveXmlField xmlField, IAveField destField)
        {
            if (xmlField.Type == AveFieldType.User &&
                (string.Equals(destField.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase)
                || string.Equals(destField.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)))
            {
                if (mappedValues != null && mappedValues.Count > 0)
                {
                    for (int k = 0; k < mappedValues.Count; k++)
                    {
                        mappedValues[k] = GetUserLoginWithoutPrefix(mappedValues[k]);
                    }
                }
            }
        }

        private string GetUserLoginWithoutPrefix(string loginWithPrefix)
        {
            string value = loginWithPrefix;
            if (!string.IsNullOrEmpty(loginWithPrefix))
            {
                //参照AveUser的NoPrefixLoginName的实现方式
                int index = loginWithPrefix.IndexOf('|');
                if (index > 0)
                {
                    log.Debug("Remove prefix from the user login {0} while mapping column from User to Metadata", loginWithPrefix);
                    value = loginWithPrefix.Substring(index + 1);
                }
            }
            return value;
        }

        /// <summary>
        /// 获取源端column对应的xmlField和目的端column
        /// </summary>
        private bool GetXmlFieldAndDestField(string fieldName, bool ensureField, ref AveXmlField xmlField, ref IAveField destField)
        {
            bool fieldAddOrUpdate = false;
            //正常从源端备份过来的field（除去BuiltIn 的column）都有xmlfield，而根据excel在目的端创建出来的column没有xmlField
            if (xmlFields != null && xmlFields.ContainsKey(fieldName))
            {
                xmlField = xmlFields[fieldName];
                //在目的端查找相应的field，如果没有，根据反插的option进行处理
                destField = ensureField ? listFieldCollection.GetField(fieldName, out fieldAddOrUpdate) : listFieldCollection.GetFieldByInternalName(fieldName);
            }
            else
            {
                if (mItem.ParentList.SPList.Fields.ContainsField(fieldName))
                {
                    destField = mItem.ParentList.SPList.Fields.GetFieldByInternalName(fieldName);
                    //buildIn column没有xmlfield，需要根据目的端的column创建出来
                    XmlDocument schemaXml = new XmlDocument();
                    schemaXml.LoadXml(destField.SchemaXml);
                    xmlField = new AveXmlField(schemaXml.DocumentElement, (int)mItem.ParentWeb.SPWeb.Language);
                }
            }
            return fieldAddOrUpdate;
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
        private bool ProcessDataJunction(Dictionary<string, AveFieldValueInfo> finalColumnValues,MetadataOption option)
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
                    AveXmlField xmlField = mItem.ParentList.AveFields.GetXmlFieldBySourceFieldId(sourceFieldId);
                    IAveField destField = null;
                    try
                    {
                        if (xmlField != null)
                        {
                            bool fieldAddOrUpdate;
                            destField = mItem.ParentFolder.ParentList.AveFields.GetField(xmlField.FieldInternalName, out fieldAddOrUpdate);
                            needReload |= fieldAddOrUpdate;
                        }
                        else
                        {
                            destField = mItem.ParentFolder.ParentList.AveFields.GetFieldById(mItem.ParentFolder.ParentList.AveFields.FieldMapping.GetMappingRestoredFieldId(sourceFieldId));
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Warn("Restore data junction ensure field failed.Exception:{0}", ex);
                    }
                    if (!NeedRestoreDataJunction(xmlField, destField))
                    {
                        continue;
                    }

                    string fieldName = xmlField == null ? destField.InternalName : xmlField.FieldInternalName;
                    object finalValue;
                    if (!NeedMappingFieldValue(xmlField, false, pair.Value))
                    {
                        finalValue = ProcessDataWithoutMapping(fieldName, xmlField, destField, pair.Value,option);
                    }
                    else
                    {
                        finalValue = ProcessDataWithMapping(fieldName, xmlField, destField, pair.Value, option);
                    }

                    //ADO-135572 当lookup column没有关联的list时，365更新空的value值会抛错
                    if (destField.Type == AveFieldType.Lookup)
                    {
                        var lookupDestField = destField as IAveFieldLookup;
                        if (lookupDestField.LookupList == null)
                        {
                            continue;
                        }
                    }

                    if (destField.TypeAsString.Equals("RelatedItems"))
                    {
                        this.mItem.ParentSite.MappingManager.SiteMappingManager.AddRelatedItemsFieldValue(this.mItem.ParentWeb.SPWeb.ID, this.mItem.ParentList.SPList.ID, this.mItem.RowId, this.mItem.Version, finalValue.ToString());
                        continue;
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

        private string GetUserLoginNameById(int userId)
        {
            string loginName = string.Empty;
            object info = mItem.ParentSite.SPMembers.UserAndDomainMapping.GetUserMapping(userId);
            if (info != null)
            {
                if (info is AveSPMemberInfo)
                {
                    loginName = GetUserLoginName((info as AveSPMemberInfo).SourceInfo);
                }
                else
                {
                    loginName = GetUserLoginName(info);
                }
            }
            return loginName;
        }

        private string GetUserLoginName(object info)
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

        private object ProcessDataWithoutMapping(string fieldName, AveXmlField xmlField, IAveField destField, object value, MetadataOption option)
        {
            var dataFormatObj = DataFormatFactory.CreateInstance(xmlField, destField, mItem, originalVersion, originalRowId, mItemUserData, option);
            return dataFormatObj.CheckFieldValue(value);
        }

        private object ProcessDataWithMapping(string fieldName, AveXmlField xmlField, IAveField destField, object value, MetadataOption option)
        {
            bool isSourceMultiple = IsMultipleField(xmlField.Type, xmlField.TypeAsString);//源端是否是多值
            bool isDestMultiple = IsMultipleField(destField.Type, destField.TypeAsString);
            var sourceFieldValueInfo = GetSourceFieldValueInfo(xmlField, destField, value);
            object finalValue = null;
            List<string> mappingValues = new List<string>();

            //根据源端是否是多值进行处理源端的column value，对各个value 进行mapping，在根据目的端是否是多值，去check mapping后的value是否合法。
            if (isSourceMultiple || !string.IsNullOrEmpty(sourceFieldValueInfo.SplitString))
            {
                mappingValues = fieldMapping.GetMultiMappingValue(sourceFieldValueInfo);
            }
            else
            {
                mappingValues.Add(fieldMapping.GetMappingValue(sourceFieldValueInfo));
            }

            ProcessFieldValueByTypeChanged(fieldName, ref mappingValues, xmlField, destField);

            //ValueConvertObject 需要在mapping value之后create，因为excel mapping的column，在excel中添加的column没有source value，在mapping后，会把得到的value当做source value
            IValueConvertObject valueConvertObj = CreateValueConvertObject(fieldName, destField, sourceFieldValueInfo, option);
            if (isDestMultiple)
            {
                finalValue = valueConvertObj.ConvertMultiValue(mappingValues);
            }
            else
            {
                finalValue = valueConvertObj.ConvertSingleValue(mappingValues[0]);
            }
            return finalValue;
        }

        private IValueConvertObject CreateValueConvertObject(string fieldName, IAveField destField, AveSourceFieldValueInfo sourceFieldValueInfo,MetadataOption option)
        {
            IValueConvertObject valueConvertObj = null;
            if (string.IsNullOrEmpty(sourceFieldValueInfo.SourceValue))
            {
                valueConvertObj = ValueConvertObjectFactory.CreateInstance(fieldName, destField, sourceFieldValueInfo.SourceDataJunction, mItem, originalVersion, originalRowId, mItemUserData, option);
            }
            else
            {
                valueConvertObj = ValueConvertObjectFactory.CreateInstance(fieldName, destField, sourceFieldValueInfo.SourceValue, mItem, originalVersion, originalRowId, mItemUserData, option);
            }
            return valueConvertObj;
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
            if (this.listFieldCollection.WorkflowStatusFields.Contains(fieldName,StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }

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
            if (!xmlField.TypeAsString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase)
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

        private bool NeedMappingFieldValue(AveXmlField xmlField, bool isMergeToFolder, object value)
        {
            if (mItemUserData.Count != 0 && !mItemUserData.ContainsKey("#tp_ID"))
            {
                return false;
            }
            //该逻辑是CM专用。CM 应用export folder，更改其title然后import还原时对于还原folder下的file会因为找不到目的端的folder而跑一场，添加该逻辑
            if (isMergeToFolder && (xmlField.FieldInternalName.Equals("FileLeafRef", StringComparison.OrdinalIgnoreCase) || xmlField.FieldInternalName.Equals("Title", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            if (value != null && xmlField.CustomFieldInfo != null)
            {
                if (xmlField.TypeAsString.Equals("Lookup", StringComparison.OrdinalIgnoreCase))
                {
                    return value.ToString().Contains(";");
                }
                if (xmlField.TypeAsString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase))
                {
                    var dataJunction = value as Dictionary<int, string>;
                    if (dataJunction != null)
                    {
                        foreach (var pair in dataJunction)
                        {
                            return pair.Value != null;
                        }
                    }
                }
            }
            //customFieldInfo为空的情况下，标示该column没有对应的column mapping，所以不需要走value mapping逻辑
            if (xmlField.CustomFieldInfo == null)
            {
                return false;
            }
            return true;
        }

        private AveSourceFieldValueInfo GetSourceFieldValueInfo(AveXmlField xmlField, IAveField destField, object value)
        {
            bool isDestMultiple = IsMultipleField(destField.Type, destField.TypeAsString);
            AveSourceFieldValueInfo valueInfo = new AveSourceFieldValueInfo { SourceItemName = mItem.Name, SourceItemRowId = originalRowId };
            if (xmlField != null)
            {
                valueInfo.SourceFieldInfo = new AveSourceFieldInfo
                {
                    SourceDisplayName = xmlField.Title,
                    SourceInternalName = xmlField.FieldInternalName,
                    SourceType = xmlField.Type,
                    SourceTypeAsString = xmlField.TypeAsString
                };
            }
            else
            {
                valueInfo.SourceFieldInfo = new AveSourceFieldInfo
                {
                    SourceDisplayName = destField.Title,
                    SourceInternalName = destField.InternalName,
                    SourceType = destField.Type,
                    SourceTypeAsString = destField.TypeAsString
                };
            }
            valueInfo.SplitString = GetSplitChar(xmlField.CustomFieldInfo, isDestMultiple);
            valueInfo.SourceFieldInfo.RichText = xmlField.RichText;
            valueInfo.SourceFieldInfo.SourceWebAppUrl = this.mItem.ParentSite.SourceSiteInfo.WebAppUrl;
            valueInfo.SourceFieldInfo.SourceSiteUrl = this.mItem.ParentSite.SourceSiteInfo.Url;
            if (value != null)
            {
                #region Prepare Need Mapping Value
                if (xmlField.Type == AveFieldType.User && value != null)
                {
                    if (!xmlField.AllowMultipleValues)
                    {
                        int userId;
                        if (int.TryParse(value.ToString(), out userId))
                        {
                            valueInfo.SourceValue = GetUserLoginNameById(userId);
                        }
                    }
                    else
                    {
                        var sourceUserDataJunction = value as Dictionary<int, string>;
                        Dictionary<int, string> userSourceLoginNames = new Dictionary<int, string>();
                        if (sourceUserDataJunction != null)
                        {
                            foreach (var pair in sourceUserDataJunction)
                            {
                                userSourceLoginNames[pair.Key] = GetUserLoginNameById(pair.Key);
                            }
                        }
                        valueInfo.SourceDataJunction = userSourceLoginNames;
                    }
                }
                else if (xmlField.TypeAsString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase))
                {
                    var lookupDataJunction = value as Dictionary<int, string>;
                    if (lookupDataJunction != null)
                    {
                        valueInfo.SourceDataJunction = lookupDataJunction;
                    }
                    else
                    {
                        valueInfo.SourceValue = value.ToString();
                    }
                }
                else
                {
                    valueInfo.SourceValue = value.ToString().Trim();
                }
                #endregion
            }
            return valueInfo;
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
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ConverToFieldAttributeError, e.ToString());
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

        private bool IsMultipleField(AveFieldType type, string typeAsString)
        {
            using (new AvePerformanceScope("Restore.ItemUserAndJunctionData.IsMultipleField"))
            {
                switch (type)
                {
                    case AveFieldType.MultiChoice:
                        return true;
                    case AveFieldType.User:
                        if (typeAsString.Equals("UserMulti", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        break;
                    case AveFieldType.Lookup:
                        if (typeAsString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        break;
                    case AveFieldType.Note:
                        return true;
                    case AveFieldType.Invalid:
                        if (typeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        if (typeAsString.Equals("HTML", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        break;
                }
                return false;
            }
        }

        private string GetSplitChar(AveCustomFieldInfo info, bool isDestMultiple)
        {
            if (info is AveCustomLookupFieldInfo)
            {
                return (info as AveCustomLookupFieldInfo).SeparateChar;
            }
            if (info is AveCustomMetadataFieldInfo)
            {
                return (info as AveCustomMetadataFieldInfo).SeparateChar;
            }
            if (info is AveCustomChangeToDesInfo)
            {
                return (info as AveCustomChangeToDesInfo).SeparateChar;
            }
            return null;
        }
    }
}
