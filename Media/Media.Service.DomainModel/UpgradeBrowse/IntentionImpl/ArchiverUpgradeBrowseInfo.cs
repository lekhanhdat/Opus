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
    using System.Text;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using global::Media.Common;

    #endregion

    public class ArchiverUpgradeBrowseInfo
        : BrowseInfoBase
    {
        public LogicalDeviceDto IndexLogicalDevice { set; get; }
        public TreeNodeLevel TreeNodeLevel { set; get; }
        public String DirectoryPath { set; get; }
        public PlatformType PlatformType { set; get; }

        public ArchiverUpgradeBrowseInfo()
        { }

        public ArchiverUpgradeBrowseInfo(ArchiverUpgradeBrowseDto param)
        {
            IndexLogicalDevice = param.IndexLogicalDevice;
            TreeNodeLevel = EnumConverter.ToEnum<TreeNodeLevel>(param.TreeNodeLevel.ToString());
            DirectoryPath = param.DirectoryPath;
            PlatformType = EnumConverter.ToEnum<PlatformType>(param.PlatformType.ToString());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ArchiverImportBrowseInfo: ");
            sb.Append(IndexLogicalDevice.ToString());
            sb.Append(" ");
            sb.Append(DirectoryPath.ToString());
            sb.Append(" ");
            sb.Append(TreeNodeLevel.ToString());
            sb.Append(" ");
            sb.Append(PlatformType.ToString());
            return sb.ToString();
        }
    }
}