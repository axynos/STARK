// @flow
import React, { Component } from 'react';
import type { Children } from 'react';
import TitleBar from './TitleBar';
import SideNav from './SideNav';

export default class App extends Component {
  props: {
    children: Children
  };

  render() {
    return (
      <div>
        <TitleBar />
        <div className="row no-gutters">
          <SideNav />
          {this.props.children}
        </div>
      </div>
    );
  }
}
