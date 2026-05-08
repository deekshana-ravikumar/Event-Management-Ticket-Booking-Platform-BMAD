export type UserRole = 'Attendee' | 'Organizer' | 'SuperAdmin' | 'CheckinStaff';
export type UserStatus = 'PendingVerification' | 'Active' | 'Suspended' | 'Deactivated';
export type OrgStatus = 'PendingApproval' | 'Active' | 'Suspended' | 'Rejected';

export interface User {
  userId: string;
  email: string;
  role: UserRole;
  orgStatus?: OrgStatus;
}

export interface LoginResponse {
  accessToken: string;
  expiresIn: number;
  userId: string;
  email: string;
  role: UserRole;
  orgStatus?: OrgStatus;
}
