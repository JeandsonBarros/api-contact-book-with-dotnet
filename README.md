<div>
  <img src="https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/MySQL-005C84?style=for-the-badge&logo=mysql&logoColor=white" />
  <img src="https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=JSON%20web%20tokens&logoColor=white"/>
  <img src="https://img.shields.io/badge/json-5E5C5C?style=for-the-badge&logo=json&logoColor=white"/>
</div>

# API Contact Book System with .NET

API developed for contact management. The API has the following functionality:
- Authentication
- Sending email to reset forgotten password
- Management of contacts
- Documentation with Swagger

<img src="./Prints/Screenshot_2.jpg" />

## Packages used in the project
- Microsoft.EntityFrameworkCore.Design<br/>
- Microsoft.AspNetCore.Identity.EntityFrameworkCore<br/>
- Microsoft.AspNetCore.Authentication.JwtBearer<br/>
- Pomelo.EntityFrameworkCore.MySql<br/>

## Execution
Before running the API, you need to complete the following step:

### Install Core Entity Framework Tools
Core Entity Framework Tools for the .NET Command Line Interface. You only need to install it once on the machine,
if you have already installed it, you don't need to run this command:
`dotnet tool install --global dotnet-ef`<br/>

### App Settings
Now you need to pass the appsettings.Development.json file,
the necessary settings for the database, jwt token and email.

- JWTToken config:<br/>
  `"JWT": {
    "Secret": "SecretJWTAuthentication"
},`
 <p/>

- Email config:<br/>
  `"Email":{
    "Host":"smtp.gmail.com",
    "Port": 587,
    "EmailFrom": "example@email.com",
    "PasswordFrom":"passwordemailexample"
},`
<p/>

- Data Base MySql config:<br/>
  `"ConnectionStrings": {
    "DefaultConnection": "server=localhost;user=root;password='';database=ContactBook;"
    }`

<p/>

The appsettings.Development.json file should look like this:
<img src="./Prints/Screenshot_1.jpg" />

### Data Base migration and update
The following commands are used to prepare the configurations of the
tables in the database and execute the configurations accordingly:

`dotnet-ef migrations add CreateTables`<br/>
`dotnet ef database update`

### Comand to run project
Command to run the project If all the previous steps were successfully executed,
just run the following command in the console to run the project:<br/>
`dotnet watch run`
