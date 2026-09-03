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

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    class NumberDataFormatForHSMConnector : BaseDataFormatForHSMConnector
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(NumberDataFormatForHSMConnector));

        public NumberDataFormatForHSMConnector(AveXmlField xmlField, IAveField destField, IAveListItem mItem) :
            base(xmlField, destField, mItem)
        {
        }

        public override object CheckFieldValue(object value)
        {
            var destNumberField = destField.Type == AveFieldType.Currency ? destField as IAveFieldCurrency : destField as IAveFieldNumber;
            double doubleValue;
            if (!double.TryParse(value.ToString(), out doubleValue))
            {
                log.Debug("The value was not correct format,Value:{0}", value);
                return null;
            }

            if (doubleValue < destNumberField.MinimumValue)
            {
                return null;
            }
            if (doubleValue > destNumberField.MaximumValue)
            {
                return null;
            }
            return doubleValue;
        }
    }
}
