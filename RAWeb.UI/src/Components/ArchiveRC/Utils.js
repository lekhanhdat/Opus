export const sortRestoreCenterUrlList = (urlList, sortField) => {
    return urlList.sort((a, b) => {
        const urlA = new URL(a[sortField]);
        const urlB = new URL(b[sortField]);
        const hostCompare = urlA.hostname.localeCompare(urlB.hostname);
        if (hostCompare !== 0) {
            return hostCompare;
        }
        return urlA.pathname.localeCompare(urlB.pathname);
    });
};

export const sortUrlByArchiveTime = (urlList, isDesc = false) => {
    return urlList.sort((a, b) => {
        const ta = BigInt(a.ArchiveTime);
        const tb = BigInt(b.ArchiveTime);
        if (isDesc) {
            return ta < tb ? 1 : -1;
        }
        return ta > tb ? 1 : -1;
    });
}