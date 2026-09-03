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
using AvePoint.RA.CommonUtil;
using Google.Apis.DriveLabels.v2;
using Google.Apis.DriveLabels.v2.Data;
using Google.Apis.Services;
using RAGoogle.Extension;
using System.Reflection;

namespace RAGoogle.API.API
{
    internal class LabelApi : IDisposable
    {
        private readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private DriveLabelsService _service;

        internal LabelApi(BaseClientService.Initializer initializer)
        {
            _service = new DriveLabelsService(initializer);
            _service.HttpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        internal async Task<List<GoogleAppsDriveLabelsV2Label>> ListAllLabelsAsync(FileQuery query = null, bool throwExceptionForUnauthorized = false)
        {
            var result = new List<GoogleAppsDriveLabelsV2Label>();
            int retry = 0;
            bool hasException;
            do
            {
                try
                {
                    retry++;
                    var request = _service.Labels.List();
                    request.PageSize = 100;
                    request.Fields = "*";
                    request.PublishedOnly = query?.PublishedOnly;
                    request.View = query.IsLabelViewFull ? LabelsResource.ListRequest.ViewEnum.LABELVIEWFULL : LabelsResource.ListRequest.ViewEnum.LABELVIEWBASIC;
                    request.UseAdminAccess = query.UseDomainAdminAccess;
                    if (!string.IsNullOrEmpty(query?.QuotaUser) && query.QuotaUser.Length < 40)
                    {
                        request.QuotaUser = query.QuotaUser;
                    }
                    do
                    {
                        var response = await request.ExecuteExAsync(throwExceptionForUnauthorized) ?? throw new CommonException("There is something wrong when listing all labels, the api response is null");
                        if (response.Labels != null)
                        {
                            result.AddRange(response.Labels);
                        }
                        request.PageToken = response.NextPageToken;
                    } while (!string.IsNullOrEmpty(request.PageToken));
                    hasException = false;
                }
                catch (Exception e)
                {
                    await HandleException(e, retry);
                    hasException = true;
                }
            } while (retry < 3 && hasException);
            return result;
        }
        internal async Task<GoogleAppsDriveLabelsV2Label> CreateLabelAsync(GoogleAppsDriveLabelsV2Label label)
        {
            try
            {
                LabelsResource.CreateRequest req = _service.Labels.Create(label);
                req.UseAdminAccess = true;
                label = await req.ExecuteAsync();
                return await PublishedLabelAsync(label.Name);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to create label to google. Error: {ex}");
                throw;
            }
        }

        internal async Task<GoogleAppsDriveLabelsV2Label> UpdateLabelAsync(GoogleAppsDriveLabelsV2DeltaUpdateLabelRequestUpdateLabelPropertiesRequest label, string name)
        {
            try
            {
                var delta = new GoogleAppsDriveLabelsV2DeltaUpdateLabelRequest
                {
                    Requests = new List<GoogleAppsDriveLabelsV2DeltaUpdateLabelRequestRequest>
            {
                new GoogleAppsDriveLabelsV2DeltaUpdateLabelRequestRequest
                {
                    UpdateLabel = new GoogleAppsDriveLabelsV2DeltaUpdateLabelRequestUpdateLabelPropertiesRequest
                    {
                        Properties = label.Properties
                    }
                }
            },
                    UseAdminAccess = true
                };

                LabelsResource.DeltaRequest req = _service.Labels.Delta(delta, name);
                var updatedLabel = (await req.ExecuteAsync()).UpdatedLabel;
                return await PublishedLabelAsync(updatedLabel.Name);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to update label to google. Error: {ex}");
                throw;
            }
        }

        internal async Task DeleteLabelAsync(string labelName)
        {
            try
            {
                LabelsResource.DeleteRequest req = _service.Labels.Delete(labelName);
                req.UseAdminAccess = true;
                await req.ExecuteAsync();
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to delete label to google. Error: {ex}");
                throw;
            }
        }

        internal async Task<GoogleAppsDriveLabelsV2Label> DisableLabelAsync(string labelName)
        {
            try
            {
                GoogleAppsDriveLabelsV2DisableLabelRequest body = new()
                {
                    UseAdminAccess = true
                };
                LabelsResource.DisableRequest req = _service.Labels.Disable(body, labelName);
                var labelDisable = await req.ExecuteAsync();
                return await SearchLabelByName(labelDisable.Name);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to disable label to google. Error: {ex}");
                throw;
            }
        }
        internal async Task<GoogleAppsDriveLabelsV2Label> EnableLabelAsync(string labelName)
        {
            try
            {
                GoogleAppsDriveLabelsV2EnableLabelRequest body = new()
                {
                    UseAdminAccess = true
                };
                LabelsResource.EnableRequest req = _service.Labels.Enable(body, labelName);
                var labelEnable = await req.ExecuteAsync();
                return await SearchLabelByName(labelEnable.Name);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to enable label to google. Error: {ex}");
                throw;
            }
        }

        internal async Task<GoogleAppsDriveLabelsV2Label> PublishedLabelAsync(string labelName)
        {
            try
            {
                GoogleAppsDriveLabelsV2PublishLabelRequest body = new()
                {
                    UseAdminAccess = true,
                };

                LabelsResource.PublishRequest req = _service.Labels.Publish(body, labelName);
                var labelPublished = await req.ExecuteAsync();
                return await SearchLabelByName(labelPublished.Name);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to published label to google. Error: {ex}");
                throw;
            }
        }

        public async Task<GoogleAppsDriveLabelsV2Label> SearchLabelByName(string labelName)
        {
            try
            {
                var req = _service.Labels.Get(labelName);
                req.View = LabelsResource.GetRequest.ViewEnum.LABELVIEWFULL;
                req.UseAdminAccess = true;
                return await req.ExecuteAsync();
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to search label to google. Error: {ex}");
                throw;
            }
        }

        private async Task HandleException(Exception e, int retry)
        {
            logger.Warn($"Failed to list all labels. retry:{retry}. Error: {e}");
            /*if (e.Message.Contains("unauthorized_client", StringComparison.OrdinalIgnoreCase))
            {
                throw e;
            }*/
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _service?.Dispose();
            _service = null;
        }

        ~LabelApi()
        {
            Dispose(false);
        }
    }
}
