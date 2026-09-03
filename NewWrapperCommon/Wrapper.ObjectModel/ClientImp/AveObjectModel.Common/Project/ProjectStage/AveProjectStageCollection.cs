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
    class AveProjectStageCollection : AveAbstractCommonCollection<IAveProjectStage>, IAveProjectStageCollection
    {
        private IAveRequest mRequest;
        private object locker = new object();

        public AveProjectStageCollection(IAveRequest request, List<Dictionary<string, object>> props)
        {
            this.mRequest = request;
            InitProjectStageCollection(props);
        }

        private void InitProjectStageCollection(List<Dictionary<string, object>> props)
        {
            mListData = new List<IAveProjectStage>(props.Count);
            foreach (var prop in props)
            {
                var stage = new AveProjectStage(mRequest, prop);
                mListData.Add(stage);
            }
        }

        public IAveProjectStage GetByGuid(Guid uid)
        {
            lock (locker)
            {
                return mListData.Find(
                    delegate(IAveProjectStage s)
                    {
                        return s.Id.Equals(uid);
                    });
            }
        }

        public IAveProjectStage GetById(string objectId)
        {
            throw new NotImplementedException();
        }

        public IAveProjectStage GetByName(string name)
        {
            lock (locker)
            {
                return mListData.Find(
                    delegate(IAveProjectStage s)
                    {
                        return string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase);
                    });
            }
        }

        public IAveProjectStage Add(AveProjectStageInfo info)
        {
            Dictionary<string, object> prop = mRequest.AddStage(info);
            AveProjectStage stage = new AveProjectStage(mRequest, prop);
            mListData.Add(stage);
            return stage;
        }
    }
}
