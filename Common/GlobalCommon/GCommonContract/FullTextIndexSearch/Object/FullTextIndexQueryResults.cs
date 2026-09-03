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



using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Media.Object;

namespace AvePoint.Common.FullTextIndexSearch.Object
{
    [DataContract]
    public class FullTextIndexQueryResults : IEnumerable<SearchRequestResult>
    {
        /// <summary>
        /// 总页数.
        /// </summary>
        [DataMember]
        public int TempTotalDocs { get; set; }

        /// <summary>
        /// Search Results
        /// </summary>
        [DataMember]
        public List<SearchRequestResult> SeachResults {get;set;}

        /// <summary>
        /// 失败的Profile
        /// </summary>
        [DataMember]
        public Dictionary<string,string> FallarProfile { get; set; }

        public FullTextIndexQueryResults()
        {
            this.FallarProfile = new Dictionary<string, string>();
            this.SeachResults = new List<SearchRequestResult>();
        }

        /// <summary>
        /// 获得从MediaSearch的总记录.
        /// </summary>
        public int TotalDocs
        {
            get { return TempTotalDocs; }
            set 
            {
                TempTotalDocs += value; 
            }
        }

        public void AddRange(List<SearchRequestResult> results)
        {
            this.SeachResults.AddRange(results);
        }

        public IEnumerator<SearchRequestResult> GetEnumerator()
        {
           
            return this.SeachResults.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
          
            return this.SeachResults.GetEnumerator();
        }

        public void ClearResults()
        {
            this.SeachResults = new List<SearchRequestResult>();
        }
    }
}
