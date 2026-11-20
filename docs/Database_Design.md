# RaceDay Database Design

## 1. Users

Stores all RaceDay accounts.

| Attribute | Type | Description |
|---|---|---|
| UserId | INT | Primary key |
| FirstName | NVARCHAR(50) | User first name |
| LastName | NVARCHAR(50) | User surname |
| Email | NVARCHAR(255) | Unique email address |
| PasswordHash | NVARCHAR(255) | Hashed password |
| Role | NVARCHAR(20) | Organiser or Participant |
| PhoneNumber | NVARCHAR(20) | Contact number |
| CreatedAt | DATETIME2 | Account creation date |

## 2. Events

Stores races and other events.

| Attribute | Type | Description |
|---|---|---|
| EventId | INT | Primary key |
| OrganiserId | INT | Foreign key to Users |
| EventName | NVARCHAR(150) | Event name |
| Description | NVARCHAR(1000) | Event description |
| EventDate | DATE | Event date |
| StartTime | TIME | Event start time |
| Location | NVARCHAR(200) | Event location |
| DistanceKm | DECIMAL(6,2) | Main event distance |
| Status | NVARCHAR(20) | Planned, Open, Closed or Completed |
| CreatedAt | DATETIME2 | Creation date |

## 3. Categories

Stores race categories.

| Attribute | Type | Description |
|---|---|---|
| CategoryId | INT | Primary key |
| CategoryName | NVARCHAR(100) | Category name |
| Description | NVARCHAR(500) | Category description |

## 4. EventCategories

Associates categories with individual events.

| Attribute | Type | Description |
|---|---|---|
| EventCategoryId | INT | Primary key |
| EventId | INT | Foreign key to Events |
| CategoryId | INT | Foreign key to Categories |
| EntryFee | DECIMAL(10,2) | Entry fee |
| MaximumParticipants | INT | Maximum entries |

## 5. Enrolments

Stores participant event registrations.

| Attribute | Type | Description |
|---|---|---|
| EnrolmentId | INT | Primary key |
| ParticipantId | INT | Foreign key to Users |
| EventCategoryId | INT | Foreign key to EventCategories |
| EnrolmentDate | DATETIME2 | Registration date |
| Status | NVARCHAR(20) | Registered, Cancelled or Completed |

## 6. Results

Stores participant race results.

| Attribute | Type | Description |
|---|---|---|
| ResultId | INT | Primary key |
| EnrolmentId | INT | Foreign key to Enrolments |
| FinishTime | TIME | Finishing time |
| Position | INT | Overall/category position |
| RecordedAt | DATETIME2 | Time result was recorded |

## Event Design Decisions

Each event belongs to one Organiser through `OrganiserId`.

The event stores:

- Event name
- Description
- Date
- Start time
- Location
- Distance
- Status
- Creation date

The Organiser relationship allows the API to determine which Organiser created an event.

This will support role-based authorisation in Part 2.

## Category Design Decisions

Categories are stored independently from events.

The `EventCategories` entity connects a category to an event.

This allows the same category definition to be associated with multiple events while allowing event-specific information such as:

- Entry fee
- Maximum participants

## Enrolment Design Decisions

An enrolment belongs to one participant and one event category.

This means that when a participant enters an event, the database records exactly which category they selected.

A unique constraint will prevent the same participant from registering for the same event category more than once.

## Result Design Decisions

Results are linked to an enrolment rather than directly to a participant.

This ensures that every result belongs to a specific registration.

A result can contain:

- Finish time
- Position
- Recorded date

An enrolment may exist without a result because the participant may have registered but not yet completed the event.