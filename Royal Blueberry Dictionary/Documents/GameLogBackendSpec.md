# Game Log — Spring Boot Backend Spec

Tài liệu này mô tả model MongoDB, REST routes và contract API cho **Flashcard Game Log**, dựa trên logic hiện tại của WPF client.

## 1. Logic phía FE (tham chiếu)

| File | Vai trò |
|------|---------|
| `Model/Game/GameModels.cs` | `GameSession`, `GameLog`, `GameSettings`, `GameCompletionData` |
| `Service/GameLogService.cs` | Lưu local JSON tại `%LocalAppData%/RoyalBlueberryDictionary/GameLogs/GameLog.json` |
| `ViewModel/GameViewModel.cs` | Chơi game, tính `knownCards` / `unknownCards` / `accuracy`, gọi `AddSession()` khi Finish |
| `View/Dialogs/GameHistoryDialog.xaml.cs` | Hiển thị stats + 20 session gần nhất, xóa toàn bộ lịch sử |

### Luồng khi kết thúc game

1. User chọn nguồn từ (`All` hoặc `tagId`) và số thẻ trong `GameSettingsDialog`.
2. `GameViewModel.CompleteGame()` tính:
   - `knownCards` = thẻ đánh dấu Known + thẻ đã xem nhưng không Skip
   - `unknownCards` = số thẻ Skip
   - `accuracyPercentage` = `knownCards / totalCards * 100`
   - `skippedCardIndices`, `skippedWords`
   - `duration` = `endTime - startTime`
3. `GameLogService.AddSession(session)` append vào log local.

### Trường `dataSource`

| Giá trị FE | Ý nghĩa |
|------------|---------|
| `"All"` | Toàn bộ từ của user |
| `"<tagId>"` | Từ theo tag (số nguyên dạng string) |

`dataSourceName` là label hiển thị UI, ví dụ: `"📚 All Words"`, `"🏷️ IELTS"`.

---

## 2. Thiết kế MongoDB

Mỗi **session** là 1 document riêng (dễ query, phân trang, sync từng lần chơi).  
Stats tổng hợp (`totalGamesPlayed`, `averageAccuracy`...) **tính bằng aggregation**, không lưu document `GameLog` riêng.

### Collection: `Game Sessions`

```java
package com.royalblueberry.dictionary.model;

import lombok.*;
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.index.CompoundIndex;
import org.springframework.data.mongodb.core.index.Indexed;
import org.springframework.data.mongodb.core.mapping.Document;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;

@Getter
@Setter
@AllArgsConstructor
@NoArgsConstructor
@Document(collection = "Game Sessions")
@CompoundIndex(name = "user_startTime_idx", def = "{'userId': 1, 'startTime': -1}")
public class GameSession {

    @Id
    private String id;

    @Indexed
    private String userId;

    private LocalDateTime startTime;
    private LocalDateTime endTime;

    /** "All" hoặc tagId dạng string */
    private String dataSource;
    private String dataSourceName;

    private int totalCards;
    private int knownCards;
    private int unknownCards;
    private double accuracyPercentage;

    /** Thời lượng tính bằng giây (FE: TimeSpan → gửi số giây) */
    private long durationSeconds;

    private List<Integer> skippedCardIndices = new ArrayList<>();
    private List<String> skippedWords = new ArrayList<>();

    private LocalDateTime createdAt;
}
```

### DTO — Request lưu session (POST)

```java
package com.royalblueberry.dictionary.dto.gamelog;

import lombok.*;

import java.time.LocalDateTime;
import java.util.List;

@Getter
@Setter
@AllArgsConstructor
@NoArgsConstructor
public class SaveGameSessionRequest {

    private LocalDateTime startTime;
    private LocalDateTime endTime;
    private String dataSource;
    private String dataSourceName;
    private int totalCards;
    private int knownCards;
    private int unknownCards;
    private double accuracyPercentage;
    private long durationSeconds;
    private List<Integer> skippedCardIndices;
    private List<String> skippedWords;
}
```

### DTO — Response thống kê tổng (GET summary)

```java
package com.royalblueberry.dictionary.dto.gamelog;

import lombok.*;

@Getter
@Setter
@AllArgsConstructor
@NoArgsConstructor
public class GameLogSummaryResponse {

    private int totalGamesPlayed;
    private int totalCardsStudied;
    private double averageAccuracy;
    private long totalStudyTimeSeconds;
}
```

---

## 3. Repository

```java
package com.royalblueberry.dictionary.repository;

import com.royalblueberry.dictionary.model.GameSession;
import org.springframework.data.domain.Pageable;
import org.springframework.data.mongodb.repository.MongoRepository;

import java.util.List;

public interface GameSessionRepository extends MongoRepository<GameSession, String> {

    List<GameSession> findByUserIdOrderByStartTimeDesc(String userId, Pageable pageable);

    long countByUserId(String userId);

    void deleteByUserId(String userId);
}
```

