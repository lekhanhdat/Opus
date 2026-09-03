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




namespace AvePoint.Media.Storage.CAStor
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
using Scsp;
    using AvePoint.Media.Storage.Util;
    #endregion

    class CAStorOpenParameter : OpenParameter
    {
        private LocatorType locatorType = LocatorType.None;

        public LocatorType LocatorType
        {
            get { return locatorType; }
            set { locatorType = value; }
        }
        public string PrimaryNodes { get; set; }
        public int PrimaryPort { get; set; }
        public string ClusterName { get; set; }

        public string PrimaryPublisher { get; set; }
        public int PrimaryPublisherPort { get; set; }

        public bool UseRemoteCluster { get; set; }
        public int RemoteClusterType { get; set; }
        public string RemoteCSNHost { get; set; }
        public int RemoteCSNPort { get; set; }

        public string ScspProxyHost { get; set; }
        public int ScspProxyPort { get; set; }
        public string RemoteClusterName { get; set; }

        public ushort Replication { get; set; }
        public int CompressionType { get; set; }
        public int DerferCompresstion { get; set; }

        public string ObjectId { get; set; }
        public string BucketName { get; set; }

        public string JobId { get; set; }
        public string PlanId { get; set; }
        public string CycleId { get; set; }

        //for extender
        public string FarmName { get; set; }
        public string WebApp { get; set; }
        public string PoolId { get; set; }
        public string SiteCollection { get; set; }

        public bool IsLocalClientFailed { get; set; }

        private int retryInterval = 30000; //30 seconds

        public override int RetryInterval
        {
            get
            {
                return this.retryInterval;
            }
            set
            {
                this.retryInterval = value;
            }
        }
    }
}
