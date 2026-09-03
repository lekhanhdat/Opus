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

namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// Contains identifier for a user.
	/// </summary>
	public sealed class UserIdentifier
	{

		private static readonly UserIdentifier AnyUserSingleton = new UserIdentifier("AnyUser", UserIdentifierType.UniqueId);

		/// <summary>
		/// Gets type of the <see cref="T:Portal.ADAL.UserIdentifier" />.
		/// </summary>
		public UserIdentifierType Type
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets Id of the <see cref="T:Portal.ADAL.UserIdentifier" />.
		/// </summary>
		public string Id
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets an static instance of <see cref="T:Portal.ADAL.UserIdentifier" /> to represent any user.
		/// </summary>
		public static UserIdentifier AnyUser => AnyUserSingleton;

		internal bool IsAnyUser
		{
			get
			{
				if (Type == AnyUser.Type)
				{
					return Id == AnyUser.Id;
				}
				return false;
			}
		}

		internal string UniqueId
		{
			get
			{
				if (IsAnyUser || Type != 0)
				{
					return null;
				}
				return Id;
			}
		}

		internal string DisplayableId
		{
			get
			{
				if (IsAnyUser || (Type != UserIdentifierType.OptionalDisplayableId && Type != UserIdentifierType.RequiredDisplayableId))
				{
					return null;
				}
				return Id;
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="type"></param>
		public UserIdentifier(string id, UserIdentifierType type)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				throw new ArgumentNullException("id");
			}
			Id = id;
			Type = type;
		}
	}
}