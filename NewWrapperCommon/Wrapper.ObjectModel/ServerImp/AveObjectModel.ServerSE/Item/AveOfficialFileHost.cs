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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveOfficialFileHost : AveAutoSerializingObject, IAveOfficialFileHost
    {
        private SPOfficialFileHost mOfficialFileHost;

        public AveOfficialFileHost(SPOfficialFileHost officialFileHost)
            : base(officialFileHost)
        {
            mOfficialFileHost = officialFileHost;
        }

        public AveOfficialFileHost(bool bCreateUniqueId)
            : this(new SPOfficialFileHost(bCreateUniqueId))
        { }

        internal SPOfficialFileHost OfficialFileHost
        {
            get
            {
                return mOfficialFileHost;
            }
        }

        #region IAveOfficialFileHost Members

        public void CopyFrom(IAveOfficialFileHost srcHost)
        {
            mOfficialFileHost.CopyFrom((srcHost as AveOfficialFileHost).OfficialFileHost);
        }

        public AveOfficialFileAction Action
        {
            get
            {
                return (AveOfficialFileAction)mOfficialFileHost.Action;
            }
            set
            {
                mOfficialFileHost.Action = (SPOfficialFileAction)value;
            }
        }

        public string Explanation
        {
            get
            {
                return mOfficialFileHost.Explanation;
            }
            set
            {
                mOfficialFileHost.Explanation = value;
            }
        }

        public Uri OfficialFileUrl
        {
            get
            {
                return mOfficialFileHost.OfficialFileUrl;
            }
            set
            {
                mOfficialFileHost.OfficialFileUrl = value;
            }
        }

        public bool ShowOnSendToMenu
        {
            get
            {
                return mOfficialFileHost.ShowOnSendToMenu;
            }
            set
            {
                mOfficialFileHost.ShowOnSendToMenu = value;
            }
        }

        public Guid UniqueId
        {
            get { return mOfficialFileHost.UniqueId; }
        }

        public string OfficialFileName
        {
            get
            {
                return mOfficialFileHost.OfficialFileName;
            }
            set
            {
                mOfficialFileHost.OfficialFileName = value;
            }
        }

        #endregion
    }
}
