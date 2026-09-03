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
using AvePoint.Records.Core.Utilities.Extensions;
using Box.V2.Models;

namespace RABox
{
    public class BoxUserProxy
    {
        public string Id { get; internal set; }

        public string Name { get; internal set; }

        public string LoginName { get; internal set; }

        public string Role { get; internal set; }

        public string Status { get; internal set; }

        private readonly BoxClientContext _clientContext;

        private Guid uniqueId = Guid.Empty;

        public Guid UniqueId
        {
            get
            {
                if (uniqueId == Guid.Empty)
                {
                    uniqueId = $"{_clientContext.ConnectionInfo.EnterpriseId}/{Id}".ToMd5();
                }
                return uniqueId;
            }
        }

        public BoxUserProxy(BoxClientContext clientContext, BoxUser boxUser)
        {
            _clientContext = clientContext;
            InitProperties(boxUser);
        }


        private BoxUserProxy InitProperties(BoxUser _boxUser)
        {
            if (_boxUser == null)
            {
                return this;
            }

            Id = _boxUser.Id;
            Name = _boxUser.Name;
            LoginName = _boxUser.Login;
            Role = _boxUser.Role;
            Status = _boxUser.Status;

            return this;
        }
    }
}
