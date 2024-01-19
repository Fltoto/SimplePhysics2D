# SimplePhysics2D 性能优秀且操作简单的2D物理引擎  
# Author:Fltoto  
# 使用方法  
1.创建物理世界: SPWorld2D world = new SPWorld();  
2.创建SPRigBody2D: SPRigBody.CreateBox/Circle/Polygon  
3.把RigBody添加到世界中 world.AddComponent(body);  
4.启动世界world.Run();  
5.获取RigBody信息:body.Position/Rotation/LinearVelocity.....  
6.对Box类型的RigBody添加额外的碰撞体:body.AddColVertices  
7.对RigBody修改质量，body.SetMass  
更多的内容请自行翻阅代码  