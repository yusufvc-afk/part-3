# RaceDay - API Endpoint Plan (Part 2)

This document updates the Part 1 plan to match the required ASP.NET Core session-based API and the six-table RaceDay database. All `/api/` routes are case-insensitive in ASP.NET Core. `Any` means authenticated. `Organiser` endpoints require organiser role and, for event-specific changes, ownership. `Participant` endpoints require participant role. Protected routes return **401** when unauthenticated and **403** for the wrong role or an event owned by someone else. Requests with invalid data return **400**; missing resources return **404**. The API uses a server-side session cookie, not JWT. Passwords are hashed, never returned.

## Authentication
| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Create an organiser or participant account | None | `{firstName,lastName,email,password,role,phoneNumber?}` | 201 user ID, name and role; 400 invalid fields; 409 duplicate email |
| POST | /api/auth/login | Validate credentials and create a server-side session | None | `{email,password}` | 200 user ID and role, Set-Cookie; 400 invalid input; 401 invalid credentials |
| POST | /api/auth/logout | End current session | Any | None | 204; 401 unauthenticated |

## User profile
| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/users/me | View own profile | Any | None | 200 profile excluding password hash; 401 |
| PUT | /api/users/me | Update own personal details | Any | `{firstName,lastName,email,phoneNumber?}` | 200 updated profile; 400; 401; 409 duplicate email |
| GET | /api/users/{id} | Look up participant basic details for result administration | Organiser | None | 200 limited participant profile; 401; 403; 404 |

## Events
| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events | Browse upcoming events | None | None | 200 event list |
| GET | /api/events/mine | View own organised events | Organiser | None | 200 list; 401; 403 |
| GET | /api/events/{id} | View event details and linked categories | None | None | 200 event; 404 |
| POST | /api/events | Create an event | Organiser | `{eventName,description?,eventDate,startTime,location,distanceKm,eventType,status?}` | 201 event; 400; 401; 403 |
| PUT | /api/events/{id} | Update own event | Organiser | `{eventName,description?,eventDate,startTime,location,distanceKm,eventType,status}` | 200 event; 400; 401; 403; 404 |
| DELETE | /api/events/{id} | Delete own event when permitted by enrolment rules | Organiser | None | 204; 401; 403; 404; 409 dependent enrolments |

## Categories and event categories
`Categories` are reusable definitions. `EventCategories` links a category to an event and stores entry fee and capacity. This preserves the original SQL relationship.

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/categories | List reusable category definitions | None | None | 200 list |
| GET | /api/events/{eventId}/categories | List categories available for an event | None | None | 200 list with eventCategoryId; 404 event missing |
| POST | /api/events/{eventId}/categories | Attach a new or existing category to own event | Organiser | `{categoryId?,categoryName?,description?,entryFee,maximumParticipants}` | 201 event category; 400; 401; 403; 404; 409 duplicate |
| PUT | /api/categories/{id} | Update event-category capacity/fee (id is eventCategoryId) | Organiser | `{entryFee,maximumParticipants}` | 200; 400; 401; 403; 404 |
| DELETE | /api/categories/{id} | Remove an event-category link when no enrolments depend on it | Organiser | None | 204; 401; 403; 404; 409 |

## Enrolments
| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/enrolments | Enrol current participant in an event category | Participant | `{eventCategoryId}` | 201 enrolment; 400 category full or closed; 401; 403; 404; 409 duplicate |
| GET | /api/enrolments/mine | View own enrolments | Participant | None | 200 list; 401; 403 |
| GET | /api/events/{eventId}/enrolments | View enrolments for own event | Organiser | None | 200 list; 401; 403; 404 |
| DELETE | /api/enrolments/{id} | Cancel own enrolment | Participant | None | 204 (or 200 updated status); 401; 403; 404; 409 if already finalised |

## Results
| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/enrolments/{enrolmentId}/results | Record a finish time and position for an enrolment in own event | Organiser | `{finishTime,position}` | 201 result; 400; 401; 403; 404; 409 duplicate result |
| PUT | /api/results/{id} | Correct result belonging to own event | Organiser | `{finishTime,position}` | 200; 400; 401; 403; 404 |
| GET | /api/results/mine | View personal result history | Participant | None | 200 list; 401; 403 |
| GET | /api/events/{eventId}/results | View event results/leaderboard | None | None | 200 list; 404 |

## Part 1 to Part 2 amendments
- JWT token login replaced with server-side session login and logout.
- `fullName` split into `firstName` and `lastName`, matching Users columns.
- `province` removed from event payload because the original SQL stores `Location` instead.
- Added `startTime`, `distanceKm`, and `eventType` to event requests. `EventType` is a Part 2 schema addition (Run, Walk, Cycle) and must be added to SQL, ERD, and EF Core consistently.
- Category requests use the `EventCategories` junction entity; enrolment uses `eventCategoryId` rather than `categoryId`.
- Removed unsupported race-number and result-status fields from promised responses/requests.
- Authentication and authorisation errors are explicitly documented.
