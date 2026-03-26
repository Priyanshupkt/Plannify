---
modified: 2026-03-26T23:04:20+05:30
---
# API Contract

## Faculty TimeGrid Lite

---

## 1. Overview

This document defines the API structure for the Faculty TimeGrid Lite system.

Although the application is built using Razor Pages, these endpoints represent how the system can expose its functionality in a structured manner.

---

## 2. Base URL

```
http://localhost:5000/api
```

---

## 3. Common Conventions

- Data format: JSON
    
- HTTP methods used:
    
    - GET → Retrieve data
        
    - POST → Create data
        
    - PUT → Update data
        
    - DELETE → Remove data
        

---

## 4. Teacher APIs

### 4.1 Get All Teachers

```
GET /teachers
```

**Response:**

```json
[
  {
    "id": 1,
    "name": "John Paul",
    "initials": "JP"
  }
]
```

---

### 4.2 Create Teacher

```
POST /teachers
```

**Request:**

```json
{
  "name": "John Paul",
  "initials": "JP"
}
```

---

### 4.3 Update Teacher

```
PUT /teachers/{id}
```

---

### 4.4 Delete Teacher

```
DELETE /teachers/{id}
```

---

## 5. Subject APIs

### 5.1 Get All Subjects

```
GET /subjects
```

---

### 5.2 Create Subject

```
POST /subjects
```

**Request:**

```json
{
  "name": "Database Systems",
  "code": "DB101"
}
```

---

## 6. Class APIs

### 6.1 Get All Classes

```
GET /classes
```

---

### 6.2 Create Class

```
POST /classes
```

**Request:**

```json
{
  "batchName": "MCA-A",
  "roomNo": "G03"
}
```

---

## 7. Timetable APIs

### 7.1 Create Timetable Slot

```
POST /timetable
```

**Request:**

```json
{
  "day": "Monday",
  "startTime": "09:00",
  "endTime": "10:00",
  "teacherId": 1,
  "subjectId": 2,
  "classId": 3,
  "isGap": false
}
```

---

### 7.2 Get Timetable by Teacher

```
GET /timetable?teacherId={id}
```

**Response:**

```json
[
  {
    "day": "Monday",
    "startTime": "09:00",
    "endTime": "10:00",
    "subject": "DBMS",
    "class": "MCA-A",
    "isGap": false
  }
]
```

---

## 8. Error Handling

Standard error responses:

```json
{
  "message": "Invalid request"
}
```

---

## 9. Status Codes

- 200 → Success
    
- 201 → Created
    
- 400 → Bad Request
    
- 404 → Not Found
    
- 500 → Server Error
    

---

## 10. Conclusion

This API structure demonstrates how the system can be extended into a service-based architecture.

It ensures clarity in data communication and aligns with standard REST practices.