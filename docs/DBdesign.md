---
modified: 2026-03-26T23:00:04+05:30
---
# Database Design

## Faculty TimeGrid Lite

---

## 1. Overview

The system uses a **relational database (SQLite)** to store and manage all timetable-related data.

The database is designed to maintain clear relationships between teachers, subjects, classes, and timetable slots.

---

## 2. Entities and Tables

The system consists of four main tables:

1. Teachers
    
2. Subjects
    
3. Classes
    
4. TimetableSlots
    

---

## 3. Table Structures

### 3.1 Teachers Table

Stores information about faculty members.

|Field|Type|Description|
|---|---|---|
|Id|Integer (PK)|Unique identifier|
|Name|Text|Full name of teacher|
|Initials|Text|Short form (e.g., JP, SB)|

---

### 3.2 Subjects Table

Stores subject-related information.

|Field|Type|Description|
|---|---|---|
|Id|Integer (PK)|Unique identifier|
|Name|Text|Subject name|
|Code|Text|Subject code|

---

### 3.3 Classes Table

Stores batch/class details.

|Field|Type|Description|
|---|---|---|
|Id|Integer (PK)|Unique identifier|
|BatchName|Text|Class name (e.g., MCA-A)|
|RoomNo|Text|Assigned room|

---

### 3.4 TimetableSlots Table

Core table storing timetable entries.

|Field|Type|Description|
|---|---|---|
|Id|Integer (PK)|Unique identifier|
|Day|Text|Day of the week|
|StartTime|Text|Start time|
|EndTime|Text|End time|
|TeacherId|Integer (FK)|Reference to Teachers table|
|SubjectId|Integer (FK)|Reference to Subjects table|
|ClassId|Integer (FK)|Reference to Classes table|
|IsGap|Boolean|Indicates free period|

---

## 4. Relationships

The database follows a **one-to-many relationship model**:

- One Teacher → Many TimetableSlots
    
- One Subject → Many TimetableSlots
    
- One Class → Many TimetableSlots
    

---

## 5. Entity Relationship Summary

- Each timetable slot is linked to:
    
    - One teacher
        
    - One subject
        
    - One class
        
- A teacher can have multiple slots across different days
    
- A subject can appear in multiple slots
    
- A class can have multiple scheduled slots
    

---

## 6. Normalization

The database follows **basic normalization principles**:

- No redundant data storage
    
- Each entity stored in a separate table
    
- Relationships handled using foreign keys
    

This ensures consistency and reduces data duplication.

---

## 7. Data Integrity

- Primary keys ensure uniqueness
    
- Foreign keys maintain relationships
    
- Basic validation ensures required fields are not empty
    

---

## 8. Design Decisions

- SQLite chosen for simplicity and easy deployment
    
- Time stored as text to simplify input handling
    
- Separate tables used to maintain modular structure
    

---

## 9. Limitations

- No constraint for overlapping timetable slots
    
- No enforcement of advanced scheduling rules
    
- No cascading rules implemented
    

---

## 10. Conclusion

The database design is simple, normalized, and sufficient for managing timetable data effectively.

It supports all required operations while remaining easy to implement and maintain for a mini project.