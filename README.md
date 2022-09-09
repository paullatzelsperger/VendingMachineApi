# About this project

This is my solution to a coding challenge posed by MVP Match, for original instructions see [here](https://mvpmatch.notion.site/Backend-1-9a5476e6cb7848ec9f620ce8a64c0d06). 

These are the assumptions and adaptations I made:

- I used Basic Auth because it was the most straight-forward for testing, debugging and developing. Switching it out for JWT or oAuth would not be too hard, but API tests and the postman collection would have to be adapted. The entire API is sessionless. 
- The project is self-hosted (as opposed to: hosted by an IIS instance). Thus it can be started much like a traditional program, or a Spring Boot app. The webserver is embedded in the application.
- The application does not perform any password hashing on its own, again for simplicity's sake. It is assumed that in production enviroments they would be hashed and salted and stored in a safe location, e.g. something like Azure Keyvault or Hashicorp Vault. The application treats passwords as opaque strings assuming that API clients enforce any password complexity requirements and perform the hashing. Especially when things like JWT or OAuth are used, the user credentials would likely be stored out-of-band.
- the CRUD endpoint for `/user` does not allow to change the password. While it would be trivial to implement, in real-world scenarios there usually is a dedicated endpoint and workflow for that.
- the `/user` endpoint uses a `UserDto` for reading and updating, that does not have the password for data privacy/security reasons.
- Added the `admin` role: in addition to the `buyer` and `seller` role, the `admin` role was added. I did this to improve security of the `/user` API. Only admins can see all users, or see, modify and delete other users. However, every user can see, update and delete its own record.
- In-memory stores: currently all data retention happens through EF Core using the `InMemory` provider. Using EF Core, this could be swapped out for a persistent storage, such as Postgres, in which case migrations would have to be taken into consideration as well.
- Ownership of a product cannot be transferred: the spec states:
  > while POST, PUT and DELETE can be called only by the seller user who created the product
  
  which - taken literally - means that transferring ownership would make it impossible to track the _original_ creator.



## Project structure
The project is written in .NET 6 using the C# 10 language level. The solution consists of 3 projects:
- `VendingMachine.Api`: contains all application glue code and api controllers etc. This is the runnable application project.
- `VendingMachine.Api.Test`: this contains integration/e2e tests for the application
- `VendingMachine.Core`: contains services and some core interfaces
- `VendingMachine.Core`: contains model classes, e.g. `User`, `Project`,...
- `VendingMachine.Core.Test`: contains tests for the core project
- `VendingMachine.Data`: data layer, i.e. EF Core integration
- `VendingMachine.Data.Test`: contains some unit tests for the data layer



## Run the project
Simply clone the project and - assuming you have .NET 6 installed and available in `$PATH` - run the following command on a shell:
```bash
dotnet run
```
If you only want to run the tests, simply execute
```bash
dotnet test
```
To execute REST requests against the API, please find a Postman collection in `Resources/`. Note that before being able to execute any request an user must be created by running the respective Postman request, or by using `cUrl`:
```bash
curl --location --request POST 'http://localhost:5198/api/user' \
--header 'Content-Type: application/json' \
--data-raw '{
    "Id": "12345",
    "username": "paul",
    "password": "asdf",
    "deposit": 150,
    "roles":[
        "buyer", "admin", "seller"
    ]
}'
```
_Note that if you change the `username` and `password`, you'll also have to adapt the authentication in the postman collection!_
## Future Work:

- introduce more request DTOs and response DTOs: in order to better shape the read- and write model, data transfer objects (DTOs) could be introduced alongside a mapping layer.
- add validation of request DTOs: incoming data should be validated, e.g. using [FluentValidation](https://docs.fluentvalidation.net/en/latest/)
- improve `ServiceResult` esp. when dealing with Not Authorized 403. They could carry some sort of status code so that we don't have to interpret the `FailureMessage` anymore.
- add seed data from JSON file or postman. In development scenarios it may be useful to add some seed data.

- implement transactions: currently all data access happens sequentially, but in a production environment this needs to be secured with transactions. One way to achieve that would be the [Unit-of-Work Pattern](https://dotnettutorials.net/lesson/unit-of-work-csharp-mvc/)