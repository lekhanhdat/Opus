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

using System.Collections.Generic;
using AvePoint.Wrapper.Common;
using System;

namespace AvePoint.Wrapper.CustomUtility
{
    public class AveBaseCondition
    {
        protected AveBaseCondition()
        {
            //The default filter type is filter in.
            //FilterOut = false;
        }

        public CompareAction CmpAction { get; set; }

        public int SequenceNo { get; set; }
    }

    public class AveContentTypeCondition : AveBaseCondition
    {
        public string ContentTypeName { get; set; }
    }

    public class AveFieldCondition : AveBaseCondition
    {
        public AveFieldCondition()
            : base()
        {
            ColumnType = null;
        }
        public string FieldDisplayName { get; set; }

        public string FieldInternalName { get; set; }

        public AveFieldType? ColumnType { get; set; }

        public FilterValue FdValue { get; set; }
    }

    public class AveCustomFilterPolicy
    {
        public List<AveBaseCondition> Conditions { get; set; }

        public string ExpressionString { get; set; }

        public FilterLevel Level { get; set; }

        private bool mFilterOut = false;
        public bool FilterOut 
        { 
            get
            {
                return mFilterOut;
            }
            set
            {
                mFilterOut = value;
            }
        }
    }

    public class FilterValue
    {
        private string value1;
        private ValueUnit value1Unit;
        private string value2;
        private ValueUnit value2Unit;

        public string Value1
        {
            get { return value1; }
            set { value1 = value; }
        }

        public ValueUnit Value1Unit
        {
            get { return value1Unit; }
            set { value1Unit = value; }
        }

        public string Value2
        {
            get { return value2; }
            set { value2 = value; }
        }

        public ValueUnit Value2Unit
        {
            get { return value2Unit; }
            set { value2Unit = value; }
        }

        public FilterValue(string value1)
            : this(value1, string.Empty)
        {
        }

        public FilterValue(string value1, string value2)
            : this(value1, ValueUnit.None, value2, ValueUnit.None)
        {
        }

        public FilterValue(string value1, ValueUnit unit1)
            : this(value1, unit1, string.Empty, ValueUnit.None)
        {
        }

        public FilterValue(string value1, ValueUnit unit1, string value2, ValueUnit unit2)
        {
            this.value1 = value1;
            this.value1Unit = unit1;
            this.value2 = value2;
            this.value2Unit = unit2;
        }

    }

    public enum CompareAction
    {
        None = 0,
        Exactly = 1,
        Contains = 8,
        #region For Number
        LessOrEqualThan = 16,
        GreaterOrEqualThan = 32,
        #endregion
        #region For DateTime
        FromTo = 2048,
        Before = 4096,
        After = 8192,
        On = 16384,
        WithIn = 32867,
        OlderThan = 65734,
        #endregion
        Equals = 262936,
        DoesNotContains = 525872,
        Match = 1051744,
        DoesNotMatch = 2103488,
        IsExactlyNot = 4206976
    }

    public enum ValueUnit
    {
        None,
        KB,
        MB,
        GB,
        Days,
        Weeks,
        Months,
        Years
    }

    public enum FilterLevel
    {
        Item = 32,
        Document = 64
    }

    public enum ConditionType
    {
        Field,
        ContentType
    }
}
