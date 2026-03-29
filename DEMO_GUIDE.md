# 🎬 LIVE DEMO SCRIPT FOR PROFESSOR

## Pre-Demo Checklist (Complete Before Demo)

```bash
# 1. Verify application is running
curl -s http://localhost:5152/Admin/Timetable/AutoGenerate | grep -q "Auto-Generate Timetable" && echo "✓ Application is running"

# 2. Verify database is seeded
sqlite3 /home/cy3pher/Documents/WorkSpace-Dev/Plannify/Plannify/timegrid.db \
  "SELECT 'Teachers: ' || COUNT(*) FROM Teachers; \
   SELECT 'Subjects: ' || COUNT(*) FROM Subjects; \
   SELECT 'Classes: ' || COUNT(*) FROM ClassBatches; \
   SELECT 'Rooms: ' || COUNT(*) FROM Rooms;"

# 3. Verify build succeeded
cd /home/cy3pher/Documents/WorkSpace-Dev/Plannify && dotnet build -c Debug 2>&1 | grep "Build succeeded"
```

## LIVE DEMO FLOW (10 minutes)

### Step 1: Open Application (30 seconds)
```
URL: http://localhost:5152/Admin/Timetable/AutoGenerate
```

**Show to Professor:**
- "This is the AutoGenerate Timetable page"
- "All validation errors are shown in real-time"
- "We've added validation for:"
  - Valid time ranges
  - Positive slot duration
  - Active academic year and semester

---

### Step 2: Fill Form with VALID Values (1 minute)

**Fields:**
1. **Academic Year:** Select "2025-26" (from dropdown)
2. **Semester:** Select "Semester 1" (from dropdown) 
3. **Class:** Leave blank (means generate for all classes)
4. **Start Hour:** `9`
5. **End Hour:** `17`
6. **Slot Duration (minutes):** `60`
7. **Clear Existing:** Leave unchecked

**Show to Professor:**
- "Notice the dropdowns are pre-populated with valid data"
- "Teachers and subjects are properly assigned to each class"
- "All 14 teachers are available"
- "All 19 subjects are assigned to Semester 1"

---

### Step 3: Click "Generate Timetable" Button (2 minutes)

**Expected Result:**
```
✓ Timetable generated successfully! [XX] slots created.
```

