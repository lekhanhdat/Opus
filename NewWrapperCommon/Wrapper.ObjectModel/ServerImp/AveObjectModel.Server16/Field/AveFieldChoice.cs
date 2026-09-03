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

namespace AvePoint.ObjectModel.Server16
{
    class AveFieldChoice : AveFieldMultiChoice, IAveFieldChoice
    {
        private SPFieldChoice mFieldChoice;

        public AveFieldChoice(AveFieldCollection fieldColl, SPFieldChoice field)
            : base(fieldColl, field)
        {
            mFieldChoice = field;
        }

        public AveFieldChoice(SPFieldChoice fieldChoice)
            : base(fieldChoice)
        {
            mFieldChoice = fieldChoice;
        }

        #region IAveFieldChoice Members

        public AveChoiceFormatType EditFormat
        {
            get
            {
                return (AveChoiceFormatType)mFieldChoice.EditFormat;
            }
            set
            {
                mFieldChoice.EditFormat = (SPChoiceFormatType)value;
            }
        }

        public override Type FieldValueType
        {
            get
            {
                return typeof(string);
            }
        }

        public override object GetFieldValue(string value)
        {
            return value;
        }

        public System.Collections.Specialized.StringCollection ChoicesJumpTo
        {
            get { return mFieldChoice.ChoicesJumpTo; }
        }

        public string FillinChoiceJumpTo
        {
            get
            {
                return mFieldChoice.FillinChoiceJumpTo;
            }
            set
            {
                mFieldChoice.FillinChoiceJumpTo = value;
            }
        }

        #endregion
    }
}
