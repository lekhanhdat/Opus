const Long = require("long");
const existElement = (resource, userResources) => {
    //var userPermission = unWrapperPermision(userPMark);
    var reg = new RegExp(resource, 'i');
    return userResources
        && userResources.length > 0
        && userResources.some(element => element.Value?.match(reg) || element.Value === resource);
}

export const checkPermission = (resource, userResources) => {
    //console.log('checkPermission', resource, userResources);
    return existElement(resource, userResources || RM.UserResources);
}
