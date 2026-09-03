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
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Label;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Model;
using Google.Apis.DriveLabels.v2.Data;
using RAGoogle.API.API;
using RAGoogle.Extension;
using RAGoogle.GoogleObjDiscover.Services;
using RAGoogle.Models.Enums;

namespace RAGoogle.Services;

public class GoogleLabelService : BaseService, IDisposable
{
    private static IRALogger logger = RALogger.GetInstance(typeof(GoogleLabelService));
    private LabelApi _labelApi;

    public GoogleLabelService(RMAosGoogleAppProfile app, string impersonateUser = "") : base(app, impersonateUser, GoogleScopeType.DriveWithLabel)
    {
        _labelApi = new(initializer);
    }

    #region label api
    public async Task<List<GoogleAppsDriveLabelsV2Label>> ListLabelsPublishedAsync(bool throwExceptionForUnauthorized = false)
    {
        List<GoogleAppsDriveLabelsV2Label> labelsPublished = [];
        try
        {
            var query = new FileQuery
            {
                PublishedOnly = true,
                IsLabelViewFull = true,
                UseDomainAdminAccess = true,
            };
            labelsPublished = await _labelApi.ListAllLabelsAsync(query, throwExceptionForUnauthorized: throwExceptionForUnauthorized);
        }
        catch (Exception ex)
        {
            logger.Error($"Get labels published failed, Message: {ex}");
            throw;
        }
        return labelsPublished;
    }

    public async Task<List<GoogleAppsDriveLabelsV2Label>> ListDraffLabelsAsync()
    {
        List<GoogleAppsDriveLabelsV2Label> labels = [];
        try
        {
            var query = new FileQuery
            {
                PublishedOnly = false,
                IsLabelViewFull = true,
                UseDomainAdminAccess = true,
            };
            labels = (await _labelApi.ListAllLabelsAsync(query)).Where(label => GoogleLabelExtension.ConvertState(label.Lifecycle.State) == State.Published && label.Lifecycle.HasUnpublishedChanges.HasValue).ToList();
        }
        catch (Exception ex)
        {
            logger.Error($"Get labels published failed, Message: {ex}");
            throw;
        }
        return labels;
    }
    public async Task<List<GoogleAppsDriveLabelsV2Label>> ListDraftLabelsAsync()
    {
        List<GoogleAppsDriveLabelsV2Label> labels = [];
        try
        {
            var query = new FileQuery
            {
                PublishedOnly = false,
                IsLabelViewFull = true,
                UseDomainAdminAccess = true,
            };
            labels = (await _labelApi.ListAllLabelsAsync(query)).Where(label => GoogleLabelExtension.ConvertState(label.Lifecycle.State) == State.UnpublishedDraft).ToList();
        }
        catch (Exception ex)
        {
            logger.Error($"Get labels daft from google failed, Message: {ex}");
            throw;
        }
        return labels;
    }

