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
using System.Collections;
using System.Collections.Generic;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    
    class LookupDataFormat : BaseDataFormat
    {
        private int originalVesrion;

        public LookupDataFormat(AveXmlField xmlField, IAveField destField, AveSPItem mItem, int originalVesrion) :
            base(xmlField, destField, mItem)
        {
            this.originalVesrion = originalVesrion;
        }

        public override object CheckFieldValue(object value)
        {
            if (value != null)
            {
                var destLookupField = destField as IAveFieldLookup;
                //sourceLookupValue用来存放需要cache的lookupValue
                ArrayList sourceLookupValue = new ArrayList();
                bool needCache = false;
                //如果lookup column没有关联的list，那么此时说明column还没有还原完成，不需要check value直接cache value在post action中处理。
                Guid lookupListId = destLookupField.LookupList == null ? Guid.Empty : new Guid(destLookupField.LookupList);
                if (lookupListId == Guid.Empty)
                {
                    needCache = true;
                }
                if (xmlField.AllowMultipleValues)
                {
                    var lookupItemVaues = new List<LookupItemValue>();
                    //对于多值value没有值的情况，value为 string.Empty 
                    var pairValues = value as Dictionary<int, string>;
                    if (pairValues != null)
                    {
                        foreach (var pair in pairValues)
                        {
                            String itemColumnValue = pair.Value;
                            sourceLookupValue.Add(string.Format("{0};{1}", pair.Key.ToString(), itemColumnValue));
                      
                            if (!needCache)
                            {
                                String tempPairValue = string.Format("{0};{1}", pair.Key.ToString(), pair.Value);
                                Guid itemGuid;
                                string itemLeafName;
                                string itemColumnDisplayValue;
                                ResolveLookupFieldValue(tempPairValue, out itemGuid, out itemLeafName, out itemColumnDisplayValue);
                                var itemValue = GetDestItemValue(pair.Key, itemGuid, itemLeafName, lookupListId, destLookupField.LookupWebId);
                                if (itemValue.ItemRowId <= 0)
                                {
                                    needCache = true;
                                }
                                else if (!destLookupField.AllowMultipleValues)
                                {
                                    return WrapperSingleResult(itemValue);
                                }
                                else
                                {
                                    lookupItemVaues.Add(new LookupItemValue {
                                        ItemRowId = itemValue.ItemRowId,
                                        ItemUniqueId = itemValue.ItemUniqueId,
                                        lookupValue = itemColumnValue,
                                    });
                                }
                            }
                        }
                    }
                    if (!needCache)
                    {
                        return WrapperMultiValues(lookupItemVaues);
                    }
                }
                else
                {
                    //Replicator Pmode singleValue为“rowId#Guid”格式
                    var singleValue = value.ToString();
                    sourceLookupValue.Add(singleValue);
       
                    if (!needCache)
                    {
                        Guid itemGuid;
                        string itemLeafName;
                        string itemColumnDisplayValue;
                        int sourceItemId = ResolveLookupFieldValue(singleValue, out itemGuid, out itemLeafName, out itemColumnDisplayValue);
                        var itemValue = GetDestItemValue(sourceItemId, itemGuid, itemLeafName, lookupListId, destLookupField.LookupWebId);

                        if (itemValue.ItemRowId <= 0)
                        {
                            needCache = true;
                        }
                        else
                        {
                            return WrapperSingleResult(itemValue);
                        }
                    }
                }
            }
            return string.Empty;
        }

        protected virtual object WrapperMultiValues(List<LookupItemValue> lookupItemVaues)
        {
            IAveFieldLookupValueCollection lookupValues = mItem.ParentSite.ObjectModelFactory.CreateFieldLookupValueCollection();
            foreach (var value in lookupItemVaues)
            {
                lookupValues.Add(mItem.ParentSite.ObjectModelFactory.CreateFieldLookupValue(value.ItemRowId, value.lookupValue));
            }
            return lookupValues;
        }

        protected virtual object WrapperSingleResult(LookupItemValue itemValue)
        {
            return itemValue.ItemRowId;
        }

        protected virtual LookupItemValue GetDestItemValue(int sourceItemId, Guid itemGuid, string leafName, Guid lookupListId, Guid lookupWebId)
        {
            return new LookupItemValue { ItemRowId = sourceItemId };
        }


        private int ResolveLookupFieldValue(string singleValue, out Guid itemGuid, out string itemLeafName, out string itemColumnValueDisplayName)
        {
            bool needRestoreLookupItemByLookupValue = false;
            itemGuid = Guid.Empty;
            itemLeafName = String.Empty;
            itemColumnValueDisplayName = String.Empty;
            if (singleValue.EndsWith("*", StringComparison.OrdinalIgnoreCase))
            {
                singleValue = singleValue.TrimEnd('*');
                needRestoreLookupItemByLookupValue = true;
            }
            //var leafNameIndex = singleValue.LastIndexOf('&');
            var leafNameIndex = singleValue.IndexOf("&leafName&");
            //value中包含&说明备份了itemLeafName,只有lookup List为Document Library才可能会备份
            if (leafNameIndex > 0)
            {
                itemLeafName = singleValue.Substring(leafNameIndex + 10);
                singleValue = singleValue.Substring(0, leafNameIndex);
            }
            //var guidIndex = singleValue.IndexOf('#');
            var guidIndex = singleValue.IndexOf("#GUID#");
            //value中包含#说明备份了TPGuid
            if (guidIndex >= 0 && singleValue.Substring(guidIndex + 6).Length >= 36 && AveTypeHelper.IsGuid(singleValue.Substring(guidIndex + 6, 36)))
            {
                itemGuid = new Guid(singleValue.Substring(guidIndex + 6, 36));
                singleValue = singleValue.Substring(0, guidIndex);
            }
            var idIndex = singleValue.IndexOf(';');
            var idStr = idIndex >= 0 ? singleValue.Substring(0, idIndex) : singleValue;
            int itemId = String.IsNullOrEmpty(idStr) ? -1 : Convert.ToInt32(idStr);
            if (needRestoreLookupItemByLookupValue)
            {
                itemColumnValueDisplayName = singleValue.Substring(singleValue.IndexOf(';') + 1);
            }
            return itemId;
        }
        
    }
}
