import {Route, Switch} from "react-router-dom";
import { CSSTransition, TransitionGroup} from 'react-transition-group';
import Dashboard from "./components/Dashboard";
import Acasa from "./pages/Acasa";
import Atr from "./pages/Atr";
import Client from "./pages/Client";
import Email from "./pages/Email";
import Firma from "./pages/Firma";
import Proiect from "./pages/Proiect";
import Word from "./pages/Word";
import RegistruATR from "./pages/RegistruATR";
import Grup from "./pages/Grup";
import Folder from "./pages/Folder";
import Fisier from "./pages/Fisier";
import ClientContor from "./pages/ClientContor";
import ProiectFotovoltaice from "./pages/ProiectFotovoltaice";
import RegistruFotovoltaice from "./pages/RegistruFotovoltaice"
import "./css/app/app.css"

function App() {
  return (
  <div className="App">
    <Dashboard/>
  <div className="page">
    <Route render={({location})=>(
      <TransitionGroup>
          <CSSTransition key={location.key} classNames="fade">
            <Switch location={location}>
              <Route exact path="/" component={Acasa}/>
              <Route path="/atr" component={Atr} />
              <Route path="/client" component={Client}/>
              <Route path="/email" component={Email}/>
              <Route path="/firma" component={Firma}/>
              <Route path="/proiect" component={Proiect}/>
              <Route exact path="/proiectfotovoltaice" component={ProiectFotovoltaice}/>
              <Route exact path="/registrufotovoltaice" component={RegistruFotovoltaice}/>
              <Route path="/word" component={Word}/>
              <Route path="/registruatr" component={RegistruATR}/>
              <Route path="/grup" component={Grup}/>
              <Route path="/folder" component={Folder}/>
              <Route path="/fisier" component={Fisier}/>
              <Route path="/clientcontor" component={ClientContor}/>
              {/* Fallback route for any unmatched paths */}
              <Route component={Acasa}/>
            </Switch>
          </CSSTransition>
      </TransitionGroup>
    )}/>
  </div>
  </div>
  );
}

export default App;
