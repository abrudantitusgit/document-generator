import React from "react";
import "../css/components/formtable.css";

const FormTable = (props) => {


    return(
        <div className="formtable">
        <div className="content">
            <h2>{props.title}</h2>
           {props.children}
           </div>
        </div>
    );

}

export default FormTable;