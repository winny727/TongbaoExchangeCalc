@echo off
setlocal enabledelayedexpansion

rem 输出文件名
set "outfile=output.txt"
> "%outfile%" echo [List of images]

for /d %%A in (copper_*.imageset) do (
    rem 提取中间部分 XXX
    set "folder=%%~nA"
    rem folder = copper_XXX
    rem 去掉前缀 "copper_"
    set "name=!folder:copper_=!"

    rem 遍历该文件夹中的 png 文件
    for %%B in ("%%A\*.png") do (
        echo !name!	%%~nxB>> "%outfile%"
    )
)

echo 完成！结果已写入 %outfile%
pause