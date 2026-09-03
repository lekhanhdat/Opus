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

namespace AvePoint.ObjectModel.Server13
{
    class AveUserCodeService : AveWindowsService, IAveUserCodeService
    {
        private SPUserCodeService mUserCodeService;
        private AveUserCodeService mLocal;
        private AveBlockedSolutionCollection mBlockedSolutions;

        public AveUserCodeService(SPWindowsService service)
            : base(service)
        {
            mUserCodeService = (SPUserCodeService)service;
        }

        public AveUserCodeService(SPUserCodeService service)
            : base(service)
        {
            mUserCodeService = service;
        }

        public AveUserCodeService()
            : this(new SPUserCodeService())
        { }

        public AveUserCodeService(IAveFarm farm)
            : this(new SPUserCodeService((farm as AveFarm).Farm))
        { }

        #region IAveUserCodeService Members

        public IAveUserCodeService Local
        {
            get
            {
                if (mLocal == null)
                {
                    SPUserCodeService local = SPUserCodeService.Local;
                    if (local != null)
                    {
                        mLocal = new AveUserCodeService(local);
                    }
                }
                return mLocal;
            }
        }

        public IAveBlockedSolutionCollection BlockedSolutions
        {
            get
            {
                if (mBlockedSolutions == null)
                {
                    mBlockedSolutions = new AveBlockedSolutionCollection(mUserCodeService.BlockedSolutions);
                }
                return mBlockedSolutions;
            }
        }

        public bool UseLocalServerOnly
        {
            get
            {
                return mUserCodeService.UseLocalServerOnly;
            }
            set
            {
                mUserCodeService.UseLocalServerOnly = value;
            }
        }

        #endregion
    }
}
