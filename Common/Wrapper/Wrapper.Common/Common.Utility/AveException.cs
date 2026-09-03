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
using System.Reflection;
using System.Text;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon;
using System.Collections.Generic;

namespace AvePoint.Wrapper.Common
{
    [Serializable]
    public class AveErrorException : Exception
    {
        public AveErrorException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }

        public AveErrorException(Exception e, string format, params object[] args)
            : base(string.Format(format, args), e)
        {
        }
    }

    [Serializable]
    public class AveWarningException : Exception
    {
        public AveWarningException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }

        public AveWarningException(Exception e, string format, params object[] args)
            : base(string.Format(format, args), e)
        {
        }
    }

    [Serializable]
    public class AveXmlException : AveException
    {
        public AveXmlException(string format, params object[] args)
            : base(format, args)
        { }

        public AveXmlException(Exception e, string format, params object[] args)
            : base(e, format, args)
        { }
    }

    [Serializable]
    public class AveCloseException : AveException
    {
        public AveCloseException(string format, params object[] args)
            : base(format, args)
        { }

        public AveCloseException(Exception e, string format, params object[] args)
            : base(e, format, args)
        { }
    }
    /// <summary>
    /// Value<0 : Restore Failed or don't need to Restore
    /// Value>=0: Restore Successful
    /// </summary>
    public enum AveRestoreResult
    {
        Failed = Int32.MinValue,
        SkipRecycleBinData = -11,//include recycle bin and not overwrite
        SkipTheSameItem = -10,//skip item conflict and same modified
        Omit = -1, //Don't need to restore or the file isn't item 
        Normal = 100,//Normal restore
        ResoreLessVersion = 0,//Restore Version By Sql (current version is bigger than original version)
        RestoreEqualVersion = 1, //current version less original version,create a version equal original version
        RestoreBiggerVersion = 2,//current version equal original version 
        SkipRestoreItemMetaData = -2, //item no need restore metadata, but is not the same definition as failed, such as 07 explore view restore.
    }

    [Serializable]
    public class AveRestoreException : AveException
    {
        private AveRestoreResult mResult = AveRestoreResult.Normal;

        public AveRestoreResult Result
        {
            get { return mResult; }
        }
        public AveRestoreException(AveRestoreResult result, string format, params object[] args)
            : base(format, args)
        {
            mResult = result;
        }

        public AveRestoreException(AveRestoreResult result, Exception e, string format, params object[] args)
            : base(e, format, args)
        {
            mResult = result;
        }
    }

    [Serializable]
    public class AveDatabaseException : Exception
    {
        // Methods
        public AveDatabaseException()
        { }

        public AveDatabaseException(string message)
            : base(message)
        { }

        public AveDatabaseException(string error, Exception e)
            : base(error, e)
        { }
    }

    [Serializable]
    public class AveConcurrencyException : AveDatabaseException
    {
        internal AveConcurrencyException(string message, Exception e)
            : base(message, e)
        { }


    }

    //public class AveObjectMissingException : Exception
    //{
    //    public AveObjectMissingException(string message)
    //        : base(message)
    //    {
    //    }
    //}
    [Serializable]
    public class AveRPCException : AveException
    {
        private MethodBase mMethod;
        private object[] mArgs;
        private Dictionary<string, object> mDetails;

        public AveRPCException(MethodBase method, object[] methodArgs, string format, params object[] args)
            : base(format, args)
        {
            mMethod = method;
            mArgs = methodArgs;
            mDetails = new Dictionary<string, object>();
        }

        public AveRPCException(MethodBase method, object[] methodArgs, Exception e, string format, params object[] args)
            : base(e, format, args)
        {
            mMethod = method;
            mArgs = methodArgs;
            mDetails = new Dictionary<string, object>();
        }

        public Dictionary<string, object> Details
        {
            get
            {
                return mDetails;
            }
        }

        public MethodBase Method
        {
            get
            {
                return mMethod;
            }
        }

        public object[] Args
        {
            get
            {
                return mArgs;
            }
        }

        public override string ToString()
        {
            if (mArgs != null)
            {
                return base.ToString();
            }
            else
            {
                StringBuilder sb = new StringBuilder(base.ToString());
                sb.Append("    ").Append(mMethod.Name);
                ParameterInfo[] parameters = mMethod.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    sb.Append(parameters[i].Name)
                      .Append(":")
                      .Append(mArgs[i])
                      .Append("    ");
                }
                return sb.ToString();
            }
        }
    }

    [Serializable]
    public class AveUpdatedConcurrencyException : AveConcurrencyException
    {
        public AveUpdatedConcurrencyException(string message, Exception e)
            : base(message, e)
        { }
    }

    [Serializable]
    public class AveSearchServiceNotFoundException : Exception
    {
        public AveSearchServiceNotFoundException()
        { }

        public AveSearchServiceNotFoundException(string message, Exception e)
            : base(message, e)
        { }
    }

    [Serializable]
    public class AveVerifyItemMetadataValueNotFoundException : Exception
    {
        public AveVerifyItemMetadataValueNotFoundException(string message)
            : base(message)
        { }
    }

    //public class AveSecurityTrimingException : Exception
    //{
    //    public AveSecurityTrimingException()
    //    { }

    //    public AveSecurityTrimingException(string message, Exception e)
    //        : base(message, e)
    //    { }
    //}
}
