// @flow
import React, { Component } from 'react';
import type { Children } from 'react';
import TitleBar from '../components/titlebar/TitleBar';
import SideNav from '../components/sidenav/SideNav';

export default class App extends Component {
  props: {
    children: Children
  };

  render() {
    return (
      <div>
        <TitleBar />
        <SideNav />
        {this.props.children}
      </div>
    );
  }
}
