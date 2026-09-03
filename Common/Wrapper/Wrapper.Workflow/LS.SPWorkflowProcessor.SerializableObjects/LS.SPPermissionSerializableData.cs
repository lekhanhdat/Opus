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

namespace LS.SPWorkflowProcessor.SerializableObjects
{
    public enum SPPrincipalType
    {
        Invalid,
        User,
        Group,
    }

    [Serializable]
    public class SPPermissionSerializableData
    {
        public bool mHasUniqueRoleAssignments;
        public List<SPRoleAssignmentSerializableData> mRoleAssignmentCollection;
    }

    [Serializable]
    public class SPRoleAssignmentSerializableData
    {
        public List<SPPermissionLevelSerializableData> mRoleDefinitionBindings;
        public SPPrincipalSerializableData mPrincipalUnit;
    }

    [Serializable]
    public class SPPermissionLevelSerializableData
    {
        public int mId;
        public long mBasePermissionLng;
        public string mName;
        public string mDescription;
        public string mXML;
        public bool mHidden;
    }

    [Serializable]
    public class SPPrincipalSerializableData
    {
        public string mName;
        public string mDisplayName;
        public string mEmail;
        public string mNote;
        public string mWebTitle;
        public SPPrincipalType mPrincipalType;
        public SPPrincipalSerializableData mOwner;
        public List<SPPrincipalSerializableData> mUsers;
    }
}
