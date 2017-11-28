import {Injectable} from '@angular/core';
import {Http} from '@angular/http';
import 'rxjs/add/operator/map';
import {AuthenticatedHttpService} from './authenticatedHttpService';
import { ConfigService } from './configService';

@Injectable()
export class OrdersService {

  constructor(private _http: AuthenticatedHttpService, private _config: ConfigService) {
  }

  public getOrders() {
    return this._http.get(this._config.WebApiBaseUrl + "orders")
      .map(result => result.json());
  }
}