    public async Task<IDictionary<string, string>> ListLabelsBasicAsync()
    {
        List<GoogleAppsDriveLabelsV2Label> labelsPublished = [];
        try
        {
            var query = new FileQuery
            {
                PublishedOnly = false,
                IsLabelViewFull = false,
                UseDomainAdminAccess = true,
            };
            labelsPublished = await _labelApi.ListAllLabelsAsync(query);
            return labelsPublished.ToDictionary(x => x.Id, x => x.Properties.Title);
        }
        catch (Exception ex)
        {
            logger.Error($"Get labels published failed, Message: {ex}");
            throw;
        }
    }
    public async Task<IDictionary<string, GoogleAppsDriveLabelsV2Label>> ListAllLabelsAsync()
    {
        List<GoogleAppsDriveLabelsV2Label> labelsPublished = [];
        try
        {
            var query = new FileQuery
            {
                PublishedOnly = true,
                IsLabelViewFull = true,
                UseDomainAdminAccess = true,
            };
            labelsPublished = await _labelApi.ListAllLabelsAsync(query);
            return labelsPublished.ToDictionary(x => x.Id, x => x);
        }
        catch (Exception ex)
        {
            logger.Error($"Get labels published failed, Message: {ex}");
            throw;
        }
    }
    public async Task<RMLabel> UpdateLabelToGoogleAsync(RMLabel label, GoogleAppsDriveLabelsV2Label labelGoogle)
    {
        try
        {
            var updateLabel = new GoogleAppsDriveLabelsV2DeltaUpdateLabelRequestUpdateLabelPropertiesRequest
            {
                Properties = new GoogleAppsDriveLabelsV2LabelProperties
                {
                    Title = label.Name,
                    Description = label.Description
                }
            };

            if (labelGoogle.Lifecycle.HasUnpublishedChanges == true)
            {
                await _labelApi.PublishedLabelAsync(labelGoogle.Name);
            }

            var updatedLabel = await _labelApi.UpdateLabelAsync(updateLabel, labelGoogle.Name);

            updatedLabel.MapGoogleLabelToRMLabelForUpdate(label);

            return label;
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred during update label to google. Error: {ex}");
            throw;
        }
    }
    public async Task<RMLabel> CreateLabelToGoogleAsync(RMLabel label)
    {
        try
        {
            var newLabel = await _labelApi.CreateLabelAsync(label.MapRMLabelToGoogleLabel());

            newLabel.MapGoogleLabelToRMLabelForUpdate(label);

            return label;
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred during create label to google. Error: {ex}");
            throw;
        }
    }
    public async Task DeleteLabelToGoogleAsync(LabelValue values, GoogleAppsDriveLabelsV2Label labelGoogle)
    {
        try
        {
            var labelDisabel = labelGoogle;
            if (GoogleLabelExtension.ConvertState(labelGoogle.Lifecycle.State) != State.Disabled)
            {
                labelDisabel = await _labelApi.DisableLabelAsync(labelGoogle.Name);
            }
            await _labelApi.DeleteLabelAsync(labelDisabel.Name);
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred during delete label to google. Error: {ex}");
            throw;
        }
    }
    public async Task DeleteTermToGoogleAsync(GoogleAppsDriveLabelsV2Label labelGoogle)
    {
        try
        {
            var labelDisabel = labelGoogle;
            if (GoogleLabelExtension.ConvertState(labelGoogle.Lifecycle.State) != State.Disabled)
            {
                labelDisabel = await _labelApi.DisableLabelAsync(labelGoogle.Name);
            }
            await _labelApi.DeleteLabelAsync(labelDisabel.Name);
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred during delete term to google. Error: {ex}");
            throw;
        }
    }
    public async Task<string> CreateTermToGoogleAsync(RMTerm term, RMGoogleLabelInfo googleLabelInfo)
    {
        try
        {
            var newLabel = await _labelApi.CreateLabelAsync(term.MapRMTermToGoogleLabel());
            googleLabelInfo.State = (int)State.Published;
            newLabel.MapGoogleLabelToRMGoogleLabelInfoForUpdate(term, googleLabelInfo);
            return newLabel.Name;
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred during create term to google. Error: {ex}");
            throw;
        }
    }
    public async Task<GoogleAppsDriveLabelsV2Label> UpdateTermToGoogleAsync(RMTerm term, GoogleAppsDriveLabelsV2Label labelGoogle, RMGoogleLabelInfo lableInfoUpdated, TermChanged termChanged)
    {
        try
        {
            var properties = new GoogleAppsDriveLabelsV2LabelProperties();
            switch (termChanged)
            {
                case TermChanged.NameChanged:
                    properties.Title = term.Name;
                    properties.Description = labelGoogle.Properties.Description;
                    break;
                case TermChanged.DescriptionChanged:
                    properties.Title = labelGoogle.Properties.Title;
                    properties.Description = term.Description;
                    break;
                default:
                    properties.Title = term.Name;
                    properties.Description = term.Description;
                    break;
            }
            var updateLabel = new GoogleAppsDriveLabelsV2DeltaUpdateLabelRequestUpdateLabelPropertiesRequest
            {
                Properties = properties
            };

            if (labelGoogle.Lifecycle.HasUnpublishedChanges == true)
            {
                await _labelApi.PublishedLabelAsync(labelGoogle.Name);
            }

            var updatedLabel = await _labelApi.UpdateLabelAsync(updateLabel, labelGoogle.Name);
            lableInfoUpdated.State = (int)State.Published;
            updatedLabel.MapGoogleLabelToRMGoogleLabelInfoForUpdate(term, lableInfoUpdated);
            return updatedLabel;
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred during update term to google. Error: {ex}");
            throw;
        }
    }
    public async Task DisableTermToGoogle(RMTerm term, string labelName, RMGoogleLabelInfo rMGoogleLabelInfo)
    {
        try
        {
            var labelDisabel = await _labelApi.DisableLabelAsync(labelName);
            rMGoogleLabelInfo.State = (int)State.Disabled;
            labelDisabel.MapGoogleLabelToRMGoogleLabelInfoForUpdate(term, rMGoogleLabelInfo);
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred during disable term to google. Error: {ex}");
            throw;
        }
    }
    public async Task<GoogleAppsDriveLabelsV2Label> EnableTermToGoogle(RMTerm term, string labelName, RMGoogleLabelInfo rMGoogleLabelInfo)
    {
        try
        {
            var labelEnable = await _labelApi.EnableLabelAsync(labelName);
            rMGoogleLabelInfo.State = (int)State.Published;
            labelEnable.MapGoogleLabelToRMGoogleLabelInfoForUpdate(term, rMGoogleLabelInfo);
            return labelEnable;
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred during enable term to google. Error: {ex}");
            throw;
        }
    }
    #endregion

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _labelApi?.Dispose();
        _labelApi = null;
    }

    ~GoogleLabelService()
    {
        Dispose(false);
    }
}
