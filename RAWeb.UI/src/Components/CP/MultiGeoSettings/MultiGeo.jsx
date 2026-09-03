import React, { useEffect, useState } from "react";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import "./index.less";
import MultiGeoSave from "./MultiGeoSave";
import { MultiGEOToggle } from "./Constants";
import RouterUrls from "../../../Constants/RouterUrls";
import { isEnableMultiGeoFeature } from "../../../Utilities/CommonUtil";

const MultiGeo = (props) => {
    const isGlobalFeatureEnabled = isEnableMultiGeoFeature();
    const [isMultiGeoEnabled, setIsMultiGeoEnabled] = useState(isGlobalFeatureEnabled);
    const [multiGeoState, setMultiGeoState] = useState(isGlobalFeatureEnabled ? MultiGEOToggle.On : MultiGEOToggle.Off);
    const [toggleLocked, setToggleLocked] = useState(isGlobalFeatureEnabled);
    const [geoList, setGeoList] = useState([]);
    const [ipErrors, setIpErrors] = useState([]);

    useEffect(() => {
        const loadGeoList = async () => {
            $$.loading(true);
            let option = {
                url: "/api/MultiGEOSettingApi/GetAllMultiGeoSetting",
                method: "GET",
            };
            fetchUtility(option).then((res) => { 
                $$.loading(false);
                if (res && res.length > 0) {
                    setGeoList(res);
                }
            }).catch((e) => {
                $$.loading(false);
            });
        };
        loadGeoList();
    }, []);

    const octet = '(?:25[0-5]|2[0-4]\\d|1\\d{2}|[1-9]?\\d)';
    const singleTokenRegex = new RegExp(`^${octet}(?:\\.${octet}){3}(?:\\/${octet})?$`);
    const ipShapeRegex = /^\d{1,3}(?:\.\d{1,3}){3}(?:\/\d{1,3})?$/;
    
    const validateIpString = (val) => {
        if (!val || val.trim() === "") return "";
        const tokens = val.split(',').map(s => s.trim()).filter(s => s !== "");
        if (tokens.length === 0) return "";
        for (const t of tokens) {
            if (ipShapeRegex.test(t)) {
                const [ipPart, rangePart] = t.split("/");
                const octets = ipPart.split(".");
                const hasOutOfRangeOctet = octets.some(o => Number(o) > 255);
                if (hasOutOfRangeOctet) {
                    return RMResx.RM_AR_CP_Valid_IP_Range;
                }
                if (rangePart !== undefined) {
                    const range = Number(rangePart)
                    const lastOctet = Number(octets[3]);
                    if (range < lastOctet) {
                        return RMResx.RM_AR_CP_Valid_IP_Range;
                    }
                }
            }
            if (!singleTokenRegex.test(t)) {
                return RMResx.RM_AR_CP_Valid_IP_Range;
            }
        }
        return "";
    };

    const handleIpChange = (index, eOrValue) => {
        const value = (eOrValue && typeof eOrValue === "object" && "target" in eOrValue)
            ? (eOrValue.target.value ?? "")
            : (eOrValue ?? "");
        const validationMessage = validateIpString(String(value));
        
        setGeoList(prev => {
            const next = [...prev];
            next[index] = { ...next[index], IPAddresses: value };
            return next;
        });

        setIpErrors(prev => {
            const nextErrors = Array.isArray(prev) ? [...prev] : [];
            while (nextErrors.length <= index) nextErrors.push("");
            nextErrors[index] = validationMessage;
            return nextErrors;
        });
    };

    const handleToggleEnable = () => {
        setIsMultiGeoEnabled(prev => !prev);
    }

    const handleValidateAndGetPayload = () => {
        const currentErrors = geoList.map(item => validateIpString(item.IPAddresses));
        setIpErrors(currentErrors);

        const hasErrors = currentErrors.some(err => err !== "");
        if (hasErrors) {
            return null;
        }
        
        return geoList;
    };

    const handleSaveSuccess = () => {
        setIsMultiGeoEnabled(true);
        setMultiGeoState(MultiGEOToggle.On);
        setToggleLocked(true);
    };

    const handleCancel = () => {
        props.history.push({
            pathname: RouterUrls.CP_Index
        });
    };

    return (
        <div className="reco-multi-geo-settings">
            <section className="reco-multi-geo-header">
                <$g.SiteMap  data={[SiteMapLinks.CP, SiteMapLinks.CP_MultiGeo]} />
            </section>
            <section className="reco-multi-geo-content">
                <p>{RMResx.RM_AR_CP_Multi_Geo_Toggle_Title}</p>
                <div className="reco-multi-geo-toggle">
                    <div style={{ position: 'relative', display: 'inline-block' }}>
                        <R.Switch
                            checked={isMultiGeoEnabled}
                            disabled={toggleLocked}
                        />
                        {!toggleLocked && (
                            <div
                                onClick={handleToggleEnable}
                                aria-label="Open multi-geo confirmation"
                                style={{
                                    position: 'absolute',
                                    top: 0,
                                    left: 0,
                                    right: 0,
                                    bottom: 0,
                                    cursor: 'pointer',
                                    zIndex: 10
                                }}
                            />
                        )}
                    </div>
                    <span className="multi-geo-status">{multiGeoState}</span>
                </div>
                {isMultiGeoEnabled && <div className="DC-and-IP">
                    <div className="description">
                        <p className="desc-col">{RMResx.RM_AR_CP_Multi_Geo_DC}</p>
                        <p className="desc-col">{RMResx.RM_AR_CP_Multi_Geo_IP}</p>
                        <$g.Popover>{RMResx.RM_AR_CP_Multi_Geo_IP_Placeholder}</$g.Popover>
                    </div>
                    {
                        geoList.map((item, index) => (
                            <div className="input-group" key={`multi-geo-${index}`}>
                                <R.Input
                                    id={"raMulti-geo-dc-" + index}
                                    className="dc-input"
                                    value={item.DCDisplayName}
                                    disabled
                                />
                                <div className="ip-input-wrapper" style={{ flex: '1 1 80%' }}>
                                    <R.Validation>
                                        <R.Input
                                            id={"raMulti-geo-ip-" + index}
                                            className="ip-input"
                                            value={item.IPAddresses}
                                            onChange={arg => handleIpChange(index, arg)}
                                            placeholder={RMResx.RM_AR_CP_Multi_Geo_IP_Placeholder}
                                        />
                                        <R.ValidationFaker of={"#raMulti-geo-ip-" + index} valid={!ipErrors[index]} message={ipErrors[index]}/>
                                    </R.Validation>
                                </div>
                            </div>
                        ))
                    }
                    <MultiGeoSave 
                        onValidate={handleValidateAndGetPayload} 
                        onSaveSuccess={handleSaveSuccess} 
                        onCancel={handleCancel}
                        ipErrors={ipErrors}
                    />
                </div>}
            </section>
        </div>
    );
}

export default MultiGeo;