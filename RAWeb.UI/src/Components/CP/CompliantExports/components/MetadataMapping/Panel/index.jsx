import { forwardRef, useEffect, useImperativeHandle, useState } from "react";
import {
    ConfiguratorContentSource,
    ConfiguratorFormatValue,
    FormatType,
} from "../../../Constants";
import { getFormatTypeList } from "../../../utils";

function MetadataMappingPanel(props, ref) {
    const { mappingRowData, selectedFormat, selectedContentSource } = props;

    const [mappingInfo, setMappingInfo] = useState({});
    const [formatType, setFormatType] = useState({
        list: getFormatTypeList(FormatType.Date),
        selected: FormatType.Date,
    })

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            return $$.verify("allValidation");
        },
        getMappingInfo: () => mappingInfo,
    }));

    useEffect(() => {
        if (mappingRowData) {
            if (selectedFormat !== ConfiguratorFormatValue.VEO && mappingRowData.Format === null) {
                setFormatType({
                    list: getFormatTypeList(FormatType.String),
                    selected: FormatType.String,
                });
            }
            setMappingInfo(mappingRowData);
        } else {
            if (selectedFormat === ConfiguratorFormatValue.VEO) {
                switch (selectedContentSource) {
                    case ConfiguratorContentSource.EXO:
                        setMappingInfo({
                            MetadataName: "", // DisplayName
                            TreeNodeName: "", // TreeNodeName
                            ExchangeMetadata: "", // MappedKey
                            DefaultValue: "", // Format
                            ExchangeMetadataAsSource: false, // Additional
                        });
                        break;
                    default: // SPO_OD
                        setMappingInfo({
                            MetadataName: "", // DisplayName
                            TreeNodeName: "", // TreeNodeName
                            SharePointMetadata: "", // MappedKey
                            DefaultValue: "", // Format
                            SharePointMetadataAsSource: false, // Additional
                        });
                        break;
                }
            } else {
                setMappingInfo({
                    DisplayName: "",
                    MappedKey: "",
                    DefaultValue: "",
                    Format: "",
                    Prefix: "",
                    Additional: false,
                });
            }
        }
    }, [mappingRowData]);

    const handleChangeType = (args) => {
        const newValue = args.newValue.value;

        setMappingInfo((prev) => ({
            ...prev,
            Format: newValue === FormatType.String ? null : '',
        }));
        setFormatType((prev) => ({
            ...prev,
            selected: args.newValue.value,
        }));
    }

    const handleChangeValue = (field, value) => {
        if (selectedFormat === ConfiguratorFormatValue.VEO) {
            const isEXO = selectedContentSource === ConfiguratorContentSource.EXO;
            const fieldMap = {
                DisplayName: "MetadataName",
                TreeNodeName: "TreeNodeName",
                MappedKey: isEXO ? "ExchangeMetadata" : "SharePointMetadata",
                DefaultValue: "DefaultValue",
                Additional: isEXO
                    ? "ExchangeMetadataAsSource"
                    : "SharePointMetadataAsSource",
            };

            setMappingInfo({
                ...mappingInfo,
                [fieldMap[field]]: value,
            });
        } else {
            setMappingInfo({
                ...mappingInfo,
                [field]: value,
            });
        }
    };

    // Used for input and checkbox of panel
    const handleGetDataMappingPanel = (field) => {
        if (selectedFormat === ConfiguratorFormatValue.VEO) {
            const isEXO = selectedContentSource === ConfiguratorContentSource.EXO;

            const fieldMap = {
                DisplayName: mappingInfo.MetadataName,
                TreeNodeName: mappingInfo.TreeNodeName,
                MappedKey: isEXO
                    ? mappingInfo.ExchangeMetadata
                    : mappingInfo.SharePointMetadata,
                Format: mappingInfo.DefaultValue,
                Additional: isEXO
                    ? mappingInfo.ExchangeMetadataAsSource
                    : mappingInfo.SharePointMetadataAsSource,
            };

            return fieldMap[field];
        }

        return mappingInfo[field];
    };

    const verifyCharacters = (value) => {
        if (!value) return RMResx.RM_ES_CompliantExport_Metadata_DisplayNameValidateMsg;
        try {
            const xmlString = `<${value}></${value}>`;
            const parser = new DOMParser();
            const doc = parser.parseFromString(xmlString, "application/xml");
            const parserError = doc.getElementsByTagName("parsererror")[0];
            if (parserError) {
                return RMResx.RM_ES_CompliantExport_Metadata_NodeNameValidateMsg;
            }
            return true;
        } catch (error) {
            console.error("Parse node name error!");
            return RMResx.RM_ES_CompliantExport_Metadata_NodeNameValidateMsg;
        }
    };

    return (
        <R.Validation>
            <div id="allValidation">
                <div className="flex flex-column gap-xs">
                    <div tabIndex={0} className="require">
                        {RMResx.RM_ES_CompliantExport_Metadata_DisplayName}
                    </div>
                    <R.Validation
                        element="Input"
                        require={
                            RMResx.RM_ES_CompliantExport_Metadata_DisplayNameValidateMsg
                        }
                    >
                        <R.Input
                            id="raMetadataMappingColumnDisplayName"
                            type="text"
                            value={handleGetDataMappingPanel("DisplayName")}
                            width="100%"
                            onChange={(newValue) =>
                                handleChangeValue("DisplayName", newValue)
                            }
                        />
                    </R.Validation>
                </div>
                {selectedFormat === ConfiguratorFormatValue.VEO && (
                    <div className="flex flex-column gap-xs margin-top-l">
                        <div tabIndex={0} className="require">
                            {RMResx.RM_ES_CompliantExport_Metadata_NodeName}
                        </div>
                        <R.Validation
                            element="Input"
                            rules={{
                                customVerify: verifyCharacters,
                            }}
                        >
                            <R.Input
                                id="raMetadataMappingColumnNodeName"
                                type="text"
                                value={handleGetDataMappingPanel("TreeNodeName")}
                                width="100%"
                                onChange={(newValue) =>
                                    handleChangeValue("TreeNodeName", newValue)
                                }
                            />
                        </R.Validation>
                    </div>
                )}
                <div className="flex flex-column gap-xs margin-top-l">
                    <div tabIndex={0}>
                        {RMResx.RM_ES_CompliantExport_Metadata_MappingKey}
                    </div>
                    <R.Input
                        id="raMetadataMappingColumnMappedKey"
                        type="text"
                        value={handleGetDataMappingPanel("MappedKey")}
                        width="100%"
                        onChange={(newValue) =>
                            handleChangeValue("MappedKey", newValue)
                        }
                    />
                </div>
                <div className="flex flex-column gap-xs margin-top-l">
                    <div tabIndex={0}>
                        {
                            RMResx.RM_ES_CompliantExport_Metadata_DefaultValue
                        }
                    </div>
                    <R.Input
                        id="raMetadataMappingColumnDefaultValue"
                        type="text"
                        value={mappingInfo.DefaultValue}
                        width="100%"
                        onChange={(newValue) =>
                            handleChangeValue("DefaultValue", newValue)
                        }
                    />
                </div>
                {selectedFormat !== ConfiguratorFormatValue.VEO && (
                    <>
                        <div className="flex flex-column gap-xs margin-top-l">
                            <div tabIndex={0}>
                                {RMResx.RM_ES_CompliantExport_Metadata_Type}
                            </div>
                            <R.Combobox
                                id="raMetadataMappingType"
                                tooltipField="name"
                                width="100%"
                                textField="name"
                                valueField="value"
                                checkedField="checked"
                                linkMode={false}
                                searchable={false}
                                items={formatType.list}
                                onChange={handleChangeType}
                                aria={{
                                    ariaLabel: RMResx.RM_ES_CompliantExport_Metadata_Type,
                                }}
                            />
                        </div>
                        {formatType.selected === FormatType.Date && (
                            <div className="flex flex-column gap-xs margin-top-l">
                                <div tabIndex={0}>
                                    {RMResx.RM_ES_CompliantExport_Metadata_Format}
                                </div>
                                <R.Input
                                    id="raMetadataMappingColumnFormat"
                                    type="text"
                                    value={mappingInfo.Format}
                                    width="100%"
                                    placeholder={RMResx.RM_ES_CompliantExport_Metadata_Format_Placeholder}
                                    onChange={(newValue) =>
                                        handleChangeValue("Format", newValue)
                                    }
                                />
                            </div>
                        )}
                        <div className="flex flex-column gap-xs margin-top-l">
                            <div tabIndex={0}>
                                {RMResx.RM_ES_CompliantExport_Metadata_Prefix}
                            </div>
                            <R.Input
                                id="raMetadataMappingColumnPrefix"
                                type="text"
                                value={mappingInfo.Prefix}
                                width="100%"
                                onChange={(newValue) =>
                                    handleChangeValue("Prefix", newValue)
                                }
                            />
                        </div>
                    </>
                )}
                <div className="margin-top-l">
                    <R.Checkbox
                        text={RMResx.RM_ES_CompliantExport_Metadata_Additional}
                        title={RMResx.RM_ES_CompliantExport_Metadata_Additional}
                        checked={handleGetDataMappingPanel("Additional")}
                        onChange={(checked) =>
                            handleChangeValue("Additional", checked)
                        }
                    />
                </div>
            </div>
        </R.Validation>
    );
}

export default forwardRef(MetadataMappingPanel);
