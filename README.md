# API Contact book System with .NET

API developed for contact management. The API has the following functionality:
- Authentication
- Sending email to reset forgotten password
- Management of contacts
- Documentation with Swagger

<img src="./Prints/Screenshot_2.jpg" />

## Execution
Before running APA, you need to complete the following step:

### Installation of packages
`dotnet add package Microsoft.EntityFrameworkCore.Design`<br/>
`dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore`<br/>
`dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer`<br/>
`dotnet add package Pomelo.EntityFrameworkCore.MySql`<br/>

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