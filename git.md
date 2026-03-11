# Git + GitHub + Jira Workflow Guide

This document explains how we work with **Git, GitHub and Jira** during the project.
The goal is to keep our work organized and avoid conflicts while still keeping the workflow simple.

---

# 1. General Workflow

The normal flow for every task looks like this:

```
Jira task
   ↓
Create branch
   ↓
Work locally
   ↓
Push branch
   ↓
Open Pull Request
   ↓
Review (Rares does this)
   ↓
Merge to main
```

**Important rules**

* Never push directly to `main` (I set it up so that this can't happen but I didn't test it)
* Always create a branch for a task (This also happens automatically, see bellow)
* Every branch should correspond to a **Jira task**
* Every PR should reference the **Jira task**

---

# 2. Jira Workflow

Each task/feature/bug is tracked in **Jira**.

Typical lifecycle:

```
To Do → In Progress → In Review → Done
```

Meaning:

**To Do**
The task was created to be worked on soon.

**In Progress**
Someone started working on it.

**In Review**
Code is finished and a Pull Request was opened.

**Done**
The PR was approved and merged.

---

## Multiple people working on the same feature

If a feature is large:

Create a **Story/Feature** and then create **Subtasks**.

Example:

Story: User Authentication

Subtasks:

* Backend API
* Database schema
* Frontend login page
* Tests

Each subtask has **one assignee** and its **own branch** which is created from the **story/feature branch**.

---

# 3. Branch Naming Convention

Branch names must reference the **Jira task key**.

Format:

```
type/JIRA-KEY-short-description
```

Examples:

```
feat/KAN-23-add-route-api
fix/KAN-45-login-validation
refactor/KAN-31-clean-auth-service
docs/KAN-12-update-readme
```

---

## Branch types

```
feat      → new feature
fix       → bug fix
refactor  → code improvement without behavior change
docs      → documentation changes
chore     → maintenance / config / small tasks
```

Example:

```
feat/KAN-18-add-climb-filter
```

---

# 4. Commit Message Convention

Format:

```
type(JIRA-KEY): short description
```

Examples:

```
feat(KAN-18): add climb filtering by difficulty
fix(KAN-21): fix crash when route list is empty
refactor(KAN-11): simplify auth middleware
docs(KAN-7): update project setup instructions
chore(KAN-3): add docker configuration
```

Guidelines:

* Keep messages short
* Use present tense
* Focus on **what changed**

Good:

```
feat(KAN-22): add route creation endpoint
```

Please don't do:

```
stuff
update
fix things
```

---

# 5. Basic Git Commands

This section covers the most common commands we will use.

---

# Clone the repository

First time setup:

```bash
git clone <repo-url>
cd <repo>
```

---

# Update local repository

Always update your local `main` before starting work.

```
git switch main
git pull origin
```

## Troubleshooting
If you get errors like `unstaged/uncommited changes on the local branch` when trying to switch branches,
you can either **stage (git add)** and **commit** your changes or you can **stash (git stash)** them.

---

# Create a new branch

Create a branch from `main`:

## The Automatic Way (Recomanded)
I set up jira in such a way that when a task is moved to **In Progress** a branch is automatically created.
So all you need to do is set the task's status to in progress and then go on you local machine and get the
branch from the remote reapo. Easy right? 

We do this so that jira can automatically track our work.
```bash
# after setting the task to `In Progress` (maybe a minute later, it can take some time for the branch to get created)
git fetch --all # this gets all the updates from remote (including the new branch)

git branch -r # showes all the remote branches, make sure the one you want to work on is there, it can be easyly identified by the jira tag (KAN-x)
# Bonus: to see all the branches (remote and local) the `-a` flag can be used

# then we can create a local branch tracking a remote branch by doing
git switch -c branch-name origin/branch-name

```

## The Manual Way
This is not really recomandad because in this way I can't see your branches from github until you push them.
```bash
git checkout -b feat/KAN-23-add-route-api
```

---

# Check branch status

```bash
git status
```

Shows changed files and staged files.

---

# Stage changes

Add files to the commit:

```bash
git add file_name
```

or all changes:

```bash
git add .
```

---

# Commit changes

```bash
git commit -m "feat(KAN-23): add route api endpoint"
```

---

# Push branch to GitHub

```bash
git push -u origin branch-name
```

Example:

```bash
git push -u origin feat/KAN-23-add-route-api
```

---

# 6. Opening a Pull Request

After pushing your branch:

1. Go to the GitHub repository
2. Click **New Pull Request** (yellow banner above the repository content)
3. Set base branch: `main` (or the parent branch if you are on a longer lived branch)
4. Set compare branch: your branch

PR title example:

```
feat(KAN-23): add route api endpoint
```

PR description should include:

```
Jira: KAN-23

Summary:
Implemented the API endpoint for creating climbing routes.

Changes:
- added route handler
- added validation
- added database insert
```

---

# 7. Keeping Your Branch Updated

If `main` (or the parent branch in general) changed while you were working, update your branch.

First fetch new changes:

```bash
git fetch origin
```

Then rebase your branch on top of main:

```bash
git rebase origin/main
# replace main if needed with the actual parent branch name
```

If conflicts happen:

1. Fix the files
2. Stage them

```bash
git add .
```

3. Continue rebase

```bash
# this can also be done interactively in the IDE, you do you
git rebase --continue
```

Then push again:

```bash
git push --force-with-lease
```

This updates the PR safely.

---

# 8. Pulling Latest Changes Before Work

Recommended pre-work routine:

```bash
git switch main
git pull origin
git switch feat/JIRA-KEY-description
```

---

# 9. After PR is Merged

Once your PR is merged:

Update your local repository:

```bash
git switch main
git pull origin main
```

Then delete your branch locally:

```bash
git branch -d branch-name
```

## Troubleshooting
If git pulls the classic `branch not fully merged, are you sure you want to delete the branch` with you it is probably because 
I squash merged and the merge history is inconsistent. When this happens if you are sure that all your changes were correctly merged
force the deletion with the `-D` flag (ex. `git branch -D branch-name`), if you prefere you can write me a message and we can confirm 
together that everyhting is fine (or not idk)

---

# 11. If Something Goes Wrong

Git mistakes are normal.

Before panicking:

1. Run

```bash
git status
```

2. Ask in the team chat

3. Someone (most surely me) will help fix it.

Almost everything in Git can be recovered (with some AI and magic).

---

