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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.Search.Query;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOSearchProvider:IAveOSearchProvider
    {
        public AveOSearchProvider()
        {
           mLocalSharePointProviderName = "Local SharePoint Provider";
           mLocalSharePointProviderId = "FA947043-6046-4F97-9714-40D4C113963D";
           mLocalSharePointProviderGuid = new Guid(LocalSharePointProviderId);
           mLocalSharePointProviderFlow = "Microsoft.SharePointSearchProviderFlow";
           mLocalPeopleProviderName = "Local People Provider";
           mLocalPeopleProviderId = "E4BCC058-F133-4425-8FFC-1D70596FFD33";
           mLocalPeopleProviderGuid = new Guid(LocalPeopleProviderId);
           mLocalPeopleProviderFlow = "Microsoft.PeopleSearchFlow";
           mRemoteSharePointProviderName = "Remote SharePoint Provider";
           mRemoteSharePointProviderId = "1E0C8601-2E5D-4CCB-9561-53743B5DBDE7";
           mRemoteSharePointProviderGuid = new Guid(RemoteSharePointProviderId);
           mRemoteSharePointProviderFlow = "Microsoft.RemoteSharepointFlow";
           mRemotePeopleProviderName = "Remote People Provider";
           mRemotePeopleProviderId = "E377CAAA-FCAF-4a1b-B7A1-E69A506A07AA";
           mRemotePeopleProviderGuid = new Guid(RemotePeopleProviderId);
           mRemotePeopleProviderFlow = "Microsoft.RemoteSharepointFlow";
           mOpenSearchProviderName = "OpenSearch Provider";
           mOpenSearchProviderId = "3A17E140-1574-4093-BAD6-E19CDF1C0121";
           mOpenSearchProviderGuid = new Guid(OpenSearchProviderId);
           mOpenSearchProviderFlow = "Microsoft.OpenSearchProviderFlow";
           mExchangeSearchProviderName = "Exchange Search Provider";
           mExchangeSearchProviderId = "3A17E140-1574-4093-BAD6-E19CDF1C0122";
           mExchangeSearchProviderGuid = new Guid(ExchangeSearchProviderId);
           mExchangeSearchProviderFlow = "Microsoft.ExchangeSearchProviderFlow";
           mAcronymDefinitionProviderName = "Acronym Definition Provider";
           mAcronymDefinitionProviderId = "50fe6aad-274f-4f99-a5f4-5ed05999db7c";
           mAcronymDefinitionProviderGuid = new Guid(AcronymDefinitionProviderId);
           mAcronymDefinitionProviderFlow = "Microsoft.AcronymDefinitionProviderFlow";
           mBestBetProviderName = "Best Bet Provider";
           mBestBetProviderId = "8D07E50A-C924-4FB0-94B1-89573E0C2200";
           mBestBetProviderGuid = new Guid(BestBetProviderId);
           mBestBetProviderFlow = "Microsoft.BestBetProviderFlow";
           mPersonalFavoritesProviderName = "Personal Favorites Provider";
           mPersonalFavoritesProviderId = "8E35D350-E91E-4a6f-BE79-5009E5ED2A84";
           mPersonalFavoritesProviderGuid = new Guid(PersonalFavoritesProviderId);
           mPersonalFavoritesProviderFlow = "Microsoft.PersonalFavoritesProviderFlow";
           mRemoteProviders = new HashSet<Guid> { RemoteSharePointProviderGuid, RemotePeopleProviderGuid, OpenSearchProviderGuid, ExchangeSearchProviderGuid };

        }
        private string mAcronymDefinitionProviderFlow;
        public string AcronymDefinitionProviderFlow
        {
            get { return mAcronymDefinitionProviderFlow; }
        }
        private Guid mAcronymDefinitionProviderGuid;
        public Guid AcronymDefinitionProviderGuid
        {
            get { return mAcronymDefinitionProviderGuid; }
        }
        private string mAcronymDefinitionProviderId;
        public string AcronymDefinitionProviderId
        {
            get { return mAcronymDefinitionProviderId; }
        }
        private string mAcronymDefinitionProviderName;
        public string AcronymDefinitionProviderName
        {
            get { return mAcronymDefinitionProviderName; }
        }
        private string mBestBetProviderFlow;
        public string BestBetProviderFlow
        {
            get { return mBestBetProviderFlow; }
        }
        private Guid mBestBetProviderGuid;
        public Guid BestBetProviderGuid
        {
            get { return mBestBetProviderGuid; }
        }
        private string mBestBetProviderId;
        public string BestBetProviderId
        {
            get { return mBestBetProviderId; }
        }
        private string mBestBetProviderName;
        public string BestBetProviderName
        {
            get { return mBestBetProviderName; }
        }
        private string mExchangeSearchProviderFlow;
        public string ExchangeSearchProviderFlow
        {
            get { return mExchangeSearchProviderFlow; }
        }
        private Guid mExchangeSearchProviderGuid;
        public Guid ExchangeSearchProviderGuid
        {
            get { return mExchangeSearchProviderGuid; }
        }
        private string mExchangeSearchProviderId;
        public string ExchangeSearchProviderId
        {
            get { return mExchangeSearchProviderId; }
        }
        private string mExchangeSearchProviderName;
        public string ExchangeSearchProviderName
        {
            get { return mExchangeSearchProviderName; }
        }
        private int mFlowNameMaxLength;
        public int FlowNameMaxLength
        {
            get { return mFlowNameMaxLength; }
        }
        private string mLocalPeopleProviderFlow;
        public string LocalPeopleProviderFlow
        {
            get { return mLocalPeopleProviderFlow; }
        }
        private Guid mLocalPeopleProviderGuid;
        public Guid LocalPeopleProviderGuid
        {
            get { return mLocalPeopleProviderGuid; }
        }
        private string mLocalPeopleProviderId;
        public string LocalPeopleProviderId
        {
            get { return mLocalPeopleProviderId; }
        }
        private string mLocalPeopleProviderName;
        public string LocalPeopleProviderName
        {
            get { return mLocalPeopleProviderName; }
        }
        private string mLocalSharePointProviderFlow;
        public string LocalSharePointProviderFlow
        {
            get { return mLocalSharePointProviderFlow; }
        }
        private Guid mLocalSharePointProviderGuid;
        public Guid LocalSharePointProviderGuid
        {
            get { return mLocalSharePointProviderGuid; }
        }
        private string mLocalSharePointProviderId;
        public string LocalSharePointProviderId
        {
            get { return mLocalSharePointProviderId; }
        }
        private string mLocalSharePointProviderName;
        public string LocalSharePointProviderName
        {
            get { return mLocalSharePointProviderName; }
        }
        private int mNameMaxLength;
        public int NameMaxLength
        {
            get { return mNameMaxLength; }
        }
        private string mOpenSearchProviderFlow;
        public string OpenSearchProviderFlow
        {
            get { return mOpenSearchProviderFlow; }
        }
        private Guid mOpenSearchProviderGuid;
        public Guid OpenSearchProviderGuid
        {
            get { return mOpenSearchProviderGuid; }
        }
        private string mOpenSearchProviderId;
        public string OpenSearchProviderId
        {
            get { return mOpenSearchProviderId; }
        }
        private string mOpenSearchProviderName;
        public string OpenSearchProviderName
        {
            get { return mOpenSearchProviderName; }
        }
        private string mPersonalFavoritesProviderFlow;
        public string PersonalFavoritesProviderFlow
        {
            get { return mPersonalFavoritesProviderFlow; }
        }
        private Guid mPersonalFavoritesProviderGuid;
        public Guid PersonalFavoritesProviderGuid
        {
            get { return mPersonalFavoritesProviderGuid; }
        }
        private string mPersonalFavoritesProviderId;
        public string PersonalFavoritesProviderId
        {
            get { return mPersonalFavoritesProviderId; }
        }
        private string mPersonalFavoritesProviderName;
        public string PersonalFavoritesProviderName
        {
            get { return mPersonalFavoritesProviderName; }
        }
        private string mRemotePeopleProviderFlow;
        public string RemotePeopleProviderFlow
        {
            get { return mRemotePeopleProviderFlow; }
        }
        private Guid mRemotePeopleProviderGuid;
        public Guid RemotePeopleProviderGuid
        {
            get { return mRemotePeopleProviderGuid; }
        }
        private string mRemotePeopleProviderId;
        public string RemotePeopleProviderId
        {
            get { return mRemotePeopleProviderId; }
        }
        private string mRemotePeopleProviderName;
        public string RemotePeopleProviderName
        {
            get { return mRemotePeopleProviderName; }
        }
        private HashSet<Guid> mRemoteProviders;
        public HashSet<Guid> RemoteProviders
        {
            get { return mRemoteProviders; }
        }
        private string mRemoteSharePointProviderFlow;
        public string RemoteSharePointProviderFlow
        {
            get { return mRemoteSharePointProviderFlow; }
        }
        private Guid mRemoteSharePointProviderGuid;
        public Guid RemoteSharePointProviderGuid
        {
            get { return mRemoteSharePointProviderGuid; }
        }
        private string mRemoteSharePointProviderId;
        public string RemoteSharePointProviderId
        {
            get { return mRemoteSharePointProviderId; }
        }
        private string mRemoteSharePointProviderName;
        public string RemoteSharePointProviderName
        {
            get { return mRemoteSharePointProviderName; }
        }
    }
}
