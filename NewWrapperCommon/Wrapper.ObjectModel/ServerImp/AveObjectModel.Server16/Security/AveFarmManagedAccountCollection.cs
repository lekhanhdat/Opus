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
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;
using System.Security.Principal;
using AvePoint.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveFarmManagedAccountCollection : AveAbstractCommonCollection<IAveManagedAccount>, IAveFarmManagedAccountCollection
    {
        private SPFarmManagedAccountCollection mFarmManagedAccountCollection;

        public AveFarmManagedAccountCollection(SPFarmManagedAccountCollection farmManagedAccountCollection)
            : base(farmManagedAccountCollection)
        {
            mFarmManagedAccountCollection = farmManagedAccountCollection;
        }

        public AveFarmManagedAccountCollection(IAveFarm farm)
            : this(new SPFarmManagedAccountCollection((farm as AveFarm).Farm))
        { }

        public IAveManagedAccount FindOrCreateAccount(SecurityIdentifier sid)
        {
            bool flag;
            return this.FindOrCreateAccount(sid, out flag);
        }

        public IAveManagedAccount FindOrCreateAccount(string username)
        {
            bool flag;
            return this.FindOrCreateAccount(username, out flag);
        }

        public IAveManagedAccount FindOrCreateAccount(SecurityIdentifier sid, out bool alreadyExists)
        {
            alreadyExists = false;
            Type refType = alreadyExists.GetType().MakeByRefType();
            return new AveManagedAccount((SPManagedAccount)AveAssemblyUtility.InvokeMethod(mFarmManagedAccountCollection, "FindOrCreateAccount", new Type[] { typeof(SecurityIdentifier), refType }, new object[] { sid, alreadyExists }));
        }

        public IAveManagedAccount FindOrCreateAccount(string username, out bool alreadyExists)
        {
            return this.FindOrCreateAccount(AveUserUtility.AccountNameToSid(username), out alreadyExists);
        }

        public IAveManagedAccount this[string username]
        {
            get
            {
                SPManagedAccount managedAccount = mFarmManagedAccountCollection[username];
                if (managedAccount == null)
                {
                    return null;
                }
                return new AveManagedAccount(managedAccount);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveManagedAccount(t as SPManagedAccount);
        }

        public override int Count
        {
            get { return mFarmManagedAccountCollection.Count; }
        }
    }
}
