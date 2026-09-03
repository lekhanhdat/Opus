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
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Upgrade;

namespace AvePoint.ObjectModel.Server16
{
    [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
    class AveDatabaseSequence : IAveDatabaseSequence
    {
        private SPDatabaseSequence mDatabaseSequence;
        private const string mDatabaseSequence_Type = "Microsoft.SharePoint.Upgrade.SPDatabaseSequence";

        public AveDatabaseSequence()
        { }

        public AveDatabaseSequence(SPDatabaseSequence databaseSequence)
        {
            mDatabaseSequence = databaseSequence;
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        internal SPDatabaseSequence DatabaseSequence
        {
            get
            {
                return mDatabaseSequence;
            }
        }

        public Version GetVersion(IAveDatabase database, Guid id, Version defaultVersion, IAveQuerySession session, IAveDatabaseSequence sequence)
        {
            Type sqlSessionType = AveAssemblyUtility.GetType("Microsoft.SharePoint.Utilities.SqlSession");
            Type[] paramTypes = new Type[] { typeof(SPDatabase), typeof(Guid), typeof(Version), sqlSessionType, typeof(SPDatabaseSequence) };
            object[] paramObjs = new object[] { database != null ? (database as AveDatabase).Database : null, id, defaultVersion, session != null ? (session as AveQuerySession).SqlSession : null, sequence != null ? (sequence as AveDatabaseSequence).DatabaseSequence : null };
            return (Version)AveAssemblyUtility.InvokeStaticMethod(mDatabaseSequence_Type, "GetVersion", paramTypes, paramObjs);
        }
    }
}
