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
    using AvePoint.Wrapper.Common;
    using System;
    using System.IO;
    class CreateFolderRequest : SharePointRestBase<EmptyObject>
    {
        public string FolderServerRelativeUrl { get; set; }

        public override Stream PostRequestStream
        {
            get
            {
                var body = "{ '__metadata': { 'type': 'SP.Folder' }, 'ServerRelativeUrl': '" + this.FolderServerRelativeUrl + "'}";
                return ConvertToMMStream(body);
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
                return $"{this.restBaseUrl}{EndPoint.CreateFolder()}";
            }
        }

        protected override void ValidateArguments()
        {
            if (string.IsNullOrEmpty(this.FolderServerRelativeUrl)) throw new ArgumentNullException(nameof(this.FolderServerRelativeUrl));
        }

        public CreateFolderRequest(string siteUrl, ITokenProvider tokenProvider) : base(siteUrl, tokenProvider)
        {
            this.OnRequestFailed += ThrowIfParentFolderNotExist;
        }

        public void ThrowIfParentFolderNotExist(ReliableHttpWebRequest request, Exception ex)
        {
            if (ex.StatusCode() == System.Net.HttpStatusCode.InternalServerError)
            {
                throw new FileNotFoundException("Failed to create folder, parent folder not exist", ex);
            }
        }
    }
}