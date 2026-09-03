function existElement(resource, userResources) {
    var reg = new RegExp(resource, 'i');
    return userResources &&
        userResources.length > 0 &&
        userResources.some(function(element) { return element.Value.match(reg); });
}

function validPermission(userPMark, checkPermission) {
    var cp = eval(checkPermission).toString(10);
    return Long.fromString(userPMark + '').and(Long.fromString(cp)).equals(Long.fromString(cp));
}

function checkPermission(resource, userResources) {
    //console.log('checkPermission', resource, userResources);
    return existElement(resource, userResources);
}