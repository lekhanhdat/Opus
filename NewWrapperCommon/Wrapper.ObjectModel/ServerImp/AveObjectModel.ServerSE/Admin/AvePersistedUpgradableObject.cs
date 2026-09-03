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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;
using AvePoint.Common;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.ServerSE
{
    class AvePersistedUpgradableObject : AvePersistedObject, IAvePersistedUpgradableObject
    {
        protected SPPersistedUpgradableObject mPersistedUpgradableObject;

        public AvePersistedUpgradableObject(SPPersistedUpgradableObject persistedUpgradableObject)
            : base(persistedUpgradableObject)
        {
            mPersistedUpgradableObject = persistedUpgradableObject;
        }

        public AvePersistedUpgradableObject(string name, IAvePersistedObject parent)
            : base(name, parent)
        {
            mPersistedUpgradableObject = base.PersistedObject as SPPersistedUpgradableObject;
        }

        public AvePersistedUpgradableObject(string name, IAvePersistedObject parent, Guid id)
            : base(name, parent, id)
        {
            mPersistedUpgradableObject = base.PersistedObject as SPPersistedUpgradableObject;
        }

        public AvePersistedUpgradableObject()
            : this(new SPPersistedUpgradableObject())
        { }

        public Dictionary<Guid, Version> Versions
        {
            get { return AveAssemblyUtility.GetPropertyValue(mPersistedUpgradableObject, "Versions") as Dictionary<Guid, Version>; }
        }

        public bool NeedsUpgrade
        {
            get
            {
                return mPersistedUpgradableObject.NeedsUpgrade;
            }
            set
            {
                mPersistedUpgradableObject.NeedsUpgrade = value;
            }
        }

        public bool NeedsUpgradeIncludeChildren
        {
            get
            {
                return mPersistedUpgradableObject.NeedsUpgradeIncludeChildren;
            }
        }

        public AveTriState IsBackwardsCompatible
        {
            get
            {
                return (AveTriState)mPersistedUpgradableObject.IsBackwardsCompatible;
            }
            set
            {
                mPersistedUpgradableObject.IsBackwardsCompatible = (TriState)value;
            }
        }
    }
}
