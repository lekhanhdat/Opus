export default {
    getContext() {
        return {
            createScheduleUrl: "/api/BoxSetting/CreateBoxDisposeSchedule",
            updateScheduleUrl: "/api/BoxSetting/UpdateBoxDisposeSchedule",
            deleteScheduleUrl: "/api/BoxSetting/DeleteBoxDisposeSchedule",
            breakScheduleUrl: "/api/BoxSetting/BreakBoxDisposeSchedule",
            scheduleType: 44,
            showScheduleSkipAction: false,
            noScheduleText: RMResx.RM_JS_ScheduleSetting_NoSchedule,
            scheduleSettingTitle: RMResx.RM_JS_SPS_EditTitle_ScheduleSetting,
            showNewIntervalSetting: true,
        };
    }
};