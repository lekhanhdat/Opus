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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System;

namespace AvePoint.ObjectModel.Server19
{
    class AveFieldText : AveField, IAveFieldText
    {
        private SPFieldText mFieldText;

        public AveFieldText(SPFieldText fieldText)
            : base(fieldText)
        {
            mFieldText = fieldText;
        }

        public AveFieldText(AveFieldCollection fieldColl, SPFieldText fieldText)
            : base(fieldColl, fieldText)
        {
            mFieldText = fieldText;
        }
        public string XPath
        {
            get
            {
                return mFieldText.XPath;
            }
            set
            {
                mFieldText.XPath = value;
            }
        }

        public override Type FieldValueType
        {
            get
            {
                return typeof(string);
            }
        }

        #region IAveFieldText Members

        public int MaxLength
        {
            get
            {
                return mFieldText.MaxLength;
            }
            set
            {
                mFieldText.MaxLength = value;
            }
        }

        public int DifferencingLimit
        {
            get
            {
                return mFieldText.DifferencingLimit;
            }
            set
            {
                mFieldText.DifferencingLimit = value;
            }
        }

        #endregion
    }
}
