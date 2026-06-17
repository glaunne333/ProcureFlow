insert into "Departments" ("Id", "Name")
values
  ('11111111-1111-1111-1111-111111111111', 'Finance'),
  ('22222222-2222-2222-2222-222222222222', 'Operations'),
  ('33333333-3333-3333-3333-333333333333', 'IT')
on conflict ("Id") do nothing;

insert into "Vendors" ("Id", "Name", "ContactEmail", "IsActive")
values
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Acme Office Supplies', 'sales@acme-demo.test', true),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Northwind Hardware', 'orders@northwind-demo.test', true),
  ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Contoso IT Services', 'hello@contoso-demo.test', true)
on conflict ("Id") do nothing;

insert into "Users" ("Id", "Name", "Email", "PasswordHash", "Role", "DepartmentId", "IsActive")
values
  (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    'Employee Demo',
    'employee@demo.com',
    'pbkdf2-sha256$100000$KVyCThXO1sXX0nx+iGpsYg==$PPrq+AVjcA1h8FVlpwp9FXoH99hHsaqtk8/KPtaxNZY=',
    'Employee',
    '22222222-2222-2222-2222-222222222222',
    true
  ),
  (
    '99999999-9999-9999-9999-999999999999',
    'Manager Demo',
    'manager@demo.com',
    'pbkdf2-sha256$100000$DP1E7AXHxEoUUNH9Vn7EVw==$Ek/kCLzKpKK+4s+7vrSZ4tRFGex4c62Fk8gzKfonU84=',
    'Manager',
    '22222222-2222-2222-2222-222222222222',
    true
  ),
  (
    'ffffffff-ffff-ffff-ffff-ffffffffffff',
    'Finance Demo',
    'finance@demo.com',
    'pbkdf2-sha256$100000$06AdCu/9TXvcoluWBJ50Ig==$wWwy8ss4sO3ycavg5HT095PhG7RqOTohPCLIZ6vZdGc=',
    'Finance',
    '11111111-1111-1111-1111-111111111111',
    true
  )
on conflict ("Email") do nothing;
