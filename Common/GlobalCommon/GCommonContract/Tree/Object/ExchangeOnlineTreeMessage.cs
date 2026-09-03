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


namespace AvePoint.GCommon.Contract.Tree.Object
{
    #region == using directives ==
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    #endregion
    [DataContract]
    public class ExchangeOnlineTreeMessage : AveTreeMessage
    {
        [DataMember]
        public ExchangeOnlineTreeNodeDto Node { get; set; }

        [DataMember]
        public List<ExchangeOnlineTreeNodeDto> NodeList { get; set; }

        [DataMember]
        public string NodeListJson { get; set; }

        /// <summary> Restore load tree时，GUI针对IB或DB的Job设置为true。 </summary>
        [DataMember]
        public bool IsOnlyShowIncrementalData { get; set; }

        /// <summary> Restore load tree时，根据Backup level控制节点展示。 </summary>
        [DataMember]
        public EOBackupLevel BackupLevel { get; set; }

        /// <summary>
        /// just use for object based restore tree.
        /// </summary>
        [DataMember]
        public int TreeOperation { get; set; }

        [DataMember]
        public RestoreSearchFilterPolicy FilterPolicy { get; set; }

        /// <summary>
        /// Object Based Result Tree使用，标记结果类型，参考ExchangeOnlineLocateJobResultType
        /// 0： All      结果全部返回
        /// 1： Partial  结果部分返回（大于500条）
        /// 2： NoResult 没有符合条件的数据
        /// </summary>
        [DataMember]
        public int ResultType { get; set; }
    }
}
