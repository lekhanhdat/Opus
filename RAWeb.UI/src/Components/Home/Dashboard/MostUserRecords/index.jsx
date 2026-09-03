import React, { useEffect, useState } from "react";
import PropTypes from "prop-types";
import "./index.less";

import { SourceFlag } from "../Common/Constants";
import UserRecordsItem from "../Components/UserRecordsItem/index";
import EmptyContent from "../Components/EmptyContent/index";
import CacheUtility from "../Common/CacheUtility";

const UserRecordsRequestOption = (sourceFlag) => ({
    url: "/api/Dashboard/GetTop10UserRecordsWaitingApproval",
    data: sourceFlag
});

const CachePrevKey = "MUR";

const MostUserRecords = ({sourceFlags}) => {

    const [selectedSourceFlag, setSelectedSourceFlag] = useState(SourceFlag.None);

    const [datas, setDatas] = useState([]);

    useEffect(() => {
        const initializeSelectedSourceFlag = async () => {
            if (sourceFlags.length === 0) {
                return;
            }
            setSelectedSourceFlag(sourceFlags[0].value);
        };
        initializeSelectedSourceFlag();
    }, [sourceFlags]);

    useEffect(() => {
        const fetchData = async () => {
            if (selectedSourceFlag === SourceFlag.None) {
                return;
            }
            setDatas([]);
            let responseData = [];
            if (CacheUtility.Instance.has(CachePrevKey + selectedSourceFlag)) {
                responseData = CacheUtility.Instance.get(CachePrevKey + selectedSourceFlag);
            } else {
                responseData = await fetchUtility(UserRecordsRequestOption(selectedSourceFlag));
                CacheUtility.Instance.set(CachePrevKey + selectedSourceFlag, responseData);
            }
            setDatas(responseData);
        };

        fetchData();
    }, [selectedSourceFlag]);

    return (
        <div className="reco-dashboard-mur-wrapper reco-dashboard-card">
            <div className="reco-dashboard-card-title" tabIndex="0" data-tooltip="ifneed" aria-label={RMResx.RM_DSB_UserWaitingApproval_Title}>
                {RMResx.RM_DSB_UserWaitingApproval_Title}
            </div>
            <div className="reco-dashboard-mur-selector">
                <R.Combobox
                    width="100%"
                    height="100%"
                    items={sourceFlags}
                    disabled={false}
                    textField="name"
                    valueField="value"
                    searchable={false}
                    linkMode={false}
                    excludeChecked={true}
                    onChange={(args) => setSelectedSourceFlag(args.newValue.value)}
                />
            </div>
            <EmptyContent isEmpty={datas.length === 0}>
                <div className="reco-dashboard-mur-user-records">
                    {
                        datas.map((data, index) => <UserRecordsItem
                            key={data.Id}
                            index={index}
                            name={data.DisplayName}
                            email={data.UserPrincipalName}
                            count={data.Count}
                        />)
                    }
                </div>
            </EmptyContent>
        </div>
    );
};

MostUserRecords.propTypes = {
    sourceFlags: PropTypes.array
};

export default MostUserRecords;