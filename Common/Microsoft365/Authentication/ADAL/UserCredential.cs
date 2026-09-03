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
using Microsoft365.Authentication.Extension;

using System.Security;
using System;
namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// Credential used for integrated authentication on domain-joined machines.
	/// </summary>
	public sealed class UserCredential
	{
		/// <summary>
		/// Gets identifier of the user.
		/// </summary>
		public string UserName
		{
			get;
			internal set;
		}

		internal UserAuthType UserAuthType
		{
			get;
			private set;
		}

		internal SecureString SecurePassword
		{
			get;
			private set;
		}

		/// <summary>
		/// Constructor to create user credential. Using this constructor would imply integrated authentication with logged in user
		/// and it can only be used in domain joined scenarios.
		/// </summary>
		public UserCredential()
		{
			UserAuthType = UserAuthType.IntegratedAuth;
		}

		/// <summary>
		/// Constructor to create user  credential. Using this constructor would imply integrated authentication with logged in user
		/// and it can only be used in domain joined scenarios.
		/// </summary>
		/// <param name="userName">Identifier of the user application requests token on behalf.</param>
		public UserCredential(string userName)
		{
			UserName = userName;
			UserAuthType = UserAuthType.IntegratedAuth;
		}


		/// <summary>
		/// Constructor to create credential
		/// </summary>
		/// <param name="userName">Identifier of the user application requests token on behalf.</param>
		public UserCredential(string userName, SecureString securePassword)
		{
			UserName = userName;
			SecurePassword = securePassword;
			UserAuthType = UserAuthType.UsernamePassword;
		}

		internal char[] PasswordToCharArray()
		{
			if (SecurePassword != null)
			{
				return SecurePassword.ToCharArray();
			}
			return null;
		}

		internal string GenerateUniqueId()
		{
			int passwordHash = 0;
			if (SecurePassword != null)
			{
				passwordHash= SecurePassword.GetHashCodeV1();
			}
			return $"{UserName}_{passwordHash}";
		}
	}
}