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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveUserSolution : AveClientObject, IAveUserSolution
    {
        private IAveRequest mRequest;        

        public AveUserSolution(IAveRequest request, Dictionary<string, object> userSolutionProperties)
        {
            mRequest = request;
            base.DataCache.AddPropertyies(userSolutionProperties);
        }

        public Guid SolutionId
        {
            get { return base.DataCache.GetProperty<Guid>("SolutionId"); }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public int ItemId 
        {
            get 
            {
                return base.DataCache.GetProperty<int>("Id");
            }
        }

        public AveUserSolutionStatus Status
        {
            get
            {
                var tempStatus= base.DataCache.GetProperty<string>("Status");
                if (string.IsNullOrEmpty(tempStatus))
                {
                    return AveUserSolutionStatus.Deactivated;
                }
                switch (tempStatus) 
                {
                    case"1":
                        return AveUserSolutionStatus.Activated;
                    default:
                        return AveUserSolutionStatus.Disabled;
                }
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool HasAssemblies
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { return base.DataCache.GetProperty<string>("Name"); }
        }

        public string Signature
        {
            get { return base.DataCache.GetProperty<string>("SolutionHash"); }
        }
    }
}
