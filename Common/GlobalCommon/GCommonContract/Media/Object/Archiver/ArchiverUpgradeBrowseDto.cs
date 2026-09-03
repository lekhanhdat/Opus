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




namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverUpgradeBrowseDto
    {
        [DataMember]
        public LogicalDeviceDto IndexLogicalDevice { set; get; }

        [DataMember]
        public NodeLevel TreeNodeLevel { set; get; }

        [DataMember]
        public String DirectoryPath { set; get; }

        [DataMember]
        public PlatformType PlatformType { set; get; }

        [DataMember]
        public ProductVersion ProductVersion { set; get; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Archiver Upgrade Browse DTO: ");
            stringBuilder.AppendFormat("Tree Node Level: {0}, ", this.TreeNodeLevel);
            stringBuilder.AppendFormat("Directory Path: {0}, ", this.DirectoryPath);
            stringBuilder.AppendFormat("Index Logical Device: {0}", this.IndexLogicalDevice);
            return stringBuilder.ToString();
        }
    }
}
