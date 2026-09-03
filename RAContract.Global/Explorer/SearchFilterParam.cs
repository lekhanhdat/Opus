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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Common;

namespace AvePoint.RA.Contract.Explorer
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchFilterParam
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid FolderId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public Guid TermId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> ClassCodeIds { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int DataSource { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool SkipHold { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string ScopeId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public SearchFilterInfo Filter { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public SearchPageInfo PageInfo { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IncludeTotal { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long DueDate { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchFilterInfo
    {   
        [DataMember(EmitDefaultValue = false)]
        public List<int> RecordStatus { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string SearchScope { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int SearchLevel { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<int> NodeTypes { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchPageInfo 
    {
        [DataMember(EmitDefaultValue = false)]
        public string PageIndex { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int PageSize { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int Total { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool HasNextPage { get; set; }
    }
}
