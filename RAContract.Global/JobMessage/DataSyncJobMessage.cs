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

namespace AvePoint.RA.Contract.Global.JobMessage
{
    public class DataSyncJobMessage
    {
        [DataMember(EmitDefaultValue = false)]
        public List<Contract.Global.Object.RMSPTreeNode> TreeNodes { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Dictionary<string, SiteInfo> SiteInformationDic { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long MainJobStartTime { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public AvePoint.RA.Contract.Global.Object.SOArchiverSettings ArchiverSetting { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Dictionary<Guid, AvePoint.RA.Contract.Global.Object.RMTermInfo> Terms { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Dictionary<Guid, AvePoint.RA.Contract.Global.Object.Rule> Rules { get;  set; }
        [DataMember(EmitDefaultValue = false)]
        public Dictionary<Guid, AvePoint.RA.Contract.Global.Object.RMRuleItemCollection> TermAndRulesMapping { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool BulkImportEnabled { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int BulkSize { get; set; }
    }

    public class SiteInfo 
    {
        [DataMember(EmitDefaultValue = false)]
        public string BCSColumnName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long LastScanTime { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<Guid> ChangedTermIds { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string SiteUrl { get; set; }
    }
}
