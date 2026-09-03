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
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.ErrorCode;

namespace AvePoint.Wrapper.Common
{

    [Serializable]
    public class AveWrapperI18NException : Exception
    {
        public virtual TroubleshootingErrorCode ErrorCode { get; }

        private string key;
        private List<object> args = new List<object>();

        public string Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
            }
        }

        public List<object> Args
        {
            get
            {
                return args;
            }
            set
            {
                args = value;
            }
        }

        public AveWrapperI18NException() { }
        public AveWrapperI18NException(string message) : base(message) { }
        public AveWrapperI18NException(string message, Exception inner) : base(message, inner) { }
        public AveWrapperI18NException(Exception e) : base(e.Message, e) { }
        protected AveWrapperI18NException(
  System.Runtime.Serialization.SerializationInfo info,
  System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        public AveWrapperI18NException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }

        public AveWrapperI18NException(string key,string defaultValue)
            : base(defaultValue)
        {
            this.Key = key;
        }

        public AveWrapperI18NException(string key, string defaultValue,Exception innerException)
            : base(defaultValue, innerException)
        {
            this.Key = key;
        }

        public AveWrapperI18NException(string key, string defaultValue, params object[] args)
            : base(Format(defaultValue, args))
        {
            this.Key = key;
            this.Args = new List<object>(args);
        }

        private static string Format(string message, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] is AveWrapperI18NException)
                    {
                        AveWrapperI18NException exception = args[i] as AveWrapperI18NException;
                        args[i] = exception.Message;
                    }
                }
            }
            if (args != null)
            {
                return string.Format(message, args);
            }
            return message;
        }

        public string GetFormatedMessage()
        {
            try
            {
                string defaultMessage = string.Format(Message, Args);
                List<object> args = new List<object>(Args);
                List<PropertyItem> propertyItems = new List<PropertyItem>() { new PropertyItem() { PropertyType = ParamKey.Message, Key = key, Args = args.ToArray(), DefaultValue = defaultMessage } };
                return SerializerHelper.SerializeToXmlString<List<PropertyItem>>(propertyItems);
            }
            catch (Exception)
            {
                return Message;
            }
        }
    }
}
