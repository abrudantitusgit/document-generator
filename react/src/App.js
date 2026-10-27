import React from "react";
import { Redirect, Route, Switch } from "react-router-dom";
import AppShell from "./components/layout/AppShell";
import ResourceWorkspace from "./components/ResourceWorkspace";
import Acasa from "./pages/Acasa";
import Rapoarte from "./pages/Rapoarte";
import RegistruATR from "./pages/RegistruATR";
import RegistruFotovoltaice from "./pages/RegistruFotovoltaice";
import Word from "./pages/Word";
import { resourceDefinitions } from "./pages/resourceDefinitions";

// Path → resource definition for the pages built on the shared CRUD template.
const resourceRoutes = {
  "/client": "client",
  "/firma": "firma",
  "/proiect": "proiect",
  "/atr": "atr",
  "/email": "email",
  "/clientcontor": "contor",
  "/grup": "grup",
  "/folder": "folder",
  "/fisier": "fisier",
};

export default function App() {
  return (
    <Route render={({ location }) => (
      <AppShell location={location}>
        <Switch location={location}>
          <Route exact path="/" component={Acasa} />
          {Object.entries(resourceRoutes).map(([path, key]) => (
            // The key remounts the workspace so no state leaks between resources.
            <Route key={path} path={path} render={() => <ResourceWorkspace key={key} definition={resourceDefinitions[key]} />} />
          ))}
          <Route exact path={["/proiectfotovoltaice", "/registrufotovoltaice"]} component={RegistruFotovoltaice} />
          <Route path="/word" component={Word} />
          <Route path="/registruatr" component={RegistruATR} />
          <Route path="/rapoarte" component={Rapoarte} />
          <Redirect to="/" />
        </Switch>
      </AppShell>
    )} />
  );
}
