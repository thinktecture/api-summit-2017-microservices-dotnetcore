import {BrowserModule} from '@angular/platform-browser';
import {NgModule, NgZone} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {BrowserXhr, HttpModule} from '@angular/http';
import {RouterModule} from '@angular/router';

import {RootComponent} from './components/root/root';
import {ROUTES} from './routes';
import {HomeComponent} from './components/home/home';
import {HeaderComponent} from './components/header/header';
import {MenuComponent} from './components/menu/menu';
import {WindowRef} from './services/windowRef';
import {PlatformService} from './services/platformService';
import {NgProgressCustomBrowserXhr, NgProgressModule} from 'ng2-progressbar';
import {NgxElectronModule} from 'ngx-electron';
import {DesktopIntegrationService} from './services/desktopIntegrationService';
import {LoginComponent} from './components/login/login';
import {SecurityService} from './services/securityService';
import {SecurityConfiguration} from './models/securityConfiguration';
import {AuthenticatedHttpService} from './services/authenticatedHttpService';
import {AuthenticatedHttpServiceImpl} from './services/authenticatedHttpServiceImpl';
import {IsAuthenticated} from './guards/isAuthenticated';
import {StorageService} from './services/storageService';
import {LocalStorageService} from './services/localStorageService';
import {OrdersService} from './services/ordersService';
import {OrderListComponent} from './components/list/orderList';
import {PushService} from './services/pushService';
import { ConfigService } from './services/configService';

export function securityConfigurationFactory() {
  const configuration = new SecurityConfiguration();
  configuration.authorityUrl = 'http://localhost:5000/';
  configuration.clientId = 'ro.client';
  configuration.clientSecret = 'secret';
  configuration.loginRoute = '/login';
  configuration.scope = 'ordersapi openid profile';

  return configuration;
}

@NgModule({
  declarations: [
    RootComponent,
    LoginComponent,
    HomeComponent,
    HeaderComponent,
    MenuComponent,
    OrderListComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpModule,
    RouterModule.forRoot(ROUTES, { useHash: true }),
    NgProgressModule,
    NgxElectronModule
  ],
  bootstrap: [RootComponent],
  providers: [
    ConfigService,
    WindowRef,
    SecurityService,
    { provide: StorageService, useClass: LocalStorageService },
    OrdersService,
    PlatformService,
    PushService,
    DesktopIntegrationService,
    { provide: BrowserXhr, useClass: NgProgressCustomBrowserXhr },
    { provide: AuthenticatedHttpService, useClass: AuthenticatedHttpServiceImpl },
    SecurityService,
    { provide: SecurityConfiguration, useFactory: securityConfigurationFactory },
    IsAuthenticated
  ]
})
export class AppModule {
}
