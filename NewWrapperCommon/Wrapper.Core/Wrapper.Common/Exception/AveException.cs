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
using AvePoint.Exceptions;
using System.Resources;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Exception;

namespace AvePoint.Wrapper.Common
{
    [Serializable]
    public class AveWrapperBaseException : AveException
    {
        public AveWrapperBaseException(AveInternalResourceKey i18nKey, params object[] args)
            : base(i18nKey.ToString(), WrapperExceptionResource.ResourceManager.GetString(i18nKey.ToString(), WrapperExceptionResource.Culture), WrapperExceptionResource.ResourceManager.BaseName, args)
        {
        }

        public AveWrapperBaseException(System.Exception innerException, AveInternalResourceKey i18nkey, params object[] args)
            : base(i18nkey.ToString(), innerException, WrapperExceptionResource.ResourceManager.GetString(i18nkey.ToString(), WrapperExceptionResource.Culture), args)
        {
        }

        public AveWrapperBaseException(string message)
            : base(message)
        { }

        public AveWrapperBaseException(string message, Exception innerException)
            : base(innerException, message)
        {
        }

        public AveWrapperBaseException()
            : base()
        { }


    }

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
    public class AveWarningException : AveWrapperBaseException
    {
        public AveWarningException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        {
        }

        public AveWarningException(Exception e, AveInternalResourceKey key, params object[] args)
            : base(e, key, args)
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
        SkipItemUniqueFieldConflict = -12,
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
    public class AveRestoreException : AveWrapperBaseException
    {
        private AveRestoreResult mResult = AveRestoreResult.Normal;

        public AveRestoreResult Result
        {
            get { return mResult; }
        }

        public AveRestoreException(AveRestoreResult result, AveInternalResourceKey key, params object[] args)
            : base(key, args)
        {
            mResult = result;
        }

        public AveRestoreException(AveRestoreResult result, Exception e, AveInternalResourceKey key, params object[] args)
            : base(e, key, args)
        {
            mResult = result;
        }

        //public AveRestoreException(AveRestoreResult result,Exception e, string message)
        //    : base(message,e)
        //{
        //    mResult = result;
        //}

        public AveRestoreException(AveRestoreResult result, string message)
            : base(message)
        {
            mResult = result;
        }
    }

    [Serializable]
    public class ManagedPathNotFoundException : AveWrapperBaseException
    {
        public ManagedPathNotFoundException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        { }
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

    public class AveConcurrencyException : AveDatabaseException
    {
        internal AveConcurrencyException(string message, Exception e)
            : base(message, e)
        { }


    }

    [Serializable]
    public class AveRPCException : AveException
    {
        private MethodBase mMethod;
        private object[] mArgs;

        public AveRPCException(MethodBase method, object[] methodArgs, string format, params object[] args)
            : base(format, args)
        {
            mMethod = method;
            mArgs = methodArgs;
        }

        public AveRPCException(MethodBase method, object[] methodArgs, Exception e, string format, params object[] args)
            : base(e, format, args)
        {
            mMethod = method;
            mArgs = methodArgs;
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
    public class AveVerifyItemMetadataValueNotFoundException : AveWrapperBaseException
    {
        public AveVerifyItemMetadataValueNotFoundException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        { }
    }

    [Serializable]
    public class AveVerifyPageLayoutNotFoundException : AveWrapperBaseException
    {
        public AveVerifyPageLayoutNotFoundException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        { }
    }

    [Serializable]
    public class AveSecurityTrimingException : AveWrapperBaseException
    {
        public AveSecurityTrimingException(string message, Exception e)
            : base(message, e)
        { }

        public AveSecurityTrimingException(Exception innerException, AveInternalResourceKey key, params object[] args)
            : base(innerException, key, args)
        { }
    }

    [Serializable]
    public class CompatibilityLevelSkipException : AveWrapperBaseException
    {
        public CompatibilityLevelSkipException() { }
        public CompatibilityLevelSkipException(string message) : base(message) { }
        public CompatibilityLevelSkipException(string message, Exception inner) : base(message, inner) { }
        public CompatibilityLevelSkipException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        { }
    }

    [Serializable]
    public class AveFakeUserException : AveWrapperBaseException
    {
        public AveFakeUserException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        {

        }
        public AveFakeUserException(string format, params object[] args)
            : base(string.Format(format, args))
        { }
    }

    [Serializable]
    public class AveFileNotFoundException : AveWrapperBaseException
    {
        public AveFileNotFoundException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        { }
    }

    [Serializable]
    public class AveSiteNotFoundException :Exception
    {
        public AveSiteNotFoundException(string message,Exception inner)
            : base(message, inner)
        { }
    }

    [Serializable]
    public class AveDirectoryNotFoundException : AveWrapperBaseException
    {

        public AveDirectoryNotFoundException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        {
        }
    }

    [Serializable]
    public class AveNotSupportedException : AveWrapperBaseException
    {
        public AveNotSupportedException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        {
        }
    }

    [Serializable]
    public class AveArgumentNullException : AveWrapperBaseException
    {
        public AveArgumentNullException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        { }

        public AveArgumentNullException(string message)
            : base(message)
        { }
    }

    [Serializable]
    public class AveUnauthorizedAccessException : UnauthorizedAccessException
    {
        private string userLoginName = string.Empty;

        public AveUnauthorizedAccessException(string message, string userLoginName)
            : base
            (message)
        {
            this.userLoginName = userLoginName;
        }

        public AveUnauthorizedAccessException(string message, string userLoginName, Exception innerException)
            : base(message, innerException)
        {
            this.userLoginName = userLoginName;
        }

        public string UserLoginName
        {
            get
            {
                return userLoginName;
            }
        }
    }

    [Serializable]
    public class AveArgumentException : AveWrapperBaseException
    {
        public AveArgumentException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        { }

        public AveArgumentException(string message)
            : base(message)
        { }
    }

    [Serializable]
    public class AveArgumentOutOfRangeException : AveWrapperBaseException
    {
        public AveArgumentOutOfRangeException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        { }
    }


    [Serializable]
    public class AveQueryThrottledException : AveWrapperBaseException
    {
        public AveQueryThrottledException(string message) : base(message) { }
        public AveQueryThrottledException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class AveNodeModifiedAfterAuditorRetriveJob : AveWrapperBaseException
    {
        public AveNodeModifiedAfterAuditorRetriveJob(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        { }
    }

}
