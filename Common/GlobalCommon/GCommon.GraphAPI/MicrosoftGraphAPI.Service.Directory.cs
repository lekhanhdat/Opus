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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.GCommon.GraphAPI
{
    public partial class MicrosoftGraphAPIService
    {   
        public List<DeletedGroup> GetDeletedGroupsByGroupAddress(string groupAddress)
        {
            var groups = graphServiceClient.Directory.DeletedItems.GraphGroup.GetAsync(request =>
            {
                request.QueryParameters.Filter = $"mail eq '{ODataSpecialCharactersConverter.ConvertMailForSDK(groupAddress)}'";
            }).Result?.Value;
            return groups?.Select(dg =>
            {
                return new DeletedGroup()
                {
                    Id = dg.Id,
                    DeletedDateTime = dg.DeletedDateTime,
                    MailAddress = dg.Mail,
                    Visibility = dg.Visibility,
                    DisplayName = dg.DisplayName,
                    Description = dg.Description,
                    CreatedDateTime = dg.CreatedDateTime.ToString()
                };
            }).ToList() ?? [];

        }

        public void RemoveDirectoryGroup(string groupId)
        {
            new DeleteDirectoryGroup(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController).GetApiResult();
        }
    }
}