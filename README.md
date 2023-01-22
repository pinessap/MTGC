# MTGC | Ines Petrusic

## Git Link
https://github.com/pinessap/MTGC

## Design
Mein Programm stellt einen multithreaded REST Server dar, welcher Daten in und aus einer Postgres-Datenbank lädt. Dabei wird jede Verbindung in einem neuen Thread verarbeitet. 

Bei Empfang einer Request wird diese an meine Response-Class gesendet und dort dann weiter an meine ServerMethods-Class geschickt, wobei hier nur wichtige Daten, wie z.B. der Request-Body weitergeleitet werden. Die ServerMethods-Class beinhaltet hier alle gespeicherten Daten wie User, Cards, Packages und Trading Deals. Thread safety wird durch Locks und entsprechenden Datenstrukturen (z.B. ConcurrentDictionary) gewährt. Werden Daten nicht nur abgerufen, sondern erstellt/verändert/gelöscht muss hier auch die Database-Class aufgerufen werden, welche die entsprechenden SQL-Befehle (mit prepared Statements) ausführt. Thread safety wird hier durch Locks und Transactions gewährt. Die ServerMethods-Class führt also die Logik aus und sendet die entsprechenden Daten zurück an die Response-Class welche daraus dann die Response für den Client erstellt. Der Server schickt dann eben diese Response an den Client.

Ein wichtiger Punkt ist, dass bei Start des Programmes/Servers zuerst alle Daten aus der Datenbank in den Server gespeichert werden (siehe dafür den entprechenden ServerMethods-Constructor und die DB.Load-Methods). So muss, wenn ein Client Daten nur einsehen will nicht jedes Mal wieder auf die Datenbank zugegriffen werden. Dieser Ansatz wurde ebenfalls gewählt, weil ich Anfangs die gesamte Logik ohne Datenbank implementiert habe und zuerst sicher gehen wollte, dass mein Programm ohne Datenbank auch funktioniert.

![architecture](https://user-images.githubusercontent.com/1788353/213922474-fb55c7a0-a379-4336-8af2-ef1a7aff6653.png)

## Testing
### Integration Tests
Für die Integrations Tests wurde das vorgebene curl-Skript genommen und angepasst. Es mussten Anpassungen durchgeführt werden, da der User in meinem Programm beim Kauf eines Packages ein willkürliches Package erhält. Somit konnte z.B. das Definieren eines Decks fehlschlagen, da der User nciht unbedingt die angegeben Cards besessen hat. Zusätzlich wurden noch ein paar Testfälle zum curl-Skript hinzugefügt.

### Unit Tests
Im Prinzip habe ich hier versucht für alle wichtigen Klassen, welche zum eigentlichen MonsterTradingCardGame gehören, entsprechende Unit Tests zu erstellen. Insgesamt habe ich 23 Tests, bei denen ich Moq verwende, um die Database-Class zu mocken. So wird sicher gegangen, dass mein DatabaseHandler also nicht versucht unabsichtlich mit meiner Datenbank zu interagieren. So kann ich auch einrichten, dass die entsprechenden Database-Methods immer true returnen und nicht meine Unit Tests zu ServerMethods-Methoden verfälschen.

## Lessons Learned
Ein großer Punkt den ich aus dem Projekt mitnehme ist, dass am Anfang eine gute Planung der Gesamtarchitektur viel Zeit im Nachhinein beim Implementieren sparen kann. Ich denke bei einer ausführlicheren Planung hätte ich später weniger Änderungen durchführen müssen. 

Es bewährt sich auch im Vorhinein sich genügend über verwendete libraries, frameworks, etc. zu informieren. So hab ich z.B. erst im Nachhinein gelernt, dass Npgsql einen connection pool verwendet und sich quasi selbst um diesen kümmert. Also hätte ich alternativ auch pro thread eine neue Connection erstellen können. In Sachen Performance und Scalability bringen Connection Pools oft einen Vorteil.

Durch das Projekt habe ich ebenfalls gelernt wie und wofür man Mocking verwendet und wie diese in Unit Tests sich als sehr hilfreich herausstellen können. So kann man z.B. verifizieren ob bestimmte Methoden aufgerufen wurden ohne irgendwelche nicht relevanten Werte oder Werte, auf welche nicht einfach zugegriffen werden können vergleichen zu müssen. 

## Additional Features
### Unique Feature
#### Booster
Als mandatory unique feaature habe ich im Battle einen Booster implementiert. Pro Battle kann dabei jeder der beiden Spieler nur einmal diesen Booster bekommen. Und zwar bekommt ein Spieler für eine Runde einen x2 Damage Booster, sobald dieser nur noch eine Card im Deck hat. Somit erhöht sich seine Chance, noch länger im Battle zu bleiben und eventuell dieses noch umzudrehen. Zu Beachten ist hier, dass bei einem Booster die Specialities (z.B. Dragon vs Goblin) außer Kraft treten, nicht aber die element effectiveness. Somit ist die Chance, trotzdem noch mit dem Booster zu verlieren immer noch sehr hoch, wenn du z.B. mit deiner Feuer-Card gegen eine Wasser-Card antreten musst. Der Booster wird nämlich am Anfang berechnet (siehe CardsBattle-Method in Battle-Class).

### Optional Feautures
#### Critical Hit
Pro runde hat hier jeder der beiden Spieler eine 10%-Chance, einen Critical Hit zu machen (siehe getCrit-Method in Battle-Class). Hier wird am Ende der Damage-Berechnung nochmal der Damage x1.5 gerechnet (siehe CardsBattle-Method in Battle-Class). 

#### Elo Calculation
Statt der einfach +3 für einen Win, -5 für einen Loss wird hier bei der Elo-Berechnung zuerst ein Erwartungswert berechnet und mithilfe von diesem dann der entsprechende Elo Wert. 

```
int K = GetKValue(playerElo, opponentElo);
            double expectedScore = 1 / (1 + Math.Pow(10, (opponentElo.Value - playerElo.Value) / 400.0));
            if (isDraw)
            {
                return playerElo.Value + (int)(K * (0.5 - expectedScore));
            }
            else
            {
                return playerElo.Value + (int)(K * ((playerWon ? 1 : 0) - expectedScore));
            }
```

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
