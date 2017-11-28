import {Observable} from 'rxjs/Rx';

export abstract class StorageService {
  public abstract save(key: string, value: any): Observable<void>;

  public abstract load<T>(key: string): Observable<T>;

  public abstract delete(key: string): Observable<void>;
}
