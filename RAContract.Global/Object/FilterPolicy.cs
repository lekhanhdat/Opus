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




namespace AvePoint.RA.Contract.Global.Object
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    
    #endregion

    
    public class FilterPolicy
    {
        [DataMember]
        public int SequenceNo { get; set; }
        [DataMember]
        public int Level { get; set; }
        [DataMember]
        public int RuleType { get; set; }
        [DataMember]
        public string Rule { get; set; }
        [DataMember]
        public string ColumnName { get; set; }
        [DataMember]
        public int Condition { get; set; }
        [DataMember]
        public PolicyValue Value { get; set; }
        /// <summary>
        /// result field is an extension used for supporting rule that common filter engine can't evaluate.
        /// like CA UserAndGroup rule. common filter engine user is responsible for evaluate the policy result.
        /// and put the evaluation result into this filed.
        /// </summary>
        [DataMember]
        public Nullable<bool> Result { get; set; }
        [DataMember]
        public RuleGUIType RuleGUIType { get; set; }

        public override string ToString()
        {
            //SAAS-12633 重新写filter toString()方法、
            StringBuilder filterString = new StringBuilder();
            filterString.AppendFormat("RuleType:{0},", this.RuleType.ToString());
            filterString.AppendFormat("SequenceNo:{0},", this.SequenceNo.ToString());
            filterString.AppendFormat("Level:{0},", this.Level.ToString());
            filterString.AppendFormat("Condition:{0},", this.Condition.ToString());
            if (this.Value != null)
            {
                if (!string.IsNullOrEmpty(Value.Value1))
                {
                    filterString.AppendFormat("Value1:{0},", this.Value.Value1);
                }
                if (!string.IsNullOrEmpty(Value.Value2))
                {
                    filterString.AppendFormat("Value2:{0}", this.Value.Value2);
                }
            }
            return filterString.ToString();
            //sb.Append(SequenceNo.ToString());
            //sb.Append(" ");
            //sb.Append(Level.ToString());
            //sb.Append(" ");
            //sb.Append(Rule.ToString());
            //sb.Append(" ");
            //sb.Append(Condition.ToString());
            //sb.Append(" ");
            //if (!string.IsNullOrEmpty(Value.Value1))
            //{
            //    sb.Append(Value.Value1);
            //    sb.Append(" ");
            //}
            //if (!string.IsNullOrEmpty(Value.Value2))
            //{
            //    sb.Append(Value.Value2);
            //    sb.Append(" ");
            //}
            //return sb.ToString();
        }
    }

    
    public enum RuleGUIType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ColumnText = 1,
        [EnumMember]
        CustomPropertyText = 2,
        [EnumMember]
        ColumnNumber = 3,
        [EnumMember]
        CustomPropertyNumber = 4,
        [EnumMember]
        ColumnBoolean = 5,
        [EnumMember]
        CustomPropertyBoolean = 6,
        [EnumMember]
        ColumnDateTime = 7,
        [EnumMember]
        CustomPropertyDateTime = 8,
        [EnumMember]
        Workflow = 9,
        [EnumMember]
        AnonymousAccess = 10,
        [EnumMember]
        Attribute = 11,
        [EnumMember]
        Attachment = 12,
        [EnumMember]
        Auditing = 13,
        [EnumMember]
        Category = 14,
        [EnumMember]
        ContentType = 15,
        [EnumMember]
        CreatedBy = 16,
        [EnumMember]
        Created = 17,
        [EnumMember]
        KeepHistoryVersion = 18,
        [EnumMember]
        ListType = 19,
        [EnumMember]
        ModifiedBy = 20,
        [EnumMember]
        Modified = 21,
        [EnumMember]
        NameAndExtention = 22,
        [EnumMember]
        Name = 23,
        [EnumMember]
        Owner = 24,
        [EnumMember]
        SendDate = 25,
        [EnumMember]
        Size = 26,
        [EnumMember]
        Template = 27,
        [EnumMember]
        Title = 28,
        [EnumMember]
        Url = 29,
        [EnumMember]
        Versions = 30,
        [EnumMember]
        Versioning = 31,
        [EnumMember]
        UserAndGroup = 32,
        [EnumMember]
        Inheritance = 33,
        [EnumMember]
        StubCreationTime = 34,
        [EnumMember]
        StubLastAccessTime = 35,
        [EnumMember]
        TemplateId = 36,
        [EnumMember]
        LockStatus = 37,
    }
}
