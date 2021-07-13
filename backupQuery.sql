USE Users;
GO
BACKUP DATABASE Users
TO DISK = 'D:\Users.bak'
   WITH FORMAT,
      MEDIANAME = 'OV2USERS',
      NAME = 'Full Backup of Users';
GO