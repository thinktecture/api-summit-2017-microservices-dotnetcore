import {CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router} from '@angular/router';
import {Observable} from 'rxjs/Rx';
import {SecurityService} from '../services/securityService';
import {Injectable} from '@angular/core';
import {SecurityConfiguration} from '../models/securityConfiguration';

@Injectable()
export class IsAuthenticated implements CanActivate {
  constructor(private _securityService: SecurityService, private _router: Router, private _configuration: SecurityConfiguration) {
    if (!this._configuration.loginRoute) {
      throw new Error('loginRoute has not been configured.');
    }
  }

  public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean>|Promise<boolean>|boolean {
    return this._securityService.activateSession()
      .map(result => {
        if (!result) {
          this._router.navigate([this._configuration.loginRoute], {
            queryParams: {
              redirectTo: state.url
            }
          });
          return false;
        }

        return true;
      });
  }

}
