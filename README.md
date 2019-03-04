# LSolr
LSolr是一个.NET平台的Solr客户端
## 特性
* 配置简单，使用方便
* 没有引用第三方类库，全部由.Net Standard标准中的API写成，兼容性较好
* 代码精简，执行效率较高
* 基于.Net Standard 2.0，对于各平台支持版本如下：

平台  | 最低版本
------------- | -------------  
.NET Core  | 2.0
.NET Framework  | 4.6.1 
Mono|5.4
Xamarin.iOS|10.14
Xamarin.Mac|3.8
Xamarin.Android|8.0
通用 Windows 平台|10.0.16299

## 安装设置
1. 将Lsolr项目添加到解决方案中，或者直接引用dll文件。VS2013直接引用项目会报错，所以需要自己建个类库，然后把所有文件复制到类库中。
2. 在配置文件appsettings.json、Web.config、App.config文件中增加配置项，配置如下

参数名  | 是否必须|说明|例子
------------- | -------------| -------------| -------------
solrhttp|必须|solr的访问地址|http://aaa.bbb.com/solr/
solruserid|非必需|Basic认证的账号|
solrpsw|非必需|Basic认证的密码|
timezone|非必需|时间是否减去8小时|"true"或者"false",默认"false"

.Net Core配置示例：
```javascript
{
  "solrhttp": "http://aaa.bbb.com/solr/",
  "solruserid": "id",
  "solrpsw": "psw",
  "timezone": "true"
}
```
.Net Framework 配置示例:
```xml
<configuration>
  <appSettings>
    <add key="solrhttp" value="http://aaa.bbb.com/solr/"/>
    <add key="solruserid" value="id"/>
    <add key="solrpsw" value="psw"/>
    <add key="timezone" value="true"/>
  </appSettings>
</configuration>
```
## 使用方法
首先要建立Solr在代码中对应的类,其中class上面的SolrCore这个attribute指定了这个类对应到Solr中的哪个Core,如果和Solr种Core的名字相同则可以省略。
property上面的SolrField不是必须的，如果Solr中field的name和class中property一样的话(区分大小写),那么可以省略这个SolrField

