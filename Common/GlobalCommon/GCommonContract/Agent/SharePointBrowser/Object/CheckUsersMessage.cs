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
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.SharePointBrowser.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CheckUsersMessage : AveMessage
    {
        [DataMember]
        public SPTreeNodeDto TreeNode { get; set; }

        /// <summary>
        ///     the usernames that need to be checked
        /// </summary>
        [DataMember]
        public List<string> Contents { get; set; }

        /// <summary>
        ///     find or check
        /// </summary>
        [DataMember]
        public CheckAction Action { get; set; }

        [DataMember]
        public List<CheckUsersOperation> Operations { get; set; }

        [DataMember]
        public CheckCategory PlanCategory { get; set; }

        [DataMember]
        public bool IsFiltered { get; set; }

        [DataMember]
        public List<string> Domains { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CheckUsersInfo
    {
        [DataMember]
        public CheckUserResult ResultType { get; set; }
        [DataMember]
        public List<CheckUsersMessage> MessageList { get; set; }
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public long TimeOut { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CheckUserResult
    {
        [EnumMember]
        UnFinish,
        [EnumMember]
        Success,
        [EnumMember]
        Error,
        [EnumMember]
        TimeOut
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CheckAction
    {
        [EnumMember]
        Check,

        [EnumMember]
        Find
    }

    public enum CheckCategory
    {
        None = 0,
        PolicyEnforcer = 1,
        SecuritySearch = 2
    }

}
