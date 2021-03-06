<!-- TOC -->

- [Dapper](#dapper)
    - [安装](#安装)
    - [Dapper操作](#dapper操作)
        - [强类型集合查询](#强类型集合查询)
        - [动态类型的查询](#动态类型的查询)
        - [增删改](#增删改)
        - [查询条件的IEnumerable支持](#查询条件的ienumerable支持)
        - [多映射](#多映射)
        - [多映射-splitOn](#多映射-spliton)
        - [多个结果集映射](#多个结果集映射)
        - [存储过程的应用](#存储过程的应用)

<!-- /TOC -->

<a id="markdown-dapper" name="dapper"></a>
# Dapper

从字面理解，O是Object，对象；R是Relation，关系；M是Mapping，映射。

所以，用一句话概括就是：ORM是一种对象关系映射的技术。

* Dapper是一款轻量级ORM工具。
* Dapper是一个轻型的ORM类。代码就一个SqlMapper.cs文件，编译后就40K的一个很小的Dll.
* Dapper很快，性能优越。Dapper的速度接近与IDataReader，取列表的数据超过了DataTable。
* 支持多数据库。。Dapper可以在所有Ado.net Providers下工作，包括sqlite, sqlce, firebird, oracle, MySQL, PostgreSQL and SQL Server

>https://github.com/StackExchange/Dapper

>https://dapper-tutorial.net/dapper

<a id="markdown-安装" name="安装"></a>
## 安装
推荐使用NuGet方式进行安装

![](..\assets\ORM\dapper-nuget.png)

安装后，项目中会自动添加Dapper的引用，

![](..\assets\ORM\dapper-nuget-ref.png)

【packages.config】也会有对应的包配置
```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Dapper" version="1.50.5" targetFramework="net46" />
</packages>
```

<a id="markdown-dapper操作" name="dapper操作"></a>
## Dapper操作
以下所有操作均基于下面数据库连接配置的读取：

```cs
/// <summary>
/// 读取数据库连接配置
/// 需要添加引用 System.Configuration.dll
/// </summary>
static readonly string connStr = System.Configuration.ConfigurationManager.AppSettings["connStr"];
```

基于T_DOG表结构：
```sql
CREATE TABLE T_DOG
    (
      [ID] INT IDENTITY(1, 1) PRIMARY KEY ,
      [NAME] VARCHAR(32) ,
      [WEIGHT] FLOAT ,
      [BIRTH] DATE
    )
```

添加测试数据：
```sql
INSERT INTO dbo.T_DOG ( NAME, WEIGHT, BIRTH ) VALUES  ( '小黑',  3.1,  '2010-1-1')
INSERT INTO dbo.T_DOG ( NAME, WEIGHT, BIRTH ) VALUES  ( '小花',  2.5,  '2010-2-1')
INSERT INTO dbo.T_DOG ( NAME, WEIGHT, BIRTH ) VALUES  ( '小白',  2.2,  '2010-3-1')
INSERT INTO dbo.T_DOG ( NAME, WEIGHT, BIRTH ) VALUES  ( '二狗子',  4,  '2010-4-1')
INSERT INTO dbo.T_DOG ( NAME, WEIGHT, BIRTH ) VALUES  ( '斗牛',  5.7,  '2010-5-1')
```

Dog实体类：
```cs
public class Dog
{
    public int ID { get; set; }
    public string Name { get; set; }
    public double Weight { get; set; }
    public DateTime Birth { get; set; }
}
```

<a id="markdown-强类型集合查询" name="强类型集合查询"></a>
### 强类型集合查询

Dapper 基于IDbConnection接口扩展了Query方法，所以在使用Dapper方法时需要添加Dapper程序集的using引用：
```cs
public static IEnumerable<T> Query<T>(this IDbConnection cnn, string sql
    , object param = null, SqlTransaction transaction = null, bool buffered = true)
```

执行查询，返回对应的泛型集合，Dapper会按照属性名称进行映射：
```cs
using (IDbConnection conn = new SqlConnection(connStr))
{
    string sql = @"SELECT ID,NAME,WEIGHT,BIRTH FROM T_DOG WHERE NAME LIKE @NAME ";
    object parameters = new { NAME = "%小%" };
    List<Dog> list = conn.Query<Dog>(sql, parameters).ToList();
}
```

```cs
using (IDbConnection conn = new SqlConnection(connStr))
{
    string sql = @"SELECT ID,NAME,WEIGHT,BIRTH FROM T_DOG WHERE WEIGHT > @WEIGHT ";
    object parameters = new { WEIGHT = 2.5 };
    List<Dog> list = conn.Query<Dog>(sql, parameters).ToList();
}
```

<a id="markdown-动态类型的查询" name="动态类型的查询"></a>
### 动态类型的查询
Query方法若为指定类型，即返回动态类型集合
```cs
public static IEnumerable<dynamic> Query (this IDbConnection cnn, string sql
    , object param = null, SqlTransaction transaction = null, bool buffered = true)
```

```cs
using (IDbConnection conn = new SqlConnection(connStr))
{
    string sql = @"SELECT ID,NAME,WEIGHT,BIRTH FROM T_DOG WHERE WEIGHT > @WEIGHT ";
    object parameters = new { WEIGHT = 2.5 };
    List<dynamic> list = conn.Query(sql, parameters).ToList();
}
```

<a id="markdown-增删改" name="增删改"></a>
### 增删改
这是一个简单的参数化insert，匿名类型对象，跟原始的SqlParameter相比要简单很多
```cs
using (IDbConnection conn = new SqlConnection(connStr))
{
    string sql = @"INSERT INTO T_DOG(NAME,WEIGHT,BIRTH) VALUES(@NAME,@WEIGHT,@BIRTH) ";
    object parameters = new { NAME = "田园守护者", WEIGHT = 2.5, BIRTH = DateTime.Now };
    int res = conn.Execute(sql, parameters);
}
```

Bulk操作，批量化的操作，我们要做的就是将上面这个【对象】变成【对象集合】就可以了。
```cs
using (IDbConnection conn = new SqlConnection(connStr))
{
    string sql = @"INSERT INTO T_DOG(NAME,WEIGHT,BIRTH) VALUES(@NAME,@WEIGHT,@BIRTH) ";
    List<Dog> parameters = new List<Dog>();
    parameters.Add(new Dog()
    { Name = "jack", Weight = 1.2, Birth = new DateTime(2012, 1, 1) });
    parameters.Add(new Dog()
    { Name = "lee", Weight = 1.1, Birth = new DateTime(2012, 1, 2) });
    parameters.Add(new Dog()
    { Name = "rock", Weight = 2.2, Birth = new DateTime(2012, 1, 3) });
    
    int res = conn.Execute(sql, parameters);
}
```

相应的删除、修改也是一样的套路。暂略...

<a id="markdown-查询条件的ienumerable支持" name="查询条件的ienumerable支持"></a>
### 查询条件的IEnumerable支持
Dapper允许传入`IEnumerable<int>`类型，并自动进行转换查询

```cs
using (IDbConnection conn = new SqlConnection(connStr))
{
    string sql = @"SELECT ID,NAME,WEIGHT,BIRTH FROM T_DOG WHERE NAME IN @NAMES ";
    object parameters = new { NAMES = new string[] { "小黑", "小白", "小花" } };
    // 相当于执行 SELECT ID,NAME,WEIGHT,BIRTH FROM T_DOG WHERE NAME IN ('小黑', '小白', '小花')
    List<Dog> list = conn.Query<Dog>(sql, parameters).ToList();
}
```

<a id="markdown-多映射" name="多映射"></a>
### 多映射
Dapper支持将一行结果值映射对应到多个对象，这样做可以大大提高效率。

在上述表T_DOG基础上再添加一张表T_PERSON，两表之间的关系为`T_DOG.OWNER=T_PERSON.ID`，SQL脚本如下：

```sql
-- 创建主人表
CREATE TABLE T_PERSON
    (
      ID INT IDENTITY(1, 1) PRIMARY KEY ,
      NAME VARCHAR(32)
    )
INSERT  INTO T_PERSON ( NAME ) VALUES  ( '二郎神' )
INSERT  INTO T_PERSON ( NAME ) VALUES  ( '孙悟空' )
```

```sql
-- 修改 表T_DOG结构，增加[OWNER]为狗主人
ALTER TABLE dbo.T_DOG ADD [OWNER] INT 

-- 根据体重设置新增的字段[OWNER]
UPDATE dbo.T_DOG SET OWNER = (CASE WHEN WEIGHT>2.5 THEN 1 ELSE 2 END);
```

对应的强类型如下：
```cs
public class Dog
{
    public int ID { get; set; }
    public string Name { get; set; }
    public double Weight { get; set; }
    public DateTime Birth { get; set; }
    /// <summary>
    /// 狗主人，也是一个对象
    /// </summary>
    public Person Owner { get; set; }
}

public class Person
{
    public int ID { get; set; }
    public string Name { get; set; }
}
```

传统方式都是在查询得到所有的Dog集合后，然后遍历再去查询所有Dog对象对应的Owner对象，当数据量很大时，此方式非常影响效率，而且很麻烦。


`Query<TFirst, TSecond, TReturn>`提供了映射到多个对象的方式，TFirst表示第一个对象，TSecond表示第二个对象，TReturn为返回对象。

`Func<TFirst, TSecond, TReturn> map`委托，参数为(TFirst, TSecond)，需要返回TReturn类型对象，即在方法中需要指明如何关联。

```cs
public static IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(this IDbConnection cnn
, string sql, Func<TFirst, TSecond, TReturn> map
, object param = null
, ........);
```

关联的多个对象有重名时，按照顺序进行关联进行映射：
```cs
using (IDbConnection conn = new SqlConnection(connStr))
{
    string sql = @"
SELECT A.ID,A.NAME,WEIGHT,BIRTH,PE.ID,PE.NAME FROM T_DOG A 
LEFT JOIN T_PERSON PE ON A.OWNER= PE.ID";
    List<Dog> list = conn.Query<Dog, Person, Dog>(sql
        , (dog, per) =>
            {
                dog.Owner = per;
                return dog;
            }).ToList();
}
```

<a id="markdown-多映射-spliton" name="多映射-spliton"></a>
### 多映射-splitOn
当结果集映射到多个对象嵌套时，如下Company示例：
```cs
public class Company
{
    public int CID { get; set; }
    public string Name { get; set; }
    public Mailing Mailing { get; set; }
    public Physical Physical { get; set; }
}

public class Mailing
{
    public int MID { get; set; }
    public string Name { get; set; }
}

public class Physical
{
    public int PID { get; set; }
    public string Name { get; set; }
}
```

Company类和Mailing类、Physical类是关联关系。

上述案例中，对应的Mailing和Physical主键并不是默认的id，此处需要指定splitOn，如下：
```cs
using (IDbConnection conn = new SqlConnection(connStr))
{
    // 多映射到对象嵌套时，需要注意字段的顺序
    string sql = @"
SELECT 1 as cid,'iflytek' as name
,11 as mid,'now@11.com' as name
,22 as pid,'文津西路' as name
";
    Company result = conn.Query<Company, Mailing, Physical, Company>(sql
        , (com, mail, phy) =>
    {
        com.Mailing = mail;
        com.Physical = phy;
        return com;
    }, splitOn: "mid,pid").FirstOrDefault();
}
```

<a id="markdown-多个结果集映射" name="多个结果集映射"></a>
### 多个结果集映射
Dapper支持在一次查询中将多个查询结果集映射到不同集合

```cs
using (IDbConnection conn = new SqlConnection(connStr))
{
    string sql = @"
SELECT ID,NAME,WEIGHT,BIRTH FROM T_DOG
SELECT ID,NAME FROM T_PERSON
SELECT 1 AS MID, 'now@126.com' AS NAME 
";
    var multi = conn.QueryMultiple(sql);
    List<Dog> listDog = multi.Read<Dog>().ToList();
    List<Person> listPerson = multi.Read<Person>().ToList();
    Mailing mail = multi.Read<Mailing>().FirstOrDefault();
}
```

<a id="markdown-存储过程的应用" name="存储过程的应用"></a>
### 存储过程的应用

基于前面章节所提到的存储过程`sp_paged_data`参见[sp_paged_data](../DB/SQLServer_Program.md)

分页类Pager参见[Pager](./ADO.NET.md)

```cs
/// <summary>
/// Dapper 分页查询
/// </summary>
/// <typeparam name="T">需要映射到的实体类类型</typeparam>
/// <param name="sqlTable">关系表名</param>
/// <param name="sqlColumns">投影列，如*</param>
/// <param name="sqlWhere">条件子句(可为空)，eg：and id=1</param>
/// <param name="sqlSort">排序语句(不可为空，必须有排序字段)，eg：id</param>
/// <param name="pageIndex">当前页码索引号，从0开始</param>
/// <param name="pageSize">每页显示的记录条数</param>
/// <returns>分页对象</returns>
public static Pager<T> QueryByPager<T>(string sqlTable, string sqlColumns, string sqlWhere
, string sqlSort, int pageIndex, int pageSize)
{
    using (IDbConnection conn = new SqlConnection(connStr))
    {
        Pager<T> pager = new Pager<T>();
        var p = new DynamicParameters();
        p.Add("@sqlTable", sqlTable);
        p.Add("@sqlColumns", sqlColumns);
        p.Add("@sqlWhere", sqlWhere);
        p.Add("@sqlSort", sqlSort);
        p.Add("@pageIndex", pageIndex);
        p.Add("@pageSize", pageSize);
        // 指定记录总数为输出类型
        p.Add("@rowTotal", dbType: DbType.Int32, direction: ParameterDirection.Output);

        pager.Rows = conn.Query<T>("sp_paged_data", p, commandType: CommandType.StoredProcedure).ToList();
        pager.Total = p.Get<int>("@rowTotal");
        return pager;
    }
}
```

基于上面的`QueryByPager`方法封装，调用如下：
```cs
// 每页5条记录，查询第二页
Pager<Dog> res = DapperHelper.QueryByPager<Dog>("T_DOG", "ID,NAME,WEIGHT,BIRTH", "", "ID", 1, 5);
```

---

参考引用：

https://www.cnblogs.com/huangxincheng/p/5828470.html

https://blog.csdn.net/wyljz/article/details/68926745

https://stackoverflow.com/questions/25921402/how-do-i-get-multi-mapping-to-work-in-dapper




