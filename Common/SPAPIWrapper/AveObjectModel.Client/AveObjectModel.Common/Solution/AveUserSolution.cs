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

        public AveUserSolution(IAveRequest request, IDictionary<string, object> userSolutionProperties)
        {
            mRequest = request;
            base.DataCache.AddPropertyies(userSolutionProperties);
        }

        #region IAveUserSolution Members

        public Guid SolutionId
        {
            get { return base.DataCache.GetProperty<Guid>("SolutionId"); }
        }

        public AveUserSolutionStatus Status
        {
            get
            {
                return base.DataCache.GetProperty<AveUserSolutionStatus>("Status");
            }
            set
            {
                base.DataCache.AddChangedProperty("Status", value);
            }
        }

        public bool HasAssemblies
        {
            get { return base.DataCache.GetProperty<bool>("HasAssemblies"); }
        }

        public string Name
        {
            get { return base.DataCache.GetProperty<string>("Name"); }
        }

        public string Signature
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("SolutionHash"))
                {
                    if (base.DataCache.IsPropertyAvailable("FieldValues"))
                    {
                        var fieldValues = base.DataCache.GetProperty<Dictionary<string, object>>("FieldValues");
                        if (fieldValues.ContainsKey("SolutionHash"))
                        {
                            base.DataCache.AddProperty("SolutionHash",fieldValues["SolutionHash"].ToString());
                        }
                    }
                }
                return base.DataCache.GetProperty<string>("SolutionHash");
            }
        }

        #endregion

        public void Dispose()
        {
            throw new NotImplementedException();
        }


        public int ItemId
        {
            get { return base.DataCache.GetProperty<int>("Id"); }
        }
    }
}
