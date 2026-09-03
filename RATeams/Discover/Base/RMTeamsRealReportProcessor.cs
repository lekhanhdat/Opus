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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.CAMLHelper.CAML;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RADataBroker;
using AvePoint.Wrapper.Common;
using Newtonsoft.Json;
using RAArchiverCommon.DestructionCache;
using System.Data.SqlClient;
using System.Xml;
using AvePoint.RA.RACommonUtility.Extension;
using AvePoint.RA.Contract.Object.JobMessage;

namespace RATeams.Discover.Base
{
    public class RMTeamsRealReportProcessor
    {     
    }

    internal class BuiltInFieldName
    {
        public const string Name = "FileLeafRef";
        public const string DocumentSize = "FileSizeDisplay";
        public const string ModifiedTime = "Modified";
        public const string CreatedTime = "Created";
        public const string CreatedBy = "Author";
        public const string ModifiedBy = "Editor";
        public const string ContentType = "ContentType";
        public const string Title = "Title";
    }

    public enum OperationType
    {
        Created = 0,
        Destroyed = 1
    }

    public enum ObjectLevel
    {
        None,
        Document,
        PhysicalRecord,
        PhysicalFile,
        DocumentSet,
        Folder
    }
}
