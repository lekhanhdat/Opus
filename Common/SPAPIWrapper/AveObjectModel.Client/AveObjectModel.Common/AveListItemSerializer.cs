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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveListItemSerializer : IAveListItemSerializer
    {
        private AveList mList;

        public object GetObjectData()
        {
            throw new NotImplementedException();
        }

        public AveRestoreResult SetObjectData(AveListItemInfo info)
        {
            mList.SetTaxonomyField(info, -1, true, info.FieldsInfo.TermIdMapping);
            Dictionary<string, object> docData = AveList.AssembleBaseItemInfo(info);
            docData["ListTemplate"] = (int)mList.BaseTemplate;
            docData["ListEnableModeration"] = mList.EnableModeration;
            docData["ListEnableVersioning"] = mList.EnableVersioning;
            Dictionary<string, object> fields = AveList.ConvertFieldValuesToString(info.FieldsInfo.Fields);

            if (mList.BaseTemplate == AveListTemplateType.DiscussionBoard)
            {
                if (info.DocData.ContainsKey("DiscussionTopic"))
                {
                    docData["DiscussionTopic"] = info.DocData["DiscussionTopic"];
                }
                if (info.DocData.ContainsKey("ParentThreadId"))
                {
                    docData["ParentThreadId"] = info.DocData["ParentThreadId"];
                }
            }

            if (mList.BaseTemplate == AveListTemplateType.Meetings)
            {
                mList.AssemblyMeetingItemInfo(info, info.UserData, docData);
            }
            if (mList.NeedSetNullFields == null)
            {
                mList.NeedSetNullFields = mList.SetNeedSetNullFields(info.KeepDefaultValue);
            }
            fields.Add("NeedSetNullFields", mList.NeedSetNullFields);
            Dictionary<string, object> restoreResult = mList.Request.RestoreListItem(docData, fields);
            info.IsNewCreated = restoreResult.ContainsKey("IsNewCreated") ? (bool)restoreResult["IsNewCreated"] : false;

            if (!(Boolean)restoreResult["RestoreStatus"])
            {
                throw new AveRestoreException(AveRestoreResult.Failed, restoreResult["Exception"] as string);
            }
            AveListItem item = new AveListItem(mList.Request, mList.ParentWeb, mList, restoreResult["Item"] as Dictionary<string, object>, false);
            info.AveItem.ListItem = item;
            info.RowId = item.ID;
            return AveRestoreResult.Normal;
        }
    }
}
