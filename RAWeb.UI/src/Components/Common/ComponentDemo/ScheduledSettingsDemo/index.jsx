import React, { useRef, useState } from "react";
import { NewScheduleSetting } from "../../NewScheduleSetting";

const initialScheduleData = {
    scheduleType: "configure",
    startTime: new Date("2026-08-25T17:10:00"),
    interval: 3,
    intervalUnit: "weeks",
    endType: "none",
    occurrences: 1,
    endTime: new Date("2026-10-02T14:10:40"),
};

export const NewScheduledSettings = () => {
    const scheduledSettingsRef = useRef(null);
    const [scheduleData, setScheduleData] = useState(initialScheduleData);
    const [scheduleType, setScheduleType] = useState(initialScheduleData.scheduleType);

    const handleScheduleTypeChange = nextScheduleType => {
        setScheduleType(nextScheduleType);
    };

    const handleGetScheduleData = () => {
        const currentScheduleData = scheduledSettingsRef.current?.getScheduleData();
        if (currentScheduleData) {
            setScheduleData(currentScheduleData);
            console.log(currentScheduleData);
        }
    };

    return (
        <div style={{ background: "#fff", minHeight: 420, padding: 20 }}>
            <NewScheduleSetting
                ref={scheduledSettingsRef}
                scheduleData={scheduleData}
                onScheduleTypeChange={handleScheduleTypeChange}
            />
            <div style={{ marginTop: 16, textAlign: "right" }}>
                <R.Button
                    text="Get schedule data"
                    onClick={handleGetScheduleData}
                />
            </div>
            <div style={{ marginTop: 12, color: "#64748b" }}>
                Selected schedule: {scheduleType}
            </div>
        </div>
    );
};
