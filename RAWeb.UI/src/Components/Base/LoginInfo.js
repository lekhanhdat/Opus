//Login related information
const disabledGainsightEnv = new Set(["production", "test", "GCP", "GCP Test"]);

const LoginInfo = (callback) => {
    let option = {
        url: "/api/HomeApi/GetCurrentUserInfo",
        method: "POST",
    };
    $$.loading(true);
    fetchUtility(option).then(function (result) {
        $$.loading(false);
        let loginInfo = JSON.parse(result);
        let userInfo = loginInfo.UserInfo;
        RM.gData = {
            logonGroupId: userInfo.LogonGroupId,
            company: userInfo.Company,
            accountNumber: userInfo.AccountNumber,
            dataCenter: loginInfo.DataCenter,
            userName: userInfo.UserName,
            userId: userInfo.UserId,
            emailAddress: userInfo.EmailAddress,
            isPhysicalAdmin: userInfo.IsPhysicalAdmin,
            productVersion: loginInfo.ProductVersion,
            copyright: loginInfo.Copyright,
            forwardToDAORC: loginInfo.ForwardToDAORC,
            currentLanguage: loginInfo.CurrentLanguage,
            enableRecordsArchiver: userInfo.EnableRecordsArchiver,
            enableDeleteOnly: userInfo.EnableDeleteOnly,
            enableArchiverLatestVersion: userInfo.EnableArchiverLatestVersion,
            enableArchiverVersionNotIncludeLatest: userInfo.EnableArchiverVersionNotIncludeLatest,
            hasIntelligentPermission:loginInfo.HasIntelligentPermission,
            enviromentName: loginInfo.EnviromentName,
            disabledGainsight: !disabledGainsightEnv.has(loginInfo.EnviromentName),
            hasRecordsLicense: userInfo.HasRecordsLicense,
            hasArchiverLicense: userInfo.HasArchiverLicense,
            hasFileSystemLicense: userInfo.HasFileSystemLicense,
            hasDiscoveryLicense: userInfo.HasDiscoveryLicense,
            hasDiscoverySalesforceLicense: userInfo.HasDiscoverySalesforceLicense,
            hasDiscoveryGoogleLicense: userInfo.HasDiscoveryGoogleLicense,
            hasDiscoveryFileSystemLicense: userInfo.HasDiscoveryFileSystemLicense,
            hasGoogleLicense: userInfo.HasGoogleLicense,
            HasDiscoveryExportRowData: userInfo.HasDiscoveryExportRowData,
            fileExtentionsConfig: loginInfo.FileExtentionsConfig,
            exportResultLimit: loginInfo.ExportResultLimit,
            enableCustomizationApp: userInfo.EnableCustomizationApp,
            licenseType: userInfo.LicenseType,
            useArchiverImportFile: userInfo.UseArchiverImportFile,
            disableRetentionPeriodLimitation: userInfo.DisableRetentionPeriodLimitation,
            enableFilelevelBackup: userInfo.EnableFilelevelBackup,
            enableDeleteOrphanData: userInfo.EnableDeleteOrphanData,
            enableSoftDelete: userInfo.EnableSoftDelete,
            accessToken: loginInfo.AccessToken,
            aosPortalURL: loginInfo.AOSPortalURL,
            chatBotApiURL: loginInfo.ChatBotApiURL,
            chatBotPortalURL: loginInfo.ChatBotPortalURL,
            diableChatBot: loginInfo.DisableChatBot,
            existAVAUser: loginInfo.ExistAVAUser,
            enableDeleteRestoredDataFeature: loginInfo.EnableDeleteRestoredDataFeature,
            enableApplySettingScanAll: userInfo.EnableApplySettingScanAll,
            hasUpgradeTeams: userInfo.HasUpgradeTeams,
            enableTeamsFeature: userInfo.EnableTeamsFeature,
            enableZeroShotFeature: userInfo.EnableZeroShotFeature,
            enableMachineLearningFeature: userInfo.EnableMachineLearningFeature,
            enableAIRecommendationFeature: userInfo.EnableAIRecommendationFeature,
            enableArchiverOnly: userInfo.EnableArchiverOnly,
            hasManageHold: userInfo.HasManageHold ?? userInfo.HasManagerHold ?? false,
            resCdnURL: loginInfo.CDNUrl,
            enableJPMCFileSystemFeature: userInfo.EnableJPMCFileSystemFeature,
            enableCustomRetentionSettings: userInfo.EnableCustomRetentionSettings,
            enableMultiGEOFeature: userInfo.EnableMultiGEOFeature,
            currentDC: userInfo.CurrentDC || "{}",
            isMultiGeoMainDC: userInfo.IsMultiGeoMainDC,
            // StartTime: "@DateTime.Now.Date.ToShortDateString() " +"@DateTime.Now.ToString("t")",
            // isExplorerDataUpgrade: "1" @*"@ViewData["ExplorerDataMoved"]"*@
        };
        RM.TimeSettingModel = loginInfo.TimeSettingModel ? JSON.parse(loginInfo.TimeSettingModel) : loginInfo.TimeSettingModel;
        RM.Permission = loginInfo.Permission ? JSON.parse(loginInfo.Permission) : loginInfo.Permission;
        RM.RoleType = userInfo.RoleType;
        RM.UserResources = loginInfo.UserResources ? JSON.parse(loginInfo.UserResources) : loginInfo.UserResources;
        RM.AvaliableSource = loginInfo.AvaliableSource ? JSON.parse(loginInfo.AvaliableSource) : loginInfo.AvaliableSource;
        RM.userGroup = userInfo.UserGroup ? JSON.parse(userInfo.UserGroup) : userInfo.UserGroup;
        callback();
    });
};

export default LoginInfo;