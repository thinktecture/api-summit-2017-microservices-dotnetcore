import { Injectable } from "@angular/core";

@Injectable()
export class ConfigService {
    public WebApiBaseUrl = "http://localhost:53278/api/";
    public SignalRBaseUrl = "http://localhost:53278/";
}