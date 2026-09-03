import _ from "lodash";
import React, { useRef, useEffect, useState } from "react";
import ConnectionGroupTable from "./Table/ConnectionGroupTable";
import Paginate from "../AzureFileShareConfigureConnection/Paginate";
import { showToast } from "../../../Utilities/CommonUtil";
import ConnectionGroupPanel from "./Panel/ConnectionGroupPanel";

const ConnectionGroup = () => {

    const panelRef = useRef();

    const connectionGroupsCache = useRef([]);

    const [pageIndex, setPageIndex] = useState(1);

    const [pageSize, setPageSize] = useState(10);

    const [connectionGroups, setConnectionGroups] = useState([]);

    const [connectionGroupCount, setConnectionGroupCount] = useState(0);

    const [checkedConnectionGroups, setCheckedConnectionGroups] = useState([]);

    useEffect(() => {
        onReload();
    }, []);

    const onReload = async () => {
        $$.loading(true);
        const requestOption = {
            url: "/api/BoxConnection/GetAllConnectionGroups",
        };
        const result = await fetchUtility(requestOption);
        connectionGroupsCache.current = result;
        setPageIndex(1);
        setConnectionGroups(getConnectionGroupsFromCache(1, pageSize));
        setConnectionGroupCount((_.isNil(result) || _.isEmpty(result)) ? 0 : result.length);
        setCheckedConnectionGroups([]);
        $$.loading(false);
    };

    const getConnectionGroupsFromCache = (pageIndex, pageSize) => {
        if (_.isNil(connectionGroupsCache.current) || _.isEmpty(connectionGroupsCache.current)) {
            return [];
        }
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

    const onConnectionGroupCheckedChange = () => {
        const willCheckedGroups = connectionGroupsCache.current.filter(item => item.checked);
        setCheckedConnectionGroups(willCheckedGroups);
    };

    const onCreate = () => {
        panelRef.current.onShow(null);
    };

    const onEdit = (connectionGroup) => {
        panelRef.current.onShow(connectionGroup);
    };

    const onDelete = () => {
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
                    onClick: onDeleteOK,
                },
            ],
        });
    };

    const onDeleteOK = async () => {
        const clonedCheckedConnectionGroups = _.cloneDeep(checkedConnectionGroups);
        const connectionGroupId = clonedCheckedConnectionGroups.map(item => item.id);
        $$.messagedialog(false);
        $$.loading(true);
        const requestOption = {
            url: "/api/BoxConnection/DeleteConnectionGroups",
            data: connectionGroupId
        };
        const response = await fetchUtility(requestOption);
        $$.loading(false);
        if (!response.isSuccessful) {
            showToast.error(response.responseMessage);
        }
        onReload();
    };

    return (
        <div className="reco-box-conn">
            <section className="reco-box-actions">
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
                <div className="reco-box-actions-desc">
                    {
                        RMResx.RM_Common_SelectTableItemsCounter.format(checkedConnectionGroups.length, connectionGroupCount)
                    }
                </div>
            </section>
            <ConnectionGroupTable
                items={connectionGroups}
                onChangeChecked={onConnectionGroupCheckedChange}
                onEdit={onEdit}
            />
            <section className="reco-box-footer">
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