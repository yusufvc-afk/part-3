# RaceDay Swagger API Guide

## Starting the API

From the RaceDay.Api directory:

```powershell
dotnet run
```

Open the HTTPS localhost address displayed in the terminal and add `/swagger`.

## Authentication

1. Register an Organiser or Participant.
2. Log in through POST /api/auth/login.
3. Use the same Swagger browser session for authenticated requests.
4. Log out through POST /api/auth/logout.

## Main API endpoints

| Feature | Endpoint |
|---|---|
| Health | GET /api/health |
| Register | POST /api/auth/register |
| Login | POST /api/auth/login |
| Logout | POST /api/auth/logout |
| Profile | GET /api/profile/me |
| Events | GET /api/events |
| Create event | POST /api/events |
| Categories | GET /api/categories |
| Event categories | GET /api/events/{eventId}/categories |
| Enrol | POST /api/enrolments |
| My enrolments | GET /api/enrolments/my |
| Organiser enrolments | GET /api/events/{eventId}/enrolments |
| Record result | POST /api/results |
| My results | GET /api/results/my |

## Expected HTTP responses

- 200: Successful request
- 201: Resource created
- 204: Resource deleted
- 400: Invalid request
- 401: Authentication required
- 403: Insufficient permissions
- 404: Resource not found
- 409: Conflicting request

## Evidence

Capture screenshots of the Swagger endpoint list and successful authentication, event, enrolment and results requests.
