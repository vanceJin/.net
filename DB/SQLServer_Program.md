<!-- TOC -->

- [SQLServer编程](#sqlserver编程)
    - [存储](#存储)

<!-- /TOC -->
# SQLServer编程
## 存储
```sql
CREATE PROCEDURE P_IN_BATCH_DATA
AS
DECLARE @NUM INT
SET @NUM = 1
WHILE(@NUM < 1000000)
BEGIN 
	SET @NUM = @NUM + 1
	INSERT INTO TABLE_BATCH (NAME) VALUES ('王富贵')
END
```