---

## 4. Service (gợi ý)

```java
@Service
@RequiredArgsConstructor
public class GameLogService {

    private final GameSessionRepository gameSessionRepository;

    public GameSession saveSession(String userId, SaveGameSessionRequest req) {
        GameSession session = new GameSession();
        session.setUserId(userId);
        session.setStartTime(req.getStartTime());
        session.setEndTime(req.getEndTime());
        session.setDataSource(req.getDataSource());
        session.setDataSourceName(req.getDataSourceName());
        session.setTotalCards(req.getTotalCards());
        session.setKnownCards(req.getKnownCards());
        session.setUnknownCards(req.getUnknownCards());
        session.setAccuracyPercentage(req.getAccuracyPercentage());
        session.setDurationSeconds(req.getDurationSeconds());
        session.setSkippedCardIndices(req.getSkippedCardIndices());
        session.setSkippedWords(req.getSkippedWords());
        session.setCreatedAt(LocalDateTime.now());
        return gameSessionRepository.save(session);
    }

    public List<GameSession> getRecentSessions(String userId, int limit) {
        return gameSessionRepository.findByUserIdOrderByStartTimeDesc(
            userId, PageRequest.of(0, limit));
    }

    public GameLogSummaryResponse getSummary(String userId) {
        List<GameSession> sessions = gameSessionRepository
            .findByUserIdOrderByStartTimeDesc(userId, PageRequest.of(0, Integer.MAX_VALUE));

        int totalGames = sessions.size();
        int totalCards = sessions.stream().mapToInt(GameSession::getTotalCards).sum();
        double avgAccuracy = sessions.isEmpty() ? 0
            : sessions.stream().mapToDouble(GameSession::getAccuracyPercentage).average().orElse(0);
        long totalSeconds = sessions.stream().mapToLong(GameSession::getDurationSeconds).sum();

        return new GameLogSummaryResponse(totalGames, totalCards, avgAccuracy, totalSeconds);
    }

    public void clearAllSessions(String userId) {
        gameSessionRepository.deleteByUserId(userId);
    }
}
```

---

## 5. REST Routes

Base URL (đồng bộ với FE `appsettings.json`): `http://localhost:8080/api/`

Tất cả route dưới đây **yêu cầu Bearer token** (giống `packages`, `tags`).

| Method | Route | Mô tả |
|--------|-------|-------|
| `POST` | `/api/game-logs/sessions` | Lưu 1 session sau khi user Finish game |
| `GET` | `/api/game-logs/sessions` | Lấy danh sách session gần nhất |
| `GET` | `/api/game-logs/sessions/{id}` | Lấy chi tiết 1 session |
| `GET` | `/api/game-logs/summary` | Thống kê tổng (history dialog) |
| `DELETE` | `/api/game-logs/sessions` | Xóa toàn bộ lịch sử của user hiện tại |

### Controller

```java
@RestController
@RequestMapping("/api/game-logs")
@RequiredArgsConstructor
public class GameLogController {

    private final GameLogService gameLogService;

    @PostMapping("/sessions")
    public ResponseEntity<GameSession> saveSession(
            @AuthenticationPrincipal UserPrincipal principal,
            @RequestBody SaveGameSessionRequest request) {
        return ResponseEntity.ok(gameLogService.saveSession(principal.getId(), request));
    }

    @GetMapping("/sessions")
    public ResponseEntity<List<GameSession>> getRecentSessions(
            @AuthenticationPrincipal UserPrincipal principal,
            @RequestParam(defaultValue = "20") int limit) {
        return ResponseEntity.ok(gameLogService.getRecentSessions(principal.getId(), limit));
    }

    @GetMapping("/sessions/{id}")
    public ResponseEntity<GameSession> getSession(
            @AuthenticationPrincipal UserPrincipal principal,
            @PathVariable String id) {
        // verify session.userId == principal.getId()
        return ResponseEntity.ok(/* ... */);
    }

    @GetMapping("/summary")
    public ResponseEntity<GameLogSummaryResponse> getSummary(
            @AuthenticationPrincipal UserPrincipal principal) {
        return ResponseEntity.ok(gameLogService.getSummary(principal.getId()));
    }

    @DeleteMapping("/sessions")
    public ResponseEntity<Void> clearAllSessions(
            @AuthenticationPrincipal UserPrincipal principal) {
        gameLogService.clearAllSessions(principal.getId());
        return ResponseEntity.noContent().build();
    }
}
```

---

## 6. JSON Contract (camelCase — khớp FE `JsonNamingPolicy.CamelCase`)

### POST `/api/game-logs/sessions`

**Request**

