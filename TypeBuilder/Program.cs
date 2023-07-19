using System;
using System.Web.Script.Serialization;
using System.Reflection.Emit;
using System.Reflection;

namespace TypeBuilderDemo
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    class Program
    {
        public static JavaScriptSerializer Obj = new JavaScriptSerializer();
        static void Main()
        {
            GetClass();
        }

        private static void GetClass()
        {
            //获取Student类的类型
            Type type = typeof(Student);
            //创建一个动态程序集
            AssemblyName assemblyName = new AssemblyName("StudentAssembly");
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            //创建一个动态模块
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("StudentModule");
            //创建一个动态类
            TypeBuilder typeBuilder = moduleBuilder.DefineType("Student", TypeAttributes.Public);
            //为动态类添加属性
            foreach (var item in type.GetProperties())
            {
                CreateProperty(typeBuilder, item.Name, item.PropertyType);
            }
            //手动添加属性Scope
            CreateProperty(typeBuilder, "Scope", typeof(string));

            //创建动态类的类型
            Type classType = typeBuilder.CreateType();
            //创建动态类的实例
            object obj = Activator.CreateInstance(classType);
            //为动态类的属性赋值
            foreach (var item in classType.GetProperties())
            {
                if (item.Name == "Id")
                {
                    item.SetValue(obj, 1);
                }
                else if (item.Name == "Scope")
                {
                    item.SetValue(obj, "班级");
                }
                else
                {
                    item.SetValue(obj, "张三");
                }
            }
            //将动态类的实例序列化为json字符串
            string json = Obj.Serialize(obj);
            Console.WriteLine(json);
            Console.ReadKey();
        }

        //CreateProperty方法用于为动态类添加属性
        private static void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            //定义字段
            FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            //定义属性
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            //定义属性的get方法
            MethodBuilder getMethodBuilder = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            //ILGenerator用于生成get方法的IL代码
            ILGenerator getIl = getMethodBuilder.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);
            //定义属性的set方法
            MethodBuilder setMethodBuilder = typeBuilder.DefineMethod("set_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new[] { propertyType });
            //ILGenerator用于生成set方法的IL代码
            ILGenerator setIl = setMethodBuilder.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);
            setIl.Emit(OpCodes.Ret);
            //将get方法和set方法添加到属性
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);
        }
    }
}
