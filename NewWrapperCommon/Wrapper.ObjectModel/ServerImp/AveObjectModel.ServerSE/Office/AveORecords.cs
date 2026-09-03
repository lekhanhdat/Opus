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
using System.Collections;
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.RecordsManagement.RecordsRepository;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveORecords : IAveORecords
    {
        private const string mRecords_Type = "Microsoft.Office.RecordsManagement.RecordsRepository.Records";
        private const string mRecords_ConfigureListForAutoDeclarationCore_Method = "ConfigureListForAutoDeclarationCore";
        private const string mRecords_GetBoolValueFromHashtable_Method = "GetBoolValueFromHashtable";
        private const string mRecords_SetBoolIprPropertyCore_Method = "SetBoolIprPropertyCore";

        public AveORecords()
        { }

        #region IAveRecords Members

        public void ConfigureListForAutoDeclarationCore(IAveList list, bool autoDeclare, bool updateNow, ref bool needUpdate)
        {
            AveAssemblyUtility.InvokeStaticMethod(mRecords_Type, mRecords_ConfigureListForAutoDeclarationCore_Method, new object[] { (list as AveList).List, autoDeclare, updateNow, needUpdate });
        }

        public bool GetBoolValueFromHashtable(Hashtable table, string key)
        {
            bool result = false;
            object obj2 = table[key];
            string str = string.Empty;
            if (obj2 != null)
            {
                str = obj2.ToString();
            }
            if (!string.IsNullOrEmpty(str) && !bool.TryParse(str, out result))
            {
                return false;
            }
            return result;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint method name")]
        public void SetBoolIprPropertyCore(IAveList list, string propName, bool? value, bool updateNow, ref bool needUpdate)
        {
            AveAssemblyUtility.InvokeStaticMethod(mRecords_Type, mRecords_SetBoolIprPropertyCore_Method, new object[] { (list as AveList).List, propName, value, updateNow, needUpdate });
        }

        public void UnlockItem(IAveListItem itemToUnlock, string lockName)
        {
            Records.UnlockItem((itemToUnlock as AveListItem).ListItem, lockName);
        }

        public bool IsLocked(IAveListItem item)
        {
            return Records.IsLocked((item as AveListItem).ListItem);
        }

        public void UndeclareItemAsRecord(IAveListItem item)
        {
            Records.UndeclareItemAsRecord((item as AveListItem).ListItem);
        }

        public void DeclareItemAsRecord(IAveListItem item)
        {
            Records.DeclareItemAsRecord((item as AveListItem).ListItem);
        }

        public bool IsOnHold(IAveListItem item)
        {
            return Records.IsOnHold((item as AveListItem).ListItem);
        }

        public bool IsRecord(IAveListItem item)
        {
            return Records.IsRecord((item as AveListItem).ListItem);
        }

        public bool IsDeleteBlocked(IAveListItem item)
        {
            return Records.IsDeleteBlocked((item as AveListItem).ListItem);
        }
        #endregion
    }
}
