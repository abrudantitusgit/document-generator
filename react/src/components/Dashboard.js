import React from "react";
import "../css/components/dashboard.css";
import {FiAlertTriangle} from "react-icons/fi";
import {IoPersonAddOutline} from "react-icons/io5";
import {ImOffice} from "react-icons/im";
import {IoDocumentText} from "react-icons/io5";
import {SiMicrosoftword} from "react-icons/si";
import {GoRepo} from "react-icons/go";
import {FiEdit} from "react-icons/fi";
import {RiEdgeLine} from "react-icons/ri";
import {AiOutlineMail} from "react-icons/ai";
import { FaSolarPanel } from "react-icons/fa";
import logo from "../logo.png";

const Dashboard = () => {
    return(
        <div className="dashboard">
            <div className="links">
            <h2><a href="/"><img id='home-logo' src={logo} className="icon"/></a></h2>
                <a href="/client"><IoPersonAddOutline className="icon"/></a>
                <a href="/firma"><ImOffice className="icon"/></a>
                <a href="/proiect"><IoDocumentText className="icon"/></a>
                <a href="/atr"><RiEdgeLine className="icon"/></a>
                <a href="/word"><SiMicrosoftword className="icon"/></a>
                <a href="/registruatr"><GoRepo className="icon"/></a>
                <a href="/clientcontor"><FiEdit className="icon"/></a>
                <a href="/email"><AiOutlineMail className="icon"/></a>
                <a href="/grup">Grup</a>
                <a href="/folder">Folder</a>
                <a href="/fisier">Fisier</a>
                <a href="/registrufotovoltaice"><FaSolarPanel className="icon"/></a>
            </div>
        </div>
    );
}

export default Dashboard;