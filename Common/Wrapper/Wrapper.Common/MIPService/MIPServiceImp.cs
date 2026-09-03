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
using Util.MIP;

namespace AvePoint.Wrapper.Common
{
    public sealed class MIPServiceImp
    {
        private static readonly Lazy<MIPServiceImp> lazy =
            new Lazy<MIPServiceImp>(() => new MIPServiceImp());
        private static bool _hasInit = false;
        private string _office365TenantId;
        Func<string, string> _getTokenFunc;

        private MIPService _service;

        public static MIPServiceImp Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        private MIPServiceImp()
        {

        }

        public void Init(string office365TenantId, string workingUser, Util.MIP.Cloud cloudLocation, Func<string, string> getTokenFunc)
        {
            _office365TenantId = office365TenantId;
            _getTokenFunc = getTokenFunc;
            //workingUser is only used for setting protection
            //var userName = workingUser.Substring(0, workingUser.IndexOf('@'));
            MIPBuilder builder = new MIPBuilder();
            _service = builder.ConfigWorkPath("mip cache", office365TenantId)
                           .ConfigTokenProvider(getTokenFunc)
                           //.ConfigWorkUser(workingUser, userName)
                           .ConfigCloudLocation(cloudLocation)
                           .ConfigDefaultJustificationMessage("decrypt")
                           .BuildMIPService();
            _hasInit = true;
        }

        public MIPService GetService()
        {
            if (_hasInit)
            {
                return _service;
            }
            else
            {
                throw new Exception("MIPServiceImp has not init, please call Init() first.");
            }
        }
    }
}
