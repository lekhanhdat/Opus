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
using CommonModel.MethodInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.Contract.SignalR
{
    public class FileSystemUNCPathValidator : RemoteMessage<FileSystemUNCPathValidateArgs>
    {

        public override FileSystemUNCPathValidateArgs MethodArgs { get; set; }
        public override string MethodName { get { return MethodMapping.MT[typeof(FileSystemUNCPathValidator)]; } }

    }

    public enum ValidateResultEnum
    {
        Succeed,
        Failed
    }

    public enum FileSystemPathType
    {
        Unknown = 0,
        Unc = 1,
        Dfs = 2
    }

    public class ValidateResult
    {
        public ValidateResultEnum Result { get; set; }

        public string Message { get; set; }

        // Key: ConnectionId, Value: final path (original UNC or redirected DFS target)
        public Dictionary<Guid, string> UNCPaths { get; set; }

        // Key: ConnectionId, Value: path type
        public Dictionary<Guid, FileSystemPathType> PathType { get; set; }
    }

    public class FileSystemUNCPathValidateExecute : RemoteInvoke<FileSystemUNCPathValidateArgs, ValidateResult>
    {
        public override FileSystemUNCPathValidateArgs MethodArgs { get; set; }
        public override ValidateResult MethodResult { get; set; }

        public override string MethodName => MethodMapping.MT[typeof(FileSystemUNCPathValidateExecute)];
    }

    public class FileSystemUNCPathValidateArgs
    {
        public string TenantId { set; get; }

        public Guid BatchId { set; get; }

        public string UserName { set; get; }

        public string Password { set; get; }

        public Dictionary<Guid, string> UNCPaths { set; get; }
        public bool isEnabledJPMC { set; get; }
    }
}
