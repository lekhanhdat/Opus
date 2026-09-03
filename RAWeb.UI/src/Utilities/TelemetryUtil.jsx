export function addTelemetryRecord(module, eventType, args) {
    let telemetryDto = {
        Module: module,
        EventType: eventType,
        Args: args || null
    };
    let option = {
        url: '/api/HomeApi/AddTelemetryRecord',
        data: telemetryDto
    };
    fetchUtility(option).then((res) => {

    }).catch((e) => {
    });
}