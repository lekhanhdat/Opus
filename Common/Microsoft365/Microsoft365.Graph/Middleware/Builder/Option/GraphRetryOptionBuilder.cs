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


namespace Microsoft365.Graph.Middleware;

internal class GraphRetryOptionBuilder : RetryOptionBuilder
{
    public override int HttpMaxRetry { get; } = 5;
    public override int HttpInitialDelay { get; } = 5;
    protected override bool ShouldRetry(int delay, int attempt, Exception error)
    {
        return error switch
        {
            //Add more errors which should not retry here
            OperationCanceledException or//timeout or user canceled
            ArgumentException or
            ApiException //this error is thrown from Graph sdk which is on upper layer, add it here in case someone move SocketRetry to upper layer by mistaken
            => false,
            //Reviewed with Fariel Zhang, set default to true.
            //It is a tradeoff between performance and stability
            _ => true,
        };
    }
    protected override bool ShouldRetry(HttpStatusCode httpStatusCode, HttpContent content)
    {
        //DONOT take any dependency on the content of message. You should only code against error codes or inner error codes.
        //https://learn.microsoft.com/en-us/graph/errors#error-resource-type
        var oDataError = content.GetODataErrorAsync(decompress: true).ExecuteAsyncTask();
        var errorCode = oDataError?.Error?.Code;
        return (httpStatusCode, errorCode) switch
        {
            // Retry based on HttpStatusCode and errorCode here
            { httpStatusCode: HttpStatusCode.BadRequest, errorCode: "invalidRequest" } => true,
            // Retry based on errorCode only here
            // { errorCode: "changetoyourcode" } => true,
            _ => base.ShouldRetry(httpStatusCode, content)
        };
    }
}