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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{
    public class MoveDestination
    {
        public DestMode DestMode { set; get; }


        public SPTreeNodeDto SPTreeNode { set; get; }

        public FSTreeNodeDto FSTreeNode { set; get; }

        public string SPUrl { set; get; }

        public DestAccountType AccountType { set; get; }

        public string SPAccountProfileId { set; get; }
        public Office365AccountInfo SPAccount { set; get; }

        public string FSPath { set; get; }

        public string FSAccountProfileId { set; get; }

        public Office365AccountInfo FSAccount { set; get; }

        public Guid AveSiteId { get; set; }
        /// <summary>
        /// SiteCollection Url /  FS Connection Path
        /// </summary>
        public string RootSiteUrl { set; get; } 
        /// <summary>
        /// DesNode ContainerId(Web Application Node/Group Node)
        /// </summary>
        public string ContainerId { get; set; }

        public bool KeepSourceClassification { get; set; }
    }

    public enum DestMode
    {
        TreeMode = 0,
        UrlMode = 1
    }

    public enum DestAccountType
    {
        DomainAccount = 0,
        O365Account = 1 
    }
}
