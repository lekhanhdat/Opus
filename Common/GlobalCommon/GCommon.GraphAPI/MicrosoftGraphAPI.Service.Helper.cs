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

namespace AvePoint.GCommon.GraphAPI
{
    using static AvePoint.GCommon.GraphAPI.DirectoryObject;

    public partial class MicrosoftGraphAPIService
    {
        /// <summary>
        /// $"https://graph.microsoft.com/beta/v1.0/{userId}" 用于添加 group member
        /// </summary>
        /// <param name="teamsAppId"></param>
        /// <returns></returns>
        public string BuildTeamsAppOdataBind(string teamsAppId) => $"{resourceUrl}/{MicrosoftGraphApiBase<Empty>.Version_V1}/appCatalogs/teamsApps/{teamsAppId}";

        public DirectoryObject BuildDirectoryObject(string id, InputType type = InputType.users) => new DirectoryObject() { ODataId = $"{resourceUrl}/{MicrosoftGraphApiBase<Empty>.Version_V1}/{type}/{id}" };
        
        /// <summary>
        /// $"https://graph.microsoft.com/v1.0/users('userId')"
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GenerateOdataBindString(string id, InputType type = InputType.users) => $"{resourceUrl}/{MicrosoftGraphApiBase<Empty>.Version_V1}/{type}('{id}')";
    }
}