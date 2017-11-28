import {Component} from '@angular/core';
import {DesktopIntegrationService} from '../../services/desktopIntegrationService';
import {PushService} from '../../services/pushService';
import {SecurityService} from '../../services/securityService';

@Component({
  selector: 'app-root',
  templateUrl: 'root.html',
  styleUrls: ['root.scss']
})
export class RootComponent {
  constructor(private _securityService: SecurityService, private _pushService: PushService,  private _desktopIntegration: DesktopIntegrationService) {
    this._securityService.activateSession()
      .subscribe(() => {
        this._pushService.start();
      });

    this._desktopIntegration.register();
  }
}
