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
using System;

namespace AvePoint.ObjectModel.Server16
{
    class AveSolutionCollection : AvePersistedChildCollection<IAveSolution>, IAveSolutionCollection
    {
        private SPSolutionCollection mSolutionColl;

        public AveSolutionCollection(SPSolutionCollection solutionColl)
            : base(solutionColl)
        {
            mSolutionColl = solutionColl;
        }

        public override int Count
        {
            get
            {
                return mSolutionColl.Count;
            }
        }

        public IAveSolution Add(string path)
        {
            return new AveSolution(mSolutionColl.Add(path));
        }

        public IAveSolution Add(string path, uint lcid)
        {
            return new AveSolution(mSolutionColl.Add(path, lcid));
        }

        public IAveSolution Add(string path, string name, uint lcid)
        {
            return new AveSolution((SPSolution)AveAssemblyUtility.InvokeMethod(mSolutionColl, "Add", new Type[] { typeof(string), typeof(string), typeof(uint) }, new object[] { path, name, lcid }));
        }

        public IAveSolution Add(string path, string name, uint lcid, bool isRestore)
        {
            return new AveSolution((SPSolution)AveAssemblyUtility.InvokeMethod(mSolutionColl, "Add", new Type[] { typeof(string), typeof(string), typeof(uint), typeof(bool) }, new object[] { path, name, lcid, isRestore }));
        }

        public void Remove(string name)
        {
            mSolutionColl.Remove(name);
        }

        public void Remove(string name, uint lcid)
        {
            mSolutionColl.Remove(name, lcid);
        }
    }
}
