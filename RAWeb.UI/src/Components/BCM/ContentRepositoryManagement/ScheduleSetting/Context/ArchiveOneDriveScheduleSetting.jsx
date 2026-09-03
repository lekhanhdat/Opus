export default {
    getContext() {
        return {
            createScheduleUrl: "/api/OneDriveSettingApi/CreateDisposeSchedule",
            updateScheduleUrl: "/api/OneDriveSettingApi/UpdateDisposeSchedule",
            deleteScheduleUrl: "/api/OneDriveSettingApi/DeleteDisposeSchedule",
            breakScheduleUrl: "/api/OneDriveSettingApi/BreakDisposeSchedule",
            scheduleType: 38,
            showScheduleSkipAction: false,
            noScheduleText: RMResx.RM_AR_ScheduleSetting_NoSchedule,
            scheduleSettingTitle: RMResx.RM_AR_SPS_EditTitle_ScheduleSetting,
            showNewIntervalSetting: false,
        };
    }
};