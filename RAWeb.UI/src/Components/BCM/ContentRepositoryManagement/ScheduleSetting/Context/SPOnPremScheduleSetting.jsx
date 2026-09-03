export default {
    getContext() {
        return {
            createScheduleUrl: "/api/SPOnPremSettingApi/CreateDisposeSchedule",
            updateScheduleUrl: "/api/SPOnPremSettingApi/UpdateDisposeSchedule",
            deleteScheduleUrl: "/api/SPOnPremSettingApi/DeleteDisposeSchedule",
            breakScheduleUrl: "/api/SPOnPremSettingApi/BreakDisposeSchedule",
            scheduleType: 25,
            showScheduleSkipAction: false,
            noScheduleText: RMResx.RM_JS_ScheduleSetting_NoSchedule,
            scheduleSettingTitle: RMResx.RM_JS_SPS_EditTitle_ScheduleSetting,
            showNewIntervalSetting: true,
        };
    }
};