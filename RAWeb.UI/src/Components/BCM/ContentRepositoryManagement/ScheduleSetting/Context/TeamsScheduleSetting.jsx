export default {
    getContext() {
        return {
            createScheduleUrl: "/api/TeamsSettingApi/CreateDisposeSchedule",
            updateScheduleUrl: "/api/TeamsSettingApi/UpdateDisposeSchedule",
            deleteScheduleUrl: "/api/TeamsSettingApi/DeleteDisposeSchedule",
            breakScheduleUrl: "/api/TeamsSettingApi/BreakDisposeSchedule",
            scheduleType: 71,
            showScheduleSkipAction: true,
            showScheduleUseDecrypt: RM.gData.enableRecordsArchiver,
            noScheduleText: RMResx.RM_JS_ScheduleSetting_NoSchedule,
            scheduleSettingTitle: RMResx.RM_JS_SPS_EditTitle_ScheduleSetting,
            showNewIntervalSetting: true,
        };
    }
};