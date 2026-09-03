import React, { useEffect, useRef, useState } from "react";
import RouterUrls from "../../../Constants/RouterUrls";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import { getRequestVerificationToken } from "../../../Utilities/CommonUtil";

import Paginate from "../../Common/Paginate/index";
import { ActionMode } from "./Common/Constants";
import { MessageBox } from "./Common/MessageBox";
import ConnectorTable from "./Tables/ConnectorTable";

const List = ({ history }) => {

    const connectorCacheRef = useRef([]);

    const formRef = useRef();

    const [connectorInfoes, setConnectorInfoes] = useState([]);

    const [checkedConnectors, setCheckedConnectors] = useState([]);

    const [pageIndex, setPageIndex] = useState(1);

    const [pageSize, setPageSize] = useState(10);

    const [connectorCount, setConnectorCount] = useState(0);

    useEffect(() => {
        loadConnectors();
    }, []);

    const loadConnectors = async () => {
        $$.loading(true);
        const requestOption = {
            url: "/api/Connector/GetAll"
        };
        const res = await fetchUtility(requestOption);
        connectorCacheRef.current = res;
        const connectors = getConnectorsFromCache(1, pageSize);
        setCheckedConnectors([]);
        setConnectorCount(res.length);
        setConnectorInfoes(connectors);
        $$.loading(false);
    };

    const getConnectorsFromCache = (pageIndex, pageSize) => {
        return connectorCacheRef.current.slice((pageIndex - 1) * pageSize, pageIndex * pageSize);
    };

    const onCreate = () => {
        history.push({
            pathname: RouterUrls.Connector_CreateOrEdit + `/?MODE=${ActionMode.CREATE}`
        });
    };

    const onDelete = () => {
        MessageBox.show(
            RMResx.RM_Connector_DeleteMsg,
            async () => {
                $$.loading(true);
                const requestOption = {
                    url: "/api/Connector/Delete",
                    data: checkedConnectors.map(item => item.id)
                };
                await fetchUtility(requestOption);
                $$.loading(false);
                await loadConnectors();
            });

    };

    const onEdit = (connectorInfo) => {
        history.push({
            pathname: RouterUrls.Connector_CreateOrEdit + `/?MODE=${ActionMode.EDIT}&ID=${connectorInfo.id}`
        });
    };

    const onConnectorCheckedChange = () => {
        const willCheckedConnectors = connectorCacheRef.current.filter(item => item.checked);
        setCheckedConnectors(willCheckedConnectors);
    };

    const onDownloadScheme = (connectorInfo) => {
        document.getElementById("connectorFormId").value = connectorInfo.id;
        formRef.current.submit();
    };

    const onPageIndexChange = (pageIndex) => {
        setPageIndex(pageIndex);
        setConnectorInfoes(getConnectorsFromCache(pageIndex, pageSize));
    };

    const onPageSizeChange = (pageSize) => {
        connectorCacheRef.current.forEach(item => item.checked = false);
        setPageSize(pageSize);
        setPageIndex(1);
        setConnectorInfoes(getConnectorsFromCache(pageIndex, pageSize));
        setCheckedConnectors([]);
    };

    return (
        <div className="reco-source-connector-list">
            <section className="header">
                <$g.SiteMap
                    data={[SiteMapLinks.CP, { text: RMResx.RM_CP_Connector }]} />
            </section>
            <section className="container">
                <div className="actions">
                    {
                        checkedConnectors.length === 0 ?
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
                    <div className="selected-counter">
                        {
                            RMResx.RM_Common_SelectTableItemsCounter.format(checkedConnectors.length, connectorCount)
                        }
                    </div>
                </div>
                <ConnectorTable
                    items={connectorInfoes}
                    onEdit={onEdit}
                    onDownloadScheme={onDownloadScheme}
                    onChangeChecked={onConnectorCheckedChange}
                />
                <div className="footer">
                    <Paginate
                        hasNextPage={(pageIndex * pageSize < connectorCount)}
                        currentPageCount={connectorInfoes.length}
                        onPageIndexChange={onPageIndexChange}
                        onPageSizeChange={onPageSizeChange}
                        pageIndex={pageIndex}
                    />
                </div>
            </section>
            <section style={{ display: "none" }}>
                <form ref={formRef} action="/api/Connector/DownloadJsonScheme" method="post">
                    <input id="connectorFormId" name="id" />
                    <input name='RequestVerificationToken' type='hidden' value={getRequestVerificationToken()} readOnly />
                </form>
            </section>
        </div>
    );
};

export default List;