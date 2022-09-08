# About this project

This is my solution to a coding challenge posed by MVP Match, for original instructions see [here](https://mvpmatch.notion.site/Backend-1-9a5476e6cb7848ec9f620ce8a64c0d06). 

These are the assumptions and changes I made:

- I used Basic Auth even though it is deemed unsecure, because it was the most straight-forward for testing, debugging and developing. Switching it out for JWT or oAuth would not be too hard, but API tests would have to be adapted.
- The project is self-hosted (as opposed to: hosted by an IIS instance)
- The application does not perform any password hashing on its own, again for simplicity's sake. It is assumed that in production enviroments they would be hashed and salted and stored in a safe location, e.g. something like Azure Keyvault or Hashicorp Vault
- Added the `admin` role: in addition to the `buyer` and `seller` role, the `admin` role was added. I did this to improve security of the `/user` API. Only admins can see all users, or see, modify and delete other users. However, every user can see, update and delete its own record.
- Use persistence: currently all data retention happens in-memory (c.f. [`IEntityStore.cs`](VendingMachine.Api/DataAccess/IEntityStore.cs)). Using EF Core, this could be swapped out for a persistent storage, such as Postgres.



## Project structure
The project is written in .NET 6 using the C# 10 language level. The solution consists of 3 projects:
- `VendingMachine.Api`: contains all domain models, services, application glue code and api controllers etc.
- `VendingMachine.Test`: contains unit tests for all the services
- `VendingMachine.IntegrationTest`: contains integrations/e2e tests for all controllers

Further separating the solution in projects such as `.Api`, `.Core`, `.Models` and `.Data` would be possible but was skipped here to keep things simple.

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

- introduce request DTOs and response DTOs: in order to better shape the read- and write model, data transfer objects (DTOs) should be introduced alongside a mapping layer.
- add validation of request DTOs: incoming data should be validated, e.g. using [FluentValidation](https://docs.fluentvalidation.net/en/latest/)
- add documentation
- improve `ServiceResult` esp. when dealing with Not Authorized 403. They could carry some sort of status code so that we don't have to interpret the `FailureMessage` anymore.
- add seed data from JSON file or postman. In development scenarios it may be useful to add some seed data.

- implement transactions: currently all data access happens sequentially, but in a production environment this needs to be secured with transactions. One way to achieve that would be the [Unit-of-Work Pattern](https://dotnettutorials.net/lesson/unit-of-work-csharp-mvc/)