```json
{
  "startTime": "2026-06-09T14:30:00",
  "endTime": "2026-06-09T14:35:42",
  "dataSource": "All",
  "dataSourceName": "📚 All Words",
  "totalCards": 10,
  "knownCards": 8,
  "unknownCards": 2,
  "accuracyPercentage": 80.0,
  "durationSeconds": 342,
  "skippedCardIndices": [2, 7],
  "skippedWords": ["abandon", "zealous"]
}
```

**Response `201 Created`**

```json
{
  "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "userId": "user-abc-123",
  "startTime": "2026-06-09T14:30:00",
  "endTime": "2026-06-09T14:35:42",
  "dataSource": "All",
  "dataSourceName": "📚 All Words",
  "totalCards": 10,
  "knownCards": 8,
  "unknownCards": 2,
  "accuracyPercentage": 80.0,
  "durationSeconds": 342,
  "skippedCardIndices": [2, 7],
  "skippedWords": ["abandon", "zealous"],
  "createdAt": "2026-06-09T14:35:43"
}
```

### GET `/api/game-logs/sessions?limit=20`

**Response `200 OK`**

```json
[
  {
    "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "userId": "user-abc-123",
    "startTime": "2026-06-09T14:30:00",
    "endTime": "2026-06-09T14:35:42",
    "dataSource": "3",
    "dataSourceName": "🏷️ IELTS",
    "totalCards": 10,
    "knownCards": 8,
    "unknownCards": 2,
    "accuracyPercentage": 80.0,
    "durationSeconds": 342,
    "skippedCardIndices": [2, 7],
    "skippedWords": ["abandon", "zealous"],
    "createdAt": "2026-06-09T14:35:43"
  }
]
```

### GET `/api/game-logs/summary`

**Response `200 OK`**

```json
{
  "totalGamesPlayed": 15,
  "totalCardsStudied": 150,
  "averageAccuracy": 76.4,
  "totalStudyTimeSeconds": 5120
}
```

### DELETE `/api/game-logs/sessions`

**Response `204 No Content`**

---

## 7. Mapping FE ↔ BE

| FE (`GameSession`) | BE (`GameSession`) | Ghi chú |
|--------------------|--------------------|---------|
| `Id` | `id` | BE sinh khi POST |
| — | `userId` | Lấy từ JWT, FE không gửi |
| `StartTime` | `startTime` | `LocalDateTime` / ISO-8601 |
| `EndTime` | `endTime` | |
| `DataSource` | `dataSource` | |
| `DataSourceName` | `dataSourceName` | |
| `TotalCards` | `totalCards` | |
| `KnownCards` | `knownCards` | |
| `UnknownCards` | `unknownCards` | |
| `AccuracyPercentage` | `accuracyPercentage` | |
| `Duration` (TimeSpan) | `durationSeconds` | FE: `(int)duration.TotalSeconds` |
| `SkippedCardIndices` | `skippedCardIndices` | |
| `SkippedWords` | `skippedWords` | |
| `DurationText` | — | Chỉ UI, không lưu BE |

| FE `GameLogService` | BE route |
|-------------------|----------|
| `AddSession()` | `POST /api/game-logs/sessions` |
| `GetRecentSessions(20)` | `GET /api/game-logs/sessions?limit=20` |
| `GetTotalGamesPlayed()` + `GetTotalCardsStudied()` + `GetAverageAccuracy()` + `GetTotalStudyTime()` | `GET /api/game-logs/summary` |
| `ClearAllSessions()` | `DELETE /api/game-logs/sessions` |

---

## 8. Gợi ý tích hợp FE (bước sau)

Khi BE sẵn sàng, mở rộng `GameLogService` WPF:

```csharp
// Sau SaveLog() local, sync lên BE (fire-and-forget hoặc queue)
await _api.PostAsync<GameSession>("game-logs/sessions", new {
    startTime = session.StartTime,
    endTime = session.EndTime,
    dataSource = session.DataSource,
    dataSourceName = session.DataSourceName,
    totalCards = session.TotalCards,
    knownCards = session.KnownCards,
    unknownCards = session.UnknownCards,
    accuracyPercentage = session.AccuracyPercentage,
    durationSeconds = (long)session.Duration.TotalSeconds,
    skippedCardIndices = session.SkippedCardIndices,
    skippedWords = session.SkippedWords
});
```

`GameHistoryDialog` có thể ưu tiên đọc từ API, fallback local JSON khi offline.

---

## 9. HTTP Status codes

| Code | Tình huống |
|------|------------|
| `200` | GET thành công |
| `201` | POST tạo session thành công |
| `204` | DELETE thành công |
| `400` | Request body thiếu/sai (totalCards < 0, endTime < startTime...) |
| `401` | Chưa đăng nhập |
| `403` | Truy cập session của user khác |
| `404` | `GET /sessions/{id}` không tồn tại |
