#!/bin/bash

# LemonStudio NuGet 发布脚本

echo "开始发布 LemonStudio 到 NuGet..."

# 检查是否安装了 dotnet CLI
if ! command -v dotnet &> /dev/null; then
    echo "❌ 错误: 未找到 dotnet CLI，请先安装 .NET SDK"
    exit 1
fi

# 进入项目目录
cd Lemon

# 清理之前的构建
echo "🧹 清理之前的构建..."
dotnet clean
rm -rf ../nupkg

# 构建项目
echo "🔨 构建项目..."
dotnet build --configuration Release

if [ $? -ne 0 ]; then
    echo "❌ 构建失败"
    exit 1
fi

# 打包 NuGet 包
echo "📦 打包 NuGet 包..."
dotnet pack --configuration Release --output ../nupkg

if [ $? -ne 0 ]; then
    echo "❌ 打包失败"
    exit 1
fi

echo "✅ 打包成功！包文件位于 ../nupkg 目录"

# 检查是否设置了 NuGet API Key
if [ -z "$NUGET_API_KEY" ]; then
    echo "⚠️  请设置环境变量 NUGET_API_KEY，或使用以下命令手动发布："
    echo "   dotnet nuget push ../nupkg/*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json"
    exit 0
fi

# 发布到 NuGet
echo "🚀 发布到 NuGet..."
dotnet nuget push ../nupkg/*.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json

if [ $? -eq 0 ]; then
    echo "✅ 发布成功！"
else
    echo "❌ 发布失败"
    exit 1
fi 