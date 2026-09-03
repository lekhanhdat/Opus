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
    using AvePoint.GCommon.Contract.StorageOptimization.Connector;
    using AvePoint.GCommon.Contract.Tree;
    #region using directives

    using AvePoint.GCommon.Contract.Tree.Object;
    using System;

    #endregion using directives

    public class RestoreMarkMessage
    {
        public PropertyState Property { get; set; }

        public SecurityState Security { get; set; }

        public Boolean IsChecked { get; set; }
        public Boolean IsSelected { get; set; }
        public Boolean ParentIsSelected { get; set; }

        public Int32 VersionFlag { get; set; }
        public Boolean IsCurrentVersion { get; set; }
        public string ParentName { get; set; }
        public int ChildCount { get; set; }
        
        public String ParentPath { get; set; }
        public String SiteCollectionPath { get; set; }
        public String RealPath { get; set; }
        public RestoreMarkMessage()
        { }

        public RestoreMarkMessage(SPTreeNodeDto treeNode, Int32 versionFlag = 1)
        {
            this.VersionFlag = versionFlag;
            this.Property = treeNode.Property;
            this.Security = treeNode.Security;
            this.IsChecked = true;
            //this.IsSelected = treeNode.CheckNumber == 1;
            this.ParentIsSelected = NodeUtil.CheckParentWasChecked(treeNode);
        }

        public RestoreMarkMessage(ExchangeOnlineTreeNodeDto treeNode)
        {
            this.IsChecked = treeNode.CheckNumber == 1;
        }

        public RestoreMarkMessage(GoogleDriveTreeNodeDto treeNode)
        {
            this.IsChecked = treeNode.CheckNumber == 1;
        }

        public override string ToString()
        {
            return string.Format("RestoreMarkMessage : Property : {0}, Security :{1}", Property, Security);
        }
    }
}