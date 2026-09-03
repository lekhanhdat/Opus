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




namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net.Mail;
    using System.Reflection;

    #endregion

    /// <summary>
    /// The mail writer class is a internal class of .net mail api
    /// </summary>
    public class MimeCompatibleFormatGenerator
        : IMimeCompatibleFormatGenerator
    {
        static Type mailWriterType;
        static BindingFlags flags;
        static MethodInfo sendMethod;

        static MimeCompatibleFormatGenerator()
        {
            flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            mailWriterType = typeof(SmtpClient).Assembly.GetType("System.Net.Mail.MailWriter");
            sendMethod = typeof(MailMessage).GetMethod("Send", flags);
        }

        public void Generate(MailMessage message, Stream outPutStream)
        {
            var mailWriter = Activator.CreateInstance(
                type: mailWriterType,
                bindingAttr: flags,
                binder: null,
                args: new Object[] { outPutStream },
                culture: CultureInfo.InvariantCulture);
            sendMethod.Invoke(message, new Object[] { mailWriter, true });
        }

        public Byte[] Generate(MailMessage message)
        {
            using (var ms = new MemoryStream())
            {
                this.Generate(message, ms);
                return ms.ToArray();
            }
        }
    }
}