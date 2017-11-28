import {Component, OnInit} from '@angular/core';
import {Location} from '@angular/common';
import {PlatformService} from '../../services/platformService';
import {SecurityService} from '../../services/securityService';
import {User} from '../../models/user';
import {Router} from '@angular/router';

@Component({
  selector: 'app-header',
  templateUrl: 'header.html',
  styleUrls: ['header.scss']
})
export class HeaderComponent {
    public isLoggedIn: boolean = false;

    public get isBackChevronVisible(): boolean {
    // Mock implementation, to be extended to only show the button on iOS
    return this._location.path() !== '/home' && this._platform.isIOS;
  }

  constructor(private _location: Location, private _router: Router, private _platform: PlatformService, private _security: SecurityService) {
    this._security.userLoggedIn.subscribe((token) => {
      if (token) {
        this.isLoggedIn = true;
      }
    });
  }

  public logout(): void {
    this.isLoggedIn = false;
    this._security.logout()
      .subscribe(() => this._router.navigate(['/home']));
  }

  public goBack() {
    this._location.back();
  }
}
