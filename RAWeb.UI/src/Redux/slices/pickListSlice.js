import { createSlice } from "@reduxjs/toolkit";

const initialState = {
    num: 0
};

const pickListSlice = createSlice({
    name: "pickListSlice",
    initialState,
    reducers: {
        add: (state, action) => {
            state.num = action.payload + 1;
        }
    }
});

export const {add} =  pickListSlice.actions;

export const num = (state) => state.pickList.num;

export default pickListSlice.reducer;
