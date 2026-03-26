---
modified: 2026-03-26T22:59:43+05:30
---
# System Architecture

## Faculty TimeGrid Lite

---

## 1. Overview

Faculty TimeGrid Lite follows a **monolithic web application architecture** built using .NET 8 Razor Pages.

The system is structured into logical layers to separate concerns such as UI, business logic, and data access.

---

## 2. Architecture Type

- Monolithic Architecture
    
- Layered Design Pattern
    

This approach is chosen for simplicity, ease of development, and suitability for small-scale applications.

---

## 3. High-Level Architecture

The system consists of three main layers:

1. Presentation Layer
    
2. Application Layer
    
3. Data Layer
    

---

## 4. Layer Description

### 4.1 Presentation Layer (UI)

- Built using Razor Pages
    
- Handles user interaction
    
- Displays forms and tables
    
- Collects input from Admin and Teacher
    

**Responsibilities:**

- Render UI
    
- Capture user input
    
- Send requests to application layer
    

---

### 4.2 Application Layer

- Contains Razor Page Handlers / backend logic
    
- Acts as a bridge between UI and database
    

**Responsibilities:**

- Process user requests
    
- Apply basic business rules
    
- Perform CRUD operations
    
- Prepare data for UI
    

---

### 4.3 Data Layer

- Implemented using Entity Framework Core
    
- Uses SQLite database
    

**Responsibilities:**

- Store data
    
- Retrieve data
    
- Manage relationships between entities
    

---

## 5. Data Flow

The flow of data in the system is as follows:

User Action → Razor Page (UI) → Handler (Application Layer) → DbContext → Database  
Database → DbContext → Handler → Razor Page → Display to User

---

## 6. Component Interaction

### Example: Creating a Timetable Slot

1. Admin fills form in UI
    
2. Form submits data to Razor Page Handler
    
3. Handler validates input
    
4. Handler uses DbContext to save data
    
5. Data is stored in database
    
6. Confirmation shown on UI
    

---

### Example: Viewing Teacher Timetable

1. Teacher selects their name
    
2. Request sent to backend handler
    
3. Handler queries database using TeacherId
    
4. Relevant timetable slots are fetched
    
5. Data is displayed on UI
    

---

## 7. Design Principles Used

- Separation of Concerns
    
- Simplicity over complexity
    
- Minimal dependencies
    
- Clear data flow
    

---

## 8. Advantages of This Architecture

- Easy to develop and maintain
    
- Suitable for small projects
    
- Clear structure for learning purposes
    
- Minimal setup and deployment effort
    

---

## 9. Limitations

- Not suitable for large-scale systems
    
- Limited scalability
    
- Tight coupling between components
    

---

## 10. Conclusion

The chosen architecture ensures that the system remains simple, structured, and easy to implement while still demonstrating key software engineering concepts such as layered design and data flow management.