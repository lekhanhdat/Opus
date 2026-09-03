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



using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveServer : AvePersistedUpgradableObject, IAveServer
    {
        private SPServer mServer;
        private AveServer mLocalServer;
        private AveServiceInstanceCollection mServiceInstanceCol;

        public AveServer(SPServer server)
            : base(server)
        {
            mServer = server;
        }

        public AveServer()
            : this(new SPServer())
        { }

        public AveServer(string address)
            : this(new SPServer(address))
        { }

        public AveServer(string address, IAveFarm farm)
            : this(new SPServer(address, (farm as AveFarm).Farm))
        { }

        #region IAveServer Members

        public IAveServer Local
        {
            get
            {
                if (mLocalServer == null)
                {
                    SPServer server = SPServer.Local;
                    if (server != null)
                    {
                        mLocalServer = new AveServer(server);
                    }
                }
                return mLocalServer;
            }
        }

        internal SPServer Server
        {
            get { return mServer; }
        }

        public string Address
        {
            get
            {
                return mServer.Address;
            }
            set
            {
                mServer.Address = value;
            }
        }

        public AveServerRole LocalServerRole
        {
            get
            {
                return (AveServerRole)SPServer.LocalServerRole;
            }
        }

        public IAveServiceInstanceCollection ServiceInstances
        {
            get
            {
                if (mServiceInstanceCol == null)
                {
                    mServiceInstanceCol = new AveServiceInstanceCollection(mServer.ServiceInstances);
                }
                return mServiceInstanceCol;
            }
        }

        public AveServerRole Role
        {
            get
            {
                return (AveServerRole)mServer.Role;
            }
            set
            {
                mServer.Role = (SPServerRole)value;
            }
        }

        #endregion
    }
}
