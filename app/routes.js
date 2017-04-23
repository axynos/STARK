// @flow
import React from 'react';
import { Route, IndexRoute } from 'react-router';
import App from './containers/App';
import LibraryPage from './containers/LibraryPage';
// import HomePage from './containers/HomePage';
// import CounterPage from './containers/CounterPage';


export default (
  <Route path="/" component={App}>
    <IndexRoute component={LibraryPage} />
    {/* <IndexRoute component={HomePage} />
    <Route path="/counter" component={CounterPage} /> */}
  </Route>
);
