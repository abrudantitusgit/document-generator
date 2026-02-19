import React from "react";
import "../css/components/form.css";

const Form = (props) => {


    return(
        <div className="form">
        <div className="content">
            <h2>{props.title}</h2>
           {props.children}
           </div>
        </div>
    );

}

export default Form;