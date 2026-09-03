import React from "react";
import { TabIndex } from "../BCM/ContentRepositoryManagement/CRMForSPO";
import SPDestinationTree from "../Common/Tree/Instances/SPTree/SPDestinationTree";

export const ExportDestinationEnums = {
  OpusDownloadCenter: 0,
  SelectFromTree: 1,
};

const CommonExportDestination = ({
  value,
  onChange,
  treeData,
  onSelectedNodeChanged,
  treeRef,
}) => {
  return (
    <div className="common-export-destination">
      <div className="reco-report-profile-tree-input-title" tabIndex="0">
        ^Specify an export destination
      </div>

      <div className="reco-report-profile-scope-radio">
        <R.Radio
          group="exportDestination"
          checked={value === ExportDestinationEnums.OpusDownloadCenter}
          text="^Opus download center"
          onChange={() => onChange(ExportDestinationEnums.OpusDownloadCenter)}
        />
      </div>

      <div className="reco-report-profile-scope-radio">
        <R.Radio
          group="exportDestination"
          checked={value === ExportDestinationEnums.SelectFromTree}
          text="^Select a destination from the tree"
          onChange={() => onChange(ExportDestinationEnums.SelectFromTree)}
        />
      </div>

      {value === ExportDestinationEnums.SelectFromTree && (
        <SPDestinationTree
          ref={treeRef}
          treeData={treeData}
          mode={TabIndex.Archive}
          onSelectedNodeChanged={onSelectedNodeChanged}
        />
      )}
    </div>
  );
};

export default CommonExportDestination;
