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



using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.BusinessData.Administration;

namespace AvePoint.ObjectModel.Server19
{
    class AveEntity : AveIndividuallySecurableMetadataObject, IAveEntity
    {
        private Entity entity;
        private AveLobSystem mLobSystem;
        private AveMethodCollection mMethods;

        public AveEntity(Entity entity)
            : base(entity)
        {
            this.entity = entity;
        }

        public string Namespace 
        {
            get
            {
                return entity.Namespace;
            }
        }

        public IAveLobSystem LobSystem
        {
            get
            {
                if (mLobSystem == null)
                {
                    LobSystem lobSystem = entity.LobSystem;
                    if (lobSystem != null)
                    {
                        mLobSystem = new AveLobSystem(lobSystem);
                    }
                }
                return mLobSystem;
            }
        }

        public IAveMethodCollection Methods
        {
            get
            {
                if (mMethods == null)
                {
                    MethodCollection methodCollection = entity.Methods;
                    if (methodCollection != null)
                    {
                        mMethods = new AveMethodCollection(methodCollection);
                    }
                }
                return mMethods;
            }
        }

        public bool HasSpecificFinder()
        {
            return entity.HasSpecificFinder();
        }
    }
}
