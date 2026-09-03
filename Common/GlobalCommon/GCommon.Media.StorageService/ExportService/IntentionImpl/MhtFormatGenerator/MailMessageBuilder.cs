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




namespace AvePoint.GCommon.Media.StorageService
{
    #region using directives

    using System;
    using System.Net.Mail;

    #endregion using directives

    internal class MailMessageBuilder
        : IMailMessageBuilder
    {
        public String From { get { return "DocAve@DocAve.com"; } }

        public String DisplayName { get { return "DocAve"; } }

        public String To { get { return "DocAve@DocAve.com"; } }

        public MailMessage Build(MetaData metaData)
        {
            var mailAddress = new MailAddress(this.From, this.DisplayName);
            var message = new MailMessage(mailAddress, mailAddress);
            //message.Headers["X-DATASOURCE"] = "DocAve 6";
            //message.Headers["X-FILENAME"] = metaData.FullPath;
            //message.SubjectEncoding = metaData.SubjectEncoding;
            //message.Subject = metaData.Subject;
            //message.IsBodyHtml = true;
            //message.BodyEncoding = metaData.BodyEncoding ?? Encoding.Default;
            //message.Body = this.GetBodyHtml(metaData);
            return message;
        }

        private String GetBodyHtml(MetaData metaData)
        {
            throw new NotImplementedException();
        }
    }
}