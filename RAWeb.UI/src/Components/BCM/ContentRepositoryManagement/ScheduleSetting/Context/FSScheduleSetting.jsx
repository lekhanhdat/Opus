export default {
    getContext() {
        return {
            createScheduleUrl: "/api/FSSettingApi/CreateFSDisposeSchedule",
            updateScheduleUrl: "/api/FSSettingApi/UpdateFSDisposeSchedule",
            deleteScheduleUrl: "/api/FSSettingApi/DeleteFSDisposeSchedule",
            breakScheduleUrl: "/api/FSSettingApi/BreakFSDisposeSchedule",
            scheduleType: 22,
            showScheduleSkipAction: false,
            noScheduleText: RMResx.RM_JS_ScheduleSetting_NoSchedule,
            scheduleSettingTitle: RMResx.RM_JS_SPS_EditTitle_ScheduleSetting,
            showNewIntervalSetting: true,
        };
    }
};