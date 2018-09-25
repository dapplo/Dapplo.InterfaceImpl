//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2017 Dapplo
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Dapplo.InterfaceImpl.IlGeneration
{
	/// <summary>
	///     Internally used to generate the methods which event logic uses
	/// </summary>
	internal static class IlEventBuilder
	{
		private const BindingFlags AllBindings = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private const MethodAttributes DefaultMethodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final;

		/// <summary>
		///     Create the methods add_ remove_ and invoke_ (custom) event-name
		/// </summary>
		/// <param name="typeBuilder">TypeBuilder</param>
		/// <param name="eventInfo">eventInfo</param>
		/// <param name="baseMethods">List of string with the method names of the base class, this is used to decide what can be skipped</param>
		internal static void BuildEvent(TypeBuilder typeBuilder, EventInfo eventInfo, IList<string> baseMethods)
		{
			if (eventInfo == null)
			{
				throw new ArgumentNullException(nameof(eventInfo));
			}
				
			var parameters = eventInfo.EventHandlerType.GetMethod("Invoke", AllBindings)?.GetParameters() ?? new ParameterInfo[]{};
			var parameterTypes = (
				from parameterInfo in parameters
				select parameterInfo.ParameterType).ToArray();

			// invoke_ this method is used by the EventObservable of Dapplo.Utils
			var methodName = $"invoke_{eventInfo.Name}";
			if (!baseMethods.Contains(methodName))
			{
				var methodBuilder = typeBuilder.DefineMethod(methodName, DefaultMethodAttributes);
				methodBuilder.SetParameters(parameterTypes);
				methodBuilder.SetReturnType(typeof(void));
				IlMethodBuilder.GenerateForwardingIlMethod(methodBuilder, methodName, parameters.Length, typeof(void));
			}

			// add_
			methodName = $"add_{eventInfo.Name}";
			if (!baseMethods.Contains(methodName))
			{
				var methodBuilder = typeBuilder.DefineMethod(methodName, DefaultMethodAttributes);
				methodBuilder.SetParameters(eventInfo.EventHandlerType);
				methodBuilder.SetReturnType(typeof(void));

				IlMethodBuilder.GenerateForwardingIlMethod(methodBuilder, methodName, 1, typeof(void));
			}

			// remove_
			methodName = $"remove_{eventInfo.Name}";
			if (!baseMethods.Contains(methodName))
			{
				var methodBuilder = typeBuilder.DefineMethod(methodName, DefaultMethodAttributes);
				methodBuilder.SetParameters(eventInfo.EventHandlerType);
				methodBuilder.SetReturnType(typeof(void));

				IlMethodBuilder.GenerateForwardingIlMethod(methodBuilder, methodName, 1, typeof(void));
			}
		}
	}
}