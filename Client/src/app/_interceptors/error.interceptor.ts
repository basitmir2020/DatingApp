import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private router:Router,private toaster:ToastrService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError(err => {
        if(err){
          switch(err.status){
            case 400:
              if(err.error.errors){
                const modelStateError = [];
                for(const key in err.error.errors){
                  if(err.error.errors[key]){
                    modelStateError.push(err.error.errors[key]);
                  }
                }
                throw modelStateError.flat();
              }else{
                this.toaster.error(err.statusText,err.status);
              }
              break;
            case 401:
              this.toaster.error(err.statusText,err.status);
              break;
            case 404:
              this.router.navigateByUrl('/not-found');
              break;
            case 500:
              const navigationExtras : NavigationExtras = {state : {error:err.error}};
              this.router.navigateByUrl('/server-error',navigationExtras);
              break;
            default:
              this.toaster.error('Something Unexpected Went Wrong!');
              console.log(err);
              break;
          }
        }
        return throwError(err);
      })
    );
  }
}
