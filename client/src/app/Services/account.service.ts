import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../models/users';

@Injectable({
   providedIn: 'root'
})
export class AccountService {
   baseUrl = environment.apiUrl;
   private currentUserSource = new ReplaySubject<User>(1);
   currentUser$ = this.currentUserSource.asObservable();

   constructor(private http: HttpClient) { }

   login(model: any) {
      return this.http.post(this.baseUrl + 'account/login', model).pipe(
         map((response: User) => {
            const user = response;
            if (user) {
               this.setCurrentUser(user);
            }
         })
      );
   }

   register(model: any) {
      return this.http.post(this.baseUrl + 'account/register', model).pipe(
         map((user: User) => {
            if (user) {
               this.setCurrentUser(user);
            }
         })
      )
   }
   setCurrentUser(user: User) {
      user.roles = [];
      const roles = this.getDecodedToken(user.token).role;
      // if user is in a single role, convert to an array 
      Array.isArray(roles) ? user.roles = roles : user.roles.push(roles);

      localStorage.setItem('user', JSON.stringify(user));
      this.currentUserSource.next(user);
   }

   logout() {
      localStorage.removeItem('user');
      this.currentUserSource.next(null);
   }

   getDecodedToken(token) {
      // token is in 3 parts.  we're interested in the middle part which is [1] (0, 1, 2)
      return JSON.parse(atob(token.split('.')[1]))
   }
}
