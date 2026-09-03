export default {
    getContext() {
        return {
            createScheduleUrl: "/api/PRSettingApi/CreatePRDisposeSchedule",
            updateScheduleUrl: "/api/PRSettingApi/UpdatePRDisposeSchedule",
            deleteScheduleUrl: "/api/PRSettingApi/DeletePRDisposeSchedule",
            breakScheduleUrl: "/api/PRSettingApi/BreakPRDisposeSchedule",
            scheduleType: 19,
            showScheduleSkipAction: true,
            noScheduleText: RMResx.RM_JS_ScheduleSetting_NoSchedule,
            scheduleSettingTitle: RMResx.RM_JS_SPS_EditTitle_ScheduleSetting,
            showNewIntervalSetting: true,
        };
    }
};