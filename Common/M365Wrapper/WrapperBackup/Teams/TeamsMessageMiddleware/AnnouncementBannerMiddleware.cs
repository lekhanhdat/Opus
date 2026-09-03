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

namespace ExchangeUtility.Graph.Teams;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Newtonsoft.Json;

using Microsoft365Backup.DataBuilder.TeamHtml;
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.GraphAPI;
using ExchangeCommonWrapper;
using Util.MSAzure;

public class AnnouncementBannerMiddleware : TeamsMessageMiddleware
{
    private static readonly RALogger logger = RALogger.GetInstance(typeof(AnnouncementBannerMiddleware));

    public override void Invoke(TeamsMessageContext context)
    {
        ArgumentNullException.ThrowIfNull(context.TeamService);

        context.Message.MessageContent.HostedContents = context.Message.MessageContent.HostedContents ?? new List<HostedContent>();

        context.Message.Attachments?.Where(attachment => attachment.ContentType == TeamUtil.AttachmentAnnouncementBannerType && !string.IsNullOrEmpty(attachment.Content)).ForEach(attachment =>
        {
            if (context.Environment is AzureEnvironment.China)
            {
                throw new NotSupportedException(ExchangeConstants.ChinaNotSupportedHostedContentKey);
            }

            var announcementBannerContent = JsonConvert.DeserializeObject<AnnouncementBannerContent>(attachment.Content);
            if (announcementBannerContent.CardImageType != "uploadedImage")
            {
                return;
            }

            var uploadImageContent = JsonConvert.DeserializeObject<UploadImageContent>(attachment.Content);

            var originalImageHostedContentUrl = TeamsMessageUtility.SplitGraphApiUri(uploadImageContent.CardImageDetails.UploadedImageDetail.OriginalImage.Source);
            var croppedImageHostedContentUrl = TeamsMessageUtility.SplitGraphApiUri(uploadImageContent.CardImageDetails.UploadedImageDetail.CroppedImage.Source);

            if (!TeamsMessageUtility.HasHostedContentText(originalImageHostedContentUrl) || !TeamsMessageUtility.HasHostedContentText(croppedImageHostedContentUrl))
            {
                logger.Warn("AnnouncementBanner hosted content urls are invalid, original: {0}, cropped: {1}.",
                    uploadImageContent.CardImageDetails.UploadedImageDetail.OriginalImage.Source,
                    uploadImageContent.CardImageDetails.UploadedImageDetail.CroppedImage.Source);
                return;
            }

            var originalImageTemporaryId = Guid.NewGuid().ToString().ToLowerInvariant().Replace("-", "");
            var croppedImageTemporaryId = Guid.NewGuid().ToString().ToLowerInvariant().Replace("-", "");
            var requests = new Dictionary<string, string>
            {
                [originalImageTemporaryId] = TeamsMessageUtility.SplitGraphApiUri(uploadImageContent.CardImageDetails.UploadedImageDetail.OriginalImage.Source),
                [croppedImageTemporaryId] = TeamsMessageUtility.SplitGraphApiUri(uploadImageContent.CardImageDetails.UploadedImageDetail.CroppedImage.Source)
            };

            var useBeta = uploadImageContent.CardImageDetails.UploadedImageDetail.OriginalImage.Source.Contains("beta");
            Dictionary<string, string> backgroundImages = null;
            try
            {
                backgroundImages = context.TeamService.BatchGetHostedContentsAsString(requests, useBeta);
            }
            catch (AggregateException ex) when (ex.Flatten().InnerExceptions.Any(e => e is BatchRequestException bEx && bEx.HttpStatusCode == HttpStatusCode.Unauthorized) && context.TeamService4ServiceAccount != null)
            {
                logger.Warn("Get announcement banner background  unauthorized, we are going to try to get by service account.");
                backgroundImages = context.TeamService4ServiceAccount.BatchGetHostedContentsAsString(requests, useBeta);
            }
            catch (AggregateException ex) when (ex.Flatten().InnerExceptions.Any(e => e is BatchRequestException bEx && bEx.HttpStatusCode == HttpStatusCode.NotFound))
            {
                logger.Warn("The announcement banner background is empty.");
                return;
            }
            uploadImageContent.CardImageDetails.UploadedImageDetail.OriginalImage.Source = string.Format(TeamUtil.HostedContentSource, originalImageTemporaryId);
            context.Message.MessageContent.HostedContents.Add(new HostedContent
            {
                TemporaryId = originalImageTemporaryId,
                ContentBytes = backgroundImages[originalImageTemporaryId],
                ContentType = TeamUtil.HostedContentImageContentType
            });
            uploadImageContent.CardImageDetails.UploadedImageDetail.CroppedImage.Source = string.Format(TeamUtil.HostedContentSource, croppedImageTemporaryId);
            context.Message.MessageContent.HostedContents.Add(new HostedContent
            {
                TemporaryId = croppedImageTemporaryId,
                ContentBytes = backgroundImages[croppedImageTemporaryId],
                ContentType = TeamUtil.HostedContentImageContentType
            });
            attachment.Content = JsonConvert.SerializeObject(uploadImageContent);
        });

        Next?.Invoke(context);
    }
}