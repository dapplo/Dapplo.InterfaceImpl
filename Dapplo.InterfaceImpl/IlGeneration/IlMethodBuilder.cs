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

#endregion

namespace Dapplo.InterfaceImpl.IlGeneration
{
	/// <summary>
	///     Internally used to generate a method via IL
	/// </summary>
	internal static class IlMethodBuilder
	{
		private static readonly MethodAttributes MethodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final;
		private static readonly MethodInfo InterceptorInvoke = typeof (IExtensibleInterceptor).GetMethod("Invoke");

		/// <summary>
		///     Create the method invoke
		/// </summary>
		/// <param name="typeBuilder"></param>
		/// <param name="methodInfo"></param>
		internal static void BuildMethod(TypeBuilder typeBuilder, MethodInfo methodInfo)
		{
			var parameterTypes = (
				from parameterInfo in methodInfo.GetParameters()
				select parameterInfo.ParameterType).ToArray();

			var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes);
			methodBuilder.SetParameters(parameterTypes);
			methodBuilder.SetReturnType(methodInfo.ReturnType);

			if (methodInfo.IsGenericMethod)
			{
				var genericArguments = methodInfo.GetGenericArguments();
				methodBuilder.DefineGenericParameters(GetArgumentNames(genericArguments));
				methodBuilder.MakeGenericMethod(genericArguments);
			}

			GenerateForwardingIlMethod(methodBuilder, methodInfo.Name, parameterTypes.Length, methodInfo.ReturnType);
		}

		/// <summary>
		/// Generate a forwarding method, this calls IExtensibleInterceptor.Invoke
		/// </summary>
		/// <param name="methodBuilder">MethodBuilder</param>
		/// <param name="methodName">name of the method</param>
		/// <param name="parameters">number of parameters</param>
		/// <param name="returnType">Type</param>
		internal static void GenerateForwardingIlMethod(MethodBuilder methodBuilder, string methodName, int parameters, Type returnType)
		{
			var ilMethod = methodBuilder.GetILGenerator();

			var local = ilMethod.DeclareLocal(typeof(object[]));

			ilMethod.Emit(OpCodes.Ldc_I4, parameters);
			ilMethod.Emit(OpCodes.Newarr, typeof(object));
			ilMethod.Emit(OpCodes.Stloc, local);
			for (var i = 0; i < parameters; i++)
			{
				ilMethod.Emit(OpCodes.Ldloc, local);
				ilMethod.Emit(OpCodes.Ldc_I4, i);
				ilMethod.Emit(OpCodes.Ldarg, i + 1);
				ilMethod.Emit(OpCodes.Stelem_Ref);
			}

			// Load the instance of the class (this) on the stack
			ilMethod.Emit(OpCodes.Ldarg_0);
			ilMethod.Emit(OpCodes.Ldstr, methodName);
			ilMethod.Emit(OpCodes.Ldloc, local);

			ilMethod.Emit(OpCodes.Callvirt, InterceptorInvoke);
			if (returnType == typeof(void))
			{
				ilMethod.Emit(OpCodes.Pop);
			}
			else if (returnType.IsValueType)
			{
				ilMethod.Emit(OpCodes.Unbox_Any, returnType);
			}
			else
			{
				// Cast the return value
				ilMethod.Emit(OpCodes.Castclass, returnType);
			}
			ilMethod.Emit(OpCodes.Ret);
		}

		/// <summary>
		///     Gets the argument names from an array of generic argument types.
		/// </summary>
		/// <param name="genericArguments">The generic arguments.</param>
		private static string[] GetArgumentNames(Type[] genericArguments)
		{
			var genericArgumentNames = new string[genericArguments.Length];
			for (var i = 0; i < genericArguments.Length; i++)
			{
				genericArgumentNames[i] = genericArguments[i].Name;
			}
			return genericArgumentNames;
		}
	}
}