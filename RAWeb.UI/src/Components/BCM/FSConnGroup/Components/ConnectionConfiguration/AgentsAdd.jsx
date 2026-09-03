import React, { useState, useEffect, useImperativeHandle, forwardRef } from "react";
import { AgentTable } from "./Tables";
import Utils from "./Utils";
import { LicenseHelper } from "../../../../../Utilities/CommonUtil";

const GetFileSytemAgentsRequestOption = () => ({
    url: "/api/CPAgentMgmtApi/GetAllHasFileSystemSouceTypeAgents",
});

const isJPMCFeatureEnabled = LicenseHelper.EnableJPMCFileSystemFeature();

const DEFAULT_PAGE_SIZE = 10;

const AgentsAdd = ({ addedAgents, removedAgents, onCheckedAgent, isMultiGeoEnabled, DataCenterName }, ref) => {

    const [refreshKey, setRefreshKey] = useState(Math.random());

    const [agents, setAgents] = useState([]);

    const [checkedAgents, setCheckedAgents] = useState([]);

    const [totalCount, setTotalCount] = useState(0);

    const [pagerIndex, setPagerIndex] = useState(0);

    const [pagerSize, setPagerSize] = useState(DEFAULT_PAGE_SIZE);

    const [searchValue, setSearchValue] = useState("");

    useEffect(() => {
        const handler = async () => {
            $$.loading(true);
            const requestOption = GetFileSytemAgentsRequestOption();
            const result = await fetchUtility(requestOption);
            let agents = result.filter(item => !addedAgents.some(i => i.Id === item.Id));
            agents = agents.concat(removedAgents.filter(item => !agents.some(i => i.Id === item.Id)));
            agents = Utils.ConvertToSimpleAgents(agents);
            setAgents([...agents]);
            setRefreshKey(Math.random());
            $$.loading(false);
        };
        if(!isJPMCFeatureEnabled) {
            handler();
        } else {
            loadAgents(1, DEFAULT_PAGE_SIZE);
        }
    }, []);

    useImperativeHandle(ref, () => ({
        getNeedAddedAgents: () => {
            const needAddedAgents = agents.filter(item => checkedAgents.some(i => i === item.Id));
            return Utils.ConvertToSimpleAgents(needAddedAgents.concat(addedAgents));
        }
    }));

    const handlePageChange = (pagerIndex, pagerSize, callback) => {
        setPagerIndex(pagerIndex);
        setPagerSize(pagerSize);
        loadAgents(pagerIndex + 1, pagerSize, searchValue);
        callback(true);
    };

    const loadAgents = (pageIndex, pageSize, search = "") => {
        $$.loading(true);
        const url = isMultiGeoEnabled ?  "/api/CPAgentMgmtApi/FilterAgentsByDC" : "/api/CPAgentMgmtApi/QueryAgents";
        let option = {
            url: url,
            method: "POST",
            data: {
                SearchValue: search,
                PageIndex: pageIndex,
                PageSize: pageSize,
                AddAgentList: addedAgents.map(agent => agent.Id),
                DataCenterName: DataCenterName,
            }
        };
        fetchUtility(option).then((res) => {
            setTotalCount(res.TotalCount);
            setAgents(res.Agents);
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    const onSearch = (value) => {
        let searchValue = $.trim(value);

        setSearchValue(searchValue);
        setPagerIndex(0);
        loadAgents(1, pagerSize, searchValue);
    }

    const renderSearchBox = () => {
        return (
            <div className="margin-top-s" style={{ marginBottom: 48}}>
                <R.Searchbox
                    placeholder={RMResx.RM_CP_Agent_Placeholder_SearchBox}
                    disabled={false}
                    onSearch={onSearch}
                    width={380}
                />
            </div>
        );
    }

    const renderPager = () => {
        return (
            <div className="ra-main-footer">
                <$g.Pager
                    itemsCount={totalCount}
                    pagerIndex={pagerIndex}
                    pagerSize={pagerSize}
                    showPagerSize={true}
                    showPagerCounter={true}
                    pagerSizeOptions={[5, 10, 15]}
                    onChange={handlePageChange}
                />
            </div>
        );
    }

    return (
        <div>
            {isJPMCFeatureEnabled && renderSearchBox()}
            <AgentTable
                id="reco-agent-add-table"
                key={refreshKey}
                onChangeChecked={(checkedIds) => {setCheckedAgents(checkedIds); onCheckedAgent(checkedIds);}}
                items={agents}
            />
            {isJPMCFeatureEnabled && renderPager()}
        </div>
    );

};

export default forwardRef(AgentsAdd);