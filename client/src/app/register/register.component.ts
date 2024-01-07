import { Component, EventEmitter, Input, Output } from '@angular/core';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {

model: any = {};
@Output() canceRegister = new EventEmitter();

constructor(private accountService: AccountService, private toastr: ToastrService){

}

register(){
 this.accountService.register(this.model).subscribe({
  next : response => {
    
    this.cancel();
  },
  error: error => this.toastr.error(error.error)
 })
}

cancel(){
  this.canceRegister.emit(false);
}

}
