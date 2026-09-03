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
using Microsoft365.Authentication.ADAL.Internal;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// This class loads the assembly containing the authentication dialog classes and creates a new instance of an IWebUI.
	/// This class is necessary since there is a loose coupling between this assembly and the assembly containing Windows Forms 
	/// dependencies.
	/// </summary>
	internal class WebUIFactory : IWebUIFactory
	{

		private static MethodInfo dialogFactory;

		public static void ThrowIfUIAssemblyUnavailable()
		{
			InitializeFactoryMethod();
		}

		public IWebUI Create(PromptBehavior promptBehavior, object ownerWindow)
		{
			InitializeFactoryMethod();
			object[] parameters = new object[1]
			{
				promptBehavior
			};
			IWebUI webUI = (IWebUI)dialogFactory.Invoke(null, parameters);
			webUI.OwnerWindow = ownerWindow;
			return webUI;
		}

		private static void InitializeFactoryMethod()
		{
			if (!(null != dialogFactory))
			{
				string text = string.Format(CultureInfo.InvariantCulture, "Portal.ADAL.WindowsForms, Version={0}, Culture=neutral, PublicKeyToken=31bf3856ad364e35", new object[1]
				{
					AdalIdHelper.GetAdalVersion()
				});
				try
				{
					Assembly assembly = Assembly.Load(text);
					Type type = assembly.GetType("Portal.ADAL.Internal.BrowserDialogFactory");
					dialogFactory = type.GetMethod("CreateAuthenticationDialog", BindingFlags.Static | BindingFlags.NonPublic);
				}
				catch (FileNotFoundException innerException)
				{
					ThrowAssemlyLoadFailedException(text, innerException);
				}
				catch (FileLoadException innerException2)
				{
					ThrowAssemlyLoadFailedException(text, innerException2);
				}
			}
		}

		private static void ThrowAssemlyLoadFailedException(string webAuthenticationDialogAssemblyName, Exception innerException)
		{
			throw new AdalException("assembly_load_failed", string.Format(CultureInfo.InvariantCulture, "Loading an assembly required for interactive user authentication failed. Make sure assembly '{0}' exists", new object[1]
			{
				webAuthenticationDialogAssemblyName
			}), innerException);
		}
	}
}