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
using System;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;

namespace AvePoint.RA.SharePoint.Archiver
{
    public sealed class SiteStateTransitionScope : IDisposable
    {
        private static readonly AveLogger Logger = AveLogger.GetInstance(typeof(SiteStateTransitionScope));
        private readonly string siteCollectionUrl;
        private readonly AveObjectModelFactory aveObjectModelFactory;
        private readonly SiteState targetState;
        private SiteState? originalState;
        private bool hasAttemptedConversion;
        private bool hasChanged;
        private bool disposed;

        private IDisposable _additionalDisposable;

        public SiteStateTransitionScope(string siteCollectionUrl, AveObjectModelFactory aveObjectModelFactory, SiteState targetState)
        {
            if (string.IsNullOrWhiteSpace(siteCollectionUrl))
            {
                throw new ArgumentException("Site collection url is required.", nameof(siteCollectionUrl));
            }
            this.siteCollectionUrl = siteCollectionUrl;
            this.aveObjectModelFactory = aveObjectModelFactory ?? throw new ArgumentNullException(nameof(aveObjectModelFactory));
            this.targetState = targetState;
        }

        public bool TryConvertToTargetStatus()
        {
            if (hasAttemptedConversion)
            {
                return false;
            }
            hasAttemptedConversion = true;

            try
            {
                if (!TryGetSiteProperties(out IAveSiteProperties siteProps))
                {
                    Logger.Warn($"UTryConvertToTargetStatus failed get TryGetSiteProperties.");
                    return false;
                }

                if (!TryParseSiteState(siteProps.LockState, out SiteState currentState))
                {
                    Logger.Info($"Unable to parse site lock state:{siteProps.LockState}.");
                    return false;
                }

                originalState ??= currentState;
                Logger.Info($"TryConvertToTargetStatus.CurrentSiteState:{currentState}.ChangedState:{targetState}.");
                if ((int)currentState >= (int)targetState)
                {
                    return true;
                }

                siteProps.LockState = targetState.ToString();
                siteProps.Update();
                Logger.Info($"TryConvertToTargetStatus.Successful change site state.State:{targetState}.");
                hasChanged = true;
                return true;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to try change site lock state for site:{siteCollectionUrl} ,ex:{e}");
                if (ShouldRethrowForException(e, "converting site lock state"))
                {
                    throw new Exception("RM_AR_Restore_SiteLocked_ErrorMessage", e);
                }
                return false;
            }
        }


        internal void AttachTeamsScope4Channel(IDisposable disposable)
        {
            _additionalDisposable = disposable;
        }

        public void Dispose()
        {
            try
            {
                if (disposed)
                {
                    return;
                }
                disposed = true;

                if (!hasChanged || !originalState.HasValue)
                {
                    return;
                }

                if (!TryGetSiteProperties(out IAveSiteProperties siteProps))
                {
                    Logger.Warn($"Dispose failed get TryGetSiteProperties.");
                    return;
                }

                if (!TryParseSiteState(siteProps.LockState, out SiteState currentState))
                {
                    Logger.Info($"Unable to parse site lock state:{siteProps.LockState}.");
                    return;
                }

                if (currentState == originalState.Value)
                {
                    Logger.Info($"Dispose currentState equals originalState.");
                    return;
                }

                siteProps.LockState = originalState.Value.ToString();
                siteProps.Update();
                Logger.Info($"Dispose successful update site state.currentState:{currentState}.originalState:{originalState.Value}.");
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to restore site lock state for site:{siteCollectionUrl} ,ex:{e}");
                if (ShouldRethrowForException(e, "restoring site lock state"))
                {
                    throw new Exception("RM_AR_Restore_SiteLocked_ErrorMessage", e);
                }
            }
            finally
            {
                _additionalDisposable?.Dispose();
                _additionalDisposable = null;
            }
        }

        public bool TryGetSiteProperties(out IAveSiteProperties siteProps)
        {
            siteProps = null;
            if (string.IsNullOrWhiteSpace(siteCollectionUrl) || aveObjectModelFactory == null)
            {
                Logger.Warn($"TryGetSiteProperties.SiteCollectionUrl is null or aveObjectModelFactory is null.");
                return false;
            }

            string adminUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(aveObjectModelFactory.AccountInfo, siteCollectionUrl);
            Logger.Info($"O365 Admin Url is : {adminUrl}");
            var aveTenant = aveObjectModelFactory.CreateTenant(adminUrl);
            if (aveTenant.TryGetAdminUrlForMultiGeoTenant(siteCollectionUrl, out string geoAdminUrl))
            {
                Logger.Info($"O365 Tenant is a multiple geo tenant, admin Url is : {geoAdminUrl}");
                adminUrl = geoAdminUrl;
                aveTenant = aveObjectModelFactory.CreateTenant(adminUrl);
            }
            siteProps = aveTenant.GetSitePropertiesByUrl(siteCollectionUrl);
            return siteProps != null;
        }

        private static bool TryParseSiteState(string lockState, out SiteState state)
        {
            return Enum.TryParse(lockState, true, out state);
        }

        private bool ShouldRethrowForException(Exception exception, string action)
        {
            SiteExistence existence = GetSiteExistence();
            Logger.Info($"Site existence check result is {existence} while {action} for site:{siteCollectionUrl}.");
            if (existence == SiteExistence.No || existence == SiteExistence.Recycled)
            {
                Logger.Info($"Site collection is not available while {action}. Site:{siteCollectionUrl} Error:{exception}.");
                return false;
            }
            return true;
        }


        private SiteExistence GetSiteExistence()
        {
            string adminUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(aveObjectModelFactory.AccountInfo, siteCollectionUrl);
            var aveTenant = aveObjectModelFactory.CreateTenant(adminUrl);
            if (aveTenant.TryGetAdminUrlForMultiGeoTenant(siteCollectionUrl, out string geoAdminUrl))
            {
                adminUrl = geoAdminUrl;
                aveTenant = aveObjectModelFactory.CreateTenant(adminUrl);
            }
            return aveTenant.SiteExistsAnywhere(siteCollectionUrl);
        }
    }
}
