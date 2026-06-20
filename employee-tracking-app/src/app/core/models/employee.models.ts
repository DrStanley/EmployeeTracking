export interface Employee {
  id: string;
  employeeNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  jobTitle: string;
  departmentId: string;
  attendancePolicyId: string;
  employmentType: EmploymentType;
  isActive: boolean;
  managerId?: string;
  referredByEmail?: string;
}

export enum EmploymentType {
  FullTime = 0,
  PartTime = 1,
  Contract = 2,
  Casual = 3
}

export interface CreateEmployeeRequest {
  employeeNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  jobTitle: string;
  departmentId: string;
  attendancePolicyId: string;
  employmentType: EmploymentType;
  referredByEmail?: string;
}
