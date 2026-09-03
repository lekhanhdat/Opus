import _ from "lodash";
import { useState, useEffect } from "react";
import { BasicDataRequester } from "../../../../requests";
import Category from "../../../Category";
import { MS365DataType } from "../../../../Constants/DataOptimizeType";

const FileCategory = ({ dataOptimizeParameter, onChange, o365TenantId }) => {

    const [fileExtensionItems, setFileExtensionItems] = useState([]);

    useEffect(() => {
        const handler = async () => {
            const clonedParameter = _.cloneDeep(dataOptimizeParameter);
            const fileExtensions = await BasicDataRequester.getFileExtensions(o365TenantId);

            if (!_.isEmpty(clonedParameter.fileExtensionQueryParameter) && fileExtensions.length > 0) {
                fileExtensions.forEach((i) => {
                    if (clonedParameter.fileExtensionQueryParameter.fileExtensions.length === 0) {
                        i.checked = true;
                    } else {
                        let find = false;
                        clonedParameter.fileExtensionQueryParameter.fileExtensions.forEach(j => {
                            if (clonedParameter.fileExtensionQueryParameter.fileExtensions.length > 0 && i.id === j) {
                                i.checked = true;
                                find = true;
                            }
                        });
                        if (!find) {
                            i.checked = false;
                        }
                    }
                }
                );
                setFileExtensionItems(fileExtensions.sort(sortFun));
            } else {
                fileExtensions.map(item => {
                    item.checked = true;
                });
                setFileExtensionItems(fileExtensions.sort(sortFun));
            }
        };
        handler();
    }, [o365TenantId]);

    const sortFun = (a, b) => {
        const nameA = a.name.toUpperCase();
        const nameB = b.name.toUpperCase();
        if (nameA < nameB) {
            return -1;
        }
        if (nameA > nameB) {
            return 1;
        }
        return 0;
    };

    const onSelectedFileExtensionInfo = (ids) => {
        const clonedValue = _.cloneDeep(dataOptimizeParameter);
        clonedValue.fileExtensionQueryParameter = {
            fileExtensions : ids.length ===  fileExtensionItems.length ? [] : ids,
        };
        onChange(clonedValue);
    };

    const FileCategoryView = () => {
        return (
            <div>
                <div className="reco-optimize-title require">{RMResx.RM_FA_Inactive_OptimizationTab_FileCategoryTitle}</div>
                <div>
                    <Category
                        categoryItems={fileExtensionItems}
                        onChange={onSelectedFileExtensionInfo}
                    />
                </div>
            </div>
        );
    }

    return (dataOptimizeParameter.ms365DataType !== MS365DataType.Phl && FileCategoryView());
};

export default FileCategory;