import React, { useState, useEffect, useImperativeHandle, forwardRef } from "react";
import { ConnectionTable } from "./Tables";
import Utils from "./Utils";
import { LicenseHelper } from "../../../../../Utilities/CommonUtil";

const GetNoContainerConnectionRequestOption = () => ({
    url: "/api/ConnectionRegisterApi/GetAllNoGroupConnections",
    method: "GET"
});

const GetNoContainerConnectionRequestOption4JPMC = (data) => ({
    url: "/api/ConnectionRegisterApi/GetAllNoGroupConnections4JPMC",
    method: "POST",
    data: data,
});

const isEnabledJPMCFileSystemFeature = LicenseHelper.EnableJPMCFileSystemFeature();

const ConnectionsAdd = ({ addedConnections, removedConnections, onCheckedConn }, ref) => {

    const [refreshKey, setRefreshKey] = useState(Math.random());

    const [connections, setConnections] = useState([]);

    const [checkedConnections, setCheckedConnections] = useState([]);

    const [searchKey, setSearchKey] = useState("");

    const [pagingInfo, setPagingInfo] = useState({
        pageIndex: 0,
        pageSize: 10,
        totalCount: 0,
    })

    useEffect(() => {
        if (isEnabledJPMCFileSystemFeature) {
            setCheckedConnections(addedConnections);
        }
    }, [addedConnections]);

    useEffect(() => {
        const handler = async () => {
            $$.loading(true);
            const requestOption = GetNoContainerConnectionRequestOption();
            const resultStr = await fetchUtility(requestOption);
            const result = JSON.parse(resultStr);
            let conns = result.filter(item => !addedConnections.some(i => i.Id === item.Id));
            conns = conns.concat(removedConnections.filter(item => !conns.some(i => i.Id === item.Id)));
            conns = Utils.ConvertToSimpleConnections(conns);
            setConnections([...conns]);
            setRefreshKey(Math.random());
            $$.loading(false);
        };
        
        if (!isEnabledJPMCFileSystemFeature) {
            handler();
        }
    }, []);

    useEffect(() => {
        const handlerForJPMC = async () => {
            $$.loading(true);
            const payload = {
                PageIndex: pagingInfo.pageIndex + 1,
                PageSize: pagingInfo.pageSize,
                SearchKey: searchKey,
            }
            const requestOption = GetNoContainerConnectionRequestOption4JPMC(payload);
            const resultStr = await fetchUtility(requestOption);
            const { ConnectionList, TotalCount } = JSON.parse(resultStr);
            let conns = Utils.ConvertToSimpleConnections(Array.from(new Map([...(ConnectionList || []), ...(removedConnections || [])].map(item => [item.Id, item])).values()));
            conns = conns.map((item) => ({ ...item, checked: addedConnections.some((i) => i.Id === item.Id) || checkedConnections.some((i) => i.Id === item.Id) }))
            setConnections([...conns]);
            setPagingInfo((prev) => ({
                ...prev,
                totalCount: TotalCount || 0,
            }));
            setRefreshKey(Math.random());
            $$.loading(false);
        }

        if (isEnabledJPMCFileSystemFeature) {
            handlerForJPMC();
        }
    }, [pagingInfo.pageIndex, pagingInfo.pageSize, searchKey]);
    
    useEffect(() => {
        const currentChecked = connections.filter(item => item.checked);
        if (isEnabledJPMCFileSystemFeature && currentChecked.length > 0) {
            setCheckedConnections((prev) => {
                const missingItems = currentChecked.filter(c => !prev.some(p => p.Id === c.Id));
                return missingItems.length > 0 ? [...prev, ...missingItems] : prev;
            });
        }
    }, [connections]);

    useEffect(() => {
        const currentChecked = connections.filter(item => item.checked);
        if (isEnabledJPMCFileSystemFeature && currentChecked.length > 0) {
            setCheckedConnections((prev) => {
                const missingItems = currentChecked.filter(c => !prev.some(p => p.Id === c.Id));
                return missingItems.length > 0 ? [...prev, ...missingItems] : prev;
            });
        }
    }, [connections]);

    useImperativeHandle(ref, () => ({
        getNeedAddedConnections: () => {
            const needAddedConnections = connections.filter(item => checkedConnections.some(i => i.Id === item.Id));
            if (isEnabledJPMCFileSystemFeature) {
                return Utils.ConvertToSimpleConnections(checkedConnections);
            }
            return Utils.ConvertToSimpleConnections(needAddedConnections.concat(addedConnections));
        }
    }));

    const handlePagerChange = (pageIndex, pageSize, callback) => {
        setPagingInfo((prev) => ({
            ...prev,
            pageIndex,
            pageSize
        }));
        callback(true);
    }

    return (
        <div>
            <ConnectionTable
                id="reco-connection-add-table"
                key={refreshKey}
                onChangeChecked={(checkedItems) => {
                    const currentConnectionIds = connections.map(i => i.Id);
                    // Keep previously checked items that are NOT on the current page (other pages's selections)
                    const otherPageChecked = checkedConnections.filter(item => !currentConnectionIds.includes(item.Id));
                    const allCheckedItems = [...otherPageChecked, ...checkedItems];
                    setCheckedConnections(allCheckedItems);
                    onCheckedConn(allCheckedItems);
                }}
                items={connections}
                isAddConnPanel={true}
            />
            {isEnabledJPMCFileSystemFeature && (
                <div className="flex ra-flex-justify-between align-center padding-top-m padding-bottom-m">
                    <$g.Pager
                        itemsCount={pagingInfo.totalCount}
                        pagerIndex={pagingInfo.pageIndex}
                        pagerSize={pagingInfo.pageSize}
                        showPagerCounter={true}
                        showPagerSize={true}
                        pagerSizeOptions={[5, 10, 15]}
                        onChange={handlePagerChange} />
                </div>
            )}
        </div>
    );

};

export default forwardRef(ConnectionsAdd);