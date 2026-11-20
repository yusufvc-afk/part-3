# RaceDay Validation Checklist

## Authentication
- Registration requires valid user information.
- Duplicate email addresses are rejected.
- Passwords are hashed before storage.
- Unauthenticated requests to protected endpoints return 401.
- Users without the required role receive 403.

## Events
- Only organisers can create events.
- Only event owners can modify their events.
- Event types are Run, Walk or Cycle.
- Distance must be greater than zero.
- Completed events cannot be modified or deleted.

## Enrolments
- Only participants can enrol.
- Enrolment requires an open event.
- Duplicate active enrolments are rejected.
- Category capacity is checked.
- Cancelled enrolments do not count towards capacity.
- Enrolments with recorded results cannot be cancelled.

## Results
- Only the event organiser can record results.
- Results require a completed event.
- Cancelled enrolments cannot receive results.
- Duplicate results are rejected.
- Finish times and positions must be valid.

## Remaining hardening
- Concurrent enrolment requests require database-level capacity protection.
- Cookie-authenticated write endpoints require CSRF protection.
- Database-level finishing-position uniqueness and lifecycle rules require additional enforcement.
