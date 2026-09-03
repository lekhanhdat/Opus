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
# runner runs on a detached HEAD, create a temporary local branch for editing\

$thisDate = get-date -Format yyyy-MM-dd-HH-mm-ss
$localBranchName = ("ci_processing{0}" -f $thisDate)

git clean -f
git checkout ${CI_BUILD_REF_NAME} -f
git fetch --all
git reset --hard origin/${CI_BUILD_REF_NAME}

git checkout -b $localBranchName
git config --global user.email "fpwang@avepoint.com"
git config --global user.name "Faping Wang"
git remote set-url --push origin "git@git.avepoint.net:bunty/reco.git"

# make your changes
cd "$PSScriptRoot\..\RAWeb"

npm install
npm run build

if(!$?){
    Write-Error("npm run build command error")
    exit 1
}

git status

git commit -am ("[RECO-0000] AUTO npm run build for resource files{0}" -f $thisDate)

# push changes
# always return true so that the build does not fail if there are no changes
git push origin ${localBranchName}:${CI_BUILD_REF_NAME}

if(!$?){
    Write-Error("git push command error")
    exit 1
}


