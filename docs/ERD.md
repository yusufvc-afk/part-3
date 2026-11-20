
# RaceDay - Entity Relationship Diagram (ERD)

## Entities Overview

The RaceDay database consists of six entities. It supports event management, participant enrolments, event categories, and race results.

| Entity | Purpose |
|---|---|
| **Users** | Stores Organisers and Participants using a Role column. |
| **Events** | Stores events created by Organisers. |
| **Categories** | Stores reusable race categories, such as 10km and 21km. |
| **EventCategories** | Links Events and Categories and stores entry fees and participant limits. |
| **Enrolments** | Records Participants entering a specific EventCategory. |
| **Results** | Stores finish times and finishing positions for Enrolments. |

## Relationships and Cardinality

### 1. Users (Organiser) to Events
- Relationship: One-to-Many (1:M).
- One Organiser can create multiple Events.
- Each Event belongs to exactly one Organiser.
- Foreign Key: Events.OrganiserId references Users.UserId.

### 2. Users (Participant) to Enrolments
- Relationship: One-to-Many (1:M).
- One Participant can have multiple Enrolments.
- Each Enrolment belongs to exactly one Participant.
- Foreign Key: Enrolments.ParticipantId references Users.UserId.

### 3. Events to EventCategories
- Relationship: One-to-Many (1:M).
- One Event can have multiple EventCategories.
- Each EventCategory belongs to exactly one Event.
- Foreign Key: EventCategories.EventId references Events.EventId.

### 4. Categories to EventCategories
- Relationship: One-to-Many (1:M).
- One Category can be associated with multiple Events.
- Each EventCategory references exactly one Category.
- Foreign Key: EventCategories.CategoryId references Categories.CategoryId.

### 5. EventCategories to Enrolments
- Relationship: One-to-Many (1:M).
- One EventCategory can have multiple Enrolments.
- Each Enrolment references exactly one EventCategory.
- Foreign Key: Enrolments.EventCategoryId references EventCategories.EventCategoryId.

### 6. Enrolments to Results
- Relationship: One-to-Zero-or-One (1:0..1).
- An Enrolment may have one Result after the event.
- Each Result belongs to exactly one Enrolment.
- Foreign Key: Results.EnrolmentId references Enrolments.EnrolmentId.
- A UNIQUE constraint on Results.EnrolmentId prevents multiple results for the same enrolment.

## Many-to-Many Relationships

Events and Categories have a many-to-many relationship resolved through EventCategories.

Participants and EventCategories have a many-to-many relationship resolved through Enrolments.

These junction tables prevent duplicated relationship data and support relational database normalisation.

## Mermaid ERD

https://mermaid.ink/img/pako:eNqdVU2P2jAQ_SuRzwHxvZAbgkSLWCiibA8VUuUm02A1sdHY2UKB_14nIWwIDofmFOfNTGbevBmfiC8CIA4BnDIaIo23uOWWft4loLROxTF9ZstN9nkWWKt5GVh-G68nr-O15TGUakljMKJv9AnoxpRF1rs57opK-Udg8ErlzmiwFpE57GonOCyT-CdgGZ-ON-5mtnA71gSBKgjGqoAv6UtxcD-AKwMN2fcHHlLkC4aUs5wlz1xN5lxLxBSkj2yvmODVjHPPqc63jKR1WF8VRbVhdcwLnz4EdCezxfjNmjKpKPdhHhtddWCVyP_hbqKxUCADA39X7FgvpcIi5alOFgaqHrv3LI2ywdHYzqLR3vxJCfdgQazLFR49gKrjgh5YnMQr3TDmsz3VAjNnz1FEcY3-CsyYdCm0MfVq2ZX8b_29_aUquRp13KW_BplEptxzwMx2qSxvbt-3PdO5x_Rw7apCz4oWkpmGJi9lDb7eHw9aLQzzbXc-NxriVAy9Y4nrLMv7pViYffbHsRBCPUcarWyOcsiSEh3LF1xRxqVhWmp99BZkIYfghxJ1Ejfn5gP7-KyiBGbm51uvHGuPIkj83JbYJEQWEEdhAjaJAfWO1kdySuNsidqBbgNx9CtCkBwaAcXfDV9EArdkyy_aX0vwuxBxEQJFEu6I84tGUp-SfaATv947NxPgAeBEJFwRp9vOQhDnRA7EaffbzdFo0HsZ9lu9dqs_GNrkqI26zW6n22t1eq3uqDPqDAcXm_zN_tpqDl_6NoGAKYGL_KbLLrzLP5snIJw?type=png

## Database Constraints

- All six entities have primary keys.
- Users.Email must be unique.
- Users.Role must be Organiser or Participant.
- Events.DistanceKm must be greater than zero.
- Events.Status must be Planned, Open, Closed, or Completed.
- Categories.CategoryName must be unique.
- EventCategories has a unique combination of EventId and CategoryId.
- EventCategories.EntryFee cannot be negative.
- EventCategories.MaximumParticipants must be greater than zero.
- Enrolments has a unique combination of ParticipantId and EventCategoryId.
- Enrolments.Status must be Registered, Cancelled, or Completed.
- Results.EnrolmentId must be unique.
- Results.Position must be greater than zero.

## Database Alignment

This ERD represents the six tables defined in the original RaceDay Part 1 SQL database script.

The Payments entity has been removed because it is not implemented in the SQL schema.

The EventCategories junction entity has been included to accurately represent the many-to-many relationship between Events and Categories.

Any future schema changes required for Part 2 will be documented and reflected in the ERD and Entity Framework Core models.
