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



namespace AvePoint.ObjectModel.ServerSE
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint;
    #endregion

    class AveFieldMultiChoiceValue : IAveFieldMultiChoiceValue
    {
        private SPFieldMultiChoiceValue m_FieldMultiChoiceValue;

        public AveFieldMultiChoiceValue(SPFieldMultiChoiceValue fieldMultiChoiceValue)
        {
            m_FieldMultiChoiceValue = fieldMultiChoiceValue;
        }

        internal SPFieldMultiChoiceValue FieldMultiChoiceValue
        {
            get { return m_FieldMultiChoiceValue; }
        }

        public AveFieldMultiChoiceValue()
        {
            m_FieldMultiChoiceValue = new SPFieldMultiChoiceValue();
        }

        public AveFieldMultiChoiceValue(string fieldValue)
        {
            m_FieldMultiChoiceValue = new SPFieldMultiChoiceValue(fieldValue);
        }

        public int Count
        {
            get
            {
                return m_FieldMultiChoiceValue.Count;
            }
        }

        public void Add(string choiceValue)
        {
            this.m_FieldMultiChoiceValue.Add(choiceValue);
        }

        public string this[int index]
        {
            get
            {
                return this.m_FieldMultiChoiceValue[index];
            }
        }

        public override string ToString()
        {
            return this.m_FieldMultiChoiceValue.ToString();
        }
    }
}
