import React, { useMemo, useState } from "react";
import ManageCategoryFilterPanel from "./ManageCategoryFilterPanel";

const manageCategoryMockItems = [
    {
        id: "1",
        checked: true,
        name: "alissagroupteam",
        location: "https://pd78.share...",
        category: "Others",
        size: 71,
    },
    {
        id: "2",
        checked: false,
        name: "OpusTest3",
        location: "https://pd78.share...",
        category: "Sales",
        size: 36,
    },
    {
        id: "3",
        checked: false,
        name: "OpusTest2",
        location: "https://pd78.share...",
        category: "Marketing",
        size: 9,
    },
    {
        id: "4",
        checked: false,
        name: "AlissaSPTest3",
        location: "https://pd78.share...",
        category: "Finance",
        size: 26,
    },
    {
        id: "5",
        checked: false,
        name: "JordanXQTeam",
        location: "https://pd78.share...",
        category: "HR",
        size: 53,
    },
    {
        id: "6",
        checked: false,
        name: "RileyZTest5",
        location: "https://pd78.share...",
        category: "IT",
        size: 24,
    },
];

const manageCategoryFilterItems = [
    { name: "All categories", value: "all" },
    { name: "3 items", value: "3-items" },
    { name: "Sales", value: "Sales" },
    { name: "Marketing", value: "Marketing" },
    { name: "Finance", value: "Finance" },
    { name: "HR", value: "HR" },
    { name: "IT", value: "IT" },
    { name: "Others", value: "Others" },
];

const manageCategoryColumns = [
    {
        header: "Name",
        width: 220,
        resizeable: true,
    },
    {
        header: "Location",
        width: 260,
        resizeable: true,
    },
    {
        header: "Category",
        width: 160,
        resizeable: true,
    },
    {
        header: "Data size (GB)",
        width: 140,
        resizeable: true,
    },
];

class ManageCategoryTableRow extends R.TableRow {
    onNameClick(e) {
        e.preventDefault();
    }

    render(Row, Cell) {
        const rowData = this.props.rowData;

        return (
            <Row>
                <Cell>
                    <div className="ra-view-plan-table-text" data-tooltip="ifneed" aria-label={rowData.name}>
                        <a href="#" className="ra-main-cell-link" onClick={(e) => this.onNameClick(e)}>
                            {rowData.name}
                        </a>
                    </div>
                </Cell>
                <Cell>
                    <div className="ra-view-plan-table-text" data-tooltip="ifneed" aria-label={rowData.location}>
                        {rowData.location}
                    </div>
                </Cell>
                <Cell>
                    <div className="ra-view-plan-table-text" data-tooltip="ifneed" aria-label={rowData.category}>
                        {rowData.category}
                    </div>
                </Cell>
                <Cell>
                    <div className="ra-view-plan-table-text" data-tooltip="ifneed" aria-label={String(rowData.size)}>
                        {rowData.size}
                    </div>
                </Cell>
            </Row>
        );
    }
}

const ManageCategoryPanel = ({ onHide }) => {
    const [showFilterPanel, setShowFilterPanel] = useState(false);
    const [searchKeyword, setSearchKeyword] = useState("");
    const [filterCategory, setFilterCategory] = useState("3-items");

    const onSearchStart = (keyword) => {
        const value = typeof keyword === "string" ? keyword : "";
        setSearchKeyword(value.trim().toLowerCase());
    };

    const onOpenFilter = () => {
        setShowFilterPanel(true);
    };

    const onHideFilter = () => {
        setShowFilterPanel(false);
    };

    const onApplyFilter = ({ category }) => {
        setFilterCategory(category || "all");
        setShowFilterPanel(false);
    };

    const visibleItems = useMemo(() => {
        return manageCategoryMockItems.filter((item) => {
            const matchedSearch =
                !searchKeyword ||
                item.name.toLowerCase().includes(searchKeyword) ||
                item.location.toLowerCase().includes(searchKeyword) ||
                item.category.toLowerCase().includes(searchKeyword);

            let matchedFilter = true;
            if (filterCategory === "3-items") {
                matchedFilter = ["Sales", "Marketing", "Finance"].includes(item.category);
            } else if (filterCategory !== "all") {
                matchedFilter = item.category === filterCategory;
            }

            return matchedSearch && matchedFilter;
        });
    }, [filterCategory, searchKeyword]);

    return (
        <R.Panel
            id="ra-view-plan-manage-category-panel"
            header="Manage category"  
            size={760}
            onHide={onHide}
            onClose={onHide}
            status={{ show: true }}
            destroy={true}
        >
            <div className="ra-view-plan-manage-panel-body">
                <div className="ra-view-plan-manage-toolbar">
                    <R.Searchbox
                        placeholder="Search..."
                        width="220"
                        onSearch={onSearchStart}
                    />
                    <R.Button
                        icon="fia-filter"
                        text="Filters"
                        onClick={onOpenFilter}
                    />
                </div>

                <div className="ra-view-plan-manage-table-wrap">
                    <R.Table
                        id="ra-view-plan-manage-category-table"
                        rowTemplate={ManageCategoryTableRow}
                        items={visibleItems}
                        columns={manageCategoryColumns}
                        checkable={true}
                        frozenCount={0}
                    />
                </div>
            </div>

            <ManageCategoryFilterPanel
                show={showFilterPanel}
                onHide={onHideFilter}
                onApply={onApplyFilter}
                categoryItems={manageCategoryFilterItems}
                currentCategory={filterCategory}
            />

            <>
                <R.Button slot="buttons" text="Cancel" onClick={onHide} />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text="Save"
                    onClick={onHide}
                />
            </>
        </R.Panel>
    );
};

export default ManageCategoryPanel;