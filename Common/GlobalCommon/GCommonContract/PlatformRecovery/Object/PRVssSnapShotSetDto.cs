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




namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRVssSnapShotSetDto
    {
        [DataMember(IsRequired=true)]
        [ColumnMapAttribute(DBColumn = "JobId")]
        public string JobId { get; set; }
        [DataMember(IsRequired=true)]
        [ColumnMapAttribute(DBColumn="Id")]
        public Guid SnapShotSetId { get; set; }
        [DataMember(IsRequired=true)]
        [ColumnMapAttribute(DBColumn = "AgentId")]
        public string AgentId { get; set; }
        [DataMember(IsRequired=true)]
        [ColumnMapAttribute(DBColumn = "AgentName")]
        public string AgentName { get; set; }
        [DataMember(IsRequired = true)]
        [ColumnMapAttribute(DBColumn = "PlanId")]
        public string PlanId { get; set; }
        private List<PRVssSnapShotDto> mSnapShots = new List<PRVssSnapShotDto>();
        [DataMember]
        public List<PRVssSnapShotDto> SnapShots
        {
            get { return mSnapShots; }
            set { mSnapShots = value; }
        }

        //public PRVssSnapShotSetDto parent { get; set; }
        public Stack<PRVssSnapShotFileDto> GetDataNodesByFullPath(string fullPath)
        {
            Stack<PRVssSnapShotFileDto> mdataNodes = new Stack<PRVssSnapShotFileDto>();
            foreach (PRVssSnapShotDto snapshotNode in SnapShots)
            {
                foreach (PRVssSnapShotFileDto data in snapshotNode.DataNodeFiles)
                {
                    if (data.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        data.parent = snapshotNode;
                        mdataNodes.Push(data);
                    }
                }
            }
            return mdataNodes;
        }
        public PRVssSnapShotSetDto()
        { }
    }
}
