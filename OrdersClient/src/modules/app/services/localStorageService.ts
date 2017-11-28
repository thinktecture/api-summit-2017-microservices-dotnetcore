import {StorageService} from './storageService';
import {Observable} from 'rxjs/Rx';

export class LocalStorageService extends StorageService {
  public save(key: string, value: any): Observable<void> {
    localStorage.setItem(key, JSON.stringify(value));
    return Observable.of(void 0);
  }

  public load<T>(key: string): Observable<T> {
    const result = localStorage.getItem(key);

    if (result === null) {
      return Observable.throw(`No storage item with key ${key} found.`);
    }

    return Observable.of(<T>JSON.parse(result));
  }

  public delete(key: string): Observable<void> {
    localStorage.removeItem(key);
    return Observable.of(void 0);
  }
}
