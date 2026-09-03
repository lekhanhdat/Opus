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
using AvePoint.GCommon.Contract.CloudAppAdmin.Object;
using AvePoint.GCommon.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.CloudAppAdmin.Message
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAWCFMessage : AveMessage
    {
        [DataMember]
        public CAAAction Action { get; set; }
        [DataMember]
        public string O365TenantId { get; set; }

        private CAAParaContent _paraContent = new CAAParaContent();
        [DataMember]
        public CAAParaContent ParaContent
        {
            get
            {
                return _paraContent;
            }
            set
            {
                _paraContent = value;
            }
        }

        private CAAResultContent _resultContent = new CAAResultContent();
        [DataMember]
        public CAAResultContent ResultContent
        {
            get
            {
                return _resultContent;
            }
            set
            {
                _resultContent = value;
            }
        }
    }

    public enum CAAAction
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        GetTenantDetail,
        [EnumMember]
        CheckTenant,
        [EnumMember]
        RefreshCache,
        [EnumMember]
        SearchUserFuzzily,
        [EnumMember]
        SearchGroupFuzzily,
        [EnumMember]
        QueryUsersByObjectIds,
        [EnumMember]
        QueryGroupsByObjectIds,
    }

    [Flags]
    public enum CAAQueryCatagroy : int
    {
        [EnumMember]
        All = -1,
        [EnumMember]
        None = 0,
        [EnumMember]
        BaseApiProperty = 1,
        [EnumMember]
        BasePSProperty = 2,
        [EnumMember]
        License = 4,
        [EnumMember]
        Application = 8,
        [EnumMember]
        MailBox = 16,
        [EnumMember]
        Groups = 32,
        [EnumMember]
        Members = 64,
        [EnumMember]
        Owners = 128
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAParaContent
    {
        [DataMember]
        public int Threshold { get; set; }
        [DataMember]
        public string SearchStr { get; set; }
        [DataMember]
        public bool IsCached { get; set; }
        [DataMember]
        public List<string> ObjectIds { get; set; }
        [DataMember]
        public CAAQueryCatagroy QueryCatagroy { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAResultContent
    {
        [DataMember]
        public bool BoolFlag { get; set; }
        [DataMember]
        public List<ADTenantDetail> TenantDetails { get; set; }
        [DataMember]
        public List<ADUser> ADUsers { get; set; }
        [DataMember]
        public List<ADGroup> ADGroups { get; set; }
    }
}
