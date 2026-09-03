import React, { useEffect, useState } from "react";
import PropTypes from "prop-types";
import "./index.less";

import ProgressItem from "../Components/ProgressItem/index";
import { SourceFlag } from "../Common/Constants";
import EmptyContent from "../Components/EmptyContent/index";
import CacheUtility from "../Common/CacheUtility";

const DatasRequestOption = (sourceFlag) => ({
    url: "/api/Dashboard/GetTop10TermUsages",
    data: sourceFlag,
});

const CachePrevKey = "MUT";

const MostUsedTerms = ({sourceFlags}) => {

    const [selectedSourceFlag, setSelectedSourceFlag] = useState(SourceFlag.None);

    const [datas, setDatas] = useState([]);

    const [dataValueTotal, setDataValueTotal] = useState(0);

    useEffect(() => {
        if (sourceFlags.length === 0) {
            return;
        }
        setSelectedSourceFlag(sourceFlags[0].value);
    }, [sourceFlags]);

    useEffect(() => {
        const fetchData = async () => {
            if (selectedSourceFlag === SourceFlag.None) {
                return;
            }

            const cacheKey = CachePrevKey + selectedSourceFlag;

            setDatas([]);
            setDataValueTotal(0);

            let responseData = [];
            if (CacheUtility.Instance.has(cacheKey)) {
                responseData = CacheUtility.Instance.get(cacheKey);
            } else {
                responseData = await fetchUtility(DatasRequestOption(selectedSourceFlag));
                CacheUtility.Instance.set(cacheKey, responseData);
            }

            setDatas(responseData);
            setDataValueTotal(responseData.reduce((prev, cur) => prev + cur.Active, 0));
        };

        fetchData();
    }, [selectedSourceFlag]);

    return (
        <div className="reco-dashboard-mut-wrapper reco-dashboard-card">
            <div className="reco-dashboard-card-title" tabIndex="0">
                {RMResx.RM_DSB_TermUsage_Title}
            </div>
            <div className="reco-dashboard-mut-selector">
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
                {
                    datas.map((data, index) =>
                        <ProgressItem
                            key={data.TermName + index + new Date().getSeconds()}
                            isMostTermComponent={true}
                            title={data.TermName}
                            tooltip={data.TermFullPath}
                            percentage={((data.Active / dataValueTotal) * 100) + "%"}
                            count={data.Active}
                        />
                    )
                }
            </EmptyContent>
        </div>
    );
};

MostUsedTerms.propTypes = {
    sourceFlags: PropTypes.array
};

export default MostUsedTerms;