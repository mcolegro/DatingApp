import {
   HttpEvent, HttpHandler,
   HttpInterceptor, HttpRequest
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { BusyService } from '../Services/busy.service';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {

   constructor(private busyService: BusyService) { }

   intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
      this.busyService.busy();

      return next.handle(request).pipe(
         // delay(1000),  -- used this for development 
         finalize(() => {
            this.busyService.idle();
         })
      )
   }
}
