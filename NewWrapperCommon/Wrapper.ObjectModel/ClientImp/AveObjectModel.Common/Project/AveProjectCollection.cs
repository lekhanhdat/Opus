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
using System.Threading.Tasks;

using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveProjectCollection : AveAbstractCommonCollection<IAveProject>, IAveProjectCollection
    {
        private IAveRequest mRequest;
        private AveSite mSite;
        private IAveProjectSerializer mProjectSerializer;
        private object locker = new object();
        
        public AveProjectCollection(IAveRequest request, AveSite site, List<Dictionary<string, object>> props)
        {
            mRequest = request;
            mSite = site;
            InitProjectCollections(props);
        }

        private void InitProjectCollections(List<Dictionary<string, object>> props)
        {
            mListData = new List<IAveProject>(props.Count);
            foreach (var prop in props)
            {
                AveProject project = new AveProject(mRequest, mSite, prop);
                mListData.Add(project);
            }
        }

        public IAveProject this[string name]
        {
            get
            {
                var p = GetByName(name);
                if (p == null)
                {
                    throw new ArgumentException("Cannot find the specified project");
                }
                return p;
            }
        }

        public IAveProject this[Guid id]
        {
            get
            {
                var p = GetById(id);
                if (p == null)
                {
                    throw new ArgumentException("Cannot find the specified project");
                }
                return p;
            }
        }

        public IAveProject GetById(Guid id)
        {
            lock(locker)
            {
                return mListData.Find(
                    delegate(IAveProject p)
                    {
                        return p.Id.Equals(id);
                    });
            }
        }

        public IAveProject GetByName(string name)
        {
            lock (locker)
            {
                return mListData.Find(
                    delegate (IAveProject p)
                    {
                        return p.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
                    });
            }
        }

        public IAveProject GetByTaskListId(Guid id)
        {
            lock (locker)
            {
                return mListData.Find(
                    delegate (IAveProject p)
                    {
                        return p.TaskListId.Equals(id);
                    });
            }
        }

        public IAveProjectSerializer ProjectSerializer
        {
            get
            {
                lock(locker)
                {
                    if (this.mProjectSerializer == null)
                    {
                        this.mProjectSerializer = new AveProjectSerializer(mRequest, mSite);
                    }
                }
                return this.mProjectSerializer;
            }
        }
    }
}
