import React, {useState,useEffect } from "react";
import Form from "../components/Form";
import "../css/components/form.css";
import Combobox from "react-widgets/Combobox";
import "react-widgets/styles.css";
import { baseUrl } from "../config/api";
//import { findByPlaceholderText } from "@testing-library/react";

const Folder = () => {

    const[grup,setGrup] = useState({foldere:[]});
    const[imprima,setImprima] = useState({});
    const[proiecte,setProiecte] = useState([]);
    const[foldere,setFoldere] = useState([]);



    useEffect(() => {
      fetch(`${baseUrl}/proiect/proiecteinclude`)
      .then(response => response.ok ? response.json() : [])
      .then(data => setProiecte(Array.isArray(data) ? data : [])).catch(error=>console.log(error));
    },[]);

    useEffect(() => {
      fetch(`${baseUrl}/grup/Bransament`)
      .then(async response => response.ok ? response.json().catch(() => null) : null)
      .then(data => {
        const grupIncarcat = data && Array.isArray(data.foldere) ? data : { foldere: [] };
        setGrup(grupIncarcat);
        setFoldere(grupIncarcat.foldere);
      })
      .catch(error => console.error("Eroare la încărcarea grupului de documente", error));
    },[]);




    useEffect(()=>{
      setImprima({...imprima,grup});
    },[grup]);
 
    
    console.log(imprima)
    
    
    function AddRemove  (fisier,check) {
      
      
/*
      var folderIndex=grup.foldere.findIndex((f)=>f.id==fisier.folderID)
      if(check){
       
       tempGrup.foldere[folderIndex].fisiere.push(fisier);
  
      
      }
      else {
         var fisierIndex=tempGrup.foldere[folderIndex].fisiere.findIndex((f)=>f.id == fisier.id);
         tempGrup.foldere[folderIndex].fisiere.splice(fisierIndex,1);
      
       
       }
  
       setImprima({...imprima, grup:tempGrup})
       
       */
       
      
  
         

      console.log(fisier)
        if(check == false)
        return grup.foldere.map(folder=>{
          if(folder.id === fisier.folderID)
          {
            return {...folder, fisiere: folder.fisiere.filter(f=>f.id !== fisier.id)}
          }
          return folder;
        } )
        else
        return grup.foldere.map(folder=>{
          if(folder.id === fisier.folderID)
          {
            return {...folder, fisiere:[...folder.fisiere,fisier]}
          }
          return folder;
        } )

    }
    const HandleAddRemoveAll=(folderIndex,checkState)=>{
     
    
      var x = JSON.parse(JSON.stringify(grup.foldere))

      document.querySelectorAll(`input[type='checkbox'][id='foldercheck${folderIndex}'`).forEach(element => {
        element.checked=checkState
      });

        if (checkState){
          x[folderIndex]=foldere[folderIndex]
        }
          
        else{          
          x[folderIndex].fisiere=[]        
        }
        setGrup({...grup,foldere:x})
      
          
    }
    function Create(){
        fetch(`${baseUrl}/word/create`, {
            method: 'POST',
            headers: {
              "Content-Type": "application/json",
              "Accept": "application/json"
            },
            body: JSON.stringify(imprima)
          }).then(response => {
            if (!response.ok) {
             alert('Eroare');
            }
            else
            alert('Salvat');
          }).catch(error =>alert('Eroare'));
    }

    return (
        <Form title="Genereaza Bransament">
        <Combobox className="combobox" placeholder="Selecteaza Proiect:" data={proiecte} dataKey='nrProiect' textField={value=>value.nrProiect} onChange={value=>{setImprima({...imprima,proiect:value});}} renderListItem={({item})=>(<span><strong>{item.nrProiect}</strong>{" | " + item.denumire + " | " + item.strada + " | nr. " + item.nrStrada + " | ap. " + item.apartament + " | " + (item.firma ? item.firma.nume : item.client.nume) + " | " + (item.atr ? item.atr.nrAtr : '')}</span>)}/>
        
        <div className="checkzone">
        { 
        
          foldere.map((folder,findex)=>
          <div key={findex}>
          <h3>{folder.nume}</h3>
          <input type="checkbox" defaultChecked={true} onClick={(e)=>HandleAddRemoveAll(findex,e.target.checked)}></input>
         {folder.fisiere.map((item,index)=>
          <div className="check" key={index}>
        
         <input id={`foldercheck${findex}`}  type="checkbox" value={JSON.stringify(item)} defaultChecked={true} onChange={(e)=>{setGrup({...grup,foldere:AddRemove(JSON.parse(e.target.value),e.target.checked)});}}/>    
        
          <label>{item.nume}</label>      
          </div>
          )
         }
          </div>
          ) 
        } 
        </div>
  
        <button className="generate" onClick={()=>{Create();}}>Genereaza</button>

 

        </Form>
    );
}

export default Folder;
