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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeUtility.MicrosoftGraph
{
    /// <summary>
    /// 所有调用出处理异常
    /// </summary>
    public class ListGroups : MicrosoftGraphApiBase
    {
        //private string apiUrl = "https://graph.microsoft.com/v1.0/groups";
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlBase}/v1.0/groups";
            }
        }

        public ListGroups(string baseUrl, string token) : base(baseUrl, token)
        {
            base.httpMethod = HttpMethod.Get;
        }

        public override object GetApiResult()
        {
            ListGroupsObj result;
            JsonDeserializer(GetInfoHelper(), out result);
            return result;
        }
    }

    public class GetGroupByMail : MicrosoftGraphApiBase
    {
        //private string apiUrl = "https://graph.microsoft.com/v1.0/groups?$filter= mail eq '{0}'";
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlBase}/v1.0/groups?$filter= mail eq '{this.GroupMailBox}'";
            }
        }
        public string GroupMailBox { get; private set; }


        public GetGroupByMail(string baseUrl, string token, string groupMailBox) : base(baseUrl, token)
        {
            this.GroupMailBox = groupMailBox;
            base.httpMethod = HttpMethod.Get;
        }

        public override object GetApiResult()
        {
            ListGroupsObj result;
            JsonDeserializer(GetInfoHelper(), out result);
            return result;
        }
    }

    public class ListGroupOwners : MicrosoftGraphApiBase
    {
        //private string apiUrl = "https://graph.microsoft.com/v1.0/groups/{0}/owners";
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlBase}/v1.0/groups/{this.GroupId}/owners";
            }
        }
        public string GroupId { get; private set; }

        public ListGroupOwners(string baseUrl, string token, string groupId) : base(baseUrl, token)
        {
            this.GroupId = groupId;
            base.httpMethod = HttpMethod.Get;
        }
        public override object GetApiResult()
        {
            ListGroupOwnersObj result;
            JsonDeserializer(GetInfoHelper(), out result);
            return result;
        }
    }

    public class ListGroupMembers : MicrosoftGraphApiBase
    {
        //private string apiUrl = "https://graph.microsoft.com/v1.0/groups/{0}/members";
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlBase}/v1.0/groups/{this.GroupId}/members";
            }
        }
        public string GroupId { get; private set; }
        public ListGroupMembers(string baseUrl, string token, string groupId) : base(baseUrl, token)
        {
            this.GroupId = groupId;
            base.httpMethod = HttpMethod.Get;
        }
        public override object GetApiResult()
        {
            ListGroupMembersObj result;
            JsonDeserializer(GetInfoHelper(), out result);
            return result;
        }
    }
}