**Behind the Scenes (Technical Explanation):**
- System validates input (our FIX #1: Time range validation)
- Fetches Semester 1 from database
- Gets 9 class batches for Semester 1
- Gets 19 subjects assigned to Semester 1 (our FIX #5: SemesterNumber)
- Gets 14 active teachers (our FIX #3: Fixed teacher query)
- Generates 40 time slots (Mon-Fri, 9-17, 60 min each)
- For each class-subject pair, schedules slots checking for:
  - Teacher availability conflicts (FIX #4: Error handling)
  - Room availability conflicts
  - Class availability conflicts
- Creates timetable slots in database with error handling (our FIX #4)

---

### Step 4: Test INVALID Input (1 minute)

**Test #1: Negative Time Range**
```
Start Hour: 17
End Hour: 9
Click Generate
```
**Expected:** ✗ "Start hour must be less than end hour."

**Test #2: Invalid Slot Duration**
```
Slot Duration: 0
Click Generate
```
**Expected:** ✗ "Slot duration must be between 1 and 480 minutes."

**Show to Professor:**
- "Our validation catches all edge cases"
- "No crashes, just user-friendly error messages"
- "Each of these errors is handled in code (FIX #1 & FIX #2)"

---

### Step 5: Display Generated Schedule (2 minutes)

Navigate to: `http://localhost:5152/Admin/Timetable/ByClass`

**Show to Professor:**
1. Select a class from dropdown (e.g., "MCA-1st-A")
2. View the timetable grid
3. Point out:
   - Time slots are organized by day and time
   - Teacher names are shown
   - Room numbers are shown
   - Subject names are displayed
   - No overlaps or double bookings
   - Different colors for Theory vs Lab sessions

**Explain the Fixes:**
- "Each slot was generated with our conflict detection algorithm"
- "We fixed the teacher query to ensure teachers are available (FIX #3)"
- "Database saves are wrapped with error handling (FIX #4)"
- "All validation is in place before generating (FIX #1 & FIX #2)"

---

### Step 6: View Conflict Detection (1 minute)

Navigate to: `http://localhost:5152/Admin/Timetable/Conflicts`

**Show to Professor:**
- "If there are any conflicts, they're detected and reported here"
- "The system logs which constraints were violated"
- "This helps identify scheduling issues for manual improvement"

---

## Command Line Verification (Before/After Testing)

```bash
# See application logs in real-time
tail -f /tmp/app.log

# Check database has correct data
sqlite3 /home/cy3pher/Documents/WorkSpace-Dev/Plannify/Plannify/timegrid.db \
  "SELECT Name, COUNT(*) FROM TimetableSlots ts
   INNER JOIN ClassBatches cb ON ts.ClassBatchId = cb.Id
   GROUP BY cb.Id
   ORDER BY Name;"

# Verify no time conflicts
sqlite3 /home/cy3pher/Documents/WorkSpace-Dev/Plannify/Plannify/timegrid.db \
  "SELECT TeacherId, Day, COUNT(*) 
   FROM TimetableSlots 
   GROUP BY TeacherId, Day, StartTime 
   HAVING COUNT(*) > 1;"  # Should return nothing (no conflicts)
```

---

## Key Points to Mention to Professor

1. **Fixed 5 Critical Issues:**
   - Input validation (prevents crashes)
   - Time range validation (prevents infinite loops)
   - Teacher query fix (ensures teachers are found)
   - Database error handling (prevents FK violations)
   - Semester assignment (ensures subjects are found)

2. **Testing Strategy:**
   - Clean database rebuild to verify seeding
   - Complete sanity checks before demo
   - Test both valid and invalid inputs
   - Show error handling working correctly

3. **Next Steps for Production:**
   - Add more advanced scheduling algorithms
   - Implement teacher workload balancing
   - Add room capacity constraints
   - Create optimization for manual adjustments
   - Build conflict resolution UI

4. **Current Limitations (Be Honest):**
   - Uses greedy "first-available" scheduling (not optimal)
   - Doesn't optimize for teacher workload balance
   - Soft constraints are warnings, not hard blocks
   - Can schedule oversized classes in fallback rooms

5. **Improvements Made:**
   - ✓ Robust input validation
   - ✓ Proper error handling
   - ✓ Database consistency
   - ✓ Clean seed data
   - ✓ No crashes for valid/invalid input

---

## Troubleshooting During Demo

If the app crashes or doesn't work:

1. **Check if app is running:**
   ```bash
   curl -s http://localhost:5152 | head -5
   ```

2. **Check app logs:**
   ```bash
   tail -50 /tmp/app.log
   ```

3. **Restart app:**
   ```bash
   pkill -f "dotnet run"
   sleep 2
   cd /home/cy3pher/Documents/WorkSpace-Dev/Plannify/Plannify
   dotnet run --configuration Debug &
   sleep 8
   ```

4. **Check database:**
   ```bash
   sqlite3 /home/cy3pher/Documents/WorkSpace-Dev/Plannify/Plannify/timegrid.db ".tables"
   ```

---

## Success Criteria

✓ Application builds without errors  
✓ Application runs without crashing  
✓ AutoGenerate page loads in browser
✓ Form accepts valid input  
✓ Generate button creates slots
✓ Invalid input shows error messages (not crashes)
✓ Generated slots visible in ByClass view
✓ No time conflicts in database
✓ All 14 teachers have assignments
✓ All 19 subjects are in database

---

**DEMO TIME: ~10 minutes total**
**READY FOR PROFESSOR PRESENTATION!**
