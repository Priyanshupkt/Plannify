---
modified: 2026-03-26T22:59:18+05:30
---
# Software Requirements Specification (SRS)

## Faculty TimeGrid Lite

---

## 1. Introduction

### 1.1 Purpose

This document provides a detailed description of the functional and non-functional requirements for the Faculty TimeGrid Lite system.

It serves as a reference for development, testing, and evaluation of the project.

---

### 1.2 Scope

Faculty TimeGrid Lite is a web-based application that allows:

- Admin to manage teachers, subjects, and classes
    
- Admin to create timetable slots manually
    
- Teachers to view their assigned timetable
    

The system focuses on simplicity and clarity rather than automation.

---

### 1.3 Definitions

|Term|Meaning|
|---|---|
|Admin|User who manages data and creates timetable|
|Teacher|Faculty member who views assigned schedule|
|Slot|A single timetable entry (day + time + class + subject + teacher)|
|GAP|A free period where no class is assigned|

---

## 2. Overall Description

### 2.1 System Perspective

The system is a standalone web application built using .NET 8 Razor Pages.

It interacts with a relational database (SQLite) to store and retrieve timetable data.

---

### 2.2 User Classes

#### Admin

- Manages all data
    
- Creates timetable slots
    

#### Teacher

- Views personal timetable only
    

---

### 2.3 Assumptions

- Admin manually enters all data correctly
    
- No automated timetable generation is required
    
- System will be used in a small-scale environment
    

---

## 3. Functional Requirements

### 3.1 Teacher Management

The system shall allow admin to:

- Add a new teacher
    
- View all teachers
    
- Delete a teacher
    

---

### 3.2 Subject Management

The system shall allow admin to:

- Add a new subject
    
- View all subjects
    
- Delete a subject
    

---

### 3.3 Class Management

The system shall allow admin to:

- Add a new class/batch
    
- View all classes
    
- Delete a class
    

---

### 3.4 Timetable Slot Management

The system shall allow admin to:

- Create a timetable slot by selecting:
    
    - Day
        
    - Start time
        
    - End time
        
    - Teacher
        
    - Subject
        
    - Class
        
- Mark a slot as GAP
    
- Save the slot to the database
    

---

### 3.5 Teacher Timetable View

The system shall allow teachers to:

- Select their name
    
- View only their assigned timetable slots
    
- View GAP periods
    
- View simple workload summary:
    
    - Total classes
        
    - Total gap hours
        

---

## 4. Non-Functional Requirements

### 4.1 Performance

- The system should respond within 1 second for basic operations
    

---

### 4.2 Usability

- The UI should be simple and easy to navigate
    
- Forms should be clear and minimal
    

---

### 4.3 Reliability

- Data should be stored correctly without loss
    
- The system should not crash during normal usage
    

---

### 4.4 Maintainability

- Code should be modular and readable
    
- Database structure should be simple
    

---

### 4.5 Security

- Basic validation should be implemented
    
- No advanced authentication required (optional feature)
    

---

## 5. System Constraints

- Built using .NET 8 only
    
- Uses SQLite as database
    
- No external automation tools
    
- No AI/ML features
    

---

## 6. Future Enhancements

- Login system for teachers
    
- PDF export of timetable
    
- Slot conflict detection
    
- Admin audit logs
    

---

## 7. Conclusion

This SRS defines a simplified timetable management system that focuses on essential functionality.

It ensures that the project remains achievable while still demonstrating key software engineering concepts such as CRUD operations, database relationships, and structured UI flow.