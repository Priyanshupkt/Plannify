---
modified: 2026-03-26T23:05:46+05:30
---
# Reusable Mapping

## Faculty TimeGrid Lite

---

## 1. Overview

Reusable mapping defines how data from database entities is transformed and reused across different parts of the application, especially in UI rendering and logic handling.

This improves code clarity, reduces duplication, and ensures consistency.

---

## 2. Entity Relationships Mapping

The core relationships in the system are:

- Teacher → TimetableSlots (1:N)
    
- Subject → TimetableSlots (1:N)
    
- Class → TimetableSlots (1:N)
    

Each timetable slot acts as a central mapping point connecting all entities.

---

## 3. Timetable Data Mapping

A timetable slot combines data from multiple tables:

### Database Level Mapping

- TeacherId → Teacher.Name
    
- SubjectId → Subject.Name
    
- ClassId → Class.BatchName
    

---

### UI Level Mapping

Each slot is displayed as:

Day → Time → Class → Subject → Teacher

Example:

Monday → 9–10 → MCA-A → DBMS → JP

---

## 4. View Model Concept

Instead of directly exposing raw database entities, data is structured into a display-friendly format.

### Example Mapping

TimetableSlot → View Model:

- Day
    
- StartTime
    
- EndTime
    
- TeacherName
    
- SubjectName
    
- ClassName
    
- IsGap
    

---

## 5. Reusable Components

### 5.1 Dropdown Data

- Teacher list reused in:
    
    - Timetable creation
        
- Subject list reused in:
    
    - Timetable creation
        
- Class list reused in:
    
    - Timetable creation
        

---

### 5.2 Table Display

- Same table structure reused for:
    
    - Teachers list
        
    - Subjects list
        
    - Classes list
        

---

## 6. Data Fetching Reuse

- Common query pattern:
    
    - Fetch timetable by TeacherId
        
- This logic can be reused for:
    
    - Teacher dashboard
        
    - Future API endpoints
        

---

## 7. Benefits of Reusable Mapping

- Reduces code duplication
    
- Improves maintainability
    
- Ensures consistent data display
    
- Simplifies UI rendering
    

---

## 8. Design Approach

- Keep mapping simple and readable
    
- Avoid unnecessary transformation layers
    
- Use consistent naming across entities
    

---

## 9. Future Scope

- Introduce dedicated ViewModels for better separation
    
- Add mapping utilities or services
    
- Extend mapping for API responses
    

---

## 10. Conclusion

Reusable mapping ensures that data flows efficiently from the database to the UI while maintaining clarity and consistency across the application.

It plays a key role in keeping the system structured and easy to maintain.