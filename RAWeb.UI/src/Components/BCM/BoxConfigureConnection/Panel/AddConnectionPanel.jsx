import _ from "lodash";
import React, { useState, useImperativeHandle, forwardRef } from "react";
import AddConnectionTable from "../Table/AddConnectionTable";
import { useStableCallback } from "../../../Common/Hooks";

const AddConnectionPanel = ({ onAddConnection }, ref) => {

    const [isShow, setIsShow] = useState(false);

    const [connections, setConnections] = useState([]);

    const [checkedConnections, setCheckedConnections] = useState([]);

    useImperativeHandle(ref, () => ({
        onShow: (availableConnection) => {
            const clonedConnections = _.cloneDeep(availableConnection);
            clonedConnections.forEach(item => item.checked = false);
            setConnections(clonedConnections);
            setCheckedConnections([]);
            setIsShow(true);
        }
    }));

    const onHide = () => {
        setIsShow(false);
    };

    const onConnectionCheckedChange = () => {
        setCheckedConnections(connections.filter(item => item.checked));
    };

    const onAdd = useStableCallback(() => {
        if (checkedConnections.length === 0) {
            return false;
        }

        checkedConnections.forEach(item => item.checked = false);
        onAddConnection(checkedConnections);
        setIsShow(false);
    });

    return (
        <R.Panel
            id="reco-box-panel"
            header={RMResx.RM_FS_Register_Add}
            size={660}
            status={{ show: isShow }}
            onHide={onHide}
            destroy={false}
            actionType="back"
        >
            <div className="br" slot="header">
                <span className="reco-box-panel-header">{RMResx.RM_FS_Register_EditCorrelateConnections_SubTitle}</span>
            </div>
            <div>
                <AddConnectionTable
                    key={Math.random()}
                    tableId="reco-box-conn-add-table"
                    items={connections}
                    onChangeChecked={onConnectionCheckedChange}
                />
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onHide} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_FS_Register_Add} disabled={checkedConnections.length === 0} onClick={onAdd} />
            </>
        </R.Panel>
    );
};

export default forwardRef(AddConnectionPanel);