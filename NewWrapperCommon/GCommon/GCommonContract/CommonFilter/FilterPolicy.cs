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



namespace AvePoint.GCommon.Contract.CommonFilter
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FilterPolicy
    {
        [DataMember]
        public int SequenceNo { get; set; }
        [DataMember]
        public PolicyLevel Level { get; set; }
        [DataMember]
        public PolicyRuleType RuleType { get; set; }
        [DataMember]
        public PolicyRuleBase Rule { get; set; }
        [DataMember]
        public PolicyCondition Condition { get; set; }
        [DataMember]
        public PolicyValue Value { get; set; }
        /// <summary>
        /// result field is an extension used for supporting rule that common filter engine can't evaluate.
        /// like CA UserAndGroup rule. common filter engine user is responsible for evaluate the policy result.
        /// and put the evaluation result into this filed.
        /// </summary>
        [DataMember]
        public Nullable<bool> Result { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(SequenceNo.ToString());
            sb.Append(" ");
            sb.Append(Level.ToString());
            sb.Append(" ");
            sb.Append(Rule.ToString());
            sb.Append(" ");
            sb.Append(Condition.ToString());
            sb.Append(" ");
            if (!string.IsNullOrEmpty(Value.Value1))
            {
                sb.Append(Value.Value1);
                sb.Append(" ");
            }
            if (!string.IsNullOrEmpty(Value.Value2))
            {
                sb.Append(Value.Value2);
                sb.Append(" ");
            }
            return sb.ToString();
        }
    }
}
