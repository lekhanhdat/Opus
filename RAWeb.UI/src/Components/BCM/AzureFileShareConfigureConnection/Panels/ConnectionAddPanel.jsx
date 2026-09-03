import React, { useState, useImperativeHandle, forwardRef } from "react";
import _ from "lodash";
import { useStableCallback } from "../Hooks/index";
import ConnectionSimpleTable from "../Tables/ConnectionSimpleTable";

const ConnectionAddPanel = ({ onAddConnection }, ref) => {

    const [show, setShow] = useState(false);

    const [connections, setConnections] = useState([]);

    const [checkedConnections, setCheckedConnections] = useState([]);    

    useImperativeHandle(ref, () => ({
        onShow: (availableConnection) => {
            const clonedConnections = _.cloneDeep(availableConnection);
            clonedConnections.forEach(item => item.checked = false);
            setConnections(clonedConnections);
            setCheckedConnections([]);
            setShow(true);
        }
    }));

    const onConnectionCheckedChange = () => {
        setCheckedConnections(connections.filter(item => item.checked));
    };

    const onHide = () => {
        setShow(false);
    };

    const onSave = useStableCallback(() => {
        if (checkedConnections.length === 0) {
            return false;
        }

        checkedConnections.forEach(item => item.checked = false);
        onAddConnection(checkedConnections);
        setShow(false);
    });

    return (
        <R.Panel
            id="reco-az-panel"
            header={RMResx.RM_FS_Register_Add}
            size={660}
            status={{ show: show }}
            onHide={onHide}
            destroy={false}
            actionType="back"
        >
            <div className="br" slot="header">
                <span className="reco-az-panel-header">{RMResx.RM_FS_Register_EditCorrelateConnections_SubTitle}</span>
            </div>
            <div>
                <ConnectionSimpleTable
                    key={Math.random()}
                    tableId="reco-az-simple-conn-add-table"
                    items={connections}
                    onChangeChecked={onConnectionCheckedChange}
                />
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onHide} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_FS_Register_Add} disabled={checkedConnections.length === 0} onClick={onSave} />
            </>
        </R.Panel>
    );
};

export default forwardRef(ConnectionAddPanel);