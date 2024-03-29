import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {


@Output() canceRegister = new EventEmitter();

registerForm : FormGroup = new FormGroup({});
maxDate: Date = new Date();
validationErrors : string[] | undefined;


ngOnInit(): void {
  this.initializeForm();
  this.maxDate.setFullYear(this.maxDate.getFullYear() - 18); 
}

constructor(private accountService: AccountService, private toastr: ToastrService, 
  private fb: FormBuilder, private router: Router){

}

initializeForm(){
  this.registerForm = this.fb.group({
    gender: ['male'],
    username: ['', Validators.required],
    knownAs: ['', Validators.required],
    dateOfBirth: ['', Validators.required],
    city: ['', Validators.required],
    country: ['', Validators.required],
    password: ['',[Validators.required,Validators.minLength(4),Validators.maxLength(8)]],
    confirmPassword : ['',[Validators.required,this.matchValue('password')]]
  })

  this.registerForm.controls['password'].valueChanges.subscribe({
    next:() => this.registerForm.controls['confirmPassword'].updateValueAndValidity()
  })
}

matchValue(matchTo: string): ValidatorFn 
{
  return (control : AbstractControl) => {
    return control.value === control.parent?.get(matchTo)?.value ? null : {notMatching: true}
  }
}

register() {
  const dob = this.GetDateOnly(this.registerForm.controls['dateOfBirth'].value)
  const values = {...this.registerForm.value, dateOfBirth: this.GetDateOnly(dob)}
  this.accountService.register(values).subscribe({
    next: response => {
      this.router.navigateByUrl('/members');
    },
    error: error => {
      this.validationErrors = error;
    } 
  })
}

cancel(){
  this.canceRegister.emit(false);
}

private GetDateOnly(dob: string | undefined) {
  if (!dob) return;
  let theDob = new Date(dob);
  return new Date(theDob.setMinutes(theDob.getMinutes()-theDob.getTimezoneOffset())).toISOString().slice(0,10);
}

}
