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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HybridServer.EF.Entity
{
    public class Agent
    {
        public string AgentId;

        public string TenantId;

        public string ConnectionId;

        public DateTime RegistrationTime;

        public DateTime LastConnected;

        public ConnectionStatus Status;
    }

    //[Table("Managers")]
    //public class Manager
    //{
    //    [Column]
    //    public string ManagerId;

    //    [Column]
    //    public string ConnectionId;

    //    [Column]
    //    public DateTime RegistrationTime;

    //    [Column]
    //    public DateTime LastConnected;

    //    [Column]
    //    public ConnectionStatus Status;
    //}

    public enum ConnectionStatus
    {
        Undefiend = 0,
        Connected = 1,
        Disconnected = 2
    }


}
