import React, { useRef } from "react";
import ExportSettingsTree from "../../Tree/Instances/ExportSettings";

export const ExportSettingsTreeDemo = () => {
    const exportSettingsTreeRef = useRef();
    const [selectedNode, setSelectedNode] = React.useState(null);

    const handleGetAllData = () => {
        console.log(exportSettingsTreeRef.current.getTreeData());
    };

    const onSelectedNode = (treeNode) =>{
        //treeNode.ChildTable is load the table of right;
        console.log(treeNode);
        setSelectedNode(treeNode)
    }

    const handleUpdateCurrentItem = () => {
        let newNode = {
            ...selectedNode,
            TreeNodeName: "Updated Node Name"
        }
        exportSettingsTreeRef.current.refreshSelectedNode(newNode);
    }

    return (
        <div>
            <ExportSettingsTree ref={exportSettingsTreeRef} onSelectedNode={onSelectedNode}/>
            <div style={{ marginTop: "20px"}}>
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={"Get all data"}
                    onClick={handleGetAllData}
                />
                 <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={"upload the data"}
                    onClick={handleUpdateCurrentItem}
                />
            </div>
        </div>
    );
};
