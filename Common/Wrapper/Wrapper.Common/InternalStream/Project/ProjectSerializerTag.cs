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
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Common
{
    public class ProjectSerializerTag
    {
        public const string PUBLISHEDPROJECT = "PublishedProject";
        public const string ENTERPRISEPROJECTTYPES = "EnterpriseProjectTypes";
        public const string ENTERPRISEPROJECTTYPE = "EnterpriseProjectType";
        public const string CALENDARS = "Calendars";
        public const string CALENDAR = "Calendar";
        public const string LOOKUPTABLES = "LookupTables";
        public const string LOOKUPTABLE = "LookupTable";
        public const string CUSTOMFIELDS = "CustomFields";
        public const string CUSTOMFIELD = "CustomField";
        public const string ENTERPRISERESOURCES = "EnterpriseResources";
        public const string ENTERPRISERESOURCE = "EnterpriseResource";
        public const string PHASES = "Phases";
        public const string PHASE = "Phase";
        public const string STAGES = "Stages";
        public const string STAGE = "Stage";
        public const string DRAFTPROJECT = "DraftProject";
        public const string PUBLISHEDTASKS = "PublishedTasks";
        public const string DRAFTTASKS = "DraftTasks";
        public const string TASK = "Task";
        public static Guid SHAREPOINTTASKLISTPROJECTTYPEID = new Guid("f4066fec-bd67-4db9-8e6f-9cb3d3b297a6");
        public static Guid ENTERPRISEPROJECTTYPEID = new Guid("09fa52b4-059b-4527-926e-99f9be96437a");
    }
}
