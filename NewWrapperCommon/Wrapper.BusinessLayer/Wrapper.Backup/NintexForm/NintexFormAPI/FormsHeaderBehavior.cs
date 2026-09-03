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
/* FormsHeaderBehavior.cs

   Copyright (c) 2014 - Nintex. All Rights Reserved.  
   This code released under the terms of the  
   Microsoft Reciprocal License (MS-RL,  http://opensource.org/licenses/MS-RL.html.)
   
*/

using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace AvePoint.Wrapper.Backup
{
    class FormsHeaderBehavior : WebHttpBehavior
    {
        // The form digest to use when validating client requests.
        private string mFormDigest;

        /// <summary>
        /// Creates an instance, using the specified form digest.
        /// </summary>
        /// <param name="formDigest">The form digest to use.</param>
        public FormsHeaderBehavior(string formDigest)
        {
            mFormDigest = formDigest;
        }

        /// <summary>
        /// Adds a new message inspector to the service endpoint.
        /// </summary>
        /// <param name="endpoint">The Nintex Forms service endpoint.</param>
        /// <param name="runtime">The client runtime.</param>
        /// <remarks>The custom client message inspector adds the X-RequestDigest 
        /// header property to requests and sets the value of the header property 
        /// to the form digest.</remarks>
        public override void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime runtime)
        {
            runtime.MessageInspectors.Add(new FormsHeaderInspector(mFormDigest));
            base.ApplyClientBehavior(endpoint, runtime);
        }

        //public override void Validate(ServiceEndpoint endpoint)
        //{
        //    base.Validate(endpoint);
        //}

        //public override void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        //{
        //    base.AddBindingParameters(endpoint, bindingParameters);
        //}

        //public override void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        //{
        //    base.ApplyDispatchBehavior(endpoint, endpointDispatcher);
        //}
    }
}
