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
namespace Microsoft365.Graph.Service;
public partial class GraphDriveService
{
    #region Download file content. DO NOT Add DownloadFileByPathAsync, always use item-id when donwload file content.
    // DO NOT Add DownloadFileByPathAsync
    // 当item path中包含一些特殊组合(%20)时，sdk无法正确escape，会导致请求失败, Engineering Competition/A&%100  +(@!)~-_/Document.docx

    /// <summary>
    /// Download file content by id.
    /// The implementation support large file with fixed size buffer.
    /// The implementation support resume download on poor network connection.
    /// https://docs.microsoft.com/en-us/graph/api/driveitem-get-content?view=graph-rest-1.0&tabs=http#partial-range-downloads
    /// https://github.com/microsoftgraph/msgraph-sdk-dotnet/tree/dev/docs#downloadLargeFile
    /// </summary>
    /// <param name="driveId">Drive id</param>
    /// <param name="driveItemId">Drive item id</param>
    /// <param name="cancellationToken">Cancellation token, if it support cancellation, set the timeout large enough for download all bytes of the file</param>
    /// <returns></returns>
    [GraphAPI("/drives/{drive-id}/items/{item-id}/content")]
    public async Task<Stream> DownloadFileByIdAsync(
        string driveId,
        string driveItemId,
        CancellationToken cancellationToken)
    {
        var item = await GetDriveItemAsync(driveId, driveItemId, cancellationToken);
        ArgumentNullException.ThrowIfNull(item);
        return await DownloadFileByIdAsync(driveId, item, cancellationToken);
    }

    [GraphAPI("/drives/{drive-id}/items/{item-id}/content")]
    public async Task<Stream> DownloadFileByIdAsync(
        string driveId,
        DriveItem driveItem,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(driveId);
        ArgumentNullException.ThrowIfNull(driveItem);
        driveItem.Id?.ThrowIfNullOrEmpty();
        string? downloadUrl = driveItem.DownloadUrl();
        return await DownloadService.OpenStreamAsync(GetDownloadUrlAsync, cancellationToken);

        async Task<string> GetDownloadUrlAsync(bool force)
        {
            if (string.IsNullOrEmpty(downloadUrl) || force)
            {
                var item = await GetDriveItemAsync(driveId, driveItem.Id!, cancellationToken);
                downloadUrl = item?.DownloadUrl();
            }
            return downloadUrl.EnsureIfNotNullOrEmpty();
        }
    }

    #endregion

    [GraphAPI("/drives/{drive-id}/root:/{file-path}:/createUploadSession", Method = "POST")]
    public async Task<UploadResult<DriveItem>> UploadLargeFileAsync(
    string driveId,
    string itemPath,
    Stream stream,
    bool overwrite,
    CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(driveId);
        ArgumentNullException.ThrowIfNullOrEmpty(itemPath);

        var uploadSessionRequestBody = new Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession.CreateUploadSessionPostRequestBody
        {
            Item = new DriveItemUploadableProperties
            {
                AdditionalData = new Dictionary<string, object>
                {
                    { "@microsoft.graph.conflictBehavior", overwrite ? "replace" : "fail" },
                },
            },
        };

        var uploadSession = await client.Drives[driveId]
            .Items["root"]
            .ItemWithPath(itemPath)
            .CreateUploadSession
            .PostAsync(uploadSessionRequestBody, cancellationToken: cancellationToken);

        int maxSliceSize = 10 * 1024 * 1024;
        var fileUploadTask = new LargeFileUploadTask<DriveItem>(
            uploadSession, stream, maxSliceSize, client.RequestAdapter);

        return await fileUploadTask.UploadAsync(cancellationToken: cancellationToken);
    }

}
