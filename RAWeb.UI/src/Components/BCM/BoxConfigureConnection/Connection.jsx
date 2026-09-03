import _ from "lodash";
import { useRef, useState } from "react";
import { useEffect } from "react";
import ConnectionTable from "./Table/ConnectionTable";
import Paginate from "../AzureFileShareConfigureConnection/Paginate";
import ConnectionPanel from "./Panel/ConnectionPanel";
import { showToast } from "../../../Utilities/CommonUtil";

const Connection = () => {

    const panelRef = useRef();

    const connectionsCache = useRef([]);

    const [connections, setConnections] = useState([]);

    const [connectionCount, setConnectionCount] = useState(0);

    const [checkedConnections, setCheckedConnections] = useState([]);

    const [pageIndex, setPageIndex] = useState(1);

    const [pageSize, setPageSize] = useState(10);

    const [codeParam, setCodeParam] = useState("");

    useEffect(() => {
        onReload();
    }, []);

    useEffect(() => {
        if (RM.Url.getParam(window.location.href, "code")) {
            setCodeParam(RM.Url.getParam(window.location.href, "code"));
        }
    }, [RM.Url.getParam(window.location.href, "code")])

    const onReload = async () => {
        $$.loading(true);
        const requestOption = {
            url: "/api/BoxConnection/GetAllConnections",
        };
        const result = await fetchUtility(requestOption);
        connectionsCache.current = result;
        setPageIndex(1);
        setConnections(getConnectionsFromCache(1, pageSize));
        setConnectionCount((_.isNil(result) || _.isEmpty(result)) ? 0 : result.length);
        setCheckedConnections([]);
        $$.loading(false);
    };

    const getConnectionsFromCache = (pageIndex, pageSize) => {
        if (_.isNil(connectionsCache.current) || _.isEmpty(connectionsCache.current)) {
            return [];
        }
        return connectionsCache.current.slice((pageIndex - 1) * pageSize, pageIndex * pageSize);
    };

    const onCreate = () => {
        panelRef.current.onShow(null);
    };

    const onEdit = (connection) => {
        panelRef.current.onShow(connection);
    };

    const onDelete = () => {
        const hasRelatedGroupConnection = checkedConnections.some(item => item.isRelatedConnectionGroup);
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: hasRelatedGroupConnection ? RMResx.RM_FS_Register_DeleteUsingConnection : RMResx.RM_PRM_PRE_Msg_ConfirmDeletePhyObj,
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
                    onClick: onDeleteOK,
                },
            ],
        });
    };

    const onDeleteOK = async () => {
        const clonedCheckedConnections = _.cloneDeep(checkedConnections);
        const connectionId = clonedCheckedConnections.map(item => item.id);
        $$.messagedialog(false);
        $$.loading(true);
        const requestOption = {
            url: "/api/BoxConnection/DeleteConnections",
            data: connectionId
        };
        const response = await fetchUtility(requestOption);
        $$.loading(false);
        if (!response.isSuccessful) {
            showToast.error(response.responseMessage);
        }
        onReload();
    };

    const onPageIndexChange = (pageIndex) => {
        setPageIndex(pageIndex);
        setConnections(getConnectionsFromCache(pageIndex, pageSize));
    };

    const onPageSizeChange = (pageSize) => {
        connectionsCache.current.forEach(item => item.checked = false);
        setPageSize(pageSize);
        setPageIndex(1);
        setConnections(getConnectionsFromCache(1, pageSize));
        setCheckedConnections([]);
    };

    const onConnectionCheckedChange = () => {
        const willCheckedConnections = connectionsCache.current.filter(item => item.checked);
        setCheckedConnections(willCheckedConnections);
    };

    return (
        <div className="reco-box-conn">
            <section className="reco-box-actions">
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
                <div className="reco-box-actions-desc">
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
            <section className="reco-box-footer">
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
                codeParam={codeParam}
            />
        </div>
    );
};

export default Connection;