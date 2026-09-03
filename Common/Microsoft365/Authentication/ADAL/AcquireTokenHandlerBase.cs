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
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft365.Authentication.ADAL
{
	internal abstract class AcquireTokenHandlerBase
    {
		protected const string NullResource = "null_resource_as_optional";

		protected static readonly Task CompletedTask = Task.FromResult(result: false);

		private readonly TokenCache tokenCache;

		protected Exception RefreshException;

		protected readonly CacheQueryData CacheQueryData;

		internal CallState CallState
		{
			get;
			set;
		}

		protected bool SupportADFS
		{
			get;
			set;
		}

		protected Authenticator Authenticator
		{
			get;
			private set;
		}

		protected string Resource
		{
			get;
			set;
		}

		protected ClientKey ClientKey
		{
			get;
			private set;
		}

		protected TokenSubjectType TokenSubjectType
		{
			get;
			private set;
		}

		protected string UniqueId
		{
			get;
			set;
		}

		protected string DisplayableId
		{
			get;
			set;
		}

		protected UserIdentifierType UserIdentifierType
		{
			get;
			set;
		}

		protected bool LoadFromCache
		{
			get;
			set;
		}

		protected bool StoreToCache
		{
			get;
			set;
		}

		protected AcquireTokenHandlerBase(Authenticator authenticator, TokenCache tokenCache, string resource, ClientKey clientKey, TokenSubjectType subjectType,UserCredential userCredential, bool callSync)
		{
			Authenticator = authenticator;
			CallState = CreateCallState(Authenticator.CorrelationId, callSync);
			ADALLogger.Information(CallState, string.Format(CultureInfo.InvariantCulture, "=== Token Acquisition started:\n\tAuthority: {0}\n\tResource: {1}\n\tClientId: {2}\n\tCacheType: {3}\n\tAuthentication Target: {4}\n\t", authenticator.Authority, resource, clientKey.ClientId, (tokenCache != null) ? (tokenCache.GetType().FullName + string.Format(CultureInfo.InvariantCulture, " ({0} items)", new object[1]
			{
				tokenCache.Count
			})) : "null", subjectType));
			this.tokenCache = tokenCache;
			RefreshException = null;
			if (string.IsNullOrWhiteSpace(resource))
			{
				ArgumentNullException ex = new ArgumentNullException("resource");
				ADALLogger.Error(CallState, ex);
				throw ex;
			}
			Resource = ((resource != "null_resource_as_optional") ? resource : null);
			ClientKey = clientKey;
			TokenSubjectType = subjectType;
			LoadFromCache = (tokenCache != null);
			StoreToCache = (tokenCache != null);
			SupportADFS = false;
			CacheQueryData = new CacheQueryData();
			CacheQueryData.Authority = Authenticator.Authority;
			CacheQueryData.Resource = Resource;
			CacheQueryData.ClientId = ClientKey.ClientId;
			CacheQueryData.SubjectType = TokenSubjectType;
			CacheQueryData.UniqueId = userCredential.GenerateUniqueId();
			CacheQueryData.DisplayableId = DisplayableId;
		}

		public async Task<AuthenticationResult> RunAsync()
		{
			bool notifiedBeforeAccessCache = false;
			try
			{
				await PreRunAsync();
				AuthenticationResult result = null;
				if (LoadFromCache)
				{
					NotifyBeforeAccessCache();
					notifiedBeforeAccessCache = true;
					result = tokenCache.LoadFromCache(CacheQueryData, CallState);
					result = ValidateResult(result);
					if (result != null && result.AccessToken == null && result.RefreshToken != null)
					{
						result = await RefreshAccessTokenAsync(result);
						if (result != null)
						{
							tokenCache.StoreToCache(result, Authenticator.Authority, Resource, ClientKey.ClientId, TokenSubjectType,CacheQueryData.UniqueId,CacheQueryData.DisplayableId, CallState);
						}
					}
				}
				if (result == null)
				{
					await PreTokenRequest();
					result = await SendTokenRequestAsync();
					PostTokenRequest(result);
					if (StoreToCache)
					{
						if (!notifiedBeforeAccessCache)
						{
							NotifyBeforeAccessCache();
							notifiedBeforeAccessCache = true;
						}
						tokenCache.StoreToCache(result, Authenticator.Authority, Resource, ClientKey.ClientId, TokenSubjectType,CacheQueryData.UniqueId, CacheQueryData.DisplayableId, CallState);
					}
				}
				await PostRunAsync(result);
				return result;
			}
			catch (Exception ex)
			{
				ADALLogger.Error(CallState, ex);
				throw;
			}
			finally
			{
				if (notifiedBeforeAccessCache)
				{
					NotifyAfterAccessCache();
				}
			}
		}

		protected virtual AuthenticationResult ValidateResult(AuthenticationResult result)
		{
			return result;
		}

		public static CallState CreateCallState(Guid correlationId, bool callSync)
		{
			correlationId = ((correlationId != Guid.Empty) ? correlationId : Guid.NewGuid());
			return new CallState(correlationId, callSync);
		}

		protected virtual Task PostRunAsync(AuthenticationResult result)
		{
			LogReturnedToken(result);
			return CompletedTask;
		}

		protected virtual async Task PreRunAsync()
		{
			await Authenticator.UpdateFromTemplateAsync(CallState);
			ValidateAuthorityType();
		}

		protected virtual Task PreTokenRequest()
		{
			return CompletedTask;
		}

		protected virtual void PostTokenRequest(AuthenticationResult result)
		{
			Authenticator.UpdateTenantId(result.TenantId);
		}

		protected abstract void AddAditionalRequestParameters(RequestParameters requestParameters);

		protected virtual async Task<AuthenticationResult> SendTokenRequestAsync()
		{
			RequestParameters requestParameters = new RequestParameters(Resource, ClientKey);
			AddAditionalRequestParameters(requestParameters);
			return await SendHttpMessageAsync(requestParameters);
		}

		protected async Task<AuthenticationResult> SendTokenRequestByRefreshTokenAsync(string refreshToken)
		{
			return await SendHttpMessageAsync(new RequestParameters(Resource, ClientKey)
			{
				["grant_type"] = "refresh_token",
				["refresh_token"] = refreshToken
			});
		}

		private async Task<AuthenticationResult> RefreshAccessTokenAsync(AuthenticationResult result)
		{
			AuthenticationResult newResult = null;
			if (Resource != null)
			{
				ADALLogger.Verbose(CallState, "Refreshing access token...");
				try
				{
					newResult = await SendTokenRequestByRefreshTokenAsync(result.RefreshToken);
					Authenticator.UpdateTenantId(result.TenantId);
					if (newResult.IdToken != null)
					{
						return newResult;
					}
					newResult.UpdateTenantAndUserInfo(result.TenantId, result.IdToken, result.UserInfo);
					return newResult;
				}
				catch (AdalException ex)
				{
					ADALLogger.Error(CallState, ex);
					AdalServiceException ex2 = ex as AdalServiceException;
					if (ex2 != null && ex2.ErrorCode == "invalid_request")
					{
						throw new AdalServiceException("failed_to_refresh_token", "Failed to refresh token. " + ex2.Message, ex2.ServiceErrorCodes, (WebException)ex2.InnerException);
					}
					RefreshException = ex;
					return null;
				}
			}
			return newResult;
		}

		private async Task<AuthenticationResult> SendHttpMessageAsync(RequestParameters requestParameters)
		{
			string uri = HttpHelper.CheckForExtraQueryParameter(Authenticator.TokenUri);
			TokenResponse tokenResponse = await HttpHelper.SendPostRequestAndDeserializeJsonResponseAsync<TokenResponse>(uri, requestParameters, CallState);
			AuthenticationResult result = OAuth2Response.ParseTokenResponse(tokenResponse, CallState);
			if (result.RefreshToken == null && requestParameters.ContainsKey("refresh_token"))
			{
				result.RefreshToken = requestParameters["refresh_token"];
				ADALLogger.Verbose(CallState, "Refresh token was missing from the token refresh response, so the refresh token in the request is returned instead");
			}
			result.IsMultipleResourceRefreshToken = (!string.IsNullOrWhiteSpace(result.RefreshToken) && !string.IsNullOrWhiteSpace(tokenResponse.Resource));
			return result;
		}

		private void NotifyBeforeAccessCache()
		{
			tokenCache.OnBeforeAccess(new TokenCacheNotificationArgs
			{
				TokenCache = tokenCache,
				Resource = Resource,
				ClientId = ClientKey.ClientId,
				UniqueId = UniqueId,
				DisplayableId = DisplayableId
			});
		}

		private void NotifyAfterAccessCache()
		{
			tokenCache.OnAfterAccess(new TokenCacheNotificationArgs
			{
				TokenCache = tokenCache,
				Resource = Resource,
				ClientId = ClientKey.ClientId,
				UniqueId = UniqueId,
				DisplayableId = DisplayableId
			});
		}

		private void LogReturnedToken(AuthenticationResult result)
		{
			if (result.AccessToken != null)
			{
				//string text = PlatformSpecificHelper.CreateSha256Hash(result.AccessToken);
				//string text2 = (result.RefreshToken == null) ? "[No Refresh Token]" : PlatformSpecificHelper.CreateSha256Hash(result.RefreshToken);
				ADALLogger.Information(CallState, "=== Token Acquisition finished successfully.");
			}
		}

		private void ValidateAuthorityType()
		{
			if (!SupportADFS && Authenticator.AuthorityType == AuthorityType.ADFS)
			{
				throw new AdalException("invalid_authority_type", string.Format(CultureInfo.InvariantCulture, "This method overload is not supported by '{0}'", new object[1]
				{
					Authenticator.Authority
				}));
			}
		}
	}
}