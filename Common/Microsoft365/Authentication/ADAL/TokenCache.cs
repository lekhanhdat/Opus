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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// Token cache class used by <see cref="T:Portal.ADAL.AuthenticationContext" /> to store access and refresh tokens.
	/// </summary>
	public class TokenCache
	{
		internal delegate Task<AuthenticationResult> RefreshAccessTokenAsync(AuthenticationResult result, string resource, ClientKey clientKey, CallState callState);

		internal readonly IDictionary<TokenCacheKey, AuthenticationResult> tokenCacheDictionary;

		private volatile bool hasStateChanged;

		private readonly object cacheLock = new object();

		/// <summary>
		/// Static token cache shared by all instances of AuthenticationContext which do not explicitly pass a cache instance during construction.
		/// </summary>
		public static TokenCache DefaultShared
		{
			get;
			private set;
		}

		/// <summary>
		/// Notification method called before any library method accesses the cache.
		/// </summary>
		public TokenCacheNotification BeforeAccess
		{
			get;
			set;
		}

		/// <summary>
		/// Notification method called before any library method writes to the cache. This notification can be used to reload
		/// the cache state from a row in database and lock that row. That database row can then be unlocked in <see cref="P:Portal.ADAL.TokenCache.AfterAccess" /> notification.
		/// </summary>
		public TokenCacheNotification BeforeWrite
		{
			get;
			set;
		}

		/// <summary>
		/// Notification method called after any library method accesses the cache.
		/// </summary>
		public TokenCacheNotification AfterAccess
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the flag indicating whether cache state has changed. ADAL methods set this flag after any change. Caller application should reset 
		/// the flag after serializing and persisting the state of the cache.
		/// </summary>
		public bool HasStateChanged
		{
			get
			{
				lock (cacheLock)
				{
					return hasStateChanged;
				}
			}
			set
			{
				lock (cacheLock)
				{
					hasStateChanged = value;
				}
			}
		}

		/// <summary>
		/// Gets the nunmber of items in the cache.
		/// </summary>
		public int Count
		{
			get
			{
				lock (cacheLock)
				{
					return tokenCacheDictionary.Count;
				}
			}
		}

		static TokenCache()
		{
			DefaultShared = new TokenCache();
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		public TokenCache()
		{
			tokenCacheDictionary = new ConcurrentDictionary<TokenCacheKey, AuthenticationResult>();
		}

		/// <summary>
		/// Constructor receiving state of the cache
		/// </summary>        
		public TokenCache(byte[] state)
			: this()
		{
			Deserialize(state);
		}

		/// <summary>
		/// Serializes current state of the cache as a blob. Caller application can persist the blob and update the state of the cache later by 
		/// passing that blob back in constructor or by calling method Deserialize.
		/// </summary>
		/// <returns>Current state of the cache as a blob</returns>
		public byte[] Serialize()
		{
			lock (cacheLock)
			{
				using (Stream stream = new MemoryStream())
				{
					BinaryWriter binaryWriter = new BinaryWriter(stream);
					binaryWriter.Write(2);
					ADALLogger.Information(null, "Serializing token cache with {0} items.", tokenCacheDictionary.Count);
					binaryWriter.Write(tokenCacheDictionary.Count);
					foreach (KeyValuePair<TokenCacheKey, AuthenticationResult> item in tokenCacheDictionary)
					{
						binaryWriter.Write(string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}", ":::", item.Key.Authority, item.Key.Resource, item.Key.ClientId, (int)item.Key.TokenSubjectType));
						binaryWriter.Write(item.Value.Serialize());
					}
					int count = (int)stream.Position;
					stream.Position = 0L;
					BinaryReader binaryReader = new BinaryReader(stream);
					return binaryReader.ReadBytes(count);
				}
			}
		}

		/// <summary>
		/// Deserializes state of the cache. The state should be the blob received earlier by calling the method Serialize.
		/// </summary>
		/// <param name="state">State of the cache as a blob</param>
		public void Deserialize(byte[] state)
		{
			lock (cacheLock)
			{
				if (state == null)
				{
					tokenCacheDictionary.Clear();
				}
				else
				{
					using (Stream stream = new MemoryStream())
					{
						BinaryWriter binaryWriter = new BinaryWriter(stream);
						binaryWriter.Write(state);
						binaryWriter.Flush();
						stream.Position = 0L;
						BinaryReader binaryReader = new BinaryReader(stream);
						int num = binaryReader.ReadInt32();
						if (num != 2)
						{
							ADALLogger.Warning(null, "The version of the persistent state of the cache does not match the current schema, so skipping deserialization.");
						}
						else
						{
							tokenCacheDictionary.Clear();
							int num2 = binaryReader.ReadInt32();
							for (int i = 0; i < num2; i++)
							{
								string text = binaryReader.ReadString();
								string[] array = text.Split(new string[1]
								{
									":::"
								}, StringSplitOptions.None);
								AuthenticationResult authenticationResult = AuthenticationResult.Deserialize(binaryReader.ReadString());
								TokenCacheKey key = new TokenCacheKey(array[0], array[1], array[2], (TokenSubjectType)int.Parse(array[3], CultureInfo.InvariantCulture), authenticationResult.UserInfo);
								tokenCacheDictionary.Add(key, authenticationResult);
							}
							ADALLogger.Information(null, "Deserialized {0} items to token cache.", num2);
						}
					}
				}
			}
		}

		/// <summary>
		/// Reads a copy of the list of all items in the cache. 
		/// </summary>
		/// <returns>The items in the cache</returns>
		public virtual IEnumerable<TokenCacheItem> ReadItems()
		{
			lock (cacheLock)
			{
				TokenCacheNotificationArgs tokenCacheNotificationArgs = new TokenCacheNotificationArgs();
				tokenCacheNotificationArgs.TokenCache = this;
				TokenCacheNotificationArgs args = tokenCacheNotificationArgs;
				OnBeforeAccess(args);
				List<TokenCacheItem> list = new List<TokenCacheItem>();
				foreach (KeyValuePair<TokenCacheKey, AuthenticationResult> item in tokenCacheDictionary)
				{
					list.Add(new TokenCacheItem(item.Key, item.Value));
				}
				OnAfterAccess(args);
				return list;
			}
		}

		/// <summary>
		/// Deletes an item from the cache.
		/// </summary>
		/// <param name="item">The item to delete from the cache</param>
		public virtual void DeleteItem(TokenCacheItem item)
		{
			lock (cacheLock)
			{
				if (item == null)
				{
					throw new ArgumentNullException("item");
				}
				TokenCacheNotificationArgs tokenCacheNotificationArgs = new TokenCacheNotificationArgs();
				tokenCacheNotificationArgs.TokenCache = this;
				tokenCacheNotificationArgs.Resource = item.Resource;
				tokenCacheNotificationArgs.ClientId = item.ClientId;
				tokenCacheNotificationArgs.UniqueId = item.UniqueId;
				tokenCacheNotificationArgs.DisplayableId = item.DisplayableId;
				TokenCacheNotificationArgs args = tokenCacheNotificationArgs;
				OnBeforeAccess(args);
				OnBeforeWrite(args);
				TokenCacheKey tokenCacheKey = tokenCacheDictionary.Keys.FirstOrDefault(item.Match);
				if (tokenCacheKey != null)
				{
					tokenCacheDictionary.Remove(tokenCacheKey);
					ADALLogger.Information(null, "One item removed successfully");
				}
				else
				{
					ADALLogger.Information(null, "Item not Present in the Cache");
				}
				HasStateChanged = true;
				OnAfterAccess(args);
			}
		}

		/// <summary>
		/// Clears the cache by deleting all the items. Note that if the cache is the default shared cache, clearing it would
		/// impact all the instances of <see cref="T:Portal.ADAL.AuthenticationContext" /> which share that cache.
		/// </summary>
		public virtual void Clear()
		{
			lock (cacheLock)
			{
				TokenCacheNotificationArgs tokenCacheNotificationArgs = new TokenCacheNotificationArgs();
				tokenCacheNotificationArgs.TokenCache = this;
				TokenCacheNotificationArgs args = tokenCacheNotificationArgs;
				OnBeforeAccess(args);
				OnBeforeWrite(args);
				ADALLogger.Information(null, string.Format(CultureInfo.InvariantCulture, "Clearing Cache :- {0} items to be removed", new object[1]
				{
					Count
				}));
				tokenCacheDictionary.Clear();
				ADALLogger.Information(null, "Successfully Cleared Cache");
				HasStateChanged = true;
				OnAfterAccess(args);
			}
		}

		internal void OnAfterAccess(TokenCacheNotificationArgs args)
		{
			lock (cacheLock)
			{
				if (AfterAccess != null)
				{
					AfterAccess(args);
				}
			}
		}

		internal void OnBeforeAccess(TokenCacheNotificationArgs args)
		{
			lock (cacheLock)
			{
				if (BeforeAccess != null)
				{
					BeforeAccess(args);
				}
			}
		}

		internal void OnBeforeWrite(TokenCacheNotificationArgs args)
		{
			lock (cacheLock)
			{
				if (BeforeWrite != null)
				{
					BeforeWrite(args);
				}
			}
		}

		internal AuthenticationResult LoadFromCache(CacheQueryData cacheQueryData, CallState callState)
		{
			lock (cacheLock)
			{
				ADALLogger.Verbose(callState, "Looking up cache for a token...");
				AuthenticationResult authenticationResult = null;
				KeyValuePair<TokenCacheKey, AuthenticationResult>? keyValuePair = LoadSingleItemFromCache(cacheQueryData, callState);
				if (keyValuePair.HasValue)
				{
					TokenCacheKey key = keyValuePair.Value.Key;
					authenticationResult = keyValuePair.Value.Value;
					if (authenticationResult.ExpiresOn <= DateTime.UtcNow + TimeSpan.FromMinutes(5.0))
					{
						authenticationResult.AccessToken = null;
						ADALLogger.Verbose(callState, "An expired or near expiry token was found in the cache");
					}
					else if (!key.ResourceEquals(cacheQueryData.Resource))
					{
						ADALLogger.Verbose(callState, string.Format(CultureInfo.InvariantCulture, "Multi resource refresh token for resource '{0}' will be used to acquire token for '{1}'", new object[2]
						{
							key.Resource,
							cacheQueryData.Resource
						}));
						AuthenticationResult authenticationResult2 = new AuthenticationResult(null, null, authenticationResult.RefreshToken, DateTimeOffset.MinValue);
						authenticationResult2.UpdateTenantAndUserInfo(authenticationResult.TenantId, authenticationResult.IdToken, authenticationResult.UserInfo);
						authenticationResult = authenticationResult2;
					}
					else
					{
						ADALLogger.Verbose(callState, string.Format(CultureInfo.InvariantCulture, "{0} minutes left until token in cache expires", new object[1]
						{
							(authenticationResult.ExpiresOn - DateTime.UtcNow).TotalMinutes
						}));
					}
					if (authenticationResult.AccessToken == null && authenticationResult.RefreshToken == null)
					{
						tokenCacheDictionary.Remove(key);
						ADALLogger.Information(callState, "An old item was removed from the cache");
						HasStateChanged = true;
						authenticationResult = null;
					}
					if (authenticationResult != null)
					{
						ADALLogger.Information(callState, "A matching item (access token or refresh token or both) was found in the cache");
					}
				}
				else
				{
					ADALLogger.Information(callState, "No matching token was found in the cache");
				}
				return authenticationResult;
			}
		}

		internal void StoreToCache(AuthenticationResult result, string authority, string resource, string clientId, TokenSubjectType subjectType,string cacheQueryUniqueId,string cacheDisplayableId, CallState callState)
		{
			lock (cacheLock)
			{
				ADALLogger.Verbose(callState, "Storing token in the cache...");
				OnBeforeWrite(new TokenCacheNotificationArgs
				{
					Resource = resource,
					ClientId = clientId,
					UniqueId = cacheQueryUniqueId,
					DisplayableId = cacheDisplayableId
				});
				TokenCacheKey key = new TokenCacheKey(authority, resource, clientId, subjectType, cacheQueryUniqueId, cacheDisplayableId);
				tokenCacheDictionary[key] = result;
				ADALLogger.Verbose(callState, "An item was stored in the cache");
				UpdateCachedMrrtRefreshTokens(result, authority, clientId, subjectType);
				HasStateChanged = true;
			}
		}

		private void UpdateCachedMrrtRefreshTokens(AuthenticationResult result, string authority, string clientId, TokenSubjectType subjectType)
		{
			lock (cacheLock)
			{
				if (result.UserInfo != null && result.IsMultipleResourceRefreshToken)
				{
					List<KeyValuePair<TokenCacheKey, AuthenticationResult>> list = (from p in QueryCache(authority, clientId, subjectType, result.UserInfo.UniqueId, result.UserInfo.DisplayableId)
					where p.Value.IsMultipleResourceRefreshToken
					select p).ToList();
					foreach (KeyValuePair<TokenCacheKey, AuthenticationResult> item in list)
					{
						item.Value.RefreshToken = result.RefreshToken;
					}
				}
			}
		}

		private KeyValuePair<TokenCacheKey, AuthenticationResult>? LoadSingleItemFromCache(CacheQueryData cacheQueryData, CallState callState)
		{
			lock (cacheLock)
			{
				List<KeyValuePair<TokenCacheKey, AuthenticationResult>> source = QueryCache(cacheQueryData.Authority, cacheQueryData.ClientId, cacheQueryData.SubjectType, cacheQueryData.UniqueId, cacheQueryData.DisplayableId);
				List<KeyValuePair<TokenCacheKey, AuthenticationResult>> source2 = (from p in source
				where p.Key.ResourceEquals(cacheQueryData.Resource)
				select p).ToList();
				int num = source2.Count();
				if (num > 1 && cacheQueryData.AssertionHash != null)
				{
					source2 = (from p in source2
					where p.Value.UserAssertionHash.Equals(cacheQueryData.AssertionHash)
					select p).ToList();
					num = source2.Count();
				}
				KeyValuePair<TokenCacheKey, AuthenticationResult>? result = null;
				switch (num)
				{
				case 1:
					ADALLogger.Information(callState, "An item matching the requested resource was found in the cache");
					result = source2.First();
					break;
				case 0:
				{
					List<KeyValuePair<TokenCacheKey, AuthenticationResult>> source3 = (from p in source
					where p.Value.IsMultipleResourceRefreshToken
					select p).ToList();
					if (source3.Any())
					{
						result = source3.First();
						ADALLogger.Information(callState, "A Multi Resource Refresh Token for a different resource was found which can be used");
					}
					break;
				}
				default:
					throw new AdalException("multiple_matching_tokens_detected");
				}
				return result;
			}
		}

		/// <summary>
		/// Queries all values in the cache that meet the passed in values, plus the 
		/// authority value that this AuthorizationContext was created with.  In every case passing
		/// null results in a wildcard evaluation.
		/// </summary>
		private List<KeyValuePair<TokenCacheKey, AuthenticationResult>> QueryCache(string authority, string clientId, TokenSubjectType subjectType, string uniqueId, string displayableId)
		{
			lock (cacheLock)
			{
				return tokenCacheDictionary.Where(delegate(KeyValuePair<TokenCacheKey, AuthenticationResult> p)
				{
					if (p.Key.Authority == authority && (string.IsNullOrWhiteSpace(clientId) || p.Key.ClientIdEquals(clientId)) && (string.IsNullOrWhiteSpace(uniqueId) || p.Key.UniqueId == uniqueId) && (string.IsNullOrWhiteSpace(displayableId) || p.Key.DisplayableIdEquals(displayableId)))
					{
						return p.Key.TokenSubjectType == subjectType;
					}
					return false;
				}).ToList();
			}
		}
	}
}