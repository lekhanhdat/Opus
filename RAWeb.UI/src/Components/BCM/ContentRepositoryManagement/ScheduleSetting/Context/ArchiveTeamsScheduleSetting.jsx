export default {
    getContext() {
        return {
            createScheduleUrl: "/api/TeamsSettingApi/CreateDisposeSchedule",
            updateScheduleUrl: "/api/TeamsSettingApi/UpdateDisposeSchedule",
            deleteScheduleUrl: "/api/TeamsSettingApi/DeleteDisposeSchedule",
            breakScheduleUrl: "/api/TeamsSettingApi/BreakDisposeSchedule",
            scheduleType: 70,
            showScheduleSkipAction: false,
            noScheduleText: RMResx.RM_AR_ScheduleSetting_NoSchedule,
            scheduleSettingTitle: RMResx.RM_AR_SPS_EditTitle_ScheduleSetting,
            showNewIntervalSetting: false,
        };
    }
};