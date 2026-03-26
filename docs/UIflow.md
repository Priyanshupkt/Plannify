---
modified: 2026-03-26T23:04:46+05:30
---
# UI Flow

## Faculty TimeGrid Lite

---

## 1. Overview

This document describes how users navigate through the system and interact with different pages.

The UI is designed to be simple, intuitive, and task-focused.

---

## 2. Main Navigation

The application starts with a simple entry point:

Home Page → Select Role (Admin / Teacher)

---

## 3. Admin Flow

The Admin manages all system data and creates the timetable.

### Step-by-Step Flow

1. Admin opens dashboard
    
2. Admin selects required module:
    
    - Manage Teachers
        
    - Manage Subjects
        
    - Manage Classes
        
    - Create Timetable
        

---

### Admin Navigation Flow

```id="b2kzzp"
Dashboard
   ↓
------------------------
| Teachers Management  |
| Subjects Management  |
| Classes Management   |
| Timetable Creation   |
------------------------
```

---

### Timetable Creation Flow

```id="ql7bb1"
Open Timetable Page
      ↓
Fill Slot Form
      ↓
Select:
  - Day
  - Time
  - Teacher
  - Subject
  - Class
      ↓
(Optional) Mark GAP
      ↓
Save Slot
      ↓
Stored in Database
```

---

## 4. Teacher Flow

Teachers only interact with the system to view their timetable.

### Step-by-Step Flow

1. Teacher selects their name
    
2. System fetches timetable data
    
3. Timetable is displayed
    

---

### Teacher Navigation Flow

```id="4lbizn"
Select Teacher
      ↓
Fetch Data (by TeacherId)
      ↓
Display Timetable
```

---

## 5. Timetable Display Structure

The timetable is shown in a simple readable format:

```id="uyv67d"
Day → Time → Class → Subject → GAP
```

Example:

```id="a51hzg"
Monday
  9–10 → MCA-A → DBMS
  10–11 → GAP
```

---

## 6. Navigation Summary

- Admin handles data input and management
    
- Teacher only consumes data (view only)
    

---

## 7. Design Principles

- Minimal navigation steps
    
- Clear separation between Admin and Teacher flows
    
- No complex menus or nested pages
    
- Focus on usability and clarity
    

---

## 8. Error Handling in UI

- Empty forms show validation messages
    
- No data available → show “No timetable found”
    
- Invalid inputs are prevented before submission
    

---

## 9. Conclusion

The UI flow is designed to be simple and efficient, ensuring that users can complete tasks with minimal steps while maintaining clarity and usability.