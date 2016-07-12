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
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.InterfaceImpl. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Dapplo.InterfaceImpl.Implementation;
using Dapplo.Log.Facade;

#endregion

namespace Dapplo.InterfaceImpl.IlGeneration
{
	/// <summary>
	///     Internally used to generate a method via IL
	/// </summary>
	internal static class IlGetSetBuilder
	{
		private static readonly LogSource Log = new LogSource();

		private static readonly MethodInfo GetValue = typeof (GetInfo).GetProperty("Value").GetGetMethod();
		private static readonly MethodInfo InterceptorGet = typeof (IExtensibleInterceptor).GetMethod("Get");
		private static readonly MethodInfo InterceptorSet = typeof (IExtensibleInterceptor).GetMethod("Set");

		private static readonly MethodAttributes SetGetMethodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
																		MethodAttributes.Virtual | MethodAttributes.Final;

		/// <summary>
		///     Create getter and/or setter
		/// </summary>
		/// <param name="typeBuilder"></param>
		/// <param name="propertyInfo"></param>
		internal static void BuildGetSet(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
		{
			// Special logic to allow indexer
			var callingConventions = CallingConventions.Any;
			var propertyAttributes = PropertyAttributes.HasDefault;
			var propertyBuilder = typeBuilder.DefineProperty(propertyInfo.Name, propertyAttributes, callingConventions, propertyInfo.PropertyType, null);

			// Create Get if the property can be read
			if (propertyInfo.CanRead)
			{
				var getterBuilder = BuildGetter(typeBuilder, propertyInfo);
				propertyBuilder.SetGetMethod(getterBuilder);
			}

			// Create Set if the property can be written
			if (propertyInfo.CanWrite)
			{
				var setterBuilder = BuildSetter(typeBuilder, propertyInfo);
				propertyBuilder.SetSetMethod(setterBuilder);
			}
		}

		/// <summary>
		///     Build the getter for the property
		/// </summary>
		/// <param name="typeBuilder"></param>
		/// <param name="propertyInfo"></param>
		/// <returns>MethodBuilder with the getter</returns>
		internal static MethodBuilder BuildGetter(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
		{
			var parameterTypes = Type.EmptyTypes;
			var isIndexer = propertyInfo.GetIndexParameters().Length > 0;
			if (isIndexer)
			{
				parameterTypes = new[] {propertyInfo.GetIndexParameters().First().ParameterType};
			}
			var getterBuilder = typeBuilder.DefineMethod("get_" + propertyInfo.Name, SetGetMethodAttributes, propertyInfo.PropertyType, parameterTypes);
			var ilGetter = getterBuilder.GetILGenerator();

			// Load the instance of the class (this) on the stack
			ilGetter.Emit(OpCodes.Ldarg_0);
			// Load the name of the property on the stack
			ilGetter.Emit(OpCodes.Ldstr, propertyInfo.Name);
			// Call the interceptor.Get method, this returns a GetInfo
			ilGetter.Emit(OpCodes.Callvirt, InterceptorGet);

			// Call the get_Value method of the GetInfo
			ilGetter.Emit(OpCodes.Callvirt, GetValue);

			ilGetter.Emit(propertyInfo.PropertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, propertyInfo.PropertyType);

			// Return the object on the stack, left by the Get call
			ilGetter.Emit(OpCodes.Ret);

			return getterBuilder;
		}

		/// <summary>
		///     Build the Setter for the property
		/// </summary>
		/// <param name="typeBuilder">TypeBuilder</param>
		/// <param name="propertyInfo">PropertyInfo which defines the type and name</param>
		/// <returns>MethodBuilder with the Setter</returns>
		internal static MethodBuilder BuildSetter(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
		{
			var setterBuilder = typeBuilder.DefineMethod("set_" + propertyInfo.Name, SetGetMethodAttributes, null, new[] {propertyInfo.PropertyType});
			var ilSetter = setterBuilder.GetILGenerator();

			// Load the instance of the class (this) on the stack
			ilSetter.Emit(OpCodes.Ldarg_0);
			// Load the name of the property on the stack
			ilSetter.Emit(OpCodes.Ldstr, propertyInfo.Name);
			// Load the argument with the value on the stack
			ilSetter.Emit(OpCodes.Ldarg_1);
			if (propertyInfo.PropertyType.IsValueType)
			{
				ilSetter.Emit(OpCodes.Box, propertyInfo.PropertyType);
			}

			// Call the "SetMethod" method
			ilSetter.Emit(OpCodes.Callvirt, InterceptorSet);

			// return
			ilSetter.Emit(OpCodes.Ret);

			return setterBuilder;
		}
	}
}