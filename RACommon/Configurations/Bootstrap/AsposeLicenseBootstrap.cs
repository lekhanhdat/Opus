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
namespace AvePoint.RA.Common.Configurations.Bootstrap;

using System.IO;
using System.Text;

public static class AsposeLicenseBootstrap
{
    private static bool IsLicenseSetup = false;

    public static void Setup()
    {
        if(IsLicenseSetup)
        {
            return;
        }
        // var asposeZipLicense = new Aspose.Zip.License();
        // using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(LicenseInfo)))
        // {
        // asposeZipLicense.SetLicense(ms);
        // }
        var asposeEmailLicense = new Aspose.Email.License();
        using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(LicenseInfo)))
        {
            asposeEmailLicense.SetLicense(ms);
        }

        IsLicenseSetup = true;
    }
    private const string LicenseInfo = @"<License>
  <Data>
    <LicensedTo>AvePoint</LicensedTo>
    <EmailTo>it_billing@avepoint.com</EmailTo>
    <LicenseType>Developer OEM</LicenseType>
    <LicenseNote>1 Developer And Unlimited Deployment Locations</LicenseNote>
    <OrderID>260608205606</OrderID>
    <UserID>336519</UserID>
    <OEM>This is a redistributable license</OEM>
    <Products>
      <Product>Aspose.Total for .NET</Product>
    </Products>
    <EditionType>Professional</EditionType>
    <SerialNumber>18f7d596-6e11-4df0-976e-a01a96ece318</SerialNumber>
    <SubscriptionExpiry>20270611</SubscriptionExpiry>
    <LicenseVersion>3.0</LicenseVersion>
    <LicenseInstructions>https://purchase.aspose.com/policies/use-license</LicenseInstructions>
  </Data>
  <Signature>YpNdb/uqR5U6DrBiqw1HljYggDXO3+8qA3MYI5FRMDYbYKYrcd/IviUYgJEJ+0EIp9jtvEo+xzGgtnnA8nKKjdfU9hgIQ4L2xU/bIuGUkD4Y+IvdNHsC/XfeaastEOcMr8F0vdUMoJ1p1xtbe1m/e4muys6pM+dyalp+HWrMnD9dOfHaYL4ROd7RZ55tmsTVR6+eh3PTglG+48ZKY+4ZefZGj1gEZp9dZKC9IQ+JnrT/1BFzCKFUa0V0ilDTrgUHqbwZqvpTZqKrUK7/9cszU0UZhtf9r4iVjsUTZyPvz/myy1KpZ7Y4bXYPW6kXSiRXijGMjdHstGj0A+h7fNyUGQ==</Signature>
</License>";


}
