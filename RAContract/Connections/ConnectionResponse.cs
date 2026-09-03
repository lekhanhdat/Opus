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
using AvePoint.GCommon.Contract.Server.ControlPanel.SolutionManager.Object;
using AvePoint.RA.Contract.AzureFileShare.Model;
using Newtonsoft.Json;

namespace AvePoint.RA.Contract.Connections
{
    public class ConnectionResponse
    {
        [JsonProperty("isSuccessful")]
        public bool IsSuccessful { get; set; }
        [JsonProperty("responseErrorType")]
        public ConnectionResponseErrorType ResponseErrorType { get; set; }
        [JsonProperty("responseMessage")]
        public string ResponseMessage { get; set; }
        public static ConnectionResponse Succeeded()
        {
            return new ConnectionResponse() { IsSuccessful = true };
        }
        public static ConnectionResponse Failed(ConnectionResponseErrorType responseErrorType, string responseMessage = null)
        {
            return new ConnectionResponse() { IsSuccessful = false, ResponseErrorType = responseErrorType, ResponseMessage = responseMessage };
        }
        public static ConnectionResponse Generate(bool isSuccessful, ConnectionResponseErrorType responseErrorType = ConnectionResponseErrorType.None, string responseMessage = null)
        {
            return new ConnectionResponse() { IsSuccessful = isSuccessful, ResponseErrorType = responseErrorType, ResponseMessage = responseMessage };
        }
    }

    public enum ConnectionResponseErrorType
    {
        //Common
        None = 0,
        Unknown = 1,
        NameExists = 2,
        Timeout = 3,
        ValidationError = 4,

        //BoxConnection 10~20
        ClientIdExists = 10,
        JsonFileInvalid = 11,
        EnterpriseIdExists = 12,
        AuthorizationCodeExpired = 13,
        //DropBoxConnection 20~30    
    }
}
