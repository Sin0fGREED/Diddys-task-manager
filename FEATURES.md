# Task Assignment & User Permission Features

## Overview
This document describes the new task assignment and per-user modification rights features added to Diddys Task Manager.

## New Features

### 1. User Authentication & Context
- **Login System**: Users must log in with a username before accessing tasks
- **User Context**: Tracks the current logged-in user throughout the session
- **Logout**: Users can logout and return to login screen

**Files Added:**
- `Model/User.cs` - User model with username/password
- `Services/IUserContext.cs` - Interface for user context management
- `Services/UserContext.cs` - Implementation of user context

### 2. Task Ownership & Assignment
- **Task Creator**: Every task now tracks who created it (`CreatedBy`)
- **Task Assignment**: Tasks can be assigned to team members (`AssignedTo`)
- **Unassignment**: Tasks can be unassigned from users

**Modified TaskItem Properties:**
```csharp
public string CreatedBy { get; set; }        // Username of task creator
public string? AssignedTo { get; set; }      // Username of assigned team member
```

### 3. Per-User Modification Rights
The system enforces permission rules for modifying tasks:

- **Task Creator**: Can modify, update priority, delete, assign, and unassign their own tasks
- **Assigned User**: Can toggle completion status and view assigned tasks
- **Admin User**: Can modify any task in the system (use username "admin")
- **Others**: Cannot modify tasks they didn't create or aren't assigned to

**Permission Check Method:**
```csharp
bool CanModifyTask(int taskId, string username)
{
    // Creator can always modify
    // Assigned user can modify (toggle status)
    // "admin" user can modify anything
}
```

### 4. New Service Methods

Added to `ITaskService<T>`:
- `AssignTask(int id, string assigneeName)` - Assign a task to a team member
- `UnassignTask(int id)` - Remove assignment from a task
- `GetTasksAssignedToUser(string username)` - Retrieve user's assigned tasks
- `GetTasksCreatedByUser(string username)` - Retrieve user's created tasks
- `CanModifyTask(int taskId, string username)` - Check if user can modify a task

### 5. Enhanced User Interface

New menu options in ConsoleTaskView:
1. Add Task
2. Remove Task
3. Toggle Task State
4. List Tasks
5. Update Task
6. **Assign Task** (NEW)
7. **Unassign Task** (NEW)
8. **View My Tasks (Assigned to you)** (NEW)
9. **View My Created Tasks** (NEW)
10. **Logout** (NEW)
11. Exit

## Usage Examples

### Example 1: Create and Assign a Task
```
1. User "alice" creates a task: "Fix login bug"
2. alice assigns it to "bob"
3. bob can see it in "View My Tasks"
4. bob can toggle completion, alice can modify/delete it
```

### Example 2: Transfer Responsibility
```
1. User "charlie" created task "Database migration"
2. charlie assigns to "dave"
3. If needed, charlie can unassign and reassign to someone else
```

### Example 3: Admin Override
```
1. admin user can modify ANY task regardless of who created it
2. admin can reassign tasks between team members
3. admin can complete urgent tasks
```

## Implementation Details

### Modified Files
- `Model/TaskItem.cs` - Added CreatedBy and AssignedTo properties
- `Services/ITaskService.cs` - Added new assignment methods
- `Services/TaskService.cs` - Implemented new methods with permission checks
- `View/ConsoleTaskView.cs` - Added assignment UI and user context
- `Program.cs` - Added user login flow and context initialization

### Permission Logic Flow
1. User attempts to modify a task
2. System checks who created the task
3. System checks who is assigned to the task
4. System checks if user is "admin"
5. Based on rules, allows or denies modification

## Security Notes
⚠️ **Important**: This is a basic implementation. For production use:
- Implement proper password hashing (don't store plaintext)
- Add database user validation
- Implement role-based access control (RBAC)
- Add audit logging for task modifications
- Implement session management/expiration

## Testing the Features

### Test Case 1: User Login
1. Run the application
2. Enter username: `alice`
3. Verify "Welcome, alice!" message

### Test Case 2: Create and Assign
1. Add a new task as `alice`
2. Select option 6 (Assign Task)
3. Enter another username like `bob`
4. Verify task shows "AssignedTo: bob"

### Test Case 3: View Assigned Tasks
1. Login as `bob`
2. Select option 8 (View My Tasks)
3. Should see task assigned to bob

### Test Case 4: Permission Denied
1. Login as `charlie`
2. Try to delete a task created by `alice`
3. Should see error: "You do not have permission"

## Future Enhancements
- User authentication database
- Password-based login
- Role-based permissions (Manager, Developer, Viewer)
- Task comments/activity log
- Notification system for task assignments
- Due dates and reminders
- Team/project grouping
