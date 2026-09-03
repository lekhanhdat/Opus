/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   3 Second Street, Suite 803
 *                   Jersey City, NJ 07311
 *                   United States of America
 *                   Telephone: +1-800-661-6588
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
 *  Copyright © AvePoint, Inc. 2010
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 *  $Revision: 23734 $
 *  $Author: elliu $        
 *  $Date: 2011-02-25 18:31:36 +0800 (Fri, 25 Feb 2011) $
 */

 1.	MediaStorage Solution的用法.
	(1) 只要引入工程 Media.Storage.csproj 即可， 其他功能不能引入
	(2)	在相应工程文件加入如下build脚本

	<PropertyGroup>
    <StorageHome>..\..\..\common\MediaStorage</StorageHome>
    
    <StorageProjects>
      $(StorageHome)\Media.Storage.FS\Media.Storage.FS.csproj;
      $(StorageHome)\Media.Storage.FTP\Media.Storage.FTP.csproj;
      $(StorageHome)\Media.Storage.TSM\Media.Storage.TSM.csproj;
      $(StorageHome)\Media.Storage.Centera\Media.Storage.Centera.csproj;
      $(StorageHome)\Media.Storage.Cloud.Common\Media.Storage.Cloud.Common.csproj;
      $(StorageHome)\Media.Storage.Cloud.Rackspace\Media.Storage.Cloud.Rackspace.csproj;
      $(StorageHome)\Media.Storage.Cloud.Azure\Media.Storage.Cloud.Azure.csproj;
      $(StorageHome)\Media.Storage.Cloud.Amazon\Media.Storage.Cloud.Amazon.csproj;
      $(StorageHome)\Media.Storage.Cloud.Atmos\Media.Storage.Cloud.Atmos.csproj
    </StorageProjects>
    <StorageAssembles>
      $(StorageHome)\Media.Storage\bin\$(Configuration)\StorageFS.dll;
      $(StorageHome)\Media.Storage\bin\$(Configuration)\StorageFTP.dll;
      $(StorageHome)\Media.Storage\bin\$(Configuration)\StorageTSM.dll;
      $(StorageHome)\Media.Storage\bin\$(Configuration)\StorageCentera.dll;
      $(StorageHome)\Media.Storage\bin\$(Configuration)\StorageCloudCommon.dll;
      $(StorageHome)\Media.Storage\bin\$(Configuration)\StorageCloudRackspace.dll;
      $(StorageHome)\Media.Storage\bin\$(Configuration)\StorageCloudAzure.dll;
      $(StorageHome)\Media.Storage\bin\$(Configuration)\StorageCloudAmazon.dll;
      $(StorageHome)\Media.Storage\bin\$(Configuration)\StorageCloudAtmos.dll
    </StorageAssembles>
  </PropertyGroup>
  <Target Name="AfterBuild">
    <Message Text="$(StorageProjects)" Importance="high"/>
    <MSBuild
          Projects="$(StorageProjects)"
          ContinueOnError="true"
          Targets="Rebuild"
          Properties="Configuration=$(Configuration)" TargetAndPropertyListSeparators="false" >
    </MSBuild>
    <CallTarget Targets="CopyStorageDlls"/>
  </Target>

  <Target Name="CopyStorageDlls">
    <Message Importance="high" Text="$(StorageAssembles)"/>
    <Copy ContinueOnError="true" SourceFiles="$(StorageAssembles)" DestinationFolder="$(OutputPath)"/>
  </Target>
  


2.	TSM
调试的时候需要把 Media.Storage.TSM.API这个c++工程build成相应版本, 比如 64bit就Build成x64, 32bit就Build成x86的然后把
按如下copy
x64
	StorageTSMAPI.dll
	Media.Storage.TSM.API\dll\x64\下所有文件 拷贝到相应bin目录
x86
	StorageTSMAPI.dll
	Media.Storage.TSM.API\dll\x86\下所有文件 宝贝到相应bin目录
然后把Media.Storage.TSM.API\api下所有文件拷贝到相关bin目录下
dsm目录下



3.	EMC Centera
所需第三方文件按如下方式拷贝
x64
	Media.Storage.Centera\ThirdDLL\x64\下所有文件拷贝到相应bin目录下
x86
	Media.Storage.Centera\ThirdDLL\x86\下所有文件拷贝到相应bin目录下
	

