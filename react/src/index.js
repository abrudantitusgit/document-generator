import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter, Route } from 'react-router-dom';
import App from './App';
import reportWebVitals from './reportWebVitals';
import './styles/index.css';

// Apply the saved theme before the first paint so dark mode does not flash.
try {
  const savedTheme = localStorage.getItem('ovinstal-theme');
  if (savedTheme) document.documentElement.dataset.theme = savedTheme;
} catch { /* storage blocked: the OS preference applies */ }

ReactDOM.render(
  <BrowserRouter basename={'/'}>
    <Route path="/" component={App} />
  </BrowserRouter>,
  document.getElementById('root')
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
