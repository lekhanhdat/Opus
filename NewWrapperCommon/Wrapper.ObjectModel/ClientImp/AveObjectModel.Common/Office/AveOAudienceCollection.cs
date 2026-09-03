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
using AvePoint.Wrapper.Common.Office;
using System.Collections;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOAudienceCollection : AveClientObject, IAveOAudienceCollection
    {
        private List<IAveOAudience> audienceList = new List<IAveOAudience>();
        private List<string> audienceNames = new List<string>();
        private IAveRequest request;
        public AveOAudienceCollection(IAveRequest request, List<Dictionary<string, object>> propList)
        {
            this.request = request;
            foreach (Dictionary<string, object> audienceProp in propList)
            {
                AveOAudience audience = new AveOAudience(request, audienceProp);
                if (!audienceNames.Contains(audience.AudienceName))
                {
                    audienceNames.Add(audience.AudienceName);
                    audienceList.Add(audience);
                }
            }
        }

        public IAveOAudience this[int index]
        {
            get
            {
                return audienceList[index];
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new AveOAudienceEnumerator(this.audienceList);
        }

        public int Count
        {
            get
            {
                return audienceList.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object SyncRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
    }

    public class AveOAudienceEnumerator : IEnumerator
    {
        // Fields
        private List<IAveOAudience> m_AudienceCollection;
        private int m_AudienCount;
        private int m_Index;

        // Methods
        internal AveOAudienceEnumerator(List<IAveOAudience> AudienceCollection)
        {
            this.m_AudienceCollection = AudienceCollection;
            this.m_Index = -1;
            this.m_AudienCount = AudienceCollection.Count;
        }

        public bool MoveNext()
        {
            this.m_Index++;
            if (this.m_Index >= this.m_AudienCount)
            {
                return false;
            }
            return true;
        }

        public void Reset()
        {
            this.m_Index = -1;
        }

        // Properties
        public object Current
        {
            get
            {
                return this.m_AudienceCollection[this.m_Index];
            }

        }
    }
}
