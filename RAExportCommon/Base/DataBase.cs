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
using System.Text;

namespace RAExportCommon
{
    public class DataBase
    {
        #region SiteCollection

        public string WebAppUrl { get; set; }

        public String WebApplicationGuid { get; set; }

        public String DreDatabaseName { get; set; }

        public string SiteCollectionUrl { get; set; }

        public String SiteCollectionGuid { get; set; }

        public String AutonomyCollectGroup { get; set; }

        public String AutonomyGroup { get; set; }
       

        #endregion

        #region Web

        public String SiteDescription { get; set; }

        public String SiteGuid { get; set; }

        public String SiteGuidHierarchy { get; set; }

        public String SiteTitle { get; set; }

        public String SiteUrl { get; set; }

        #endregion

        #region List

        public String ListBaseTemplate { get; set; }

        public String ListBaseType { get; set; }

        public String ListDescription { get; set; }

        public String ListGuid { get; set; }

        public String ListTitle { get; set; }

        public String ListUrl { get; set; }

        #endregion

        public Dictionary<string, object> AllColumn { get; set; }

        public Dictionary<string, string> metaInfo { get; set; }

        public String FileName { get; set; }

        //public List<String> Attachments { get; set; }

        public String DataFileExtensionName { get; set; }

        public bool HasUniqueRoleAssignments { get; set; }

        public String ExportPath { get; set; }
    }
}
