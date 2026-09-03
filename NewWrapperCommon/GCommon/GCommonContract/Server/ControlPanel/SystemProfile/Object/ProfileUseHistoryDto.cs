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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.AveModuleContract;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.SystemProfile.Object
{
    public class ProfileUseHistoryDto
    {
        public string Id { get; set; }
        public NameAndIdDto SecurityProfile { get; set; }
        public int Module { get; set; }
        public string Setting { get; set; }
        public ProfileUseHistoryType Type { get; set; }
    }

    public class ProfileUseHistorySetting
    {
        public List<ProfileUseHistoryDetailDto> Detail { get; set; }
    }

    public class ProfileUseHistoryDetailDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            ProfileUseHistoryDetailDto dto = (ProfileUseHistoryDetailDto)obj;

            return this.Name.Equals(dto.Name) && this.Type == dto.Type;
        }

        public override int GetHashCode()
        {
            return this.Type ^ this.Name.GetHashCode();
        }
    }

    public enum ProfileUseHistoryType : int
    {
        SecurityProfile = 301,
    }
    public interface IProfileUseHistory
    {
    }
}