```c#
[SolrCore("goodscore")]
public class goods
{
    [SolrField("recordno")]
    public string recordno { get; set; }
    [SolrField("goodcode")]
    public string goodcode { get; set; }
    [SolrField("price")]
    public double? price { get; set; }
    public DateTime? createtime { get; set; }
}
```
### 设置参数
如果配置文件不能读取或者想在代码中设置，可以在Query的构造函数中传参设置
```c#
Solr.Query<goods>("http://aaa.bbb.com/solr/", "id", "psw")
```
如果在Query的构造函数设置参数了，则忽略配置中设置的值
### 查询
```c#
int total=0;
List<goods> list=Solr.Query<goods>().ToList(ref total);//返回结果列表
goods list=Solr.Query<goods>().ToModel();//返回第一条数据
```
total为数据总数，如果对数据总数不关心也可省略此参数。
### 异步查询
```c#
int total=0;
DocSolr<goods> doc=Solr.Query<goods>().ToListAsync();//异步返回结果列表
goods list=Solr.Query<goods>().ToModelAsync();//异步返回第一条数据
```
因为异步方法不能有ref参数，所以结果保存在DocSolr对象中，数据总数保存在doc.response.numFound中
### 需要查询的字段
```c#
Solr.Query<goods>().Select(a => new { a.goodcode, a.price }).ToList();
```
如果字段较多，并且只想获取其中的几个字段，可以使用Select方法来限定查询的字段，来减少网络传输的消耗。
如果查询所有字段可以省略Select方法。
### 查询条件
```c#
Solr.Query<goods>().Where(a => a.goodcode=="123"&&a.price>10).ToList();
```
可以使用Where方法来增加查询条件，多个条件既可以写在一个Where方法里也可以写在多个Where方法里，上面的代码也可以写成
```c#
Solr.Query<goods>().Where(a => a.goodcode=="123").Where(a => a.price > 10).ToList();
```
多个Where方法之间是AND的关系。
现在Where方法中支持如下运算符
```c#
Solr.Query<goods>().Where(a => a.goodcode=="123").ToList();//等于
Solr.Query<goods>().Where(a => a.goodcode!="123").ToList();//不等于
Solr.Query<goods>().Where(a => a.price>10).ToList();//大于
Solr.Query<goods>().Where(a => a.price>=10).ToList();//大于等于
Solr.Query<goods>().Where(a => a.price<10).ToList();//小于
Solr.Query<goods>().Where(a => a.price<=10).ToList();//小于等于
Solr.Query<goods>().Where(a => a.goodcode.SolrLike("123","all")).ToList();//Like查询,第二个参数为all,left,right,对应哪侧模糊查询
Solr.Query<goods>().Where(a => a.goodcode.SolrNotLike("123","all")).ToList();//NotLike查询,第二个参数为all,left,right,对应哪侧模糊查询
Solr.Query<goods>().Where(a => a.goodcode.SolrIn("1,2,3").ToList();//In查询,多个值用逗号分隔
Solr.Query<goods>().Where(a => a.goodcode.SolrNotIn("1,2,3").ToList();//NotIn查询,多个值用逗号分隔
```
### 分页
分页有两种方式，一种是直接设置Solr中的start和rows参数
```c#
Solr.Query<goods>().Start(1).Rows(10).ToList();
```
另一种是使用现实界面上的分页
```c#
Solr.Query<goods>().Page(1, 10).ToList();//第一页，每页10条
```
注：如果没有分页，那么默认返回前10条数据
### 排序
```c#
Solr.Query<goods>().OrderBy(a => a.price).ToList();//正序排序，单个字段
Solr.Query<goods>().OrderDescBy(a => a.price).ToList();//倒序排序，单个字段
Solr.Query<goods>().OrderBy(a => new { a.price,a.goodcode }).ToList();//正序排序，支持多个字段
Solr.Query<goods>().OrderDescBy(a => new { a.price,a.goodcode }).ToList();//倒叙排序，支持多个字段
Solr.Query<goods>().OrderBy(a => new { a.price }).OrderDescBy(a => new { a.goodcode }).ToList();//混合排序
```
### 添加其他参数
```c#
Solr.Query<goods>().AddPara("&rows=10").ToList();//url后添加了&rows=10
```
### GroupBy分组函数
```c#
FacetSolr<goods> facetSolr = Solr.Query<goods>().Where(a => a.price>10).GroupBy(a => new { a.recordno, a.goodcode });
int count = facetSolr.numFound;//Where筛选后数据总量
foreach (FacetData<goods> item in facetSolr.data)
{
    //分组完，每组字段的值保存在entity里，在这里entity保存了recordno，goodcode的值
    goods book = item.entity;
	//每个分组数据总数
    int num = item.num;
}
FacetSolr<goods> facetSolr = Solr.Query<goods>().Where(a => a.price>10).GroupBy(a =>  a.recordno);//单个字段分组
```
Solr中group函数不能多字段分组，facet函数返回的结构为树形结构，所以为了接近SQL中的Group By，
GroupBy方法使用的时facet,并且将树形结构转化为和SQL结果一样的Table结构。
默认请求solr时facet.missing参数设为on，表示将null值也进行分组，也是为了保持和SQL一致。
## 其他功能
### 查看代码执行的各阶段耗时
```c#
String Msg = Solr.Query<goods>().Where(a => a.goodcode=="123").OutpuntTimeLine();
```
只需将ToList()方法换成OutpuntTimeLine(),就可以返回各个阶段所耗时间。
## 其他说明
* 因为时区的原因，会出现solr中的时间比数据库中时间少8小时的情况，如果在导入数据时没有处理，那么在查询时
需要将时间减去8小时。或者参数中timezone设为"true"，这样就会自动将时间减去8小时。
* 如果出现无法读取配置的情况，请在VS中将配置文件的属性中《复制到输出目录》设置为《始终复制》
* 现在只支持查询语句，后续将增加增删改以及sum等聚合函数
* 现在还没有完善的异常体系，后续会增加
* Where方法中只能查询的字段在左边，条件的值在右边
* Where方法中只能识别SolrLike,SolrNotLike,SolrIn,SolrNotIn方法，如下使用方式会出错
```c#
Solr.Query<goods>().Where(a =>  a.createtime > DateTime.Now.AddDays(1)).ToList();
```
所以请换成以下方式
```c#
DateTime dt= DateTime.Now.AddDays(1);
Solr.Query<goods>().Where(a =>  a.createtime > dt).ToList();
```