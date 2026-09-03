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

namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using System;
    using System.Collections.Generic;
    #endregion
    [Serializable]
    [Table(IndexConstants.TableNameExchangePlanner)]
    public class PlannerIndex : GroupBasicIndex
    {
        [Column("COL_PARENT_NODE_NAME")]
        public string ParentNodeName { get; set; }

        [Column("COL_PARENT_NODE_ID")]
        public string ParentNodeId { get; set; }

        [Column("COL_CURRENT_NODE_NAME")]
        public string CurrentNodeName { get; set; }

        [Column("COL_CURRENT_NODE_ID")]
        public string CurrentNodeID { get; set; }

        [Column("COL_BUCKET_NAME")]
        public string BucketName { get; set; }

        [Column("COL_BUCKET_ID")]
        public string BucketID { get; set; }

        [Column("COL_PROGRESS")]
        public int Progress { get; set; }

        [Column("COL_LABELS")]
        public string Labels { get; set; }

        [Column("COL_PRIVACY")]
        public int Privacy { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }

        public override Dictionary<string, object> GenerateInsertDatabaseParameters()
        {
            var result = base.GenerateInsertDatabaseParameters();
            result.Add("@COL_PARENT_NODE_NAME", ParentNodeName);
            result.Add("@COL_PARENT_NODE_ID", ParentNodeId);
            result.Add("@COL_CURRENT_NODE_NAME", CurrentNodeName);
            result.Add("@COL_CURRENT_NODE_ID", CurrentNodeID);
            result.Add("@COL_BUCKET_NAME", BucketName);
            result.Add("@COL_BUCKET_ID", BucketID);
            result.Add("@COL_PROGRESS", Progress);
            result.Add("@COL_LABELS", Labels);
            result.Add("@COL_PRIVACY", Privacy);
            return result;
        }
    }
}