//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
// 
//  For more information see: http://dapplo.net/
//  Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
//  This file is part of Dapplo.InterfaceImpl
// 
//  Dapplo.InterfaceImpl is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  Dapplo.InterfaceImpl is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have Config a copy of the GNU Lesser General Public License
//  along with Dapplo.InterfaceImpl. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Dapplo.InterfaceImpl
{
	public class Emitter
	{
		private const MethodAttributes SetgetMethodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final;
		private const MethodAttributes MethodAttributes = System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Virtual | System.Reflection.MethodAttributes.Final;
		private static readonly ConstructorInfo SetInfoConstructor = typeof(SetInfo).GetConstructor(Type.EmptyTypes);
		private static readonly MethodInfo GetSetInfoPropertyName = typeof (GetSetInfo).GetMethod("set_PropertyName");
		private static readonly MethodInfo GetSetInfoPropertyType = typeof(GetSetInfo).GetMethod("set_PropertyType");
		private static readonly MethodInfo SetInfoNewValue = typeof(SetInfo).GetMethod("set_NewValue");
		private static readonly MethodInfo GetInfoValue = typeof(GetInfo).GetMethod("get_Value");
		private static readonly MethodInfo GetInterceptor = typeof(IIntercepted).GetMethod("get_Interceptor");
		private static readonly MethodInfo InterceptorGet = typeof(IInterceptor).GetMethod("Get");
		private static readonly MethodInfo InterceptorSet = typeof(IInterceptor).GetMethod("Set");
		private static readonly ConstructorInfo GetInfoConstructor = typeof(GetInfo).GetConstructor(Type.EmptyTypes);

		public static Type CreateType(string assemblyNameString, string typeName, Type interfaceType)
		{
			string dllName = $"{assemblyNameString}.dll";
			var assemblyName = new AssemblyName(assemblyNameString);
			var appDomain = AppDomain.CurrentDomain;
			var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, dllName, true);

			// Create the type, and let it implement our interface
			var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class, typeof (object), new[] {interfaceType, typeof(IIntercepted)});

			var interceptorField = BuildProperty(typeBuilder, "Interceptor", typeof (IInterceptor));

			var invokeMethod = typeof (ProxyInvoker).GetMethod("Invoke");

			foreach (var propertyInfo in interfaceType.GetProperties())
			{
				if (propertyInfo.CanWrite)
				{
					var setterBuilder = typeBuilder.DefineMethod("set_" + propertyInfo.Name, SetgetMethodAttributes, typeof (void), new[] {propertyInfo.PropertyType});
					var ilSetter = setterBuilder.GetILGenerator();

					// Local SetInfo variable
					var setInfo = ilSetter.DeclareLocal(typeof(SetInfo));

					// Create new SetInfo class for the argument which are passed to the IInterceptor
					ilSetter.Emit(OpCodes.Newobj, SetInfoConstructor);
					// Store it in the local setInfo variable
					ilSetter.Emit(OpCodes.Stloc, setInfo);

					// Get the setInfo local variable value
					ilSetter.Emit(OpCodes.Ldloc, setInfo);
					// Load the name of the property on the stack
					ilSetter.Emit(OpCodes.Ldstr, propertyInfo.Name);
					// Set the value to the PropertyName property of the SetInfo (call set_PropertyName)
					ilSetter.Emit(OpCodes.Callvirt, GetSetInfoPropertyName);

					// Get the setInfo local variable value
					ilSetter.Emit(OpCodes.Ldloc, setInfo);
					// Load the name of the property on the stack
					ilSetter.Emit(OpCodes.Ldtoken, propertyInfo.PropertyType);
					// Set the value to the PropertyType property of the SetInfo (call set_PropertyType)
					ilSetter.Emit(OpCodes.Callvirt, GetSetInfoPropertyType);

					// Get the setInfo local variable value
					ilSetter.Emit(OpCodes.Ldloc, setInfo);
					// Load the value on the stack
					ilSetter.Emit(OpCodes.Ldarg_1);
					// Set the value to the NewValue property of the SetInfo (call set_NewValue)
					ilSetter.Emit(OpCodes.Callvirt, SetInfoNewValue);

					// Load the instance of the class (this) on the stack, for later
					ilSetter.Emit(OpCodes.Ldarg_0);

					// Load the instance of the class (this) on the stack
					ilSetter.Emit(OpCodes.Ldarg_0);
					// Get the interceptor value
					ilSetter.Emit(OpCodes.Ldfld, interceptorField);

					// Get the setInfo local variable value
					ilSetter.Emit(OpCodes.Ldloc, setInfo);
					// Call the "SetMethod" method
					ilSetter.Emit(OpCodes.Callvirt, InterceptorSet);
					// Return
					ilSetter.Emit(OpCodes.Ret);
				}

				if (propertyInfo.CanRead)
				{
					var getterBuilder = typeBuilder.DefineMethod("get_" + propertyInfo.Name, SetgetMethodAttributes, propertyInfo.PropertyType, Type.EmptyTypes);
					var ilGetter = getterBuilder.GetILGenerator();

					// Local GetInfo variable
					var getInfo = ilGetter.DeclareLocal(typeof(GetInfo));

					// Create new GetInfo class for the argument which are passed to the IInterceptor
					ilGetter.Emit(OpCodes.Newobj, GetInfoConstructor);
					// Store it in the local getInfo variable
					ilGetter.Emit(OpCodes.Stloc, getInfo);

					// Get the getInfo local variable value
					ilGetter.Emit(OpCodes.Ldloc, getInfo);
					// Load the name of the property on the stack
					ilGetter.Emit(OpCodes.Ldstr, propertyInfo.Name);
					// Set the value to the PropertyName property of the SetInfo (call set_PropertyName)
					ilGetter.Emit(OpCodes.Callvirt, GetSetInfoPropertyName);

					// Get the getInfo local variable value
					ilGetter.Emit(OpCodes.Ldloc, getInfo);
					// Load the name of the property on the stack
					ilGetter.Emit(OpCodes.Ldtoken, propertyInfo.PropertyType);
					// Set the value to the PropertyType property of the GetInfo (call set_PropertyType)
					ilGetter.Emit(OpCodes.Callvirt, GetSetInfoPropertyType);

					// Load the instance of the class (this) on the stack, for later
					ilGetter.Emit(OpCodes.Ldarg_0);

					// Load the instance of the class (this) on the stack
					ilGetter.Emit(OpCodes.Ldarg_0);
					// Get the interceptor value
					ilGetter.Emit(OpCodes.Callvirt, GetInterceptor);

					// Call the "SetMethod" method
					ilGetter.Emit(OpCodes.Callvirt, InterceptorGet);

					// Get the getInfo local variable value
					ilGetter.Emit(OpCodes.Ldloc, getInfo);
					// Get the value of the GetInfo (call get_Value)
					ilGetter.Emit(OpCodes.Callvirt, GetInfoValue);

					// Return
					ilGetter.Emit(OpCodes.Ret);
				}
			}

			foreach (var methodInfo in interfaceType.GetMethods())
			{
				if (methodInfo.Name.StartsWith("get_") || methodInfo.Name.StartsWith("set_"))
				{
					continue;
				}
				var parameterTypes = (
					from parameterInfo in methodInfo.GetParameters()
					select parameterInfo.ParameterType).ToArray();
				var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes, methodInfo.ReturnType, parameterTypes);
				var ilMethod = methodBuilder.GetILGenerator();

				var local = ilMethod.DeclareLocal(typeof (object[]));
				// Store "this" at the stack
				ilMethod.Emit(OpCodes.Ldarg_0);
				// Check the parameters this method has
				var argumentCount = methodInfo.GetParameters().Count();
				// Place int32 with the array size on the stack, used up by the Newarr
				ilMethod.Emit(OpCodes.Ldc_I4, argumentCount);
				// new object[arraySize] on the stack
				ilMethod.Emit(OpCodes.Newarr, typeof (object));

				// local = array
				ilMethod.Emit(OpCodes.Stloc, local);
				// Assign all arguments to the array
				for (var i = 0; i < argumentCount; i++)
				{
					// array[i] = arguments[i]
					ilMethod.Emit(OpCodes.Ldloc, local);
					ilMethod.Emit(OpCodes.Ldc_I4, i);
					ilMethod.Emit(OpCodes.Ldarg, i + 1);
					ilMethod.Emit(OpCodes.Stelem_Ref);
				}

				// Place proxyField on the stack
				//ilMethod.Emit(OpCodes.Ldfld, interceptorField);
				// Place method name on the stack
				ilMethod.Emit(OpCodes.Ldstr, methodInfo.Name);
				// Place array on the stack
				ilMethod.Emit(OpCodes.Ldloc, local);

				ilMethod.Emit(OpCodes.Callvirt, invokeMethod);
				if (methodInfo.ReturnType == typeof (void))
				{
					// If the method should not return a value, we remove this from the stack
					ilMethod.Emit(OpCodes.Pop);
				}
				ilMethod.Emit(OpCodes.Ret);
			}
			// Example for making a exe, for a methodBuilder which creates a static main
			//assemblyBuilder.SetEntryPoint(methodBuilder,PEFileKinds.Exe);
			//assemblyBuilder.Save(dllName);
			return typeBuilder.CreateType();
		}

		private static FieldBuilder BuildProperty(TypeBuilder typeBuilder, string name, Type type)
		{
			var field = typeBuilder.DefineField("m" + name, type, FieldAttributes.Private);
			var propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.None, type, null);

			const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;

			var getter = typeBuilder.DefineMethod("get_" + name, getSetAttr, type, Type.EmptyTypes);

			var getIl = getter.GetILGenerator();
			getIl.Emit(OpCodes.Ldarg_0);
			getIl.Emit(OpCodes.Ldfld, field);
			getIl.Emit(OpCodes.Ret);

			var setter = typeBuilder.DefineMethod("set_" + name, getSetAttr, null, new[] { type });

			var setIl = setter.GetILGenerator();
			setIl.Emit(OpCodes.Ldarg_0);
			setIl.Emit(OpCodes.Ldarg_1);
			setIl.Emit(OpCodes.Stfld, field);
			setIl.Emit(OpCodes.Ret);

			propertyBuilder.SetGetMethod(getter);
			propertyBuilder.SetSetMethod(setter);
			return field;
		}
	}
}