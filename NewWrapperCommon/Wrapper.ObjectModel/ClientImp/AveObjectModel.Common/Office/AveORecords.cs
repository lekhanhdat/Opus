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
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
using System.Collections;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveORecords : IAveORecords
    {
        #region IAveORecords Members

        public void ConfigureListForAutoDeclarationCore(IAveList list, bool autoDeclare, bool updateNow, ref bool needUpdate)
        {
            throw new NotImplementedException();
        }

        public bool GetBoolValueFromHashtable(Hashtable table, string key)
        {
            throw new NotImplementedException();
        }

        public void SetBoolIprPropertyCore(IAveList list, string propName, bool? value, bool updateNow, ref bool needUpdate)
        {
            throw new NotImplementedException();
        }

        public void UnlockItem(IAveListItem itemToUnlock, string lockName)
        {
            throw new NotImplementedException();
        }

        public bool IsLocked(IAveListItem item)
        {
            throw new NotImplementedException();
        }

        public void UndeclareItemAsRecord(IAveListItem item)
        {
            if (!item.FieldValues.ContainsKey("_vti_ItemHoldRecordStatus") ||
                item.FieldValues["_vti_ItemHoldRecordStatus"] == null ||
                string.IsNullOrEmpty(item.FieldValues["_vti_ItemHoldRecordStatus"].ToString()))
            {
                return;
            }
            (item.Web.Site as AveSite).Request.DeclareOrUndeclareItem(item.ID, item.ParentList.ID, item.Web.Url);
        }

        public void DeclareItemAsRecord(IAveListItem item)
        {
            if (item.FieldValues.ContainsKey("_vti_ItemHoldRecordStatus") &&
                item.FieldValues["_vti_ItemHoldRecordStatus"] != null &&
                item.FieldValues["_vti_ItemHoldRecordStatus"].ToString().Equals("273", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            (item.Web.Site as AveSite).Request.DeclareOrUndeclareItem(item.ID, item.ParentList.ID, item.Web.Url);
        }

        public bool IsOnHold(IAveListItem item)
        {
            throw new NotImplementedException();
        }

        public bool IsRecord(IAveListItem item)
        {
            throw new NotImplementedException();
        }

        public bool IsDeleteBlocked(IAveListItem item)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
