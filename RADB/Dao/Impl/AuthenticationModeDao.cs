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
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.DB.Model;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Dao.Impl
{
    [RACodeReview("Allen Yin")]
    public class AuthenticationModeDao : BaseDao<RMAuthenticationMode>, IAuthenticationModeDao
    {
        public List<RMAuthenticationMode> GetAuthenticationModes(bool OnlyEnableMode)
        {
            var context = SharedDbContext;
            List<RMAuthenticationMode> results = null;
            if (OnlyEnableMode)
            {
                results = context.AuthenticationMode.Where(am => am.Enable).ToList();
            }
            else
            {
                results = context.AuthenticationMode.ToList();
            }
            return results;
        }

        public bool ChangeAuthenticationModeStatus(int id, bool enableOrDisable)
        {
            var context = SharedDbContext;
            RMAuthenticationMode mode = context.AuthenticationMode.AsQueryable().FirstOrDefault(am => am.Id == id);
            if (mode == null)
            {
                return false;
            }
            if (mode.Enable != enableOrDisable)
            {
                mode.Enable = enableOrDisable;
                return this.Update(mode, am => am.Enable);
            }
            else
            {
                return true;
            }
        }

        public bool SetDefaultAuthenticationMode(int id)
        {
            var context = SharedDbContext;
            List<RMAuthenticationMode> updateModes = new List<RMAuthenticationMode>();
            var modes = context.AuthenticationMode.AsQueryable().Where(am => am.IsDefault || am.Id == id);
            foreach (var mode in modes)
            {
                mode.IsDefault = mode.Id == id;
                updateModes.Add(mode);
            }
            return this.BatchUpdate(updateModes) >= updateModes.Count;
        }
    }
}
