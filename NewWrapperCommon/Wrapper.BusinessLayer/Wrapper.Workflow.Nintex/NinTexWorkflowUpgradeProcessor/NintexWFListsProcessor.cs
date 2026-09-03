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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace LS.SPWorkflowProcessor
{
    class NintexWFListsProcessor
    {
        private IAveList currentList;
        private IAveWeb currentWeb;
        public NintexWFListsProcessor(IAveWeb web, IAveList list)
        {
            this.currentWeb = web;
            this.currentList = list;
        }

        public byte[] GetListsContent(Dictionary<Guid, List<string>> listLookup)
        {
            var listInfoArray = listLookup.Select(keyValue => CreateListInfo(keyValue.Key, keyValue.Value)).ToList();

            if (this.currentList != null && !listLookup.ContainsKey(currentList.ID))
            {
                listInfoArray.Add(CreateListInfo(this.currentList.ID, null));
            }

            if (listInfoArray.Count > 0)
            {
                return SerializerHelper.SerializeObjectToBytes(listInfoArray);
            }
            return null;
        }

        private ListInfo CreateListInfo(Guid listId, List<string> fieldsName)
        {
            var tempList = currentWeb.Lists[listId];
            return new ListInfo
            {
                ListId = listId.ToString(),
                Title = tempList.Title,
                Fields = CreateFields(tempList, fieldsName),
                IsCurrentList = currentList != null ? listId.Equals(currentList.ID) : false,
            };
        }


        public List<FieldInfo> CreateFields(IAveList list, List<string> keyValue)
        {
            return keyValue == null || keyValue.Count == 0 ? null : keyValue.Select(fieldName => CreateFieldInfo(list, fieldName)).Distinct(f => f.InternalName).ToList();
        }

        public FieldInfo CreateFieldInfo(IAveList list, string fieldName)
        {
            var field = list.Fields.GetField(fieldName);

            return new FieldInfo
            {
                InternalName = field.InternalName,
                FieldId = field.ID.ToString(),
                Title = field.Title,
                OutputType = "",
                TypeDisplayName = field.TypeDisplayName,
            };
        }
    }
}
