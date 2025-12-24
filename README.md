# Mark Pdf

一个简单的 Pdf 目录编辑工具，依托于 pdftk 实现。

## Usage

### Install pdftk

```
# macos - brew
brew install pdftk-java

# debian - apt
apt install pdftk
```

### MarkPdf 的使用说明

```
# 导出 pdf 书签文件
MarkPdf export --pdf your_pdf_path --mark your_mark_path

# 导入修改后的 pdf 书签文件
MarkPdf import --pdf your_pdf_path --mark your_mark_path

# 导入修改后的 pdf 书签文件, 并替换原始文件
MarkPdf import --pdf your_pdf_path --mark your_mark_path --replace
```

**目录文件结构**

目录配置由三部分构成

1. 目录的等级：由 `#` 的个数进行表示
2. 目录的名称：由 `[]` 中的文字进行表示
3. 目录的页数：由 `()` 中的数字进行表示

```
# [一级测试名称](1)
## [二级测试名称](1)
### [三级测试名称](4)
# [一级测试名称](5)
# [一级测试名称](1)
## [二级测试名称](4)
```

### How to Build

运行 dotnet 编译命令

命令文件会在 `bin/Release/net10.0/对应平台/publish/` 文件夹下找到

```shell
dotnet publish
```

## LICENSE

项目使用 MIT 协议开源
