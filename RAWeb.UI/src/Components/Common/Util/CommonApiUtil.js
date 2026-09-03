
export function checkIsCSDTenant() {
    let option = {
        url: "/api/RuleApi/CheckIsCSDTenant",
        method: "POST"
    };
    return fetchUtility(option);
}