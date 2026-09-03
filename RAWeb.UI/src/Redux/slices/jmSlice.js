import { createSlice } from "@reduxjs/toolkit";

const initialState = {
    num: 0
};

const jobMonitor = createSlice({
    name: "jobMonitor",
    initialState,
    reducers: {
        add: (state, action) => {
            state.num = action.payload + 1;
        },
    }
});

export const { add } = jobMonitor.actions;

export default jobMonitor.reducer;




