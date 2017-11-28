import {RequestOptionsArgs, Response} from '@angular/http';
import {Observable} from 'rxjs/Rx';

export abstract class AuthenticatedHttpService {
  public abstract get(url: string, requestOptions?: RequestOptionsArgs): Observable<Response>;
  public abstract post(url: string, body: any, requestOptions?: RequestOptionsArgs): Observable<Response>;
}
