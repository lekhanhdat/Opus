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
using System.Text;
using System.Collections;

namespace AvePoint.Wrapper.Common
{
    public interface IAveWorkflowAssociation : IAveAutoSerializingObject, IDisposable
    {
        Guid ParentAssociationId { get; set; }
        bool AllowManual { get; set; }
        bool AutoStartChange { get; set; }
        bool AutoStartCreate { get; set; }
        string Description { get; set; }
        bool Enabled { get; set; }
        Guid ID { get; }
        string Name { get; set; }
        AveBasePermissions PermissionsManual { get; set; }
        int RunningInstances { get; }
        Hashtable MetaData { get; }

        IAveWorkflowAssociation CreateWebAssociation(IAveWorkflowTemplate baseTemplate, string name, IAveList taskList, IAveList historyList);
        IAveWorkflowAssociation CreateWebContentTypeAssociation(IAveWorkflowTemplate baseTemplate, string name, string taskListName, string historyListName);
        void SetHistoryList(IAveList list);
        void SetTaskList(IAveList list);
        string ExportToXml();

        IAveWorkflowAssociation CreateListAssociation(IAveWorkflowTemplate baseTemplate, string name, IAveList taskList, IAveList historyList);

        IAveWorkflowAssociation CreateListContentTypeAssociation(IAveWorkflowTemplate baseTemplate, string name, IAveList taskList, IAveList historyList);

        IAveWorkflowAssociation CreateSiteContentTypeAssociation(IAveWorkflowTemplate baseTemplate, string name, string strTaskList, string strHistoryList);

        string AssociationData { get; set; }

        Guid BaseId { get; }

        IAveWeb ParentWeb { get; }

        int Author { get; }

        int AutoCleanupDays { get; set; }

        DateTime Created { get; }

        IAveList ParentList { get; }

        Guid HistoryListId { get; }

        string HistoryListTitle { get; set; }

        DateTime Modified { get; }

        Guid TaskListId { get; }

        string TaskListTitle { get; set; }

        bool IsDeclarative { get; }

        string InternalName { get; }

        bool AllowAsyncManualStart { get; set; }

        bool MarkedForDelete { get; set; }

        IAveWorkflowTemplate BaseTemplate { get; }

        AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration Configuration { get; set; }

        IAveContentTypeId ContentTypeId { get; }

        int Version { get; }

        string InternalNameStatusField { get; set; }

        bool CompressInstanceData { get;}
    }
}
