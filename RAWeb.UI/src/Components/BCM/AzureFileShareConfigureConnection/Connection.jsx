import React, { useRef, useEffect, useState, Fragment } from "react";
import Paginate from "./Paginate";
import ConnectionPanel from "./Panels/ConnectionPanel";
import ConnectionTable from "./Tables/ConnectionTable";

const GetReadRequestOption = () => ({
    url: "/api/AzureFileShareConnection/GetConnections",
});

const GetDeleteRequestOption = (connections) => ({
    url: "/api/AzureFileShareConnection/DeleteConnections",
    data: connections.map(item => item.id)
});

const Connection = () => {

    const panelRef = useRef();

    const connectionsCache = useRef([]);

    const [connections, setConnections] = useState([]);

    const [pageIndex, setPageIndex] = useState(1);

    const [pageSize, setPageSize] = useState(10);

    const [connectionCount, setConnectionCount] = useState(0);

    const [checkedConnections, setCheckedConnections] = useState([]);

    useEffect(() => {
        onReload();
    }, []);

    const onReload = async () => {
        $$.loading(true);
        const requestOption = GetReadRequestOption();
        const result = await fetchUtility(requestOption);
        connectionsCache.current = result;
        setPageIndex(1);
        setConnections(getConnectionsFromCache(1, pageSize));
        setConnectionCount(result.length);
        setCheckedConnections([]);
        $$.loading(false);
    };

    const getConnectionsFromCache = (pageIndex, pageSize) => {
        return connectionsCache.current.slice((pageIndex - 1) * pageSize, pageIndex * pageSize);
    };

    const onPageSizeChange = (pageSize) => {
        connectionsCache.current.forEach(item => item.checked = false);
        setPageSize(pageSize);
        setPageIndex(1);
        setConnections(getConnectionsFromCache(1, pageSize));
        setCheckedConnections([]);
    };

    const onPageIndexChange = (pageIndex) => {
        setPageIndex(pageIndex);
        setConnections(getConnectionsFromCache(pageIndex, pageSize));
    };

    const onConnectionCheckedChange = () => {
        const willCheckedConnections = connectionsCache.current.filter(item => item.checked);
        setCheckedConnections(willCheckedConnections);
    };

    const onCreate = () => {
        panelRef.current.onShow(null);
    };

    const onEdit = (connection) => {
        panelRef.current.onShow(connection);
    };

    const onDelete = async () => {
        const hasRelatedGroupConnection = checkedConnections.some(item => item.isRelatedConnectionGroup);
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content:
                hasRelatedGroupConnection ?
                    <Fragment>
                        <div>{RMResx.RM_FS_Register_DeleteUsingConnection}</div>
                        <div>{RMResx.RM_PRM_PRE_Msg_ConfirmDeletePhyObj}</div>
                    </Fragment> :
                    <div>
                        <div>{RMResx.RM_PRM_PRE_Msg_ConfirmDeletePhyObj}</div>
                    </div>
            ,
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
                        const requestOption = GetDeleteRequestOption(checkedConnections);
                        await fetchUtility(requestOption);
                        $$.messagedialog(false);
                        $$.loading(false);
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
                        checkedConnections.length === 0 ?
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
                        RMResx.RM_Common_SelectTableItemsCounter.format(checkedConnections.length, connectionCount)
                    }
                </div>
            </section>
            <ConnectionTable
                items={connections}
                onChangeChecked={onConnectionCheckedChange}
                onEdit={onEdit}
            />
            <section className="reco-az-footer">
                <Paginate
                    hasNextPage={(pageIndex * pageSize < connectionCount)}
                    currentPageCount={connections.length}
                    onPageIndexChange={onPageIndexChange}
                    onPageSizeChange={onPageSizeChange}
                    pageIndex={pageIndex}
                />
            </section>
            <ConnectionPanel
                ref={panelRef}
                onReload={onReload}
            />
        </div>
    );
};

export default Connection;