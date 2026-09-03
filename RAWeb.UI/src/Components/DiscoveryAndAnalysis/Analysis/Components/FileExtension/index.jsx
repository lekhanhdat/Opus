import { useEffect, useRef, useState } from "react";
import { BasicDataRequester } from "../../requests";
import _ from "lodash";

const FileExtension = ({ o365TenantId, queryParameter, onChange, ariaId }) => {

    const availabelFileExtensionOptions = useRef([]);

    const [fileExtensionOptions, setFileExtensionOptions] = useState([]);

    useEffect(() => {
        const handler = async () => {
            const items = await BasicDataRequester.getFileExtensions(o365TenantId);
            availabelFileExtensionOptions.current = items;
        };
        handler();
    }, []);

    useEffect(() => {
        const fileExtensions = availabelFileExtensionOptions.current;
        const selectedFileExtensionIds = queryParameter.fileExtensionQueryParameter.fileExtensions;
        const res = fileExtensions.map(item => ({
            id: item.id,
            name: item.name,
            checked: selectedFileExtensionIds.length === 0 || selectedFileExtensionIds.some(i => i === item.id)
        }));
        setFileExtensionOptions(res);
    }, [queryParameter]);

    const onInnerChange = (args) => {
        const clonedValue = _.cloneDeep(queryParameter);
        const selectedFileExtensionIds = fileExtensionOptions.length === args.newValue.length ? [] : args.newValue.map(item => item.id);
        clonedValue.fileExtensionQueryParameter = {
            fileExtensions: selectedFileExtensionIds
        };
        
        onChange(clonedValue);
    };

    return (
        <div className="reco-size-range">
            <div className="reco-fr-content">
                <div className="reco-fr-content-style">
                    <div>
                        <R.Validation element="Multicombobox" require>
                            <R.Multicombobox
                                id="raFileTypeMultiCombobox"
                                width="100%"
                                popupMaxHeight={400}
                                searchable={false}
                                items={fileExtensionOptions}
                                textField="name"
                                valueField="id"
                                tooltipField="name"
                                checkedField="checked"
                                onChange={onInnerChange}
                                aria={{ ariaLabel: ariaId }}
                            />
                        </R.Validation>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default FileExtension;