export default {
    getContext() {
        return {
            createScheduleUrl: "/api/SPSettingApi/CreateDisposeSchedule",
            updateScheduleUrl: "/api/SPSettingApi/UpdateDisposeSchedule",
            deleteScheduleUrl: "/api/SPSettingApi/DeleteDisposeSchedule",
            breakScheduleUrl: "/api/SPSettingApi/BreakDisposeSchedule",
            scheduleType: 37,
            showScheduleSkipAction: false,
            noScheduleText: RMResx.RM_AR_ScheduleSetting_NoSchedule,
            scheduleSettingTitle: RMResx.RM_AR_SPS_EditTitle_ScheduleSetting,
            showNewIntervalSetting: false,
        };
    }
};