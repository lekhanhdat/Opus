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
using System.Resources;
using System.Runtime.Serialization;

namespace AvePoint.Exceptions
{
    [KnownType(typeof(AveI18NException))]
    public class AveI18NException : Exception
    {
        private string i18nKey;
        private string resourceManagerBaseName;
        private List<object> parameters;


        public string I18NKey
        {
            get
            {
                return i18nKey;
            }
            set
            {
                i18nKey = value;
            }
        }


        public List<object> Parameters
        {
            get
            {
                return parameters;
            }
            set
            {
                parameters = value;
            }
        }

        public string GetI18NMessage(ResourceManager resource)
        {
            return GetI18NMessage(resource, CultureInfo.CurrentCulture);
        }

        public string GetI18NMessage(ResourceManager resource, CultureInfo culture)
        {
            if (i18nKey != null)
            {
                string result = resource.GetString(i18nKey, culture);

                if (result == null)
                {
                    return Message;
                }

                else
                {
                    if (parameters != null && parameters.Count > 0)
                    {
                        result = Format(result, parameters);
                    }
                    return result;
                }

            }

            else
            {

                return Message;
            }

        }


        #region Constructors
        public AveI18NException()
        {

        }

        public AveI18NException(string message)
            : base(message)
        {
        }

        public AveI18NException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AveI18NException(string i18nKey, string message, string resourceManagerBaseName)
            : base(message)
        {
            this.i18nKey = i18nKey;
            this.resourceManagerBaseName = resourceManagerBaseName;
        }

        public AveI18NException(string i18nKey, string message, string resourceManagerBaseName, params object[] args)
            : base(Format(message, args))
        {
            this.i18nKey = i18nKey;
            this.resourceManagerBaseName = resourceManagerBaseName;
            parameters = new List<object>(args);
        }


        public AveI18NException(string i18nKey, ResourceManager resource)
            : this(i18nKey, resource.GetString(i18nKey), resource.BaseName)
        {


        }


        public AveI18NException(string i18nKey, ResourceManager resource, params object[] args)
            : this(i18nKey, resource.GetString(i18nKey), resource.BaseName, args)
        {


        }


        public AveI18NException(System.Exception innerException)
            : base(string.Empty, innerException)
        {


        }




        public AveI18NException(System.Exception innerException, string _i18nkey, string message, params object[] args)
            : base(Format(message, args), innerException)
        {
            this.i18nKey = _i18nkey;
            parameters = new List<object>(args);
        }

        private static string Format(string message, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] is AveI18NException)
                    {
                        AveI18NException exception = args[i] as AveI18NException;
                        args[i] = exception.Message;
                    }
                }
                return string.Format(message, args);
            }
            return message;
        }

        #endregion

    }
}
