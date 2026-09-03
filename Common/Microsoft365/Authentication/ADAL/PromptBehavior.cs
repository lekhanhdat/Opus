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
namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// Indicates whether AcquireToken should automatically prompt only if necessary or whether
	/// it should prompt regardless of whether there is a cached token.
	/// </summary>
	public enum PromptBehavior
	{
		/// <summary>
		/// Acquire token will prompt the user for credentials only when necessary.  If a token
		/// that meets the requirements is already cached then the user will not be prompted.
		/// </summary>
		Auto,
		/// <summary>
		/// The user will be prompted for credentials even if there is a token that meets the requirements
		/// already in the cache.
		/// </summary>
		Always,
		/// <summary>
		/// The user will not be prompted for credentials.  If prompting is necessary then the AcquireToken request
		/// will fail.
		/// </summary>
		Never,
		/// <summary>
		/// Re-authorizes (through displaying webview) the resource usage, making sure that the resulting access
		/// token contains updated claims. If user logon cookies are available, the user will not be asked for 
		/// credentials again and the logon dialog will dismiss automatically.
		/// </summary>
		RefreshSession
	}
}