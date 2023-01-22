# MTGC | Ines Petrusic

## Git Link
https://github.com/pinessap/MTGC

## Design

![architecture](https://user-images.githubusercontent.com/1788353/213922474-fb55c7a0-a379-4336-8af2-ef1a7aff6653.png)



## Testing
### Integration Test 
### Unit Tests

## Additional Features
### Unique Feature

### Optional Feautures

## Time spent:

| date     | time (h) | comment                                                                                              |
| -------- | -------- | ---------------------------------------------------------------------------------------------------- |
| 20/12/22 | 2.5      | project and repo setup, started developing http server                                               |
| 30/12/22 | 4        | http server: implemented request and response classes                                                |
| 31/12/22 | 3.5      | implemented all user paths (/users, /users/{username}, /sessions)                                    |
| 01/01/23 | 9        | implemented all packages & cards paths (PUT /deck not finished) + /stats & scoreboard                |
| 02/01/23 | 6.5      | finished /deck and implemented all trading paths; implemented battle (no queue yet & not tested yet) |
| 03/01/23 | 7.5      | finished battle (+ battle queue), added comments to everything, fixed small bugs                     |
| 12/01/23 | 2        | added unique feature (battle boost), made methods thread safe (lock), implemented db connection      |
| 13/01/23 | 10.5     | added db operations: loading data first at start of program, register & update user                  |
| 14/01/23 | 5        | added rest of db operations                                                                          |
| 18/01/23 | 7        | added unit tests (users), fixed db connection (treated as pool), fixed battle queue                  |
| 19/01/23 | 7        | added rest of unit tests (battle, trading, package, cards)                                           |
| 20/01/23 | 4.5      | added extra feautures: critical hit in battle, higher sophisticated elo calculation, win ratio       |
| 21/01/23 | 5.5      | tested everything, fixed small bugs                                                                  |
| 22/01/23 |       | added documentation                                                                  |
|  |       | TOTAL TIME                                                              |
_______

## Class Diagram
![ClassDiagram](https://user-images.githubusercontent.com/1788353/213922478-a782a75a-ce87-4e24-ab62-4976aedf2e51.png)
_______
## Database ERD
![db](https://user-images.githubusercontent.com/1788353/213922482-edcbc08e-c137-488b-815d-f676d7f0fb88.png)
