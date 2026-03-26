---
modified: 2026-03-26T22:58:25+05:30
---
# Plan – Faculty TimeGrid Lite

## 1. Introduction

Faculty TimeGrid Lite is a lightweight .NET 8 web application designed to manage and display teacher-wise timetables in a simple and structured manner.

The system focuses on manual timetable creation by an admin and clear timetable viewing for teachers.

---

## 2. Objective

The main objective of this project is to:

- Store teacher, subject, and class details
    
- Allow admin to create timetable slots manually
    
- Provide teachers with a clear view of their assigned schedule
    
- Demonstrate CRUD operations and relational database usage in .NET
    

---

## 3. Project Scope

### Included Features

- Teacher Management (Add, View, Delete)
    
- Subject Management (Add, View, Delete)
    
- Class/Batch Management (Add, View, Delete)
    
- Timetable Slot Creation
    
- Teacher-wise Timetable Display
    
- Basic workload summary (total classes, gap hours)
    

---

### Excluded Features

To keep the project minimal and achievable:

- No PDF upload or parsing
    
- No automatic timetable generation
    
- No advanced scheduling constraints
    
- No audit logging system
    
- No authentication system (optional if time permits)
    

---

## 4. Development Plan

|Day|Task|
|---|---|
|Day 1|Project setup, database configuration|
|Day 2|CRUD implementation (Teachers, Subjects, Classes)|
|Day 3|Timetable slot creation module|
|Day 4|Teacher timetable view|
|Day 5|UI improvements, testing, bug fixes|

---

## 5. Technology Stack

- Framework: .NET 8 (Razor Pages)
    
- ORM: Entity Framework Core
    
- Database: SQLite
    
- UI: Bootstrap
    

---

## 6. Expected Outcome

At the end of the project:

- Admin can successfully create timetable entries
    
- Data is stored in a relational database
    
- Teachers can view only their assigned timetable
    
- The system demonstrates a complete working flow from data input to output
    

---

## 7. Success Criteria

The project will be considered successful if:

- All CRUD operations function correctly
    
- Timetable slots are created and stored properly
    
- Teacher-specific timetable view works without errors
    
- The application runs smoothly without crashes
    

---

## 8. Future Scope (Optional Enhancements)

- PDF export of timetable
    
- Login system for teachers
    
- Timetable validation (no overlapping slots)
    
- Audit logging for admin actions
    

---

## 9. Conclusion

This project provides a simplified yet practical implementation of a timetable management system. It focuses on clarity, usability, and core backend concepts rather than complex automation, making it ideal for a mini project.