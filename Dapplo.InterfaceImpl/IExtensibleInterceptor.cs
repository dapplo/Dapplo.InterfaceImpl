﻿//  Dapplo - building blocks for desktop applications
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
using System.Collections.Generic;
using Dapplo.InterfaceImpl.Implementation;

#endregion

namespace Dapplo.InterfaceImpl
{
	/// <summary>
	///     This is the interface for the interceptor
	///     The meaning of this interface is to make clear what the interceptor has:
	///     Extensions can register get/set/invokes
	///     It is called by a generated (or manually implemented) implementation of an interface, this implementation calls the
	///     Get/Set/Invoke
	///     Properties of the "interface" have a backingstore implemented via the special property "Properties" and some other
	///     helping properties.
	/// </summary>
	public interface IExtensibleInterceptor
	{
		/// <summary>
		///     Errors which occur during initialization are stored here
		/// </summary>
		IDictionary<string, Exception> InitializationErrors { get; }

		Type InterceptedType { get; }

		/// <summary>
		///     All the properties with their value
		/// </summary>
		IDictionary<string, object> Properties { get; set; }

		/// <summary>
		///     This is needed as a reflection cache
		/// </summary>
		IReadOnlyDictionary<string, Type> PropertyTypes { get; }

		/// <summary>
		///     Add extension to the interceptor
		/// </summary>
		/// <param name="extensionType"></param>
		void AddExtension(Type extensionType);

		string Description(string propertyName);

		/// <summary>
		///     Get method, this will go through the extensions in the specified order and give the result
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		GetInfo Get(string propertyName);

		/// <summary>
		///     Initialize the interceptor
		/// </summary>
		void Init();

		/// <summary>
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		object Invoke(string methodName, params object[] parameters);

		void RegisterGetter(int order, Action<GetInfo> getterAction);

		void RegisterMethod(string methodname, Action<MethodCallInfo> methodAction);
		void RegisterSetter(int order, Action<SetInfo> setterAction);

		/// <summary>
		///     This is called when a property set is used on the intercepted class.
		/// </summary>
		/// <param name="propertyName">property name</param>
		/// <param name="value">object value</param>
		void Set(string propertyName, object value);
	}
}