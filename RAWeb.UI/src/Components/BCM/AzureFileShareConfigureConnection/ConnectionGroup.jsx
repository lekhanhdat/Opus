import React, { useRef, useEffect, useState } from "react";
import Paginate from "./Paginate";
import ConnectionGroupPanel from "./Panels/ConnectionGroupPanel";
import ConnectionGroupTable from "./Tables/ConnectionGroupTable";

const GetReadRequestOption = () => ({
    url: "/api/AzureFileShareConnection/GetGroups",
});

const GetDeleteRequestOption = (connections) => ({
    url: "/api/AzureFileShareConnection/DeleteGroups",
    data: connections.map(item => item.id)
});

const ConnectionGroup = () => {

    const panelRef = useRef();

    const connectionGroupsCache = useRef([]);

    const [connectionGroups, setConnectionGroups] = useState([]);

    const [pageIndex, setPageIndex] = useState(1);

    const [pageSize, setPageSize] = useState(10);

    const [connectionGroupCount, setConnectionGroupCount] = useState(0);

    const [checkedConnectionGroups, setCheckedConnectionGroups] = useState([]);

    useEffect(() => {
        onReload();
    }, []);

    const onReload = async () => {
        $$.loading(true);
        const requestOption = GetReadRequestOption();
        const result = await fetchUtility(requestOption);
        connectionGroupsCache.current = result;
        setPageIndex(1);
        setConnectionGroups(getConnectionGroupsFromCache(1, pageSize));
        setConnectionGroupCount(result.length);
        setCheckedConnectionGroups([]);
        $$.loading(false);
    };

    const getConnectionGroupsFromCache = (pageIndex, pageSize) => {
        return connectionGroupsCache.current.slice((pageIndex - 1) * pageSize, pageIndex * pageSize);
    };

    const onPageSizeChange = (pageSize) => {
        connectionGroupsCache.current.forEach(item => item.checked = false);
        setPageSize(pageSize);
        setPageIndex(1);
        setConnectionGroups(getConnectionGroupsFromCache(1, pageSize));
        setCheckedConnectionGroups([]);
    };

    const onPageIndexChange = (pageIndex) => {
        setPageIndex(pageIndex);
        setConnectionGroups(getConnectionGroupsFromCache(pageIndex, pageSize));
    };

    const onConnectionCheckedChange = () => {
        const willCheckedConnections = connectionGroupsCache.current.filter(item => item.checked);
        setCheckedConnectionGroups(willCheckedConnections);
    };

    const onCreate = () => {
        panelRef.current.onShow(null);
    };

    const onEdit = (connection) => {
        panelRef.current.onShow(connection);
    };

    const onDelete = async () => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_PRM_PRE_Msg_ConfirmDeletePhyObj,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    },
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: async () => {
                        $$.loading(true);
                        const requestOption = GetDeleteRequestOption(checkedConnectionGroups);
                        await fetchUtility(requestOption);
                        $$.loading(false);
                        $$.messagedialog(false);
                        onReload();
                    },
                },
            ],
        });

    };

    return (
        <div className="reco-az-conn">
            <section className="reco-az-actions">
                <div>
                    {
                        checkedConnectionGroups.length === 0 ?
                            <R.Button
                                icon=""
                                text={RMResx.RM_JS_Common_Create}
                                primary={true}
                                classify="theme"
                                onClick={onCreate}
                            />
                            :
                            <R.Button
                                icon="fia-delete"
                                primary={false}
                                classify="default"
                                text={RMResx.RM_JS_Common_Delete}
                                onClick={onDelete}
                            />
                    }
                </div>
                <div className="reco-az-actions-desc">
                    {
                        RMResx.RM_Common_SelectTableItemsCounter.format(checkedConnectionGroups.length, connectionGroupCount)
                    }
                </div>
            </section>
            <ConnectionGroupTable
                items={connectionGroups}
                onChangeChecked={onConnectionCheckedChange}
                onEdit={onEdit}
            />
            <section className="reco-az-footer">
                <Paginate
                    hasNextPage={(pageIndex * pageSize < connectionGroupCount)}
                    currentPageCount={connectionGroups.length}
                    onPageIndexChange={onPageIndexChange}
                    onPageSizeChange={onPageSizeChange}
                    pageIndex={pageIndex}
                />
            </section>
            <ConnectionGroupPanel
                ref={panelRef}
                onReload={onReload}
            />
        </div>
    );
};

export default ConnectionGroup;