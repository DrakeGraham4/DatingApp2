import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Dating App';
  users: any;

  constructor(private accountService: AccountService) { }
  
  ngOnInit() {
    //calls method when initailizing component
    this.setCurrentUser();
  }

  setCurrentUser() {
    // because stringified in local storage, use parse to get the object out of the stringify form into our user object
    //getting the user object from local storage
    const user: User = JSON.parse(localStorage.getItem('user'));
    //setting that user object in account service
    this.accountService.setCurrentUser(user)
  }

}

