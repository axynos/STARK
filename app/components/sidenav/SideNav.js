import React, { Component } from 'react';
import SideNavButton from './SideNavButton';
import styles from './SideNav.scss';

class SideNav extends Component {
  render() {
    const sideNav = `${styles.sideNav} col-3`;
    return (
      <div className={sideNav}>
        <SideNavButton icon="library_music" label="library" active />
        <SideNavButton icon="chat_bubble" label="tts" />
        <SideNavButton icon="settings" label="settings" />
        <SideNavButton icon="favorite" iconColor="#fb4f4f" label="donate" />
        <SideNavButton icon="info" label="about" />
      </div>
    );
  }
}

export default SideNav;
