import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RegisterAttendeeComponent } from './register-attendee.component';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('RegisterAttendeeComponent', () => {
  let fixture: ComponentFixture<RegisterAttendeeComponent>;
  let component: RegisterAttendeeComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterAttendeeComponent, RouterTestingModule, NoopAnimationsModule],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterAttendeeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('form is invalid when required fields empty', () => {
    expect(component.form.valid).toBeFalse();
  });

  it('form is invalid when terms not accepted', () => {
    component.form.patchValue({
      fullName: 'Raj Kumar',
      email: 'raj@example.com',
      phone: '9876543210',
      city: 'Chennai',
      password: 'Str0ng@Pass!',
      confirmPassword: 'Str0ng@Pass!',
      termsAccepted: false,
    });
    expect(component.form.get('termsAccepted')?.valid).toBeFalse();
    expect(component.form.valid).toBeFalse();
  });

  it('password strength meter updates on input', () => {
    component.form.get('password')?.setValue('');
    expect(component.passwordStrengthLevel).toBe(0);

    component.form.get('password')?.setValue('Str0ng@Pass!');
    expect(component.passwordStrengthLevel).toBe(5);
  });

  it('shows password mismatch error', () => {
    component.form.patchValue({
      password: 'Str0ng@Pass!',
      confirmPassword: 'Different1!',
    });
    expect(component.form.errors?.['passwordMismatch']).toBeTrue();
  });

  it('form is valid with correct values', () => {
    component.form.patchValue({
      fullName: 'Raj Kumar',
      email: 'raj@example.com',
      phone: '9876543210',
      city: 'Chennai',
      password: 'Str0ng@Pass!',
      confirmPassword: 'Str0ng@Pass!',
      termsAccepted: true,
    });
    expect(component.form.valid).toBeTrue();
  });
});
