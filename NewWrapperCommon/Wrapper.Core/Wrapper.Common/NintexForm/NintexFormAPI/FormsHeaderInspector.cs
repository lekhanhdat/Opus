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
/* FormsHeaderInspector.cs

   Copyright (c) 2014 - Nintex. All Rights Reserved.  
   This code released under the terms of the  
   Microsoft Reciprocal License (MS-RL,  http://opensource.org/licenses/MS-RL.html.)
   
*/

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace AvePoint.Wrapper.Common
{
    class FormsHeaderInspector : IClientMessageInspector
    {
        private string mFormDigest;

        /// <summary>
        /// Creates an instance, specifying the form digest to use for request headers.
        /// </summary>
        /// <param name="formDigest">The form digest to use.</param>
        public FormsHeaderInspector(string formDigest)
        {
            mFormDigest = formDigest;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            // Does nothing
        }

        /// <summary>
        /// Sets the value of the X-RequestDigest header property to the form digest prior to sending the request.
        /// </summary>
        /// <param name="request">The message to be sent to the service.</param>
        /// <param name="channel">The WCF client object channel.</param>
        /// <returns>A null reference.</returns>
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;

            // If an appropriate HttpRequestMessageProperty is available, set the value of the X-RequestDigest 
            // header property to the form digest; otherwise, create a new HttpRequestMessageProperty and add 
            // the X-RequestDigest header property to it.
            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                httpRequestMessage = (HttpRequestMessageProperty)httpRequestMessageObject;
                if (string.IsNullOrEmpty(httpRequestMessage.Headers["X-RequestDigest"]))
                {
                    httpRequestMessage.Headers["X-RequestDigest"] = mFormDigest;
                }
            }
            else
            {
                httpRequestMessage = new HttpRequestMessageProperty();
                httpRequestMessage.Headers.Add("X-RequestDigest", mFormDigest);

                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
            }

            return null;
        }
    }
}
