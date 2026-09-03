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
using System.Globalization;

namespace AvePoint.Wrapper.Common
{
    public interface IAveWorkflowAssociationCollection : ICollection, IEnumerable<IAveWorkflowAssociation>, IEnumerable
    {
        IAveWorkflowAssociation this[Guid workflowAssociationId] { get; }
        IAveWorkflowAssociation this[int index] { get; }

        IAveWorkflowAssociation Add(IAveWorkflowAssociation workflowAssociation);
        IAveWorkflowAssociation GetAssociationByBaseID(Guid baseTemplateId);
        IAveWorkflowAssociation GetAssociationByBaseID(Guid baseTemplateId, bool ignoreStartSettings);
        void Remove(IAveWorkflowAssociation association);
        void Update(IAveWorkflowAssociation workflowAssociation);
        IAveWorkflowAssociation GetAssociationByName(string name, CultureInfo cultureInfo);
    }

    namespace AveWorkflowAssociationCollection
    {
        // Summary:
        //     Contains configuration properties of the workflow association.
        [Flags]
        public enum Configuration
        {
            None = 0,
            AutoStartAdd = 1,
            AutoStartChange = 2,
            AutoStartColumnChange = 4,
            AllowManualStart = 8,
            HasStatusColumn = 16,
            LockItem = 32,
            Declarative = 64,
            NoNewWorkflows = 128,
            MarkedForDelete = 512,
            GloballyDisabled = 1024,
            CompressInstanceData = 4096,
            SiteOverQuota = 8192,
            SiteWriteLocked = 16384,
            AllowAsyncManualStart = 32768,
            SkipContentTypePushDown = 65536,
        }
    }
}
