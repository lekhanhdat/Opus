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

namespace AvePoint.GCommon.GraphAPI
{
    class CreateTeam : PutRequest<TeamObj, TeamObj>
    {
        public CreateTeam(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable) : base(baseUrl, getToken, new TeamObj()
        {
            //if these properties are null, API failed with message=Object reference not set to an instance of an object.
            MemberSettings = new TeamMemberSettings(),
            MessagingSettings = new TeamMessagingSettings(),
            FunSettings = new TeamFunSettings(),
            GuestSettings = new TeamGuestSettings(),
        }, retryable)
        {
            this.GroupId = groupId;
        }

        protected override string RequestUrl => $"{this.apiUrlV1}/groups/{this.GroupId}/team";
        public string GroupId { get; set; }
    }
}