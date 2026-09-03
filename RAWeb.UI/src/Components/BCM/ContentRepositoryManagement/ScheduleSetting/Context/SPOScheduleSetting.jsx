export default {
    getContext() {
        return {
            createScheduleUrl: "/api/SPSettingApi/CreateDisposeSchedule",
            updateScheduleUrl: "/api/SPSettingApi/UpdateDisposeSchedule",
            deleteScheduleUrl: "/api/SPSettingApi/DeleteDisposeSchedule",
            breakScheduleUrl: "/api/SPSettingApi/BreakDisposeSchedule",
            scheduleType: 6,
            showScheduleSkipAction: true,
            showScheduleUseDecrypt: RM.gData.enableRecordsArchiver,
            noScheduleText: RMResx.RM_JS_ScheduleSetting_NoSchedule,
            scheduleSettingTitle: RMResx.RM_JS_SPS_EditTitle_ScheduleSetting,
            showNewIntervalSetting: true,
        };
    }
};