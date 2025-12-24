# Mark Pdf

一个简单的 Pdf 目录编辑工具，依托于 pdftk 实现。

## Usage

### Install pdftk

```
# macos - brew
brew install pdftk-java

# debian - apt
apt install pdftk

# windows - scoop
scoop install pdftk
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

```
# 前言 5
# 目录 6
# 第01章 关于软件工程生产力 15
## 1.1 从另一个角度看"提高软件工程生产力" 15
## 1.2 Jenkins 介绍 18
## 1.3 Jenkins 与 DevOps 18
# 第02章 Pipeline 入门 20
# 第03章 Pipeline 语法讲解 30
# 第04章 环境变量与构建工具 51
# 第05章 代码质量 64
# 第06章 触发 Pipeline 执行 91
# 第07章 多分支构建 107
# 第08章 参数化 Pipeline 115
# 第09章 凭证管理 126
# 第10章 制品管理 137
# 第11章 可视化构建及视图 154
# 第12章 自动化部署 161
# 第13章 通知 174
# 第14章 分布式构建与并行构建 183
# 第15章 扩展 Pipeline 200
# 第16章 Jenkins 运维 214
# 第17章 自动化运维经验 234
# 第18章 如何设计 Pipeline 246
# 后记 254
```

### How to Build

运行 dotnet 编译命令

命令文件会在 `bin/Release/net10.0/对应平台/publish/` 文件夹下找到

```shell
dotnet publish
```

## LICENSE

项目使用 MIT 协议开源
