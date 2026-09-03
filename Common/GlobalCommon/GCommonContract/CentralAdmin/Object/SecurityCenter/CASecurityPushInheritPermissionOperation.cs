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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object.SecurityCenter
{
    /// <summary>
    ///  the  action should be Update
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASecurityPushInheritPermissionOperation : CAOperation
    {
        [DataMember]
        public InheritingPermissionsAction InheritingPermissionsAction { get; set; }

        [DataMember]
        public NodeLevel LowestLevel { get; set; }

        [DataMember]
        public bool IncludeCurrentNode { get; set; }

        [DataMember]
        public bool OnlyCurrentNode { get; set; }

        [DataMember]
        public bool IsCopyPermission { get; set; }

        //result
        [DataMember]
        public int SuccessCount { get; set; }

        [DataMember]
        public int SkipCount { get; set; }

        [DataMember]
        public int FailCount { get; set; }

        [DataMember]
        public List<PushInheritDownResult> Details { get; set; }

        //public override string ToString()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append("<PushInheritPermission>");
        //    sb.Append(string.Format("<Summary SuccessCount=\"{0}\" SkipCount=\"{1}\" FailCount=\"{2}\"/>", SuccessCount, SkipCount, FailCount));
        //    sb.Append("<FailDetails>");
        //    if (Details != null && Details.Count > 0)
        //    {
        //        for (int i = Details.Count; i > 0; i--)
        //        {
        //            PushInheritDownResult detail = Details[i - 1];
        //            sb.Append(string.Format("<FailDetail Name=\"{0}\" Level=\"{1}\" Url=\"{2}\" Previous=\"{3}\" Now=\"{4}\" Comment=\"{5}\"/>", detail.NodeName, detail.NodeLevel.ToString(), detail.NodePath, detail.PreviousInherit.ToString(), detail.NowInherit.ToString(), detail.Comment));
        //        }
        //    }
        //    sb.Append("</FailDetails>");
        //    sb.Append("</PushInheritPermission>");
        //    return sb.ToString();
        //}
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PushInheritDownResult : ResultBase
    {
        [DataMember]
        public string NodeName { get; set; }

        [DataMember]
        public string NodeType { get; set; }

        [DataMember]
        public string NodePath { get; set; }

        [DataMember]
        public bool PreviousInherit { get; set; }

        [DataMember]
        public bool NowInherit { get; set; }

        [DataMember]
        public PushStatus PushStatus { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public CAStringFormatMessage FormatComment { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PushStatus
    {
        [EnumMember]
        Success,

        [EnumMember]
        Fail,

        [EnumMember]
        Skip
    }
}
