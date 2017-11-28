import {Component, HostBinding} from '@angular/core';
import {SecurityService} from '../../services/securityService';

import 'rxjs/add/operator/delay';
import {ActivatedRoute, Router} from '@angular/router';
import {PushService} from '../../services/pushService';

@Component({
  selector: 'app-security-login',
  styleUrls: ['login.scss'],
  templateUrl: 'login.html'
})
export class LoginComponent {
  @HostBinding('class.box')
  public loginCssClass = true;

  public username: string;
  public password: string;
  public error: string;

  constructor(private _securityService: SecurityService, private _activatedRoute: ActivatedRoute, private _router: Router, private _pushService: PushService) {
  }

  public submit(): void {
    this._securityService.login(this.username, this.password)
      .subscribe(
        () => {
          this._pushService.start();
          this._router.navigate([this._activatedRoute.snapshot.queryParams['redirectTo']]);
        },
        error => this.error = error
      );
  }
}
