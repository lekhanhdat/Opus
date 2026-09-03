export default {
    getContext() {
        return {
            createScheduleUrl: "/api/OneDriveSettingApi/CreateDisposeSchedule",
            updateScheduleUrl: "/api/OneDriveSettingApi/UpdateDisposeSchedule",
            deleteScheduleUrl: "/api/OneDriveSettingApi/DeleteDisposeSchedule",
            breakScheduleUrl: "/api/OneDriveSettingApi/BreakDisposeSchedule",
            scheduleType: 30,
            showScheduleSkipAction: true,
            showScheduleUseDecrypt: RM.gData.enableRecordsArchiver,
            noScheduleText: RMResx.RM_JS_ScheduleSetting_NoSchedule,
            scheduleSettingTitle: RMResx.RM_JS_SPS_EditTitle_ScheduleSetting,
            showNewIntervalSetting: true,
        };
    }
};