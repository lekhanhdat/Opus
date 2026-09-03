export const SCHEDULE_TYPES = {
	NONE: "none",
	CONFIGURE: "configure",
};

export const END_TYPES = {
	NONE: "none",
	OCCURRENCES: "occurrences",
	DATE: "date",
};

export const INTERVAL_OPTIONS = [
	{ text: "Weeks", value: "weeks", backendValue: "1" },
	{ text: "Days", value: "days", backendValue: "2" },
	{ text: "Hours", value: "hours", backendValue: "3" },
];

export const END_TYPE_OPTIONS = [
	{ value: END_TYPES.NONE, backendValue: "0" },
	{ value: END_TYPES.DATE, backendValue: "1" },
	{ value: END_TYPES.OCCURRENCES, backendValue: "2" },
];

export const getDefaultValue = () => ({
	StartTime: 0,
	EndTime: 0,
	StartTimeDate: new Date(),
	EndTimeDate: new Date(),
	TimeZoneId: RM.TimeUtil.getGlobalTimezoneInfo().id,
	Interval: 1,
	IntervalType: 0,
	EndType: 0,
	OccurrencesTotal: 1,
	IsDaylightSaving: false,
	NoSchedule: true,
});
