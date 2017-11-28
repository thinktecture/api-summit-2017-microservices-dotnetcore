import {Http, RequestOptionsArgs, Headers, Response} from '@angular/http';
import {SecurityService} from './securityService';
import {Injectable} from '@angular/core';
import {AuthenticatedHttpService} from './authenticatedHttpService';
import {Observable} from 'rxjs/Rx';

@Injectable()
export class AuthenticatedHttpServiceImpl extends AuthenticatedHttpService {
  constructor(private _http: Http, private _securityService: SecurityService) {
    super();
  }

  public get(url: string, requestOptions?: RequestOptionsArgs): Observable<Response> {
    return this._http.get(url, this.withBearerToken(requestOptions));
  }

  public post(url: string, body: any, requestOptions?: RequestOptionsArgs): Observable<Response> {
    return this._http.post(url, body, this.withBearerToken(requestOptions));
  }

  private withBearerToken(requestOptions: RequestOptionsArgs) {
    const options: RequestOptionsArgs = requestOptions || {};
    options.headers = options.headers || new Headers();
    options.headers.delete('Authorization');
    options.headers.append('Authorization', `Bearer ${this._securityService.accessToken}`);

    return options;
  }
}
