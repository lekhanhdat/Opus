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
namespace ExchangeUtility.Graph.SharePointRestAPI
{
    using Microsoft365.Authentication;
    using System;
    using System.IO;
    class AddFileRequest : SharePointRestBase<EmptyObject>
    {
        public AddFileRequest(string siteUrl, ITokenProvider tokenProvider)
            : base(siteUrl, tokenProvider)
        {

        }

        public string FileName { get; set; }

        public string  FolderServerRelativeUrl { get; set; }

        public bool OverWrite { get; set; } = false;

        public Stream Content { get; set; }

        public override Stream PostRequestStream
        {
            get
            {
                return this.Content;
            }
        }

        public override string RequestMethod
        {
            get
            {
                return METHOD_POST;
            }
        }

        public override string RequestUrl
        {
            get
            {
                // return $"{this.restBaseUrl}{EndPoint.GetFolder(this.FolderServerRelativeUrl)}{EndPoint.AddFile(this.FileName, this.OverWrite.ToString().ToLowerInvariant())}";
                return $"{this.restBaseUrl}/web/getfolderbyserverrelativeurl(@f){EndPoint.AddFile(this.FileName, this.OverWrite.ToString().ToLowerInvariant())}?@f='{this.FolderServerRelativeUrl}'";
            }
        }

        protected override void ValidateArguments()
        {
            if (string.IsNullOrEmpty(this.FileName)) throw new ArgumentNullException(nameof(this.FileName));
        }
    }
}