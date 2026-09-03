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
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    class ChoiceDataFormat : BaseDataFormat
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(ChoiceDataFormat));

        private const string splitChar = ";#";

        public ChoiceDataFormat(AveXmlField xmlField, IAveField destField, AveSPItem mItem) :
            base(xmlField, destField, mItem)
        {
        }

        public override object CheckFieldValue(object value)
        {
            if (xmlField.Type==AveFieldType.MultiChoice)
            {
                StringBuilder newValue = new StringBuilder();
                List<string> allChoice = new List<string>();
                string[] values = value.ToString().Split(new string[] {splitChar }, StringSplitOptions.RemoveEmptyEntries);
                bool hasValue = false;
                foreach (var v in values)
                {
                    if (CheckChoiceExistOrRepeated(v, allChoice))
                    {
                        newValue.Append(v);
                        newValue.Append(splitChar);
                        hasValue = true;
                        if (destField.Type == AveFieldType.Choice)
                        {
                            break;
                        }
                    }
                }
                if (hasValue)
                {
                    return newValue.Remove(newValue.Length - splitChar.Length, splitChar.Length).ToString();
                }
            }
            else
            {
                return value;
            }
            return string.Empty;
        }

        private bool CheckChoiceExistOrRepeated(string value, List<string> allChoices)
        {
            var destChoiceField = destField as IAveFieldMultiChoice;
            if (destChoiceField.InternalName.Equals("AppMetadataLocale", StringComparison.OrdinalIgnoreCase) && (destChoiceField.Choices == null || destChoiceField.Choices.Count == 0))
            {
                allChoices.Add(value);
                return true;
            }
            if (!string.IsNullOrEmpty(value) && !allChoices.Contains(value))
            {
                allChoices.Add(value);
                return true;
                //if ((destChoiceField.Choices.Contains(value) || destChoiceField.FillInChoice) && !allChoices.Contains(value))
                //{
                //    allChoices.Add(value);
                //    return true;
                //}
            }
            //if (!destChoiceField.Choices.Contains(value) && !destChoiceField.FillInChoice)
            //{
            //    log.Debug("The Choice:{0} is not exist,and the destination choice field is not all FillIn,So it can not be restored", value);
            //}
            return false;
        }
    }
}
