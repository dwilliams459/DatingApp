import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../_services/auth.service';

@Component({
    selector: 'app-nav',
    templateUrl: 'nav.component.html',
    styleUrls: ['nav.component.scss']
})
export class NavComponent implements OnInit {
    model: any = {};

    constructor(private authService: AuthService) { }

    ngOnInit() { }

    login() {
        this.authService.login(this.model).subscribe(next => {
            console.log('Logged in successfully');
        }, error => {
            console.log('Failed to login');
        });
    }

    loggedIn() {
        const token = localStorage.getItem('token');
        return !!token; // Return true if something in this token
    }

    logout() {
        localStorage.removeItem('token');
        console.log('Logout');
    }
}
