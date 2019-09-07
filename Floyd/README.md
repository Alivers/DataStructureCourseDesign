<!--
 * @Date: 2019-08-31 18:21:03
 * @LastEditors: Aliver
 * @LastEditTime: 2019-08-31 19:31:02
 -->
# 项目说明

## 目录结构

```bash
.
├── floyd-test
│   ├── 1750817-floyd-input-1.dat
│   ├── 1750817-floyd-input-2.dat
│   ├── 1750817-floyd-input-3.dat
│   ├── 1750817-floyd-output-1.dat
│   ├── 1750817-floyd-output-2.dat
│   └── 1750817-floyd-output-3.dat
├── FloydVisualization
│   ├── arc.cpp
│   ├── arc.h
│   ├── common.h
│   ├── floydframe.cpp
│   ├── floydframe.h
│   ├── FloydVisualization.pro
│   ├── FloydVisualization.pro.user
│   ├── graph.cpp
│   ├── graph.h
│   ├── main.cpp
│   ├── mainwindow.cpp
│   ├── mainwindow.h
│   ├── mainwindow.ui
│   ├── subwindow.cpp
│   ├── subwindow.h
│   ├── subwindow.ui
│   ├── vertex.cpp
│   └── vertex.h
├── makedat
│   └── makedat.cpp
└── README.md
```

## 命令参数

1. makedat 命令
> --help, 用法帮助
> --vertex [m], 指定图中的顶点数
> --arc [n], 指定图中的弧数
> --output [filename], 指定输出的数据文件名

## 编译构建

支持 Windows 和 Linux 系统运行，选择相应的构建工具即可