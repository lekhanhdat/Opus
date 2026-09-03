export default {
    getContext() {
        return {
            createScheduleUrl: "/api/GoogleDriveSettingApi/CreateDisposeSchedule",
            updateScheduleUrl: "/api/GoogleDriveSettingApi/UpdateDisposeSchedule",
            deleteScheduleUrl: "/api/GoogleDriveSettingApi/DeleteDisposeSchedule",
            breakScheduleUrl: "/api/GoogleDriveSettingApi/BreakDisposeSchedule",
            scheduleType: 62,
            noScheduleText: RMResx.RM_JS_ScheduleSetting_NoSchedule,
            scheduleSettingTitle: RMResx.RM_JS_SPS_EditTitle_ScheduleSetting,
            showNewIntervalSetting: true,
        };
    }
};