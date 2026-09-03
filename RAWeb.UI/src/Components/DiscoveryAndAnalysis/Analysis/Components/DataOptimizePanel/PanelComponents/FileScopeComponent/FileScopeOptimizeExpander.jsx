import { useState } from "react";
import WithoutModifiedDate from "../../../WithoutModifiedDate";
import SizeRange from "./SizeRange";
import FileCategory from "./FileCategory";
import ContainerOrSCScope from "./ContainerOrSCScope";
import MS365DataTypeScope from "./MS365DataTypeScope";

const FileScopeOptimizeExpander = ({ dataOptimizeParameter, onChange, o365TenantId}) => {

    return (
        <div className="margin-bottom-l">
            <R.Expander
                title={RMResx.RM_FA_DataOptimize_FileScopeExpander}
                level={2}
                status={{ show: true }}
                togglable={false}
            >
                <div>
                    <div className="reco-optimize-option">
                        <MS365DataTypeScope
                            dataOptimizeParameter={dataOptimizeParameter}
                            o365TenantId={o365TenantId}
                            onChange={onChange}
                        />
                    </div>
                    <div className="reco-optimize-option">
                        <ContainerOrSCScope
                            dataOptimizeParameter={dataOptimizeParameter}
                            o365TenantId={o365TenantId}
                            onChange={onChange}
                        />
                    </div>
                    <div className="reco-optimize-option">
                        <WithoutModifiedDate
                            title={RMResx.RM_FA_Inactive_ModifiedTitle}
                            queryParameter={dataOptimizeParameter}
                            onChange={onChange}
                        />
                    </div>
                    <div className="reco-optimize-option">
                        <SizeRange
                            dataOptimizeParameter={dataOptimizeParameter}
                            o365TenantId={o365TenantId}
                            onChange={onChange}
                        />
                    </div>
                    <div>
                        <FileCategory
                            dataOptimizeParameter={dataOptimizeParameter}
                            o365TenantId={o365TenantId}
                            onChange={onChange}
                        />
                    </div>
                </div>
            </R.Expander>
        </div>
    );
};
export default FileScopeOptimizeExpander;