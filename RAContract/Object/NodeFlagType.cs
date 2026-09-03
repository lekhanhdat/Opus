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

namespace AvePoint.RA.Contract.Object
{
    public enum NodeFlagType
    {
        UniqueId = 0,
        EnforceRetention = 1,
        AutoClassification = 2,
        ExplorerSync = 3,
        ExplorerSyncLib = 4,
        OneDriveExplorerSync = 5,
        OneDriveExplorerSyncLib = 6,
        ConnectorTimer = 7,
        //AI
        IntelligenceClassification = 8,
        BoxSync = 9,
        BoxDisposal = 10,

        // Google
        GoogleSync = 11,
        GoogleApplySetting = 12,

        #region Teams
        TeamsSync = 13,
        TeamsSyncLibrary = 14,
        TeamsUniqueId = 15,
        SiteMetrics = 16,
        #endregion
    }

    public static class UniqueIdConfig
    {
        public const string DefaultPrefix = "REC";
    }
    public enum TermChangeType
    {
        None = -1,
        TermRule = 0,
        Retention = 1,
        LabelRule = 2,
        LabelRetention = 3,
    }

    public enum DeclarationMode
    {
        None = 0,
        BlockDelete = 1,
        BlockEditDelete = 2,

    }

}
