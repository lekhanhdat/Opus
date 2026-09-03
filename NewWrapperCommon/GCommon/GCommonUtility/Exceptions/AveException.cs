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
using AvePoint.I18N;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Exceptions;

namespace AvePoint.GCommon.Utility
{
    public class AveException : AveI18NException
    {
        private bool tryFindI18nMessage;
        private Dictionary<Enum, string> contexts = new Dictionary<Enum, string>();
        protected Dictionary<Enum, string> Contexts { get { return contexts; } }

        #region Constructors
        public AveException()
        {
            this.tryFindI18nMessage = true;
        }

        public AveException(string message)
            : base(message)
        {
        }

        public AveException(Exception innerException)
            : base(string.Empty, innerException)
        {
        }

        public AveException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }

        public AveException(Exception innerException, string format, params object[] args)
            : base(string.Format(format, args), innerException)
        {
        }
		public AveException(string i18nKey,Exception innerException, string format,params object[] args) 
            : base(innerException,i18nKey,format,args) 
        { 
        }
        public AveException(string i18nKey, string message, string resourceManagerBaseName)
            : base(i18nKey, message, resourceManagerBaseName)
        {
        }

        public AveException(string i18nKey, string message, string resourceManagerBaseName, params object[] args)
            : base(i18nKey, message, resourceManagerBaseName, args)
        {
        }
        #endregion

        public override string Message
        {
            get
            {
                string message = null;
                if (tryFindI18nMessage)
                {
                    message = GetValue("ExceptionCause");
                }
                else
                {
                    message = base.Message;
                }

                if (contexts.Count > 0)
                {
                    message += "\n" + ContextKeys.GetAllContexts(contexts);
                }
                return message;
            }
        }

        private string GetValue(string prefix)
        {
            string temp = this.GetType().FullName;
            var index = temp.IndexOf("AvePoint.GCommon.Utility.Exceptions", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                temp = temp.Substring(index + "AvePoint.GCommon.Utility.Exceptions".Length);
                temp = temp.Replace(".", "_");
                temp = prefix + temp;
                var message = EventViewerResources.ResourceManager.GetString(temp);
                if (!string.IsNullOrEmpty(message))
                {
                    return message;
                }
            }
            return base.Message;
        }
    }
}
