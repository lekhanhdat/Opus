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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.PlatformRecovery.Object;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    [DataContract]
    public class SPTreeMessage : AveTreeMessage
    {
        [DataMember]
        public List<SPTreeNodeDto> NodeList { get; set; }

        [DataMember]
        public SPTreeNodeDto Node { get; set; }

        /// <summary> Restore load tree时，GUI针对IB或DB的Job设置为true。 </summary>
        [DataMember]
        public bool IsOnlyShowIncrementalData { get; set; }

        /// <summary> Restore load tree时，根据Backup level控制节点展示。 </summary>
        [DataMember]
        public BackupLevel BackupLevel { get; set; }

        /// <summary> Restore load tree时，根据Backup level控制节点展示。 </summary>
        [DataMember]
        public PRBackupLevel PRBackupLevel { get; set; }
        
        /// <summary>
        /// just use for object based restore tree.
        /// </summary>
        [DataMember]
        public int TreeOperation { get; set; }

        [DataMember]
        public RestoreSearchFilterPolicy FilterPolicy { get; set; }

        [DataMember]
        [XmlIgnore]
        public FilterPolicyInfo PolicyInfo { get; set; }

        [DataMember]
        [XmlIgnore]
        public bool IsAdvancedSearchEnable { get; set; }
    }
}
