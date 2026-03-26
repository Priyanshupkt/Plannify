---
modified: 2026-03-26T23:03:28+05:30
---
# Business Rules

## Faculty TimeGrid Lite

---

## 1. Overview

Business rules define the core logic and constraints that govern how the system operates.

These rules ensure consistency in timetable creation and display.

---

## 2. Timetable Slot Rules

1. Each timetable slot must include:
    
    - Day
        
    - Start time
        
    - End time
        
    - Teacher
        
    - Subject
        
    - Class
        
2. A timetable slot represents a single teaching session.
    
3. A slot can be marked as a **GAP**:
    
    - GAP indicates no class assigned
        
    - GAP slots are included in timetable view
        

---

## 3. Teacher Assignment Rules

1. Each slot is assigned to only one teacher.
    
2. A teacher can have multiple slots across different days.
    
3. A teacher may have GAP slots in between teaching slots.
    

---

## 4. Subject and Class Rules

1. Each slot must be linked to:
    
    - One subject
        
    - One class
        
2. A subject can be assigned to multiple classes and slots.
    
3. A class can have multiple timetable slots.
    

---

## 5. Data Entry Rules

1. All data is entered manually by the admin.
    
2. Admin is responsible for:
    
    - Selecting correct teacher, subject, and class
        
    - Entering valid time values
        
3. No automatic validation for:
    
    - Overlapping slots
        
    - Time conflicts
        

---

## 6. Teacher View Rules

1. A teacher can only view:
    
    - Their own assigned timetable slots
        
2. The system filters data using TeacherId.
    
3. The timetable view includes:
    
    - Day
        
    - Time
        
    - Class
        
    - Subject
        
    - GAP indication
        

---

## 7. Workload Calculation Rules

1. Total teaching hours:
    
    - Count of slots where IsGap = false
        
2. Total gap hours:
    
    - Count of slots where IsGap = true
        
3. No advanced calculations are performed.
    

---

## 8. System Constraints

1. No restriction on duplicate or overlapping slots.
    
2. No automatic scheduling logic is applied.
    
3. System prioritizes simplicity over strict enforcement.
    

---

## 9. Error Handling Rules

1. Empty or missing inputs should be prevented using basic validation.
    
2. Invalid entries should not be saved to the database.
    
3. System should display simple error messages where required.
    

---

## 10. Conclusion

These business rules ensure that the system remains simple, predictable, and easy to use while supporting the essential functionality of timetable management.