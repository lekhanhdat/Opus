# /********************************************************************
# *
# *  PROPRIETARY and CONFIDENTIAL
# *
# *  This file is licensed from, and is a trade secret of:
# *
# *                   AvePoint, Inc.
# *                   525 Washington Blvd, Suite 1400
# *                   Jersey City, NJ 07310
# *                   United States of America
# *                   Telephone: +1-201-793-1111
# *                   WWW: www.avepoint.com
# *
# *  Refer to your License Agreement for restrictions on use,
# *  duplication, or disclosure.
# *
# *  RESTRICTED RIGHTS LEGEND
# *
# *  Use, duplication, or disclosure by the Government is
# *  subject to restrictions as set forth in subdivision
# *  (c)(1)(ii) of the Rights in Technical Data and Computer
# *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
# *  FAR 52.227-19 (C) (June 1987).
# *
# *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
# *
# *  Unpublished - All rights reserved under the copyright laws of the United States.
# */
$variable = "APEG_Build_Number"
$git_api_token = $git_api_token

$var = $((Get-Variable -Name "$variable").Value)

$varCurrentNum = $var

"##"
"## Before value is $var"
"##"

$var = [Int]$var + 1

$header = @{
 "PRIVATE-TOKEN" = $git_api_token
} 

Invoke-WebRequest -Uri ("https://gitlab.avepoint.net/api/v4/projects/$CI_PROJECT_ID/variables/{0}?value={1}" -f $variable,$var) -Method Put -Headers $header -ErrorAction Continue -UseBasicParsing

$res = Invoke-WebRequest -Uri ("https://gitlab.avepoint.net/api/v4/projects/$CI_PROJECT_ID/variables/{0}" -f $variable) -Headers $header -UseBasicParsing | ConvertFrom-Json
"##"
"## Update result:"
"##"
$res