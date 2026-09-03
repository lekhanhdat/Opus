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

namespace AvePoint.Media.Storage.TSM
{
    using System;

    class TSMNodeInfo
    {
        public string PdID { get; set; }
        public string Nodename { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
        public string TcpServerAddress { get; set; }
        public string IncludeMC { get; set; }

        public Boolean EnableNodeProxy { get; set; }
        public String Asnodename { get; set; }

        public string CommunicationMethod { get; set; }
        public string Filespace { get; set; }
        public string ModifyTime { get; set; }
        public bool EnableLanfree { get; set; }
        public string Lanfreetcpport { get; set; }
        public string LanfreeTcpServerAddress { get; set; }
        public string LanfreeCommmethod { get; set; }

        public string ConfigFile { get; set; }
        public string ConfigFileDir { get; set; }
        public string CommConfigFile { get; set; }
        public string CommConfigFileDir { get; set; }
        public string CommDsmiDir { get; set; }
        public string CommDsmiLogDir { get; set; }
        public string CommDsmiLogName { get; set; }

        public long Capacity { get; set; }
        public long Occupancy { get; set; }
        public long SizeEstimate { get; set; }

        public bool IsSingleSession { get; set; }

        public bool IsValidate { get; set; }

        public TSMNodeInfo Clone()
        {
            return this.MemberwiseClone() as TSMNodeInfo;
        }
    }
}
