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
using AvePoint.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveUserSolutionCollection : AveAbstractCommonCollection<IAveUserSolution>, IAveUserSolutionCollection
    {
        private const string mUserSolutionCollection_ItemAtIndex_Property = "ItemAtIndex";
        private SPUserSolutionCollection mUserSolutionCollection;

        public AveUserSolutionCollection(SPUserSolutionCollection userSolutions)
            : base(userSolutions)
        {
            mUserSolutionCollection = userSolutions;
        }

        #region IAveUserSolutionCollection Members

        public IAveUserSolution this[Guid solutionId]
        {
            get
            {
                SPUserSolution userSolution = mUserSolutionCollection[solutionId];
                if (userSolution == null)
                {
                    return null;
                }
                return new AveUserSolution(userSolution);
            }
        }

        public void Remove(IAveUserSolution solution)
        {
            mUserSolutionCollection.Remove((solution as AveUserSolution).UserSolution);
        }

        public IAveUserSolution Add(int p)
        {
            SPUserSolution userSolution = mUserSolutionCollection.Add(p);
            if (userSolution == null)
            {
                return null;
            }
            return new AveUserSolution(mUserSolutionCollection.Add(p));
        }

        public override IAveUserSolution this[int index]
        {
            get
            {
                object userSolution = AveAssemblyUtility.GetPropertyValue(mUserSolutionCollection, mUserSolutionCollection_ItemAtIndex_Property);
                if (userSolution == null)
                {
                    return null;
                }
                return new AveUserSolution((SPUserSolution)userSolution);
            }
        }

        #endregion

        protected override object CreatElementInstance(object t)
        {
            return new AveUserSolution(t as SPUserSolution);
        }

        public override int Count
        {
            get { return mUserSolutionCollection.Count; }
        }
    }
}
