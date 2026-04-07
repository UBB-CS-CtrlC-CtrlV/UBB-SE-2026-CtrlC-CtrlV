IF DB_ID('BankAppDb') IS NULL
    CREATE DATABASE BankAppDb;
GO

USE BankAppDb;
GO

:r schema/01_User.sql
:r schema/02_Session.sql
:r schema/03_OAuthLink.sql
:r schema/04_Account.sql
:r schema/05_Card.sql
:r schema/06_Category.sql
:r schema/07_Transaction.sql
:r schema/08_Notification.sql
:r schema/09_NotificationPreference.sql
:r schema/10_PasswordResetToken.sql
:r schema/11_TransactionCategoryOverride.sql

:r seed/01_Categories.sql
:r seed/02_Users.sql
:r seed/03_Accounts.sql
:r seed/04_Cards.sql
:r seed/05_Transactions.sql
:r seed/06_NotificationPreferences.sql