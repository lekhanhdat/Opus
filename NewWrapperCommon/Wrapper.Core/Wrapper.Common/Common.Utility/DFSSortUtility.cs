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
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// Graph dfs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class DFSSortUtility<T>
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static IEnumerable<T> Sort(IEnumerable<T> sourceData, Func<T, T, bool> compareFun)
        {
            try
            {
                //prepare the compare data with the internal class object.  It must make a collection object here , or the object will be new one always in interator.
                var compareData = sourceData.Select((d) => new DFSInfo<T>(d)).ToList();
                foreach (var data in compareData)
                {
                    foreach (var dependency in compareData)
                    {
                        if (compareFun(data.sourceData, dependency.sourceData))
                        {
                            data.depandences.Add(dependency);
                        }
                    }
                }
                //mark the mat value
                BeginSort(compareData);
                //use the mat value to sort
                compareData.Sort();
                return compareData.Select((d) => d.sourceData);
            }
            catch (Exception e)
            {
                log.Warn("An error occourred while sort data with DFS.Error:{0}", e);
                return sourceData;
            }
        }

        private static void BeginSort(IEnumerable<DFSInfo<T>> alllist)
        {
            var currentMat = 0;
            foreach (var data in alllist)
            {
                if (!data.hasVisited)
                {
                    currentMat = DFS(data, currentMat);
                }
            }
        }

        private static int DFS(DFSInfo<T> data, int currentMat)
        {
            data.mat = ++currentMat;
            data.hasVisited = true;
            if (data.depandences != null)
            {
                foreach (var d in data.depandences)
                {
                    if (!d.hasVisited)
                    {
                        currentMat = DFS(d, currentMat);
                    }
                }
            }
            return data.mat = currentMat + 1;
        }
    }

    internal class DFSInfo<T> : IComparable
    {
        internal T sourceData;

        public List<DFSInfo<T>> depandences = new List<DFSInfo<T>>();

        internal bool hasVisited;

        internal int mat = 0;

        public DFSInfo(T source)
        {
            this.sourceData = source;
        }

        public int CompareTo(object obj)
        {
            return this.mat - (obj as DFSInfo<T>).mat;
        }

    }
}
