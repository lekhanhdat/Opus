export default {
    getContext() {
        return {
            createScheduleUrl: "/api/EXOSettingApi/CreateEXODisposeSchedule",
            updateScheduleUrl: "/api/EXOSettingApi/UpdateEXODisposeSchedule",
            deleteScheduleUrl: "/api/EXOSettingApi/DeleteEXODisposeSchedule",
            breakScheduleUrl: "/api/EXOSettingApi/BreakEXODisposeSchedule",
            scheduleType: 14,
            showScheduleSkipAction: true,
            noScheduleText: RMResx.RM_JS_ScheduleSetting_NoSchedule,
            scheduleSettingTitle: RMResx.RM_JS_SPS_EditTitle_ScheduleSetting,
            showNewIntervalSetting: true,
        };
    }
};