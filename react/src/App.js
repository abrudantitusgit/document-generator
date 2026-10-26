import React from "react";
import { Redirect, Route, Switch } from "react-router-dom";
import Dashboard from "./components/Dashboard";
import Acasa from "./pages/Acasa";
import Word from "./pages/Word";
import RegistruATR from "./pages/RegistruATR";
import RegistruFotovoltaice from "./pages/RegistruFotovoltaice";
import ResourceWorkspace from "./components/ResourceWorkspace";
import { resourceDefinitions } from "./pages/resourceDefinitions";
import "./css/app/app.css";

export default function App() {
  return (
    <Route render={({ location }) => <Dashboard location={location}>
      <Switch location={location}>
        <Route exact path="/" component={Acasa} />
        <Route path="/atr" render={() => <ResourceWorkspace definition={resourceDefinitions.atr} />} />
        <Route path="/client" render={() => <ResourceWorkspace definition={resourceDefinitions.client} />} />
        <Route path="/email" render={() => <ResourceWorkspace definition={resourceDefinitions.email} />} />
        <Route path="/firma" render={() => <ResourceWorkspace definition={resourceDefinitions.firma} />} />
        <Route path="/proiect" render={() => <ResourceWorkspace definition={resourceDefinitions.proiect} />} />
        <Route exact path="/proiectfotovoltaice" component={RegistruFotovoltaice} />
        <Route exact path="/registrufotovoltaice" component={RegistruFotovoltaice} />
        <Route path="/word" component={Word} />
        <Route path="/registruatr" component={RegistruATR} />
        <Route path="/grup" render={() => <ResourceWorkspace definition={resourceDefinitions.grup} />} />
        <Route path="/folder" render={() => <ResourceWorkspace definition={resourceDefinitions.folder} />} />
        <Route path="/fisier" render={() => <ResourceWorkspace definition={resourceDefinitions.fisier} />} />
        <Route path="/clientcontor" render={() => <ResourceWorkspace definition={resourceDefinitions.contor} />} />
        <Redirect to="/" />
      </Switch>
    </Dashboard>} />
  );
}
