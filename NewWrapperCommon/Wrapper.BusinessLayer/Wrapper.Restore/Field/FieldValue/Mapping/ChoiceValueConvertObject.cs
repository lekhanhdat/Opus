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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;

namespace AvePoint.Wrapper.Restore
{
    class ChoiceValueConvertObject : BaseValueConvertObject
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(ChoiceValueConvertObject));
        private const string splitChar = ";#";

        public ChoiceValueConvertObject(IAveField destField, AveSPItem mItem, int originalRowId)
            : base(destField, mItem, originalRowId)
        {
        }

        public override object ConvertSingleValue(string value)
        {
            if (CheckChoiceExist(value))
            {
                return value;
            }
            return null;
        }

        public override object ConvertMultiValue(List<string> values)
        {
            List<string> allChoices = new List<string>();  // 仅为了记住value 值是否被重复添加
            StringBuilder strBuilder = new StringBuilder();
            bool hasValue = false;
            foreach (var v in values)
            {
                if (allChoices.Contains(v))
                {
                    //已经处理过这个Choice，不需要重复处理。
                    continue;
                }
                allChoices.Add(v);

                if (CheckChoiceExist(v))
                {
                    strBuilder.Append(v);
                    strBuilder.Append(splitChar);
                    hasValue = true;
                }
            }
            if (hasValue)//remove redundant string.
            {
                strBuilder.Length -= splitChar.Length;
                return strBuilder.ToString();
            }
            return null;
        }

        private bool CheckChoiceExist(string value)
        {
            var destChoiceField = destField as IAveFieldMultiChoice;
            if (destChoiceField.InternalName.Equals("AppMetadataLocale", StringComparison.OrdinalIgnoreCase) && (destChoiceField.Choices == null || destChoiceField.Choices.Count == 0))
            {
                return true;
            }
            if (!string.IsNullOrEmpty(value))
            {
                //if ((destChoiceField.Choices.Contains(value) || destChoiceField.FillInChoice))
                //{
                return true;
                //}
                //else
                //{
                //    log.Debug("The Choice:{0} is not exist,and the destination choice field is not all FillIn,So it can not be restored", value);
                //}
            }
            return false;
        }
    }
}
