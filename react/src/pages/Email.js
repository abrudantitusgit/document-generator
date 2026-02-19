import React, {useState, useEffect } from "react";
import Form from "../components/Form";
import {AiFillDelete} from "react-icons/ai";
import moment from "moment";
import '../css/pages/email.css'
import { baseUrl } from "../config/api";

const Email = () => {

    const [email, setEmail] = useState({activ:true});
    const [emails, setEmails] = useState([]);

    useEffect(()=>{
        fetch(`${baseUrl}/email/list`)
        .then(response => response.json())
        .then(data => setEmails(data)).catch(error=>console.log(error));
    },[]);

    function Create(){
        fetch(`${baseUrl}/email/create`, {
            method: 'POST',
            headers: {
              "Content-Type": "application/json",
              "Accept": "application/json"
            },
            body: JSON.stringify(email)
          }).then(response => {
            if (!response.ok) {
             alert('Eroare');
            }
            else
            {
              setEmail({activ:true});
              alert('Salvat');
            }
          }).catch(error =>alert('Eroare'));
    }

    function Delete(id)
    {
        fetch(`${baseUrl}/email/delete/${id}`)
        .then(response => {
            if (!response.ok) {
             alert('Eroare');
            }
            else
            {
              var newList = emails.filter(item=>item.id !== id);
              setEmails(newList);
              alert('Salvat');
            }
          }).catch(error =>alert('Eroare'));
    }

    return(
<div>
<Form title = "Adauga Email">
<input type="email" value={email.adresa || ''} placeholder="Introdu Adresa (Ex: petrana@office...):" onChange={(e) => setEmail({...email,adresa:e.target.value})}/>
<input value={email.text || ''} placeholder="Introdu Text (Ex: Masina service ...):" onChange={(e) => setEmail({...email,text:e.target.value})}/>
<label>Introdu data trimiteri email (Ex: 12/01/2021):</label>
<input type="date" value={email.data || ''} onChange={(e) => setEmail({...email,data:e.target.value})}/>

<button className="adauga" onClick={()=>{Create();}}>Adauga</button>

</Form>


{
    emails.map(item=>
        <ul>
        <li>{'Adresa: ' + item.adresa}</li>
        <li>{'Text: ' + item.text}</li>
        <li>{'Data: ' + moment(item.data).format('DD.MM.yyyy')}</li>
        <li>{'Activ: ' + (item.activ ? '✔️' : '❌')}</li>
        <li><AiFillDelete className="delete" onClick={()=>Delete(item.id)}/></li>
        </ul>
    )
}

</div>
        );
}

export default Email;