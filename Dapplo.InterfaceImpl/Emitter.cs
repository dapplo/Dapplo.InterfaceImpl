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

using Dapplo.LogFacade;
using System;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Dapplo.InterfaceImpl
{
	public class Emitter
	{
		private static readonly LogSource Log = new LogSource();
		private static readonly MethodAttributes SetGetMethodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final;
		private static readonly ConstructorInfo SetInfoConstructor = typeof(SetInfo).GetConstructor(Type.EmptyTypes);
		private static readonly MethodInfo GetSetInfoPropertyName = typeof (GetSetInfo).GetProperty("PropertyName").GetSetMethod();
		private static readonly MethodInfo GetSetInfoPropertyType = typeof(GetSetInfo).GetProperty("PropertyType").GetSetMethod();
		private static readonly MethodInfo SetInfoNewValue = typeof(SetInfo).GetProperty("NewValue").GetSetMethod();
		private static readonly MethodInfo GetInfoValue = typeof(GetInfo).GetProperty("Value").GetGetMethod();
		private static readonly MethodInfo InterceptorGet = typeof(IInterceptor).GetMethod("Get");
		private static readonly MethodInfo InterceptorSet = typeof(IInterceptor).GetMethod("Set");
		private static readonly ConstructorInfo GetInfoConstructor = typeof(GetInfo).GetConstructor(Type.EmptyTypes);

		public static Type CreateType(string assemblyNameString, string typeName, Type interfaceType)
		{
			string dllName = $"{assemblyNameString}.dll";
			var assemblyName = new AssemblyName(assemblyNameString);
			var appDomain = AppDomain.CurrentDomain;
			var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave, AppDomain.CurrentDomain.BaseDirectory);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, dllName, false);

			// Create the type, and let it implement our interface
			var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed, typeof (object), new[] {interfaceType, typeof(IIntercepted)});

			var interceptorField = BuildProperty(typeBuilder, "Interceptor", typeof (IInterceptor));

			foreach (var propertyInfo in interfaceType.GetProperties())
			{
				if (!propertyInfo.CanRead && !propertyInfo.CanWrite)
				{
					continue;
				}
				var propertyBuilder = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.HasDefault, propertyInfo.PropertyType, null);

				if (propertyInfo.CanWrite)
				{
					var setterBuilder = typeBuilder.DefineMethod("set_" + propertyInfo.Name, SetGetMethodAttributes, typeof (void), new[] {propertyInfo.PropertyType});
					var ilSetter = setterBuilder.GetILGenerator();

					// Load the instance of the class (this) on the stack
					ilSetter.Emit(OpCodes.Ldarg_0);
					// Get the interceptor value of this._interceptor
					ilSetter.Emit(OpCodes.Ldfld, interceptorField);

					// Create new SetInfo class for the argument which are passed to the IInterceptor
					// Used in the Set() call
					ilSetter.Emit(OpCodes.Newobj, SetInfoConstructor);

					// Used in the GetSetInfo.PropertyType = assignment
					ilSetter.Emit(OpCodes.Dup);

					// Used in the GetSetInfo.PropertyName = assignment
					ilSetter.Emit(OpCodes.Dup);

					// Used in the SetInfo.NewValue = assignment
					ilSetter.Emit(OpCodes.Dup);

					// Load the argument with the value on the stack
					ilSetter.Emit(OpCodes.Ldarg_1);
					// Set the value to the NewValue property of the SetInfo (call set_NewValue)
					ilSetter.Emit(OpCodes.Callvirt, SetInfoNewValue);

					// Load the name of the property on the stack
					ilSetter.Emit(OpCodes.Ldstr, propertyInfo.Name);
					// Set the value to the PropertyName property of the SetInfo (call set_PropertyName)
					ilSetter.Emit(OpCodes.Callvirt, GetSetInfoPropertyName);

					// Load the type of the property on the stack
					ilSetter.Emit(OpCodes.Ldtoken, propertyInfo.PropertyType);
					// Convert the RuntimeTypeHandle to a type
					ilSetter.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }));
					// Set the value to the PropertyType property of the SetInfo (call set_PropertyType)
					ilSetter.Emit(OpCodes.Callvirt, GetSetInfoPropertyType);

					// Call the "SetMethod" method
					ilSetter.Emit(OpCodes.Callvirt, InterceptorSet);

					// return
					ilSetter.Emit(OpCodes.Ret);

					propertyBuilder.SetSetMethod(setterBuilder);
				}

				if (propertyInfo.CanRead)
				{
					var getterBuilder = typeBuilder.DefineMethod("get_" + propertyInfo.Name, SetGetMethodAttributes, propertyInfo.PropertyType, Type.EmptyTypes);
					var ilGetter = getterBuilder.GetILGenerator();

					// Local SetInfo variable
					var getInfo = ilGetter.DeclareLocal(typeof(GetInfo));

					// Create new GetInfo class for the argument which are passed to the IInterceptor
					ilGetter.Emit(OpCodes.Newobj, GetInfoConstructor);
					// Store it in the local setInfo variable
					ilGetter.Emit(OpCodes.Stloc, getInfo);

					// Get the getInfo local variable value
					ilGetter.Emit(OpCodes.Ldloc, getInfo);
					// Load the name of the property on the stack
					ilGetter.Emit(OpCodes.Ldstr, propertyInfo.Name);
					// Set the value to the PropertyName property of the GetSetInfo (call set_PropertyName)
					ilGetter.Emit(OpCodes.Callvirt, GetSetInfoPropertyName);

					// Get the getInfo local variable value
					ilGetter.Emit(OpCodes.Ldloc, getInfo);
					// Load the type of the property as RuntimeTypeHandle on the stack
					ilGetter.Emit(OpCodes.Ldtoken, propertyInfo.PropertyType);
					// Convert the RuntimeTypeHandle to a type
					ilGetter.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }));
					// Set the value to the PropertyType property of the GetSetInfo (call set_PropertyType)
					ilGetter.Emit(OpCodes.Callvirt, GetSetInfoPropertyType);

					// Load the instance of the class (this) on the stack
					ilGetter.Emit(OpCodes.Ldarg_0);
					// Get the interceptor value from this._interceptor
					ilGetter.Emit(OpCodes.Ldfld, interceptorField);
					// Get the getInfo local variable value
					ilGetter.Emit(OpCodes.Ldloc, getInfo);
					// Call the Get() method
					ilGetter.Emit(OpCodes.Callvirt, InterceptorGet);

					// Get the getInfo local variable value
					ilGetter.Emit(OpCodes.Ldloc, getInfo);
					// Call the get_Value method of the GetInfo
					ilGetter.Emit(OpCodes.Callvirt, GetInfoValue);

					// Cast the return value to the type of the property
					ilGetter.Emit(OpCodes.Castclass, propertyInfo.PropertyType);

					// Return the object on the stack, left by the InterceptorGet call
					ilGetter.Emit(OpCodes.Ret);

					propertyBuilder.SetGetMethod(getterBuilder);
				}
			}

			// Example for making a exe, for a methodBuilder which creates a static main
			//assemblyBuilder.SetEntryPoint(methodBuilder,PEFileKinds.Exe);
			var returnType = typeBuilder.CreateType();
			Log.Debug().WriteLine("Wrote {0} to {1}", dllName, AppDomain.CurrentDomain.BaseDirectory);
			assemblyBuilder.Save(dllName, PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);
			return returnType;
		}

		private static FieldBuilder BuildProperty(TypeBuilder typeBuilder, string name, Type type)
		{
			Log.Debug().WriteLine("Generating property {0} with type {1}", name, type.FullName);

			var backingField = typeBuilder.DefineField("_" + name.ToLowerInvariant(), type, FieldAttributes.Private | FieldAttributes.HasDefault);
			var propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.HasDefault, type, null);

			var getter = typeBuilder.DefineMethod("get_" + name, SetGetMethodAttributes, type, Type.EmptyTypes);
			var getIl = getter.GetILGenerator();
			getIl.Emit(OpCodes.Ldarg_0);
			getIl.Emit(OpCodes.Ldfld, backingField);
			getIl.Emit(OpCodes.Ret);
			propertyBuilder.SetGetMethod(getter);

			var setter = typeBuilder.DefineMethod("set_" + name, SetGetMethodAttributes, null, new[] { type });
			var setIl = setter.GetILGenerator();
			setIl.Emit(OpCodes.Ldarg_0);
			setIl.Emit(OpCodes.Ldarg_1);
			setIl.Emit(OpCodes.Stfld, backingField);
			setIl.Emit(OpCodes.Ret);
			propertyBuilder.SetSetMethod(setter);
			return backingField;
		}
	}
}