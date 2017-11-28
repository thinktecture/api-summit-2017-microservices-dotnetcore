import {Injectable} from '@angular/core';
import {BehaviorSubject} from 'rxjs/BehaviorSubject';
import {SecurityService} from './securityService';
import { HubConnection } from '@aspnet/signalr-client';
import { ConfigService } from './configService';

@Injectable()
export class PushService {
  private _hubConnection : HubConnection;

  public orderShipping: BehaviorSubject<string> = new BehaviorSubject(null);
  public orderCreated: BehaviorSubject<string> = new BehaviorSubject(null);

  constructor(private _securityService: SecurityService, private _config: ConfigService) {
  }

  public start(): void {
    // TODO: create connection
    this._hubConnection = new HubConnection(this._config.SignalRBaseUrl + 'ordersHub' + '?authorization=' + this._securityService.accessToken);
    
    this._hubConnection.on('orderCreated', () => {
      this.orderCreated.next(null);
    });

    this._hubConnection.on('shippingCreated', (orderId) => {
      this.orderShipping.next(orderId);
    });

    this._hubConnection.start()
      .then(() => console.log('SignalR connection established.'))
      .catch(err => console.error('SignalR connection not established. ' + err));
  }

  public stop(): void {
    if (this._hubConnection) {
      this._hubConnection.stop();
    }

    this._hubConnection = undefined;
  }
}
